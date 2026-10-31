using MauiLab.Usage;

namespace MauiLabTests;

public class TimelineRailTests
{
    [Fact]
    public void Compute_CurvesIntoTheParentAndKeepsTheSpine()
    {
        var depths = new[] { 1, 2, 2, 1, 1, 0, 0 };
        var categories = Enumerable.Repeat(UsageCategory.Lifecycle, depths.Length).ToArray();

        var rails = TimelineRail.Compute(depths, categories);

        Assert.False(rails[0].LineFromTop);
        Assert.True(rails[0].LineToBottom);
        Assert.Null(rails[0].CurveToLane);

        Assert.False(rails[1].LineFromTop);
        Assert.True(rails[1].LineToBottom);
        Assert.Contains(1, rails[1].VerticalLanes);

        Assert.True(rails[2].LineFromTop);
        Assert.False(rails[2].LineToBottom);
        Assert.Equal(1, rails[2].CurveToLane);

        Assert.True(rails[3].LineFromTop);
        Assert.True(rails[3].LineToBottom);
        Assert.Null(rails[3].CurveToLane);

        Assert.True(rails[4].CurveToLane == 0);
        Assert.True(rails[5].LineFromTop);
        Assert.True(rails[5].LineToBottom);
        Assert.True(rails[6].LineFromTop);
        Assert.False(rails[6].LineToBottom);
    }
}
