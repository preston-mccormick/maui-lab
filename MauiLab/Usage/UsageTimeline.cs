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

        var rows = new List<TimelineRow>();
        var sessions = relevant
            .Where(e => e.Kind == UsageEventKind.SessionStarted && e.SessionId is Guid)
            .OrderByDescending(e => e.Timestamp)
            .ThenByDescending(e => e.Sequence)
            .ToList();

        foreach (var session in sessions)
        {
            var sessionId = session.SessionId!.Value;
            var inSession = relevant.Where(e => e.SessionId == sessionId).ToList();
            var ended = inSession.FirstOrDefault(e => e.Kind == UsageEventKind.SessionEnded);
            var blocks = new List<(DateTimeOffset Sort, long Sequence, List<TimelineRow> Rows)>();

            foreach (var open in inSession.Where(e => e.Kind == UsageEventKind.PageOpened && e.PageVisitId is Guid))
            {
                var close = inSession.FirstOrDefault(e =>
                    e.Kind == UsageEventKind.PageClosed && e.PageVisitId == open.PageVisitId);
                blocks.Add((
                    close?.Timestamp ?? now,
                    close?.Sequence ?? long.MaxValue,
                    PageRows(open, close, inSession, now)));
            }

            foreach (var area in inSession.Where(e => e.Kind == UsageEventKind.AreaChanged))
            {
                blocks.Add((
                    area.Timestamp,
                    area.Sequence,
                    [AreaRow(area, now)]));
            }

            foreach (var block in blocks.OrderByDescending(block => block.Sort).ThenByDescending(block => block.Sequence))
                rows.AddRange(block.Rows);

            rows.Add(new TimelineRow(
                session.SessionId.Value.ToString("D"),
                depth: 0,
                title: "Session started",
                detail: SessionDetail(session, ended),
                marker: "Session",
                when: FormatWhen(session.Timestamp, now)));
        }

        if (root != null)
        {
            rows.Add(new TimelineRow(
                root.Id.ToString("D"),
                depth: 0,
                title: root.Kind == UsageEventKind.HistoryReset ? "History reset" : "Installed",
                detail: "",
                marker: "Root",
                when: FormatDate(root.Timestamp, now)));
        }

        return rows;
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
            return local.ToString("h:mm tt", CultureInfo.CurrentCulture);
        if (local.Year == today.Year)
            return local.ToString("MMM d, h:mm tt", CultureInfo.CurrentCulture);
        return local.ToString("MMM d, yyyy, h:mm tt", CultureInfo.CurrentCulture);
    }

    public static string PageTitle(string? name, string? route)
    {
        var label = name ?? route ?? "Page";
        return label.EndsWith(" page", StringComparison.OrdinalIgnoreCase)
            ? $"Navigated to {label}"
            : $"Navigated to {label} page";
    }

    static List<TimelineRow> PageRows(
        UsageEvent open,
        UsageEvent? close,
        List<UsageEvent> inSession,
        DateTimeOffset now)
    {
        var asOf = close?.Timestamp ?? now;
        var lifecycle = inSession.Where(e => e.PageVisitId == open.PageVisitId);
        var elapsed = close?.Elapsed ?? UsageMath.Elapsed(open.Timestamp, asOf);
        var active = close?.Active ?? UsageMath.ActiveTime(open.Timestamp, asOf, lifecycle);
        var stillOpen = close == null;
        var rows = new List<TimelineRow>
        {
            new(
                open.PageVisitId!.Value.ToString("D"),
                depth: 1,
                title: PageTitle(open.Name, open.Route),
                detail: PageDetail(stillOpen, elapsed, active),
                marker: stillOpen ? "Open" : "Page",
                when: FormatWhen(open.Timestamp, now))
        };

        var children = lifecycle
            .Where(e => e.Kind is UsageEventKind.Backgrounded or UsageEventKind.Foregrounded or UsageEventKind.ControlPressed)
            .OrderByDescending(e => e.Timestamp)
            .ThenByDescending(e => e.Sequence);
        foreach (var child in children)
        {
            if (child.Kind == UsageEventKind.ControlPressed)
                rows.AddRange(ControlRows(child, inSession, now));
            else
            {
                rows.Add(new TimelineRow(
                    child.Id.ToString("D"),
                    depth: 2,
                    title: child.Kind == UsageEventKind.Foregrounded ? "Foregrounded" : "Backgrounded",
                    detail: "",
                    marker: "Activity",
                    when: FormatWhen(child.Timestamp, now)));
            }
        }

        return rows;
    }

    static List<TimelineRow> ControlRows(UsageEvent control, List<UsageEvent> inSession, DateTimeOffset now)
    {
        var rows = new List<TimelineRow>
        {
            EventRow(control, depth: 2, title: control.Name ?? "Button", now)
        };

        var surfaces = inSession
            .Where(e => e.Kind == UsageEventKind.SurfaceOpened && e.ParentId == control.Id)
            .OrderByDescending(e => e.Timestamp)
            .ThenByDescending(e => e.Sequence);
        foreach (var surface in surfaces)
        {
            rows.Add(EventRow(surface, depth: 3, title: surface.Name ?? "Menu", now));
            var choices = inSession
                .Where(e => e.Kind == UsageEventKind.SelectionMade && e.ParentId == surface.Id)
                .OrderByDescending(e => e.Timestamp)
                .ThenByDescending(e => e.Sequence);
            foreach (var choice in choices)
                rows.Add(EventRow(choice, depth: 4, title: choice.Name ?? "Dismissed", now));
        }

        return rows;
    }

    static TimelineRow AreaRow(UsageEvent area, DateTimeOffset now) => new(
        area.Id.ToString("D"),
        depth: 1,
        title: $"{Math.Round(area.Width ?? 0).ToString("0", CultureInfo.InvariantCulture)} × {Math.Round(area.Height ?? 0).ToString("0", CultureInfo.InvariantCulture)}",
        detail: area.Mode ?? "",
        marker: "Activity",
        when: FormatWhen(area.Timestamp, now));

    static TimelineRow EventRow(UsageEvent usageEvent, int depth, string title, DateTimeOffset now) => new(
        usageEvent.Id.ToString("D"),
        depth,
        title,
        detail: "",
        marker: "Activity",
        when: FormatWhen(usageEvent.Timestamp, now));

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
