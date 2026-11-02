using Microsoft.Maui.Controls.Shapes;

namespace MauiLab.Layouts;

public static class EllipseClip
{
    public static readonly BindableProperty IsClippedProperty = BindableProperty.CreateAttached(
        "IsClipped",
        typeof(bool),
        typeof(EllipseClip),
        false,
        propertyChanged: (bindable, _, value) =>
        {
            if (bindable is VisualElement view)
                Apply(view, value is true);
        });

    public static bool GetIsClipped(BindableObject view) => (bool)view.GetValue(IsClippedProperty);

    public static void SetIsClipped(BindableObject view, bool value) => view.SetValue(IsClippedProperty, value);

    static void Apply(VisualElement view, bool clipped)
    {
        view.SizeChanged -= OnSizeChanged;
        if (!clipped)
        {
            view.Clip = null;
            return;
        }

        view.SizeChanged += OnSizeChanged;
        Update(view);
    }

    static void OnSizeChanged(object? sender, EventArgs e)
    {
        if (sender is VisualElement view)
            Update(view);
    }

    static void Update(VisualElement view)
    {
        if (view.Width <= 0 || view.Height <= 0)
            return;

        view.Clip = new EllipseGeometry(
            new Point(view.Width / 2, view.Height / 2),
            view.Width / 2,
            view.Height / 2);
    }
}
