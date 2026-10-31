namespace MauiLab.Usage;

public static class UsageCategory
{
    public const string Navigation = "Navigation";
    public const string Screen = "Screen";
    public const string Lifecycle = "Lifecycle";
    public const string Action = "Action";
    public const string Permission = "Permission";
}

public sealed record TimelineRail(
    int Depth,
    string Category,
    bool LineFromTop,
    bool LineToBottom,
    int? CurveToLane,
    int[] VerticalLanes)
{
    public bool Equals(TimelineRail? other) =>
        other is not null &&
        Depth == other.Depth &&
        Category == other.Category &&
        LineFromTop == other.LineFromTop &&
        LineToBottom == other.LineToBottom &&
        CurveToLane == other.CurveToLane &&
        VerticalLanes.AsSpan().SequenceEqual(other.VerticalLanes);

    public override int GetHashCode()
    {
        var hash = HashCode.Combine(Depth, Category, LineFromTop, LineToBottom, CurveToLane);
        foreach (var lane in VerticalLanes)
            hash = HashCode.Combine(hash, lane);
        return hash;
    }

    /// <summary>
    /// Two sweeps, one down and one up. A lane is drawn through a row when the nearest row at or
    /// below that lane's depth, in both directions, sits exactly at that depth.
    /// </summary>
    public static TimelineRail[] Compute(IReadOnlyList<int> depths, IReadOnlyList<string> categories)
    {
        var count = depths.Count;
        var rails = new TimelineRail[count];
        if (count == 0)
            return rails;

        var maxDepth = 0;
        for (var i = 0; i < count; i++)
            maxDepth = Math.Max(maxDepth, depths[i]);
        var laneCount = maxDepth + 1;

        var curves = new int?[count];
        for (var i = 0; i < count; i++)
        {
            var next = i + 1 < count ? depths[i + 1] : -1;
            curves[i] = next >= 0 && next < depths[i] ? next : null;
        }

        // openAbove[lane] after row i: the nearest row at or above row i with depth <= lane has depth == lane.
        var fromAbove = new bool[count * laneCount];
        var openAbove = new bool[laneCount];
        for (var i = 0; i < count; i++)
        {
            var depth = depths[i];
            Array.Copy(openAbove, 0, fromAbove, i * laneCount, laneCount);
            Mark(openAbove, depth);
        }

        var openBelow = new bool[laneCount];
        for (var i = count - 1; i >= 0; i--)
        {
            var depth = depths[i];
            var previous = i > 0 ? depths[i - 1] : -1;
            var next = i + 1 < count ? depths[i + 1] : -1;
            var fromTop = previous == depth || (previous > depth && curves[i - 1] == depth);

            var lanes = new List<int>();
            for (var lane = 0; lane < depth; lane++)
            {
                if (fromAbove[i * laneCount + lane] && openBelow[lane])
                    lanes.Add(lane);
            }

            rails[i] = new TimelineRail(
                depth,
                categories[i],
                fromTop,
                next >= depth,
                curves[i],
                lanes.ToArray());

            Mark(openBelow, depth);
        }

        return rails;
    }

    /// <summary>A row at <paramref name="depth"/> opens its own lane and cuts every deeper lane.</summary>
    static void Mark(bool[] open, int depth)
    {
        if (depth < 0)
            return;

        open[depth] = true;
        for (var lane = depth + 1; lane < open.Length; lane++)
            open[lane] = false;
    }
}
