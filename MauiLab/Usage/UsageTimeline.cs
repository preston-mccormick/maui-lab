using System.Globalization;

namespace MauiLab.Usage;

public static class UsageTimeline
{
    public static IReadOnlyList<TimelineRow> Project(IReadOnlyList<UsageEvent> events, DateTimeOffset now)
    {
        if (events.Count == 0)
            return [];

        var ordered = events
            .OrderBy(e => e.Timestamp)
            .ThenBy(e => e.Sequence)
            .ToList();
        var root = ordered.LastOrDefault(e =>
            e.Kind is UsageEventKind.Installed or UsageEventKind.HistoryReset);
        var relevant = root == null
            ? ordered
            : ordered.Where(e => UsageEvent.Compare(e, root) >= 0).ToList();

        var blocks = new List<(DateTimeOffset Sort, long Sequence, List<TimelineRow> Rows)>();
        var sessions = relevant
            .Where(e => e.Kind == UsageEventKind.SessionStarted && e.SessionId is Guid)
            .ToList();
        foreach (var session in sessions)
        {
            var inSession = relevant.Where(e => e.SessionId == session.SessionId).ToList();
            var rows = SessionRows(session, inSession, now);
            var newest = inSession
                .OrderByDescending(e => e.Timestamp)
                .ThenByDescending(e => e.Sequence)
                .First();
            blocks.Add((newest.Timestamp, newest.Sequence, rows));
        }

        foreach (var updated in relevant.Where(e => e.Kind == UsageEventKind.Updated))
        {
            blocks.Add((
                updated.Timestamp,
                updated.Sequence,
                [EventRow(updated, depth: 0, title: "Updated", detail: updated.Name ?? "", UsageCategory.Lifecycle, now)]));
        }

        var projected = new List<TimelineRow>();
        foreach (var block in blocks.OrderByDescending(block => block.Sort).ThenByDescending(block => block.Sequence))
            projected.AddRange(block.Rows);

        if (root != null)
        {
            projected.Add(new TimelineRow(
                root.Id.ToString("D"),
                depth: 0,
                title: root.Kind == UsageEventKind.HistoryReset ? "History reset" : "Installed",
                detail: root.Name ?? "",
                marker: "Root",
                when: FormatDate(root.Timestamp, now),
                category: UsageCategory.Lifecycle));
        }

        var rails = TimelineRail.Compute(
            projected.Select(row => row.Depth).ToArray(),
            projected.Select(row => row.Category).ToArray());
        for (var i = 0; i < projected.Count; i++)
            projected[i].Rail = rails[i];

        return projected;
    }

    public static string FormatDuration(TimeSpan span)
    {
        if (span < TimeSpan.Zero)
            span = TimeSpan.Zero;

        var totalSeconds = (long)span.TotalSeconds;
        var hours = totalSeconds / 3600;
        var minutes = totalSeconds % 3600 / 60;
        var seconds = totalSeconds % 60;

        if (hours > 0)
            return minutes == 0 ? $"{hours}h" : $"{hours}h {minutes}m";
        if (minutes > 0)
            return seconds == 0 ? $"{minutes}m" : $"{minutes}m {seconds}s";
        return $"{seconds}s";
    }

    public static string FormatWhen(DateTimeOffset timestamp, DateTimeOffset now)
    {
        var local = timestamp.ToLocalTime();
        var today = now.ToLocalTime().Date;
        if (local.Date == today)
            return local.ToString("h:mm:ss tt", CultureInfo.CurrentCulture);
        if (local.Year == today.Year)
            return local.ToString("MMM d, h:mm:ss tt", CultureInfo.CurrentCulture);
        return local.ToString("MMM d, yyyy, h:mm:ss tt", CultureInfo.CurrentCulture);
    }

    public static string PageTitle(string? name, string? route)
    {
        var label = name ?? route ?? "Page";
        return label.EndsWith(" page", StringComparison.OrdinalIgnoreCase)
            ? $"Navigated to {label}"
            : $"Navigated to {label} page";
    }

