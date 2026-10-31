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
    public static TimelineRail[] Compute(IReadOnlyList<int> depths, IReadOnlyList<string> categories)
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
}
