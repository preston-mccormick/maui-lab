using MauiLab.Layouts;
using Microsoft.Maui.Layouts;

namespace MauiLabTests;

public class FlexLayoutPageModelTests
{
    [Fact]
    public void OpensWrappedWithFourRectangles()
    {
        var model = new FlexLayoutPageModel();

        Assert.Equal(FlexDirection.Row, model.Direction);
        Assert.Equal(FlexWrap.Wrap, model.Wrap);
        Assert.Equal(4, model.Items.Count);
        Assert.Equal([48d, 96d, 64d, 120d], model.Items.Select(item => item.WidthRequest));
    }

    [Fact]
    public void AddMixed_RecordsTheCount()
    {
        var usage = new ListLayoutUsage();
        var model = new FlexLayoutPageModel(usage);

        model.AddMixed();

        Assert.Equal(("Added mixed set", "3"), Assert.Single(usage.Actions));
        Assert.Contains(model.Items, item => item.Kind == LayoutItemKind.Image);
        Assert.Contains(model.Items, item => item.Kind == LayoutItemKind.Panel);
    }

    [Fact]
    public void NoSelection_AppliesGrowToEveryItem()
    {
        var usage = new ListLayoutUsage();
        var model = new FlexLayoutPageModel(usage);

        model.EditTarget.Grow = 1;
        model.CommitGrow();

        Assert.Null(model.Selected);
        Assert.All(model.Items, item => Assert.Equal(1, item.Grow));
        Assert.Equal(("Changed grow", "1 (all)"), Assert.Single(usage.Actions));
    }

    [Fact]
    public void Selection_AppliesMarginToThatItemOnly()
    {
        var usage = new ListLayoutUsage();
        var model = new FlexLayoutPageModel(usage);
        model.Selected = model.Items[1];

        model.EditTarget.ItemMargin = 6;
        model.CommitMargin();

        Assert.Equal(new Thickness(6), model.Items[1].Margin);
        Assert.Equal(new Thickness(0), model.Items[0].Margin);
        Assert.Equal(("Changed margin", "6"), Assert.Single(usage.Actions));
    }

    [Fact]
    public void Padding_InsetsTheLayoutAndRecordsOnce()
    {
        var usage = new ListLayoutUsage();
        var model = new FlexLayoutPageModel(usage);

        model.Padding = 12;
        model.CommitPadding();
        model.CommitPadding();

        Assert.Equal(new Thickness(12), model.LayoutPadding);
        Assert.Equal(("Changed padding", "0 to 12"), Assert.Single(usage.Actions));
    }

    [Fact]
    public void SetWrap_RecordsWrap()
    {
        var usage = new ListLayoutUsage();
        var model = new FlexLayoutPageModel(usage);

        model.SetWrap(FlexWrap.NoWrap);
        model.SetWrap(FlexWrap.Wrap);

        Assert.Equal([("Changed wrap", "NoWrap"), ("Changed wrap", "Wrap")], usage.Actions);
    }
}
