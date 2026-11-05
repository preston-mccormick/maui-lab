using MauiLab.Collections;
using MauiLab.Layouts;

namespace MauiLabTests;

public class CollectionPageModelTests
{
    [Fact]
    public async Task VariedSizeIsStableAndReadable()
    {
        var red = SwatchItem.VariedSize("Red");
        Assert.Equal(red, SwatchItem.VariedSize("Red"));
        Assert.Contains(red.Width, new[] { 160d, 200d, 240d, 280d });
        Assert.Contains(red.Height, new[] { 96d, 132d, 168d, 204d });

        var model = new CollectionPageModel();
        await model.SetArrangement(SwatchArrangement.Varied);

        Assert.Contains(model.Items, item => item.Width != model.Items[0].Width);
        Assert.Contains(model.Items, item => item.Height != model.Items[0].Height);
        Assert.All(model.Items, item => Assert.InRange(item.Width, 160, 280));
        Assert.All(model.Items, item => Assert.InRange(item.Height, 96, 204));
        Assert.False(model.WidthApplies);
        Assert.False(model.HeightApplies);
    }

    [Fact]
    public void OpensOnNamedColorsAsCards()
    {
        var model = new CollectionPageModel();

        Assert.Equal("HTML/CSS colors", model.SelectedCatalog.Description);
        Assert.Equal("Name (ascending)", model.SelectedSort.Name);
        Assert.Equal(SwatchArrangement.Card, model.Arrangement);
        Assert.Equal(180, model.ItemWidth);
        Assert.Equal(120, model.ItemHeight);
        Assert.Equal(14, model.FontSize);
        Assert.Equal(0, model.Margin);
        Assert.False(model.ShowTimings);
        Assert.Equal("", model.TimingText);
        for (var i = 1; i < model.Items.Count; i++)
            Assert.True(string.Compare(model.Items[i - 1].Name, model.Items[i].Name, StringComparison.CurrentCulture) <= 0);
    }

    [Fact]
    public async Task XkcdSwapClearsAPin()
    {
        var model = new CollectionPageModel();
        model.Items[0].IsPinned = true;

        await model.SetCatalog(model.Catalogs.Single(catalog => catalog.Description == "XKCD color survey"));

        Assert.Equal(949, model.Items.Count);
        Assert.DoesNotContain(model.Items, item => item.IsPinned);
    }

    [Fact]
    public async Task SortByHueIsMonotonic()
    {
        var usage = new ListLayoutUsage();
        var model = new CollectionPageModel(usage);

        await model.SetSort(model.SortOptions.Single(option => option.Name == "Hue (ascending)"));

        Assert.Equal(("Sorted", "Hue (ascending)"), Assert.Single(usage.Actions));
        for (var i = 1; i < model.Items.Count; i++)
            Assert.True(model.Items[i].Color.GetHue() + 0.0000001 >= model.Items[i - 1].Color.GetHue());
    }

    [Fact]
    public void TapAndUnchangedFontAreQuiet()
    {
        var usage = new ListLayoutUsage();
        var model = new CollectionPageModel(usage);

        model.Paint(model.Items[0]);
        model.CommitFont();

        Assert.Empty(usage.Actions);
        Assert.Equal(model.Items[0].Color, model.CanvasColor);
    }

    [Fact]
    public async Task TimingsOnNamesTheSort()
    {
        var model = new CollectionPageModel();
        model.ShowTimings = true;

        await model.SetSort(model.SortOptions.Single(option => option.Name == "Hue (ascending)"));

        Assert.Contains("Hue (ascending)", model.TimingText);
        Assert.Contains(" s", model.TimingText);
    }

    [Fact]
    public void TimingsSwitchIsShared()
    {
        CollectionTimings.Reset();
        var first = new CollectionPageModel(new ListLayoutUsage());
        var second = new CollectionPageModel(new ListLayoutUsage());
        CollectionTimings.Attach(first);
        CollectionTimings.Attach(second);
        try
        {
            first.SetTimings(true);

            Assert.True(second.ShowTimings);

            second.SetTimings(false);

            Assert.False(first.ShowTimings);
        }
        finally
        {
            CollectionTimings.Reset();
        }
    }

    [Fact]
    public async Task ResetRestoresTheOpeningState()
    {
        var usage = new ListLayoutUsage();
        var model = new CollectionPageModel(usage);
        var background = model.CanvasColor;
        model.ShowTimings = true;
        await model.SetArrangement(SwatchArrangement.Varied);
        model.Paint(model.Items.First(item => item.Color != background));
        await model.SetSort(model.SortOptions.Single(option => option.Name == "Hue (ascending)"));
        model.Items[0].IsPinned = true;
        var before = usage.Actions.Count;

        await model.Reset();

        Assert.Equal(SwatchArrangement.Card, model.Arrangement);
        Assert.True(model.HeightApplies);
        Assert.True(model.WidthApplies);
        Assert.Equal("HTML/CSS colors", model.SelectedCatalog.Description);
        Assert.Equal("Name (ascending)", model.SelectedSort.Name);
        Assert.Equal(background, model.CanvasColor);
        Assert.Equal(180, model.ItemWidth);
        Assert.False(model.WidthLocked);
        Assert.True(model.ShowTimings);
        Assert.Contains("Reset", model.TimingText);
        Assert.DoesNotContain(model.Items, item => item.IsPinned);
        Assert.All(model.Items, item => Assert.Equal(180, item.Width));
        Assert.All(model.Items, item => Assert.Equal(120, item.Height));
        Assert.Equal(("Reset", null), usage.Actions[^1]);
        Assert.Equal(before + 1, usage.Actions.Count);
        for (var i = 1; i < model.Items.Count; i++)
            Assert.True(string.Compare(model.Items[i - 1].Name, model.Items[i].Name, StringComparison.CurrentCulture) <= 0);
    }
}
