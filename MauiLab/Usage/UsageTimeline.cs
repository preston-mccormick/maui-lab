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
            var pages = inSession
                .Where(e => e.Kind == UsageEventKind.PageOpened && e.PageVisitId is Guid)
                .Select(open =>
                {
                    var close = inSession.FirstOrDefault(e =>
                        e.Kind == UsageEventKind.PageClosed && e.PageVisitId == open.PageVisitId);
                    var sort = close?.Timestamp ?? now;
                    var sortSequence = close?.Sequence ?? long.MaxValue;
                    return (open, close, sort, sortSequence);
                })
                .OrderByDescending(page => page.sort)
                .ThenByDescending(page => page.sortSequence)
                .ToList();

            foreach (var (open, close, _, _) in pages)
            {
                var asOf = close?.Timestamp ?? now;
                var lifecycle = inSession.Where(e => e.PageVisitId == open.PageVisitId);
                var elapsed = close?.Elapsed ?? UsageMath.Elapsed(open.Timestamp, asOf);
                var active = close?.Active ?? UsageMath.ActiveTime(open.Timestamp, asOf, lifecycle);
                var stillOpen = close == null;
                rows.Add(new TimelineRow(
                    open.PageVisitId!.Value.ToString("D"),
                    depth: 1,
                    title: open.Name ?? open.Route ?? "Page",
                    detail: PageDetail(stillOpen, elapsed, active),
                    marker: stillOpen ? "Open" : "Page"));

                foreach (var child in lifecycle
                             .Where(e => e.Kind is UsageEventKind.Backgrounded or UsageEventKind.Foregrounded)
                             .OrderByDescending(e => e.Timestamp)
                             .ThenByDescending(e => e.Sequence))
                {
                    rows.Add(new TimelineRow(
                        child.Id.ToString("D"),
                        depth: 2,
                        title: child.Kind == UsageEventKind.Foregrounded ? "Foregrounded" : "Backgrounded",
                        detail: FormatWhen(child.Timestamp, now),
                        marker: "Activity"));
                }
            }

            rows.Add(new TimelineRow(
                session.SessionId.Value.ToString("D"),
                depth: 0,
                title: "Session started",
                detail: SessionDetail(session, ended, now),
                marker: "Session"));
        }

        if (root != null)
        {
            rows.Add(new TimelineRow(
                root.Id.ToString("D"),
                depth: 0,
                title: root.Kind == UsageEventKind.HistoryReset ? "History reset" : "Installed",
                detail: FormatDate(root.Timestamp, now),
                marker: "Root"));
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

    static string PageDetail(bool stillOpen, TimeSpan elapsed, TimeSpan active)
    {
        var duration = $"open {FormatDuration(elapsed)} · active {FormatDuration(active)}";
        return stillOpen ? $"in progress · {duration}" : duration;
    }

    static string SessionDetail(UsageEvent session, UsageEvent? ended, DateTimeOffset now)
    {
        var when = FormatWhen(session.Timestamp, now);
        if (ended == null)
            return when;

        var elapsed = ended.Elapsed ?? UsageMath.Elapsed(session.Timestamp, ended.Timestamp);
        var active = ended.Active ?? TimeSpan.Zero;
        var reason = ended.EndReason == UsageEndReason.Terminated ? "terminated · " : "";
        return $"{when} · {reason}open {FormatDuration(elapsed)} · active {FormatDuration(active)}";
    }

    static string FormatWhen(DateTimeOffset timestamp, DateTimeOffset now)
    {
        var local = timestamp.ToLocalTime();
        var today = now.ToLocalTime().Date;
        if (local.Date == today)
            return local.ToString("h:mm tt", CultureInfo.CurrentCulture);
        if (local.Year == today.Year)
            return local.ToString("MMM d, h:mm tt", CultureInfo.CurrentCulture);
        return local.ToString("MMM d, yyyy, h:mm tt", CultureInfo.CurrentCulture);
    }

    static string FormatDate(DateTimeOffset timestamp, DateTimeOffset now)
    {
        var local = timestamp.ToLocalTime();
        var format = local.Year == now.ToLocalTime().Year ? "MMM d" : "MMM d, yyyy";
        return local.ToString(format, CultureInfo.CurrentCulture);
    }
}
