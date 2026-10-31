using MauiLab.MenuButton.Controls;
using Microsoft.Maui.Graphics;

namespace MauiLabTests;

public class MenuPlacementTests
{
    static readonly Size Window = new(400, 800);
    static readonly Size ButtonSize = new(132, 44);

    [Fact]
    public void GrowsDownOverTheButton()
    {
        var button = new Rect(20, 30, ButtonSize.Width, ButtonSize.Height);
        var content = new Size(100, 200);

        var place = MenuPlacement.Place(button, content, Window, edge: 0);

        Assert.Equal(20d, place.X);
        Assert.Equal(30d, place.Y);
        Assert.Equal(132d, place.Width);
        Assert.Equal(200d, place.Height);
        Assert.False(place.Scroll);
    }

    [Fact]
    public void ShiftsUpAtTheBottomEdge()
    {
        var button = new Rect(20, 700, ButtonSize.Width, ButtonSize.Height);
        var content = new Size(100, 200);

        var place = MenuPlacement.Place(button, content, Window, edge: 0);

        Assert.Equal(600d, place.Y);
        Assert.Equal(200d, place.Height);
        Assert.False(place.Scroll);
        Assert.InRange(button.Y, place.Y, place.Y + place.Height);
        Assert.InRange(button.Bottom, place.Y, place.Y + place.Height);
    }

    [Fact]
    public void ShiftsLeftAtTheRightEdge()
    {
        var button = new Rect(300, 30, ButtonSize.Width, ButtonSize.Height);
        var content = new Size(200, 100);

        var place = MenuPlacement.Place(button, content, Window, edge: 0);

        Assert.Equal(200d, place.Width);
        Assert.Equal(200d, place.X);
        Assert.Equal(30d, place.Y);
        Assert.False(place.Scroll);
    }

    [Fact]
    public void WidthStaysAtLeastTheButtonWidth()
    {
        var button = new Rect(10, 10, ButtonSize.Width, ButtonSize.Height);
        var content = new Size(40, 80);

        var place = MenuPlacement.Place(button, content, Window, edge: 0);

        Assert.Equal(132d, place.Width);
        Assert.Equal(80d, place.Height);
        Assert.False(place.Scroll);
    }

    [Fact]
    public void TallContentFillsTheWindowAndScrolls()
    {
        var button = new Rect(10, 100, ButtonSize.Width, ButtonSize.Height);
        var content = new Size(132, 2000);

        var place = MenuPlacement.Place(button, content, Window, edge: 0);

        Assert.Equal(0d, place.Y);
        Assert.Equal(800d, place.Height);
        Assert.Equal(132d, place.Width);
        Assert.True(place.Scroll);
    }

    [Fact]
    public void KeepsTheMenuTopOnTheButton()
    {
        var edge = MenuPlacement.EdgeClearance;
        var button = new Rect(24, 20, ButtonSize.Width, ButtonSize.Height);
        var content = new Size(100, 200);

        var place = MenuPlacement.Place(button, content, Window);

        Assert.Equal(24d, place.X);
        Assert.Equal(20d, place.Y);
        Assert.Equal(132d, place.Width);
        Assert.True(20d < edge);
        Assert.False(place.Scroll);
    }

    [Fact]
    public void LeavesClearanceAtTheBottomEdge()
    {
        var edge = MenuPlacement.EdgeClearance;
        var button = new Rect(24, 500, ButtonSize.Width, ButtonSize.Height);
        var content = new Size(132, 400);

        var place = MenuPlacement.Place(button, content, Window);

        Assert.Equal(800d - edge - 400d, place.Y);
        Assert.Equal(800d - edge, place.Y + place.Height);
        Assert.False(place.Scroll);
    }

    [Fact]
    public void TallContentScrollsInsideTheEdgeClearance()
    {
        var edge = MenuPlacement.EdgeClearance;
        var button = new Rect(24, 100, ButtonSize.Width, ButtonSize.Height);
        var content = new Size(132, 2000);

        var place = MenuPlacement.Place(button, content, Window);

        Assert.True(place.Scroll);
        Assert.Equal(edge, place.Y);
        Assert.Equal(800d - edge * 2, place.Height);
    }
}
