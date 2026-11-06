#if IOS || MACCATALYST
using CoreGraphics;
using UIKit;
#endif
using Microsoft.Maui.Controls.Shapes;

namespace MauiLab.Contacts;

sealed class PhotoChoice
{
    public string? Selected { get; private set; }

    public event EventHandler? Changed;

    public void Select(string? path)
    {
        if (Selected == path)
            return;

        Selected = path;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}

sealed class PhotoTile : ContentView
{
    const double Rest = 0;
    const double Raised = 1;

    readonly string _directory;
    readonly PhotoChoice _choice;
    readonly Border _card;
    readonly Image _image;
    bool _selected;
    bool _placed;
    bool _listening;
    double _amount;

    public PhotoTile(string directory, PhotoChoice choice)
    {
        _directory = directory;
        _choice = choice;
        BackgroundColor = Color.FromArgb("#00000000");
        Padding = new Thickness(8, 22, 8, 18);
        _image = new Image
        {
            Aspect = Aspect.AspectFill,
            WidthRequest = 108,
            HeightRequest = 136,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Clip = new RoundRectangleGeometry
            {
                CornerRadius = new CornerRadius(14),
                Rect = new Rect(0, 0, 108, 136),
            },
        };
        _card = new Border
        {
            Padding = 0,
            WidthRequest = 108,
            HeightRequest = 136,
            BackgroundColor = Color.FromArgb("#00000000"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Content = _image,
        };
        _card.SizeChanged += (_, _) => ApplyShadow(_amount);
        Content = _card;
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            if (BindingContext is string path)
                _choice.Select(path);
        };
        GestureRecognizers.Add(tap);
        Paint(Rest);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        _image.Source = Portrait(_directory, BindingContext as string);
        Sync(animate: false);
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        if (Parent == null)
        {
            if (_listening)
                _choice.Changed -= OnChoiceChanged;
            _listening = false;
            return;
        }

        if (!_listening)
        {
            _choice.Changed += OnChoiceChanged;
            _listening = true;
        }

        Sync(animate: _placed);
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        ApplyShadow(_amount);
    }

    void OnChoiceChanged(object? sender, EventArgs e) => Sync(animate: true);

    void Sync(bool animate)
    {
        var selected = BindingContext is string path && path == _choice.Selected;
        if (!_placed)
        {
            _placed = BindingContext != null;
            _selected = selected;
            Paint(selected ? Raised : Rest);
            return;
        }

        if (selected == _selected)
            return;

        _selected = selected;
        if (!animate)
        {
            Paint(selected ? Raised : Rest);
            return;
        }

        Animate();
    }

    void Animate()
    {
        var start = _amount;
        var end = _selected ? Raised : Rest;
        _card.AbortAnimation("raise");
        new Animation(t => Paint(start + ((end - start) * t))).Commit(
            _card,
            "raise",
            length: 180,
            easing: Easing.CubicOut,
            finished: (_, _) => Paint(end));
    }

    void Paint(double amount)
    {
        _amount = amount;
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var rest = dark ? Color.FromArgb("#636366") : Color.FromArgb("#C7C7CC");
        var lit = dark ? Color.FromArgb("#64D2FF") : Color.FromArgb("#007AFF");
        _card.Scale = 1 + (0.08 * amount);
        _card.TranslationY = -10 * amount;
        _card.StrokeThickness = 1 + (2.5 * amount);
        _card.Stroke = Lerp(rest, lit, amount);
        ApplyShadow(amount);
    }

    void ApplyShadow(double amount)
    {
#if IOS || MACCATALYST
        if (_card.Handler?.PlatformView is not UIView view || view.Bounds.Width <= 0)
            return;

        var layer = view.Layer;
        layer.MasksToBounds = false;
        layer.ShadowColor = UIColor.Black.CGColor;
        layer.ShadowOpacity = (float)(0.28 + (0.32 * amount));
        layer.ShadowRadius = (nfloat)(3 + (7 * amount));
        layer.ShadowOffset = new CGSize(0, 2 + (6 * amount));
        layer.ShadowPath = UIBezierPath.FromRoundedRect(view.Bounds, 14).CGPath;
#else
        _card.Shadow = new Shadow
        {
            Brush = Brush.Black,
            Offset = new Point(0, 2 + (6 * amount)),
            Radius = (float)(3 + (7 * amount)),
            Opacity = (float)(0.28 + (0.32 * amount)),
        };
#endif
    }

    static Color Lerp(Color from, Color to, double amount) =>
        Color.FromRgba(
            from.Red + ((to.Red - from.Red) * amount),
            from.Green + ((to.Green - from.Green) * amount),
            from.Blue + ((to.Blue - from.Blue) * amount),
            from.Alpha + ((to.Alpha - from.Alpha) * amount));

    static ImageSource Portrait(string directory, string? relative)
    {
        if (!string.IsNullOrEmpty(relative))
        {
            var path = System.IO.Path.Combine(directory, relative.Replace('/', System.IO.Path.DirectorySeparatorChar));
            if (File.Exists(path))
                return ImageSource.FromFile(path);
        }

        return ImageSource.FromFile("contact_placeholder.png");
    }
}
