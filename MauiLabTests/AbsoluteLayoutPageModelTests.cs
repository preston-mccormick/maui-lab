using MauiLab.Layouts;
using Microsoft.Maui.Layouts;

namespace MauiLabTests;

public class AbsoluteLayoutPageModelTests
{
    [Fact]
    public void Add_RecordsTheIllustration()
    {
        var usage = new ListLayoutUsage();
        var model = new AbsoluteLayoutPageModel(usage);

        model.Add(model.Illustrations.Single(item => item.File == "red-bell-pepper.png"), randomBounds: false);

        Assert.Equal(("Added item", "red-bell-pepper"), usage.Actions[^1]);
        Assert.Equal("red_bell_pepper.png", model.Items[^1].ImageName);
        Assert.Equal(LayoutItemKind.Image, model.Items[^1].Kind);
    }

    [Fact]
    public void CommitMove_RecordsOnce()
    {
        var usage = new ListLayoutUsage();
        var model = new AbsoluteLayoutPageModel(usage);
        var item = model.Items[0];

        model.DragTo(item, 40, 40);
        model.DragTo(item, 120, 80);
        model.CommitMove(item);

        Assert.Equal(("Moved item", "120, 80"), Assert.Single(usage.Actions));
    }

    [Fact]
    public void Clip_RecordsEllipse()
    {
        var usage = new ListLayoutUsage();
        var model = new AbsoluteLayoutPageModel(usage);
        model.Selected = model.Items[1];

        model.SetClip(true);

        Assert.True(model.Selected.IsClipped);
        Assert.Equal(("Changed clip", "ellipse"), Assert.Single(usage.Actions));
    }

    [Fact]
    public void AddRandom_UsesTheWholeCanvasAndMixesShapes()
    {
        var model = new AbsoluteLayoutPageModel(random: new Random(7)) { CanvasSize = new Size(900, 600) };

        for (var index = 0; index < 40; index++)
            model.AddRandom();

        var added = model.Items.Skip(4).ToArray();
        Assert.All(added, item =>
        {
            Assert.True(item.Bounds.Right <= 900 && item.Bounds.Bottom <= 600);
            Assert.Contains(Path.GetFileNameWithoutExtension(item.ImageName).Replace('_', '-'), AbsoluteLayoutPageModel.IllustrationFiles);
        });
        Assert.Contains(added, item => item.Bounds.X > 600);
        Assert.Contains(added, item => item.Bounds.Y > 400);
        Assert.True(added.Select(item => item.ImageName).Distinct().Count() > 2);
    }

    [Fact]
    public void PositionEntry_RecordsOneMove()
    {
        var usage = new ListLayoutUsage();
        var model = new AbsoluteLayoutPageModel(usage);
        model.Selected = model.Items[0];
        model.BeginItemEdit();

        model.Selected.X = 120;
        model.Selected.Y = 80;
        model.CommitPosition();
        model.CommitPosition();

        Assert.Equal(("Moved item", "120, 80"), Assert.Single(usage.Actions));
    }

    [Fact]
    public void Resize_GrowsFromTheCornerAndRecordsTheSize()
    {
        var usage = new ListLayoutUsage();
        var model = new AbsoluteLayoutPageModel(usage);
        model.Selected = model.Items[0];
        model.BeginItemEdit();

        model.Selected.Bounds = AbsolutePlacement.Resize(model.Selected.Bounds, 24, -100, AbsoluteLayoutFlags.None, model.CanvasSize, 16);
        model.CommitSize();

        Assert.Equal(new Rect(24, 24, 120, 16), model.Selected.Bounds);
        Assert.Equal(("Resized item", "120 by 16"), Assert.Single(usage.Actions));
    }

    [Fact]
    public void Items_TakeTheirSizeFromBoundsAlone()
    {
        var model = new AbsoluteLayoutPageModel();
        model.Add(model.Illustrations[0], randomBounds: false);
        model.AddRandom();

        Assert.All(model.Items, item =>
        {
            Assert.Equal(-1, item.WidthRequest);
            Assert.Equal(-1, item.HeightRequest);
            Assert.True(item.Bounds.Width > 0 && item.Bounds.Height > 0);
        });
    }

    [Fact]
    public void HandleBounds_FollowsTheSelectedCorner()
    {
        var model = new AbsoluteLayoutPageModel();
        Assert.Equal(Rect.Zero, model.HandleBounds);

        model.Selected = model.Items[0]; // 24,24 96x96 → corner at 120,120
        Assert.Equal(new Rect(108, 108, 24, 24), model.HandleBounds);

        model.Selected.Bounds = new Rect(0, 0, 200, 100);
        Assert.Equal(new Rect(188, 88, 24, 24), model.HandleBounds);

        model.Selected.Flags = AbsoluteLayoutFlags.SizeProportional;
        model.Selected.Bounds = new Rect(0, 0, 0.5, 0.5);
        model.CanvasSize = new Size(800, 400);
        Assert.Equal(new Rect(388, 188, 24, 24), model.HandleBounds);

        model.Selected = null;
        Assert.Equal(Rect.Zero, model.HandleBounds);
        Assert.False(model.HasSelection);
    }

    [Fact]
    public void CanvasTap_ClearsOnlyWhenItMissesEveryItem()
    {
        var model = new AbsoluteLayoutPageModel();
        model.Selected = model.Items[0];
        model.Items[0].IsSelected = true;

        Assert.True(model.HitsItem(new Point(30, 30)));
        Assert.True(model.HitsItem(new Point(125, 125))); // on the handle, past the item's corner
        Assert.False(model.HitsItem(new Point(0, 0)));

        model.ClearSelection();

        Assert.Null(model.Selected);
        Assert.False(model.HasSelection);
        Assert.All(model.Items, item => Assert.False(item.IsSelected));
    }

    [Fact]
    public void Background_DoesNotMoveItems()
    {
        var usage = new ListLayoutUsage();
        var model = new AbsoluteLayoutPageModel(usage);
        var before = model.Items.Select(item => item.Bounds).ToArray();

        model.SetBackground(CanvasBackground.Image);

        Assert.Equal(before, model.Items.Select(item => item.Bounds));
        Assert.Equal(("Changed background", "image"), Assert.Single(usage.Actions));
    }
}
