using MauiLab.Usage;

namespace MauiLabTests;

/// <summary>
/// The pre-optimisation projection and rail algorithms, kept verbatim as an oracle.
/// They rescan the event list per session and per visit, so they are slow but obviously correct.
/// </summary>
static class LegacyTimeline
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

        var rails = ComputeRails(
            projected.Select(row => row.Depth).ToArray(),
            projected.Select(row => row.Category).ToArray());
        for (var i = 0; i < projected.Count; i++)
            projected[i].Rail = rails[i];

        return projected;
    }

    public static TimelineRail[] ComputeRails(IReadOnlyList<int> depths, IReadOnlyList<string> categories)
    {
        var rails = new TimelineRail[depths.Count];
        var curves = new int?[depths.Count];
        for (var i = 0; i < depths.Count; i++)
        {
            var next = i + 1 < depths.Count ? depths[i + 1] : -1;
            curves[i] = next >= 0 && next < depths[i] ? next : null;
        }

        for (var i = 0; i < depths.Count; i++)
        {
            var depth = depths[i];
            var previous = i > 0 ? depths[i - 1] : -1;
            var next = i + 1 < depths.Count ? depths[i + 1] : -1;
            var fromTop = previous == depth || (previous > depth && curves[i - 1] == depth);
            var lanes = new List<int>();
            for (var lane = 0; lane < depth; lane++)
            {
                if (ContinuesThrough(depths, i, lane))
                    lanes.Add(lane);
            }

            rails[i] = new TimelineRail(
                depth,
                categories[i],
                fromTop,
                next >= depth,
                curves[i],
                lanes.ToArray());
        }

        return rails;
    }

    static bool ContinuesThrough(IReadOnlyList<int> depths, int index, int lane)
    {
        var above = false;
        for (var i = index - 1; i >= 0; i--)
        {
            if (depths[i] < lane)
                return false;
            if (depths[i] == lane)
            {
                above = true;
                break;
            }
        }

        if (!above)
            return false;

        for (var i = index + 1; i < depths.Count; i++)
        {
            if (depths[i] < lane)
                return false;
            if (depths[i] == lane)
                return true;
        }

        return false;
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
                        title: UsageTimeline.PageTitle(entry.Name, entry.Route),
                        detail: PageDetail(close == null, elapsed, active),
                        marker: close == null ? "Open" : "Page",
                        when: UsageTimeline.FormatWhen(entry.Timestamp, now),
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
                case UsageEventKind.ActionTaken:
                    rows.Add(EventRow(entry, 2, entry.Name ?? "Action", entry.Detail ?? "", UsageCategory.Action, now));
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
                        when: UsageTimeline.FormatWhen(session.Timestamp, now),
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
        when: UsageTimeline.FormatWhen(entry.Timestamp, now),
        category);

    static string SizeText(UsageEvent area)
    {
        var width = Math.Round(area.Width ?? 0).ToString("0", System.Globalization.CultureInfo.InvariantCulture);
        var height = Math.Round(area.Height ?? 0).ToString("0", System.Globalization.CultureInfo.InvariantCulture);
        return $"{width} × {height}";
    }

    static string PageDetail(bool stillOpen, TimeSpan elapsed, TimeSpan active)
    {
        var duration = $"open {UsageTimeline.FormatDuration(elapsed)} · active {UsageTimeline.FormatDuration(active)}";
        return stillOpen ? $"in progress · {duration}" : duration;
    }

    static string SessionDetail(UsageEvent session, UsageEvent? ended)
    {
        if (ended == null)
            return "";

        var elapsed = ended.Elapsed ?? UsageMath.Elapsed(session.Timestamp, ended.Timestamp);
        var active = ended.Active ?? TimeSpan.Zero;
        var reason = ended.EndReason == UsageEndReason.Terminated ? "terminated · " : "";
        return $"{reason}open {UsageTimeline.FormatDuration(elapsed)} · active {UsageTimeline.FormatDuration(active)}";
    }

    static string FormatDate(DateTimeOffset timestamp, DateTimeOffset now)
    {
        var local = timestamp.ToLocalTime();
        var format = local.Year == now.ToLocalTime().Year ? "MMM d" : "MMM d, yyyy";
        return local.ToString(format, System.Globalization.CultureInfo.CurrentCulture);
    }
}
