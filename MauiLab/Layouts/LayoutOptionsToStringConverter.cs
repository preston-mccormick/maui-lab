using System.Globalization;

namespace MauiLab.Layouts;

public class LayoutOptionsToStringConverter : IValueConverter
{
    LayoutOptionsConverter converter = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LayoutOptions options)
        {
            return converter.ConvertToInvariantString(options);
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return converter.ConvertFromInvariantString(stringValue);
        }
        return null;
    }
}
