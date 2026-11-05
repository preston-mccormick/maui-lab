using MauiLab.Collections;
using MauiLab.Layouts;

namespace MauiLabTests;

public class CollectionViewPageModelTests
{
    [Fact]
    public void OpensAsATwoColumnGrid()
    {
        var model = new CollectionViewPageModel();

        Assert.Equal(ItemsLayoutOrientation.Vertical, model.Orientation);
        Assert.Equal(2, model.Span);
        Assert.Equal(ItemSizingStrategy.MeasureFirstItem, model.Sizing);
        Assert.Equal(36, model.HorizontalSpacing);
        Assert.Equal(36, model.VerticalSpacing);
        Assert.False(model.IsGrouped);
        Assert.Equal(ScrollAnchor.KeepOffset, model.Scroll);
        Assert.Equal(ItemsUpdatingScrollMode.KeepScrollOffset, model.ScrollMode);
        Assert.Contains(model.Catalogs, catalog => catalog.Description == "12-letter names");
    }

    [Fact]
    public void SizingAndSpacingRecord()
    {
        var usage = new ListLayoutUsage();
        var model = new CollectionViewPageModel(usage);

        model.SetSizing(ItemSizingStrategy.MeasureAllItems);
        model.HorizontalSpacing = 16;
        model.CommitHorizontalSpacing();

        Assert.Equal(("Changed sizing", "Measure all"), usage.Actions[0]);
        Assert.Equal(("Changed horizontal spacing", "36 to 16"), usage.Actions[1]);
    }

    [Fact]
    public void CanvasWidthFitsTheSwatches()
    {
        var usage = new ListLayoutUsage();
        var model = new CollectionViewPageModel(usage);

        model.ApplyCanvasSize(880, 600);

        Assert.Equal(4, model.Span);
        Assert.Equal(180, model.ItemWidth);
        Assert.Equal(36, model.HorizontalSpacing);
        Assert.Equal(36, model.VerticalSpacing);
        Assert.Empty(usage.Actions);

        model.ApplyCanvasSize(390, 700);

        Assert.Equal(2, model.Span);
        Assert.Equal(180, model.ItemWidth);
        Assert.Equal(15, model.HorizontalSpacing);
    }

    [Fact]
    public void AChosenSpanStaysWhenTheCanvasChanges()
    {
        var model = new CollectionViewPageModel();
        model.ApplyCanvasSize(880, 600);
        model.SetSpan(3);

        model.ApplyCanvasSize(390, 700);

        Assert.Equal(3, model.Span);
        Assert.Equal(15, model.HorizontalSpacing);
    }

    [Fact]
    public void HorizontalOrientationFitsTheHeight()
    {
        var model = new CollectionViewPageModel();
        model.SetOrientation(ItemsLayoutOrientation.Horizontal);

        model.ApplyCanvasSize(880, 390);

        Assert.Equal(2, model.Span);
        Assert.Equal(15, model.VerticalSpacing);
    }

    [Fact]
    public void GroupingKeepsSortOrder()
    {
        var model = new CollectionViewPageModel();

        model.SetGrouping(true);

        Assert.Equal(model.Items.Select(item => item.Name), model.Groups.SelectMany(group => group).Select(item => item.Name));
        Assert.All(model.Groups, group => Assert.All(group, item =>
            Assert.Equal(group.Letter, char.ToUpperInvariant(item.Name[0]).ToString())));
    }

    [Fact]
    public async Task ResetUnlocksTheAutomaticSpan()
    {
        var model = new CollectionViewPageModel();
        model.SetSpan(6);
        model.SetOrientation(ItemsLayoutOrientation.Horizontal);

        await model.Reset();
        model.ApplyCanvasSize(880, 600);

        Assert.Equal(ItemsLayoutOrientation.Vertical, model.Orientation);
        Assert.Equal(ScrollAnchor.KeepOffset, model.Scroll);
        Assert.False(model.IsGrouped);
        Assert.Equal(4, model.Span);
    }
}
