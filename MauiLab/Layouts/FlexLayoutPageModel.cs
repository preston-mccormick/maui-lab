using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Layouts;
using System.Collections.ObjectModel;

namespace MauiLab.Layouts;

public partial class FlexLayoutPageModel : ObservableObject
{
    public FlexLayoutPageModel(ILayoutUsage? usage = null)
    {
        Usage = usage ?? NoLayoutUsage.Instance;
        double[] widths = [48, 96, 64, 120];
        for (var index = 0; index < widths.Length; index++)
            Items.Add(LayoutItem.Create(LayoutItemKind.Rectangle, index, new Rect(0, 0, widths[index], 48), widths[index], 48));
        BeginItemEdit();
    }

    public ILayoutUsage Usage { get; }

    public ObservableCollection<LayoutItem> Items { get; } = [];

    /// <summary>Stands in for the selection when nothing is selected. Its values apply to every item.</summary>
    public LayoutItem AllItems { get; } = new(LayoutItemKind.Rectangle, "All items", Microsoft.Maui.Graphics.Colors.Gray);

    [ObservableProperty]
    public partial FlexDirection Direction { get; set; } = FlexDirection.Row;

    [ObservableProperty]
    public partial FlexWrap Wrap { get; set; } = FlexWrap.Wrap;

    [ObservableProperty]
    public partial FlexJustify JustifyContent { get; set; } = FlexJustify.Start;

    [ObservableProperty]
    public partial FlexAlignItems AlignItems { get; set; } = FlexAlignItems.Start;

    [ObservableProperty]
    public partial FlexAlignContent AlignContent { get; set; } = FlexAlignContent.Start;

    /// <summary>Uniform padding inside the layout. Flex has no spacing, so this and item margin are the gaps.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LayoutPadding))]
    public partial double Padding { get; set; }

    public Thickness LayoutPadding => new(double.IsFinite(Padding) && Padding >= 0 ? Padding : 0);

    double _recordedPadding;

    public void CommitPadding()
    {
        if (!double.IsFinite(Padding) || Padding < 0)
            Padding = 0;
        if (Padding == _recordedPadding)
            return;

        Usage.Record("Changed padding", $"{Format(_recordedPadding)} to {Format(Padding)}");
        _recordedPadding = Padding;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EditTarget))]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    public partial LayoutItem? Selected { get; set; }

    /// <summary>The item the Item section edits: the selection, or <see cref="AllItems"/>.</summary>
    public LayoutItem EditTarget => Selected ?? AllItems;

    public bool HasSelection => Selected != null;

    public void SetDirection(FlexDirection direction) => Set(Direction == direction, () => Direction = direction, "Changed direction", direction.ToString());

    public void SetWrap(FlexWrap wrap) => Set(Wrap == wrap, () => Wrap = wrap, "Changed wrap", wrap.ToString());

    public void SetJustify(FlexJustify justify) => Set(JustifyContent == justify, () => JustifyContent = justify, "Changed justification", justify.ToString());

    public void SetAlignItems(FlexAlignItems align) => Set(AlignItems == align, () => AlignItems = align, "Changed alignment", align.ToString());

    public void SetAlignContent(FlexAlignContent align) => Set(AlignContent == align, () => AlignContent = align, "Changed alignment", align.ToString());

    public void SetAlignSelf(FlexAlignSelf align)
    {
        if (EditTarget.AlignSelf == align)
            return;

        Apply(item => item.AlignSelf = align);
        Usage.Record("Changed item alignment", Detail(align.ToString()));
    }

    public void CommitGrow() => CommitItem(item => item.Grow, ref _grow, "Changed grow", (item, value) => item.Grow = value);

    public void CommitShrink() => CommitItem(item => item.Shrink, ref _shrink, "Changed shrink", (item, value) => item.Shrink = value);

    public void CommitBasis() => CommitItem(item => item.Basis, ref _basis, "Changed basis", (item, value) => item.Basis = value);

    public void CommitMargin() => CommitItem(item => item.ItemMargin, ref _margin, "Changed margin", (item, value) => item.ItemMargin = value);

    public void CommitOrder()
    {
        var next = EditTarget.Order;
        if (next == _order)
            return;

        Apply(item => item.Order = next);
        _order = next;
        Usage.Record("Changed order", Detail(next.ToString()));
    }

    public void SetBasisIsPercent(bool percent)
    {
        if (EditTarget.BasisIsPercent == percent)
            return;

        Apply(item => item.BasisIsPercent = percent);
        Usage.Record("Changed basis", Detail(percent ? "percent" : "length"));
    }

    public void AddOne()
    {
        Items.Add(LayoutItem.Create(LayoutItemKind.Rectangle, Items.Count, new Rect(0, 0, 72, 48), 72, 48));
        Usage.Record("Added item", null);
    }

    public void AddSet()
    {
        for (var index = 0; index < 5; index++)
            Items.Add(LayoutItem.Create(LayoutItemKind.Rectangle, Items.Count, new Rect(0, 0, 64, 48), 64, 48));
        Usage.Record("Added set", "5");
    }

    public void AddMixed()
    {
        Items.Add(LayoutItem.Create(LayoutItemKind.Rectangle, Items.Count, new Rect(0, 0, 72, 48), 72, 48));
        Items.Add(LayoutItem.Create(LayoutItemKind.Image, Items.Count, new Rect(0, 0, 72, 72), 72, 72));
        Items.Add(LayoutItem.Create(LayoutItemKind.Panel, Items.Count, new Rect(0, 0, 140, 72), 140, 72));
        Usage.Record("Added mixed set", "3");
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

    /// <summary>Snapshots the edit target so a later commit records only a real change.</summary>
    public void BeginItemEdit()
    {
        _grow = EditTarget.Grow;
        _shrink = EditTarget.Shrink;
        _basis = EditTarget.Basis;
        _margin = EditTarget.ItemMargin;
        _order = EditTarget.Order;
    }

    public void ViewInfo(string title) => Usage.Record($"Viewed {title}", null);

    double _grow;
    double _shrink;
    double _basis;
    double _margin;
    int _order;

    partial void OnSelectedChanged(LayoutItem? value) => BeginItemEdit();

    void CommitItem(Func<LayoutItem, double> read, ref double recorded, string name, Action<LayoutItem, double> write)
    {
        var next = read(EditTarget);
        if (!double.IsFinite(next))
            next = 0;
        if (next == recorded)
            return;

        Apply(item => write(item, next));
        recorded = next;
        Usage.Record(name, Detail(Format(next)));
    }

    /// <summary>Writes to the selection, or to every item when nothing is selected.</summary>
    void Apply(Action<LayoutItem> write)
    {
        if (Selected != null)
        {
            write(Selected);
            return;
        }

        write(AllItems);
        foreach (var item in Items)
            write(item);
    }

    string Detail(string value) => Selected == null ? $"{value} (all)" : value;

    void Set(bool unchanged, Action apply, string name, string detail)
    {
        if (unchanged)
            return;

        apply();
        Usage.Record(name, detail);
    }

    static string Format(double value) => value.ToString("0.##");
}
