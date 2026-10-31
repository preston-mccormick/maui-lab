using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiLab.Layouts;

public partial class GridTrack : ObservableObject
{
    public static readonly GridUnitType[] Units =
    [
        GridUnitType.Auto,
        GridUnitType.Star,
        GridUnitType.Absolute,
    ];

    public GridTrack(GridUnitType unit, double value, Color color)
    {
        Unit = unit;
        Value = value;
        Color = color;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValueVisible))]
    public partial GridUnitType Unit { get; set; }

    [ObservableProperty]
    public partial double Value { get; set; }

    [ObservableProperty]
    public partial Color Color { get; set; }

    public bool IsValueVisible => Unit != GridUnitType.Auto;

    public GridLength ToGridLength()
    {
        if (Unit == GridUnitType.Auto)
            return GridLength.Auto;

        var length = Value;
        if (!double.IsFinite(length) || length < 0)
            length = 0;

        return new GridLength(length, Unit);
    }
}
