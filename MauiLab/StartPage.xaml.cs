namespace MauiLab;

public partial class StartPage : ContentPage
{
    const double WelcomeFontSize = 30;
    const double TitleFontSize = 75;
    const double FlaskSize = 150;
    const double TitleIndent = 100;
    const double WelcomeOverlap = 20;

    Size _designSize;
    bool _hasDesignSize;
    bool _fitting;

    public StartPage()
    {
        InitializeComponent();
        SizeChanged += OnPageSizeChanged;
    }

    void OnPageSizeChanged(object? sender, EventArgs e)
    {
        if (_fitting || Width <= 0 || Height <= 0)
            return;

        _fitting = true;
        try
        {
            if (!_hasDesignSize)
            {
                ApplyScale(1);
                _designSize = Hero.Measure(double.PositiveInfinity, double.PositiveInfinity);
                _hasDesignSize = _designSize.Width > 0 && _designSize.Height > 0;
            }

            if (!_hasDesignSize)
                return;

            var availableWidth = Width - Padding.Left - Padding.Right;
            var availableHeight = Height - Padding.Top - Padding.Bottom;
            if (availableWidth <= 0 || availableHeight <= 0)
                return;

            var scale = Math.Min(
                1,
                Math.Min(availableWidth / _designSize.Width, availableHeight / _designSize.Height));
            ApplyScale(scale);
        }
        finally
        {
            _fitting = false;
        }
    }

    void ApplyScale(double scale)
    {
        WelcomeLabel.FontSize = WelcomeFontSize * scale;
        WelcomeLabel.Margin = new Thickness(0, 0, 0, -WelcomeOverlap * scale);
        TitleLabel.FontSize = TitleFontSize * scale;
        TitleRow.Margin = new Thickness(TitleIndent * scale, 0, 0, 0);
        Flask.WidthRequest = FlaskSize * scale;
        Flask.HeightRequest = FlaskSize * scale;
    }
}