    static List<TimelineRow> SessionRows(UsageEvent session, List<UsageEvent> inSession, DateTimeOffset now)
    {
        var ended = inSession.FirstOrDefault(e => e.Kind == UsageEventKind.SessionEnded);
        var rows = new List<TimelineRow>();
        foreach (var entry in inSession
                     .Where(e => e.Kind is not (UsageEventKind.PageClosed or UsageEventKind.SessionEnded))
                     .OrderByDescending(e => e.Timestamp)
                     .ThenByDescending(e => e.Sequence))
        {
            switch (entry.Kind)
            {
                case UsageEventKind.PageOpened:
                    var close = inSession.FirstOrDefault(e =>
                        e.Kind == UsageEventKind.PageClosed && e.PageVisitId == entry.PageVisitId);
                    var asOf = close?.Timestamp ?? now;
                    var lifecycle = inSession.Where(e => e.PageVisitId == entry.PageVisitId);
                    var elapsed = close?.Elapsed ?? UsageMath.Elapsed(entry.Timestamp, asOf);
                    var active = close?.Active ?? UsageMath.ActiveTime(entry.Timestamp, asOf, lifecycle);
                    rows.Add(new TimelineRow(
                        entry.PageVisitId!.Value.ToString("D"),
                        depth: 1,
                        title: PageTitle(entry.Name, entry.Route),
                        detail: PageDetail(close == null, elapsed, active),
                        marker: close == null ? "Open" : "Page",
                        when: FormatWhen(entry.Timestamp, now),
                        category: UsageCategory.Navigation));
                    break;
                case UsageEventKind.Backgrounded:
                case UsageEventKind.Foregrounded:
                    rows.Add(EventRow(
                        entry,
                        depth: 2,
                        title: entry.Kind == UsageEventKind.Foregrounded ? "Foregrounded" : "Backgrounded",
                        detail: "",
                        UsageCategory.Lifecycle,
                        now));
                    break;
                case UsageEventKind.AreaChanged:
                    rows.Add(EventRow(
                        entry,
                        depth: entry.PageVisitId == null ? 1 : 2,
                        title: string.IsNullOrWhiteSpace(entry.Mode) ? "Screen" : entry.Mode,
                        detail: SizeText(entry),
                        UsageCategory.Screen,
                        now));
                    break;
                case UsageEventKind.ControlPressed:
                    rows.Add(EventRow(entry, 2, entry.Name ?? "Button", "", UsageCategory.Action, now));
                    break;
                case UsageEventKind.SurfaceOpened:
                    rows.Add(EventRow(entry, 3, entry.Name ?? "Menu", "", UsageCategory.Action, now));
                    break;
                case UsageEventKind.SelectionMade:
                    rows.Add(EventRow(entry, 4, entry.Name ?? "Dismissed", "", UsageCategory.Action, now));
                    break;
                case UsageEventKind.PermissionRequested:
                    rows.Add(EventRow(
                        entry,
                        depth: entry.PageVisitId == null ? 1 : 2,
                        title: entry.Name ?? "Permission",
                        detail: entry.Route ?? "",
                        UsageCategory.Permission,
                        now));
                    break;
                case UsageEventKind.SessionStarted:
                    rows.Add(new TimelineRow(
                        session.SessionId!.Value.ToString("D"),
                        depth: 0,
                        title: "Session started",
                        detail: SessionDetail(session, ended),
                        marker: "Session",
                        when: FormatWhen(session.Timestamp, now),
                        category: UsageCategory.Lifecycle));
                    break;
            }
        }

        return rows;
    }

    static TimelineRow EventRow(
        UsageEvent entry,
        int depth,
        string title,
        string detail,
        string category,
        DateTimeOffset now) => new(
        entry.Id.ToString("D"),
        depth,
        title,
        detail,
        marker: "Activity",
        when: FormatWhen(entry.Timestamp, now),
        category);

    static string SizeText(UsageEvent area)
    {
        var width = Math.Round(area.Width ?? 0).ToString("0", CultureInfo.InvariantCulture);
        var height = Math.Round(area.Height ?? 0).ToString("0", CultureInfo.InvariantCulture);
        return $"{width} × {height}";
    }

    static string PageDetail(bool stillOpen, TimeSpan elapsed, TimeSpan active)
    {
        var duration = $"open {FormatDuration(elapsed)} · active {FormatDuration(active)}";
        return stillOpen ? $"in progress · {duration}" : duration;
    }

    static string SessionDetail(UsageEvent session, UsageEvent? ended)
    {
        if (ended == null)
            return "";

        var elapsed = ended.Elapsed ?? UsageMath.Elapsed(session.Timestamp, ended.Timestamp);
        var active = ended.Active ?? TimeSpan.Zero;
        var reason = ended.EndReason == UsageEndReason.Terminated ? "terminated · " : "";
        return $"{reason}open {FormatDuration(elapsed)} · active {FormatDuration(active)}";
    }

    static string FormatDate(DateTimeOffset timestamp, DateTimeOffset now)
    {
        var local = timestamp.ToLocalTime();
        var format = local.Year == now.ToLocalTime().Year ? "MMM d" : "MMM d, yyyy";
        return local.ToString(format, CultureInfo.CurrentCulture);
    }
}
