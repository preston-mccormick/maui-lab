using MauiLab.Collections;
using MauiLab.Layouts;

namespace MauiLabTests;

public class UniformItemsPageModelTests
{
    [Fact]
    public void OpensOnFitAndRefusesZero()
    {
        var usage = new ListLayoutUsage();
        var model = new UniformItemsPageModel(usage);

        Assert.Null(model.MaxColumns);
        Assert.Null(model.MaxRows);
        Assert.Equal(int.MaxValue, model.AppliedMaxColumns);

        model.SetMaxColumns(0);

        Assert.Null(model.MaxColumns);
        Assert.Empty(usage.Actions);

        model.SetMaxColumns(2);

        Assert.Equal(2, model.MaxColumns);
        Assert.Equal(("Changed max columns", "Fit to 2"), Assert.Single(usage.Actions));
    }

    [Fact]
    public void CanvasWidthMatchesTheOtherSwatchViews()
    {
        var usage = new ListLayoutUsage();
        var model = new UniformItemsPageModel(usage);

        Assert.True(model.ApplyCanvasWidth(880));
        Assert.Equal(180, model.ItemWidth);
        Assert.Equal(18, model.Margin);
        Assert.Empty(usage.Actions);

        Assert.True(model.ApplyCanvasWidth(390));
        Assert.Equal(180, model.ItemWidth);
        Assert.Equal(7.5, model.Margin);
    }

    [Fact]
    public void AChosenMarginIsNotOverwrittenByCanvasFit()
    {
        var model = new UniformItemsPageModel();
        model.ApplyCanvasWidth(880);
        model.Margin = 4;
        model.CommitMargin();

        model.ApplyCanvasWidth(390);

        Assert.Equal(4, model.Margin);
    }
}
