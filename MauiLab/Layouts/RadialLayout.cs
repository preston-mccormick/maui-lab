using Microsoft.Maui.Layouts;

namespace MauiLab.Layouts;

public class RadialLayout : Layout
{
    public static readonly BindableProperty AngleProperty =
        BindableProperty.CreateAttached("Angle", typeof(double?), typeof(RadialLayout), null, propertyChanged: OnAngleChanged);

    public static double? GetAngle(BindableObject view) => (double?)view.GetValue(AngleProperty);

    public static void SetAngle(BindableObject view, double? value) => view.SetValue(AngleProperty, value);

    public static readonly BindableProperty StartAngleProperty =
        BindableProperty.Create(nameof(StartAngle), typeof(double), typeof(RadialLayout), 0d, propertyChanged: OnArcChanged);

    public double StartAngle
    {
        get => (double)GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    public static readonly BindableProperty SweepProperty =
        BindableProperty.Create(nameof(Sweep), typeof(double), typeof(RadialLayout), 180d, propertyChanged: OnArcChanged);

    public double Sweep
    {
        get => (double)GetValue(SweepProperty);
        set => SetValue(SweepProperty, value);
    }

    public static readonly BindableProperty RadiusProperty =
        BindableProperty.Create(nameof(Radius), typeof(double), typeof(RadialLayout), 0d, propertyChanged: OnArcChanged);

    public double Radius
    {
        get => (double)GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    // A changed arc or angle does not change any child's size, so nothing asks for a new
    // pass on its own. Invalidate here so the manager runs again.
    static void OnArcChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((RadialLayout)bindable).InvalidateMeasure();

    static void OnAngleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is Element { Parent: RadialLayout layout })
            layout.InvalidateMeasure();
    }

    protected override ILayoutManager CreateLayoutManager() => new RadialLayoutManager(this);
}

public class RadialLayoutManager(RadialLayout layout) : LayoutManager(layout)
{
    public override Size Measure(double widthConstraint, double heightConstraint)
    {
        var padding = layout.Padding;
        double widest = 0, tallest = 0;
        foreach (var child in Visible())
        {
            var size = child.Measure(double.PositiveInfinity, double.PositiveInfinity);
            widest = Math.Max(widest, size.Width);
            tallest = Math.Max(tallest, size.Height);
        }

        var item = Math.Max(widest, tallest);
        var radius = layout.Radius > 0 ? layout.Radius : item;
        var span = radius * 2 + item;
        return new Size(span + padding.HorizontalThickness, span + padding.VerticalThickness);
    }

    public override Size ArrangeChildren(Rect bounds)
    {
        var children = Visible().ToList();
        if (children.Count == 0)
            return bounds.Size;

        var padding = layout.Padding;
        var inner = new Rect(
            bounds.X + padding.Left,
            bounds.Y + padding.Top,
            Math.Max(0, bounds.Width - padding.HorizontalThickness),
            Math.Max(0, bounds.Height - padding.VerticalThickness));
        var largest = children.Max(child => Math.Max(child.DesiredSize.Width, child.DesiredSize.Height));
        var fit = Math.Max(0, Math.Min(inner.Width, inner.Height) / 2d - largest / 2d);
        var radius = layout.Radius > 0 ? layout.Radius : fit;
        var center = new Point(inner.X + inner.Width / 2d, inner.Y + inner.Height / 2d);
        var points = RadialPlacement.Place(
            center,
            radius,
            layout.StartAngle,
            layout.Sweep,
            children.Select(child => GetAngle(child)).ToList());

        for (var index = 0; index < children.Count; index++)
        {
            var child = children[index];
            var size = child.DesiredSize;
            child.Arrange(new Rect(
                points[index].X - size.Width / 2d,
                points[index].Y - size.Height / 2d,
                size.Width,
                size.Height));
        }

        return bounds.Size;
    }

    IEnumerable<IView> Visible() => layout.Where(static child => child.Visibility == Visibility.Visible);

    static double? GetAngle(IView child) =>
        child is BindableObject bindable ? RadialLayout.GetAngle(bindable) : null;
}
