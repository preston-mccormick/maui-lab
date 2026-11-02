using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace MauiLab.Layouts;

public partial class RadialLayoutPageModel : ObservableObject
{
    double _recordedStart;
    double _recordedSweep = 180;
    double _recordedRadius;
    double? _recordedAngle;

    public RadialLayoutPageModel(ILayoutUsage? usage = null)
    {
        Usage = usage ?? NoLayoutUsage.Instance;
        for (var index = 0; index < 4; index++)
            Items.Add(LayoutItem.Create(LayoutItemKind.Ellipse, index, new Rect(0, 0, 48, 48), 48, 48));
    }

    public ILayoutUsage Usage { get; }

    public ObservableCollection<LayoutItem> Items { get; } = [];

    [ObservableProperty]
    public partial double StartAngle { get; set; }

    [ObservableProperty]
    public partial double Sweep { get; set; } = 180;

    [ObservableProperty]
    public partial double Radius { get; set; }

    [ObservableProperty]
    public partial LayoutItem? Selected { get; set; }

    public double AngleValue
    {
        get => Selected?.Angle ?? double.NaN;
        set
        {
            if (Selected == null)
                return;

            double? next = double.IsNaN(value) ? null : value;
            if (Selected.Angle == next)
                return;

            Selected.Angle = next;
            OnPropertyChanged();
        }
    }

    public void CommitStart() => Commit(StartAngle, ref _recordedStart, "Changed start");

    public void CommitSweep() => Commit(Sweep, ref _recordedSweep, "Changed sweep");

    public void CommitRadius() => Commit(Radius, ref _recordedRadius, "Changed radius");

    public void CommitAngle(string? text)
    {
        if (Selected == null)
            return;

        double? angle = double.TryParse(text, out var value) ? value : null;
        if (Selected.Angle != angle)
            Selected.Angle = angle;
        if (_recordedAngle == angle)
            return;

        _recordedAngle = angle;
        Usage.Record("Changed angle", angle?.ToString("0.##") ?? "");
    }

    partial void OnSelectedChanged(LayoutItem? value)
    {
        _recordedAngle = value?.Angle;
        OnPropertyChanged(nameof(AngleValue));
    }

    public void Add()
    {
        Items.Add(LayoutItem.Create(LayoutItemKind.Ellipse, Items.Count, new Rect(0, 0, 48, 48), 48, 48));
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

    static string Format(double value) => value.ToString("0.##");
}
