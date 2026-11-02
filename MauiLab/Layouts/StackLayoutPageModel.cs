using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MauiLab.Layouts;

public enum StackMode
{
    Vertical,
    Horizontal
}

public partial class StackLayoutPageModel : ObservableObject
{
    double _recordedSpacing = 8;
    double _recordedItemSpacing;

    public StackLayoutPageModel(ILayoutUsage? usage = null)
    {
        Usage = usage ?? NoLayoutUsage.Instance;
        for (var index = 0; index < 3; index++)
            Items.Add(Rectangle(index));
    }

    public ILayoutUsage Usage { get; }

    public ObservableCollection<LayoutItem> Items { get; } = [];

    [ObservableProperty]
    public partial double Spacing { get; set; } = 8;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsVertical))]
    [NotifyPropertyChangedFor(nameof(IsHorizontal))]
    public partial StackMode Mode { get; set; } = StackMode.Vertical;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedIsStack))]
    public partial LayoutItem? Selected { get; set; }

    public bool IsVertical => Mode == StackMode.Vertical;

    public bool IsHorizontal => Mode == StackMode.Horizontal;

    public bool SelectedIsStack => Selected?.IsStack == true;

    /// <summary>The collection a new item goes into: the selected stack, or the page's stack.</summary>
    public ObservableCollection<LayoutItem> Target => Selected is { IsStack: true } stack ? stack.Children : Items;

    public void SetMode(StackMode mode)
    {
        if (Mode == mode)
            return;

        Mode = mode;
        Usage.Record("Changed orientation", mode.ToString());
    }

    public void AddOne()
    {
        Target.Add(Rectangle(Count()));
        Usage.Record("Added item", null);
    }

    public void AddSet()
    {
        var target = Target;
        for (var index = 0; index < 3; index++)
            target.Add(LayoutItem.Create(LayoutItemKind.Ellipse, Count(), new Rect(0, 0, 56, 56), 56, 56));
        Usage.Record("Added set", null);
    }

    /// <summary>Adds a nested stack that runs across the target's axis, seeded with two items.</summary>
    public void AddStack()
    {
        var target = Target;
        var vertical = Target == Items ? !IsVertical : !(Selected?.IsVertical ?? true);
        var stack = LayoutItem.Create(LayoutItemKind.Stack, Count(), new Rect(0, 0, -1, -1), -1, -1);
        stack.IsVertical = vertical;
        stack.Children.Add(Rectangle(Count() + 1));
        stack.Children.Add(Rectangle(Count() + 2));
        target.Add(stack);
        Usage.Record("Added stack", vertical ? "Vertical" : "Horizontal");
    }

    public void ClearSelection()
    {
        ClearFlags(Items);
        Selected = null;
    }

    static void ClearFlags(IEnumerable<LayoutItem> items)
    {
        foreach (var item in items)
        {
            item.IsSelected = false;
            if (item.IsStack)
                ClearFlags(item.Children);
        }
    }

    public void Remove(LayoutItem? item)
    {
        if (item == null || !RemoveFrom(Items, item))
            return;

        if (Selected == item)
            Selected = null;
        Usage.Record("Removed item", null);
    }

    public void CommitSpacing()
    {
        if (_recordedSpacing == Spacing)
            return;

        Usage.Record("Changed spacing", $"{Format(_recordedSpacing)} to {Format(Spacing)}");
        _recordedSpacing = Spacing;
    }

    public void SetStackOrientation(bool vertical)
    {
        if (Selected is not { IsStack: true } stack || stack.IsVertical == vertical)
            return;

        stack.IsVertical = vertical;
        Usage.Record("Changed orientation", vertical ? "Vertical" : "Horizontal");
    }

    public void BeginStackEdit()
    {
        if (Selected is { IsStack: true } stack)
            _recordedItemSpacing = stack.Spacing;
    }

    public void CommitStackSpacing()
    {
        if (Selected is not { IsStack: true } stack || stack.Spacing == _recordedItemSpacing)
            return;

        Usage.Record("Changed spacing", $"{Format(_recordedItemSpacing)} to {Format(stack.Spacing)}");
        _recordedItemSpacing = stack.Spacing;
    }

    public void ViewInfo(string title) => Usage.Record($"Viewed {title}", null);

    static bool RemoveFrom(ObservableCollection<LayoutItem> items, LayoutItem item)
    {
        if (items.Remove(item))
            return true;

        return items.Any(child => child.IsStack && RemoveFrom(child.Children, item));
    }

    int Count() => CountAll(Items);

    static int CountAll(IEnumerable<LayoutItem> items) =>
        items.Sum(item => 1 + (item.IsStack ? CountAll(item.Children) : 0));

    static LayoutItem Rectangle(int index) =>
        LayoutItem.Create(LayoutItemKind.Rectangle, index, new Rect(0, 0, 72, 48), 72, 48);

    static string Format(double value) => value.ToString("0.##");
}
