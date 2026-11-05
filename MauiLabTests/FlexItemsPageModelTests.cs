using MauiLab.Collections;
using MauiLab.Layouts;
using Microsoft.Maui.Layouts;

namespace MauiLabTests;

public class FlexItemsPageModelTests
{
    [Fact]
    public void OpensWrappedWithNoPadding()
    {
        var model = new FlexItemsPageModel();

        Assert.Equal(FlexDirection.Row, model.Direction);
        Assert.Equal(FlexWrap.Wrap, model.Wrap);
        Assert.Equal(0, model.Padding);
        Assert.Equal(model.SelectedCatalog.Colors.Count, model.Items.Count);
    }

    [Fact]
    public void WrapAndPaddingRecord()
    {
        var usage = new ListLayoutUsage();
        var model = new FlexItemsPageModel(usage);

        model.SetWrap(FlexWrap.NoWrap);
        model.Padding = 16;
        model.CommitPadding();

        Assert.Equal(("Changed wrap", "NoWrap"), usage.Actions[0]);
        Assert.Equal(("Changed padding", "0 to 16"), usage.Actions[1]);
    }

    [Fact]
    public async Task SurveyCatalogFinishesAt949()
    {
        var model = new FlexItemsPageModel();

        Assert.DoesNotContain(model.Catalogs, catalog => catalog.Description == "12-letter names");

        await model.SetCatalog(model.Catalogs.Single(catalog => catalog.Description == "XKCD color survey"));

        Assert.Equal(949, model.Items.Count);
    }

    [Fact]
    public void CanvasWidthFitsTheSwatches()
    {
        var usage = new ListLayoutUsage();
        var model = new FlexItemsPageModel(usage);

        model.ApplyCanvasWidth(880);

        Assert.Equal(180, model.ItemWidth);
        Assert.Equal(18, model.Margin);
        Assert.Equal(7.5, model.Padding);
        Assert.Empty(usage.Actions);

        model.ApplyCanvasWidth(390);

        Assert.Equal(180, model.ItemWidth);
        Assert.Equal(7.5, model.Margin);
        Assert.Equal(0, model.Padding);
    }

    [Fact]
    public void AChosenMarginStaysWhenTheCanvasChanges()
    {
        var model = new FlexItemsPageModel();
        model.ApplyCanvasWidth(880);
        model.Margin = 4;
        model.CommitMargin();

        model.ApplyCanvasWidth(390);

        Assert.Equal(4, model.Margin);
        Assert.Equal(0, model.Padding);
    }
}
