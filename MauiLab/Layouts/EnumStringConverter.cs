using System.Globalization;

namespace MauiLab.Layouts;

/// <summary>
/// Not tested yet.  
/// 
/// Use x:TypeArguments to pass in the type of the enum in XAML.
/// </summary>
/// <typeparam name="T"></typeparam>
public class EnumToStringConverter<T> : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Type realTargetType = typeof(T);
        if (realTargetType.IsEnum)
        {
            Enum.TryParse(realTargetType, value?.ToString(), out object? result);
            return result;
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString();
    }
}