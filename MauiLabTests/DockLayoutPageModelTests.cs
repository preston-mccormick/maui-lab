using MauiLab.Layouts;

namespace MauiLabTests;

public class DockLayoutPageModelTests
{
    [Fact]
    public void OpensWithExpandLastAndAFillingLastItem()
    {
        var model = new DockLayoutPageModel();

        Assert.True(model.ExpandLast);
        Assert.Equal(
            [LayoutDock.Top, LayoutDock.Bottom, LayoutDock.Left, LayoutDock.Right, LayoutDock.None],
            model.Items.Select(item => item.Dock));
        Assert.Equal(-1, model.Items[0].WidthRequest);
        Assert.Equal(64, model.Items[0].HeightRequest);
        Assert.Equal(72, model.Items[2].WidthRequest);
        Assert.Equal(-1, model.Items[2].HeightRequest);
        Assert.Equal(-1, model.Items[4].WidthRequest);
        Assert.Equal(-1, model.Items[4].HeightRequest);
    }

    [Fact]
    public void SetEdge_RecordsTop()
    {
        var usage = new ListLayoutUsage();
        var model = new DockLayoutPageModel(usage);

        model.SetEdge(model.Items[2], LayoutDock.Top);

        Assert.Equal(("Changed edge", "Top"), Assert.Single(usage.Actions));
    }

    [Fact]
    public void SetExpandLast_RecordsOff()
    {
        var usage = new ListLayoutUsage();
        var model = new DockLayoutPageModel(usage);

        model.SetExpandLast(false);

        Assert.False(model.ExpandLast);
        Assert.Equal(("Changed expand last", "Off"), Assert.Single(usage.Actions));
    }
}

public class RadialLayoutPageModelTests
{
    [Fact]
    public void OpensOnAHalfArc()
    {
        var model = new RadialLayoutPageModel();

        Assert.Equal(0, model.StartAngle);
        Assert.Equal(180, model.Sweep);
        Assert.Equal(0, model.Radius);
        Assert.Equal(4, model.Items.Count);
    }

    [Fact]
    public void CommitSweep_RecordsTheChange()
    {
        var usage = new ListLayoutUsage();
        var model = new RadialLayoutPageModel(usage);

        model.Sweep = 270;
        model.CommitSweep();

        Assert.Equal(("Changed sweep", "180 to 270"), Assert.Single(usage.Actions));
    }
}
