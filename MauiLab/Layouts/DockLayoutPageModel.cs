using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MauiLab.Layouts;

public partial class DockLayoutPageModel : ObservableObject
{
    double _recordedHorizontal;
    double _recordedVertical;

    public DockLayoutPageModel(ILayoutUsage? usage = null)
    {
        Usage = usage ?? NoLayoutUsage.Instance;
        Items.Add(Docked(LayoutDock.Top, 0, -1, 64));
        Items.Add(Docked(LayoutDock.Bottom, 1, -1, 64));
        Items.Add(Docked(LayoutDock.Left, 2, 72, -1));
        Items.Add(Docked(LayoutDock.Right, 3, 72, -1));
        var fill = LayoutItem.Create(LayoutItemKind.Rectangle, 4, new Rect(0, 0, -1, -1), -1, -1);
        fill.Dock = LayoutDock.None;
        Items.Add(fill);
    }

    public ILayoutUsage Usage { get; }

    public ObservableCollection<LayoutItem> Items { get; } = [];

    public LayoutDock[] Edges { get; } = Enum.GetValues<LayoutDock>();

    [ObservableProperty]
    public partial bool ExpandLast { get; set; } = true;

    [ObservableProperty]
    public partial double HorizontalSpacing { get; set; }

    [ObservableProperty]
    public partial double VerticalSpacing { get; set; }

    [ObservableProperty]
    public partial LayoutItem? Selected { get; set; }

    public void SetEdge(LayoutItem item, LayoutDock edge)
    {
        if (item.Dock == edge)
            return;

        item.Dock = edge;
        Usage.Record("Changed edge", edge.ToString());
    }

    public void SetExpandLast(bool expand)
    {
        if (ExpandLast == expand)
            return;

        ExpandLast = expand;
        Usage.Record("Changed expand last", expand ? "On" : "Off");
    }

    public void CommitHorizontalSpacing() => Commit(HorizontalSpacing, ref _recordedHorizontal, "Changed horizontal spacing");

    public void CommitVerticalSpacing() => Commit(VerticalSpacing, ref _recordedVertical, "Changed vertical spacing");

    public void Add()
    {
        Items.Insert(Items.Count - 1, Docked(LayoutDock.Right, Items.Count, 72, 48));
        Usage.Record("Added item", null);
    }

    public void ClearSelection()
    {
        foreach (var item in Items)
            item.IsSelected = false;
        Selected = null;
    }

    public void Remove(LayoutItem? item)
    {
        if (item == null || !Items.Remove(item))
            return;

        if (Selected == item)
            Selected = null;
        Usage.Record("Removed item", null);
    }

    public void ViewInfo(string title) => Usage.Record($"Viewed {title}", null);

    void Commit(double value, ref double recorded, string name)
    {
        if (recorded == value)
            return;

        Usage.Record(name, $"{Format(recorded)} to {Format(value)}");
        recorded = value;
    }

    static LayoutItem Docked(LayoutDock edge, int index, double width, double height)
    {
        var item = LayoutItem.Create(LayoutItemKind.Rectangle, index, new Rect(0, 0, width, height), width, height);
        item.Dock = edge;
        return item;
    }

    static string Format(double value) => value.ToString("0.##");
}
