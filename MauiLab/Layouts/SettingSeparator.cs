namespace MauiLab.Layouts;

public class SettingSeparator : BoxView
{
    public SettingSeparator()
    {
        HeightRequest = 1;
        HorizontalOptions = LayoutOptions.Fill;
        Margin = new Thickness(0, 8);
        SetBinding(ColorProperty, new AppThemeBinding
        {
            Light = Color.FromArgb("#C6C6C8"),
            Dark = Color.FromArgb("#3A3A3C")
        });
    }
}
