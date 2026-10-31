using System.Globalization;

namespace MauiLab.Usage;

public static class UsageTimeline
{
    public static IReadOnlyList<TimelineRow> Project(IReadOnlyList<UsageEvent> events, DateTimeOffset now)
    {
        if (events.Count == 0)
            return [];

        // One stable sort; everything after this is a linear pass over the sorted list.
        var ordered = events
            .OrderBy(e => e.Timestamp)
            .ThenBy(e => e.Sequence)
            .ToList();
        var root = ordered.LastOrDefault(e =>
            e.Kind is UsageEventKind.Installed or UsageEventKind.HistoryReset);
        var relevant = root == null
            ? ordered
            : ordered.Where(e => UsageEvent.Compare(e, root) >= 0).ToList();

        // Group once by session, keeping sorted order inside each group.
        var sessions = new List<UsageEvent>();
        var bySession = new Dictionary<Guid, List<UsageEvent>>();
        var updates = new List<UsageEvent>();
        foreach (var entry in relevant)
        {
            if (entry.Kind == UsageEventKind.Updated)
                updates.Add(entry);

            if (entry.SessionId is not Guid sessionId)
                continue;

            if (!bySession.TryGetValue(sessionId, out var group))
            {
                group = [];
                bySession[sessionId] = group;
            }

            group.Add(entry);
            if (entry.Kind == UsageEventKind.SessionStarted)
                sessions.Add(entry);
        }

        var blocks = new List<(DateTimeOffset Sort, long Sequence, List<TimelineRow> Rows)>();
        foreach (var session in sessions)
        {
            var inSession = bySession[session.SessionId!.Value];
            var rows = SessionRows(session, inSession, now);
            var newest = inSession[^1];
            blocks.Add((newest.Timestamp, newest.Sequence, rows));
        }

        foreach (var updated in updates)
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

    /// <param name="inSession">Every event in the session, oldest first.</param>
    static List<TimelineRow> SessionRows(UsageEvent session, List<UsageEvent> inSession, DateTimeOffset now)
    {
        // One pass to index the session: the first close per visit, and every event per visit.
        UsageEvent? ended = null;
        var closes = new Dictionary<Guid, UsageEvent>();
        var byVisit = new Dictionary<Guid, List<UsageEvent>>();
        foreach (var entry in inSession)
        {
            if (entry.Kind == UsageEventKind.SessionEnded)
                ended ??= entry;

            if (entry.PageVisitId is not Guid visitId)
                continue;

            if (entry.Kind == UsageEventKind.PageClosed)
                closes.TryAdd(visitId, entry);

            if (!byVisit.TryGetValue(visitId, out var visit))
            {
                visit = [];
                byVisit[visitId] = visit;
            }

            visit.Add(entry);
        }

        var rows = new List<TimelineRow>();
        for (var index = inSession.Count - 1; index >= 0; index--)
        {
            var entry = inSession[index];
            if (entry.Kind is UsageEventKind.PageClosed or UsageEventKind.SessionEnded)
                continue;

            switch (entry.Kind)
            {
                case UsageEventKind.PageOpened:
                    var visitId = entry.PageVisitId!.Value;
                    var close = closes.GetValueOrDefault(visitId);
                    var asOf = close?.Timestamp ?? now;
                    var lifecycle = byVisit[visitId];
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
