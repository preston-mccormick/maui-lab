using MauiLab.Layouts;
using Microsoft.Maui.Layouts;

namespace MauiLabTests;

public class AbsolutePlacementTests
{
    [Fact]
    public void Move_AbsolutePanAddsTheDelta()
    {
        var moved = AbsolutePlacement.Move(new Rect(10, 20, 30, 40), 15, -5, AbsoluteLayoutFlags.None, new Size(200, 100));

        Assert.Equal(new Rect(25, 15, 30, 40), moved);
    }

    [Fact]
    public void Move_ProportionalPanKeepsTheFractionWhenTheCanvasGrows()
    {
        var flags = AbsoluteLayoutFlags.PositionProportional;
        var moved = AbsolutePlacement.Move(new Rect(0.5, 0.25, 0.2, 0.1), 20, 0, flags, new Size(200, 100));

        Assert.Equal(0.6, moved.X, 5);
        var before = AbsolutePlacement.ToPixels(moved, flags, new Size(200, 100));
        var after = AbsolutePlacement.ToPixels(moved, flags, new Size(400, 200));
        Assert.Equal(before.X / 200d, after.X / 400d, 5);
        Assert.Equal(before.Y / 100d, after.Y / 200d, 5);
    }

    [Fact]
    public void Random_SeededRectangleStaysInsideTheCanvas()
    {
        var rect = AbsolutePlacement.Random(new Random(1), new Size(200, 100), new Size(20, 10));

        Assert.InRange(rect.X, 0, 180);
        Assert.InRange(rect.Y, 0, 90);
        Assert.Equal(20, rect.Width);
        Assert.Equal(10, rect.Height);
    }
}
