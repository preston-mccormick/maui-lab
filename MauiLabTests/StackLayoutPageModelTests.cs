using MauiLab.Layouts;

namespace MauiLabTests;

public class StackLayoutPageModelTests
{
    [Fact]
    public void OpensVerticalWithThreeRectangles()
    {
        var model = new StackLayoutPageModel();

        Assert.Equal(StackMode.Vertical, model.Mode);
        Assert.Equal(8, model.Spacing);
        Assert.Equal(3, model.Items.Count);
        Assert.All(model.Items, item => Assert.Equal(LayoutItemKind.Rectangle, item.Kind));
    }

    [Fact]
    public void SetMode_RecordsHorizontal()
    {
        var usage = new ListLayoutUsage();
        var model = new StackLayoutPageModel(usage);

        model.SetMode(StackMode.Horizontal);

        Assert.Equal(StackMode.Horizontal, model.Mode);
        Assert.Equal(("Changed orientation", "Horizontal"), Assert.Single(usage.Actions));
    }

    [Fact]
    public void UnchangedSpacing_RecordsNothing()
    {
        var usage = new ListLayoutUsage();
        var model = new StackLayoutPageModel(usage);

        model.CommitSpacing();

        Assert.Empty(usage.Actions);
    }

    [Fact]
    public void AddStack_RunsAcrossTheParentAndHoldsTwoItems()
    {
        var usage = new ListLayoutUsage();
        var model = new StackLayoutPageModel(usage);

        model.AddStack();

        var stack = model.Items[^1];
        Assert.Equal(LayoutItemKind.Stack, stack.Kind);
        Assert.False(stack.IsVertical);
        Assert.Equal(2, stack.Children.Count);
        Assert.Equal(("Added stack", "Horizontal"), Assert.Single(usage.Actions));
    }

    [Fact]
    public void AddOne_GoesIntoTheSelectedStack()
    {
        var model = new StackLayoutPageModel();
        model.AddStack();
        var stack = model.Items[^1];
        model.Selected = stack;

        model.AddOne();

        Assert.Equal(3, stack.Children.Count);
        Assert.Equal(4, model.Items.Count);
    }

    [Fact]
    public void Remove_FindsANestedItem()
    {
        var usage = new ListLayoutUsage();
        var model = new StackLayoutPageModel(usage);
        model.AddStack();
        var nested = model.Items[^1].Children[0];

        model.Remove(nested);

        Assert.Single(model.Items[^1].Children);
        Assert.Equal(("Removed item", null), usage.Actions[^1]);
    }

    [Fact]
    public void StackOrientation_RecordsOnlyOnChange()
    {
        var usage = new ListLayoutUsage();
        var model = new StackLayoutPageModel(usage);
        model.AddStack();
        model.Selected = model.Items[^1];
        usage.Actions.Clear();

        model.SetStackOrientation(vertical: false);
        model.SetStackOrientation(vertical: true);

        Assert.Equal(("Changed orientation", "Vertical"), Assert.Single(usage.Actions));
    }
}
