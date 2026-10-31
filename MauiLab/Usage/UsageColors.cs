using System.Globalization;

namespace MauiLab.Usage;

/// <summary>Theme-aware colour lookup shared by the rail drawable and the row template.</summary>
public static class UsageColors
{
    public static Color ForCategory(string? category)
    {
        var (light, dark) = category switch
        {
            UsageCategory.Screen => ("UsageScreen", "UsageScreenOnDark"),
            UsageCategory.Lifecycle => ("UsageLifecycle", "UsageLifecycleOnDark"),
            UsageCategory.Action => ("UsageAction", "UsageActionOnDark"),
            UsageCategory.Permission => ("UsagePermission", "UsagePermissionOnDark"),
            _ => ("UsageNavigation", "UsageNavigationOnDark")
        };
        return Themed(light, dark, Microsoft.Maui.Graphics.Colors.Black);
    }

    public static Color Themed(string lightKey, string darkKey, Color fallback)
    {
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        if (Application.Current?.Resources.TryGetValue(dark ? darkKey : lightKey, out var value) == true &&
            value is Color color)
            return color;
        return fallback;
    }
}

/// <summary>
/// Binds a row's <see cref="TimelineRow.Category"/> to a text colour. One converter replaces a
/// stack of data triggers per cell, which is what made cells slow to create.
/// </summary>
public sealed class CategoryColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        UsageColors.ForCategory(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
