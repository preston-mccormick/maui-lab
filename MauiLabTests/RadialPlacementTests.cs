using MauiLab.Layouts;

namespace MauiLabTests;

public class RadialPlacementTests
{
    static readonly Point Center = new(100, 100);

    [Fact]
    public void WiderSweep_KeepsTheStartAngle()
    {
        var narrow = RadialPlacement.Place(Center, 50, 0, 180, [null, null, null, null]);
        var wide = RadialPlacement.Place(Center, 50, 0, 270, [null, null, null, null]);

        Assert.Equal(narrow[0], wide[0]);
        Assert.Equal(RadialPlacement.PointOnArc(Center, 50, 0), narrow[0]);
        Assert.Equal(RadialPlacement.PointOnArc(Center, 50, 180), narrow[^1]);
        Assert.Equal(RadialPlacement.PointOnArc(Center, 50, 270), wide[^1]);
    }

    [Fact]
    public void AngleOverride_LeavesTheEvenSpacing()
    {
        var placed = RadialPlacement.Place(Center, 50, 0, 180, [null, 10, null, null]);

        Assert.Equal(RadialPlacement.PointOnArc(Center, 50, 0), placed[0]);
        Assert.Equal(RadialPlacement.PointOnArc(Center, 50, 10), placed[1]);
        Assert.Equal(RadialPlacement.PointOnArc(Center, 50, 90), placed[2]);
        Assert.Equal(RadialPlacement.PointOnArc(Center, 50, 180), placed[3]);
    }
}
