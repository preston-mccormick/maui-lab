using System.Globalization;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Layouts;

namespace MauiLab.Layouts;

public sealed class KindMatchConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value?.ToString() == parameter?.ToString();

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Stroke thickness for a selection frame: 2 when selected, 0 otherwise.</summary>
public sealed class SelectedStrokeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? 2d : 0d;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class EnumMatchConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value?.ToString() == parameter?.ToString();

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class DockPositionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is LayoutDock dock
            ? dock switch
            {
                LayoutDock.Top => DockPosition.Top,
                LayoutDock.Left => DockPosition.Left,
                LayoutDock.Right => DockPosition.Right,
                LayoutDock.Bottom => DockPosition.Bottom,
                _ => DockPosition.None
            }
            : DockPosition.None;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DockPosition position
            ? position switch
            {
                DockPosition.Top => LayoutDock.Top,
                DockPosition.Left => LayoutDock.Left,
                DockPosition.Right => LayoutDock.Right,
                DockPosition.Bottom => LayoutDock.Bottom,
                _ => LayoutDock.None
            }
            : LayoutDock.None;
}

public sealed class FlexBasisConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not double basis)
            return FlexBasis.Auto;

        var percent = values[1] is true;
        if (basis <= 0 && !percent)
            return FlexBasis.Auto;

        var length = percent ? basis / 100d : basis;
        return new FlexBasis((float)length, percent);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
