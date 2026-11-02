using MauiLab.Layouts;

namespace MauiLabTests;

public class LayoutSelectionTests
{
    [Fact]
    public void Stack_ClearSelection_ClearsANestedItem()
    {
        var model = new StackLayoutPageModel();
        model.AddStack();
        var stack = model.Items.Single(item => item.IsStack);
        var child = stack.Children[0];
        stack.IsSelected = true;
        child.IsSelected = true;
        model.Selected = child;

        model.ClearSelection();

        Assert.Null(model.Selected);
        Assert.False(stack.IsSelected);
        Assert.False(child.IsSelected);
    }

    [Fact]
    public void Flex_ClearSelection_ReturnsToAllItems()
    {
        var model = new FlexLayoutPageModel();
        model.Selected = model.Items[0];
        model.Items[0].IsSelected = true;

        model.ClearSelection();

        Assert.Null(model.Selected);
        Assert.False(model.HasSelection);
        Assert.All(model.Items, item => Assert.False(item.IsSelected));
    }

    [Fact]
    public void Dock_ClearSelection_DropsTheSelectedItem()
    {
        var model = new DockLayoutPageModel();
        model.Selected = model.Items[1];
        model.Items[1].IsSelected = true;

        model.ClearSelection();

        Assert.Null(model.Selected);
        Assert.False(model.Items[1].IsSelected);
    }

    [Fact]
    public void Radial_ClearSelection_DropsTheSelectedItem()
    {
        var model = new RadialLayoutPageModel();
        model.Selected = model.Items[2];
        model.Items[2].IsSelected = true;

        model.ClearSelection();

        Assert.Null(model.Selected);
        Assert.False(model.Items[2].IsSelected);
    }

    [Fact]
    public void Grid_ClearSelection_DropsTheSelectedChild()
    {
        var model = new GridLayoutPageModel();
        var child = new LayoutItem(LayoutItemKind.Rectangle, "Child", Microsoft.Maui.Graphics.Colors.Orange) { IsSelected = true };
        model.Children.Add(child);
        model.Selected = child;

        model.ClearSelection();

        Assert.Null(model.Selected);
        Assert.False(child.IsSelected);
    }
}
