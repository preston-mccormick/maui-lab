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

    [Fact]
    public void Compute_MatchesTheReferenceOnRandomDepthSequences()
    {
        var random = new Random(31);
        for (var round = 0; round < 200; round++)
        {
            var length = random.Next(0, 60);
            var depths = Enumerable.Range(0, length).Select(_ => random.Next(0, 5)).ToArray();
            var categories = Enumerable.Repeat(UsageCategory.Action, length).ToArray();

            var expected = LegacyTimeline.ComputeRails(depths, categories);
            var actual = TimelineRail.Compute(depths, categories);

            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void Equality_ComparesLaneContents()
    {
        var left = new TimelineRail(2, UsageCategory.Action, true, false, 1, [0, 1]);
        var right = new TimelineRail(2, UsageCategory.Action, true, false, 1, [0, 1]);
        var other = new TimelineRail(2, UsageCategory.Action, true, false, 1, [0]);

        Assert.Equal(left, right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
        Assert.NotEqual(left, other);
    }
}
