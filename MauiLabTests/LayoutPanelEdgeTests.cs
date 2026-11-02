using MauiLab.Layouts;

namespace MauiLabTests;

public class LayoutPanelEdgeTests
{
    [Theory]
    [InlineData(799, PanelEdge.Bottom)]
    [InlineData(800, PanelEdge.Right)]
    [InlineData(1200, PanelEdge.Right)]
    [InlineData(0, PanelEdge.Bottom)]
    public void ForWidth_SplitsAt800(double width, PanelEdge edge) =>
        Assert.Equal(edge, LayoutPanelEdge.ForWidth(width));

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ForWidth_NonFiniteIsBottom(double width) =>
        Assert.Equal(PanelEdge.Bottom, LayoutPanelEdge.ForWidth(width));
}