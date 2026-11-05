using MauiLab.Collections;
using MauiLab.Layouts;

namespace MauiLabTests;

public class LinearCollectionPageModelTests
{
    [Fact]
    public void OpensAsAVerticalList()
    {
        var model = new LinearCollectionPageModel();

        Assert.Equal(ItemsLayoutOrientation.Vertical, model.Orientation);
        Assert.True(model.UsesSideSwipe);
        Assert.Equal(0, model.ItemSpacing);
        Assert.Equal(ItemSizingStrategy.MeasureFirstItem, model.Sizing);
        Assert.Equal(ScrollAnchor.KeepOffset, model.Scroll);
        Assert.True(model.OfferPin);
        Assert.False(model.OfferCopy);
        Assert.True(model.OfferDelete);
        Assert.Contains(model.Catalogs, catalog => catalog.Description == "12-letter names");
    }

    [Fact]
    public void HorizontalSwipesAcrossTheScroll()
    {
        var usage = new ListLayoutUsage();
        var model = new LinearCollectionPageModel(usage);

        model.SetOrientation(ItemsLayoutOrientation.Horizontal);

        Assert.False(model.UsesSideSwipe);
        Assert.Equal(("Changed orientation", "Horizontal"), usage.Actions[0]);
    }

    [Fact]
    public void PinCopyAndDelete()
    {
        var usage = new ListLayoutUsage();
        var model = new LinearCollectionPageModel(usage);
        var alice = model.Items.Single(item => item.Name == "AliceBlue");
        var index = model.Items.IndexOf(alice);

        model.Pin(alice);
        model.Pin(alice);
        model.Copy(alice);
        model.Delete(alice);

        Assert.False(alice.IsPinned);
        Assert.Equal("AliceBlue", model.Items[index].Name);
        Assert.Equal(alice.Color, model.Items[index].Color);
        Assert.DoesNotContain(model.Items, item => ReferenceEquals(item, alice));
        Assert.Equal(("Pinned swatch", "AliceBlue"), usage.Actions[0]);
        Assert.Equal(("Unpinned swatch", "AliceBlue"), usage.Actions[1]);
        Assert.Equal(("Copied swatch", "AliceBlue"), usage.Actions[2]);
        Assert.Equal(("Deleted swatch", "AliceBlue"), usage.Actions[3]);
    }

    [Fact]
    public async Task ChoosingTheCatalogRestoresADeletedName()
    {
        var model = new LinearCollectionPageModel();
        var alice = model.Items.Single(item => item.Name == "AliceBlue");

        model.Delete(alice);
        await model.SetCatalog(model.SelectedCatalog);

        Assert.Contains(model.Items, item => item.Name == "AliceBlue");
    }

    [Fact]
    public void SpacingAndCopyActionRecord()
    {
        var usage = new ListLayoutUsage();
        var model = new LinearCollectionPageModel(usage);

        model.Spacing = 12;
        model.CommitSpacing();
        model.SetOfferCopy(true);

        Assert.Equal(("Changed spacing", "0 to 12"), usage.Actions[0]);
        Assert.Equal(("Changed copy action", "On"), usage.Actions[1]);
    }
}
