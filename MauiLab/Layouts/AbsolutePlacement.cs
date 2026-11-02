using Microsoft.Maui.Layouts;

namespace MauiLab.Layouts;

public static class AbsolutePlacement
{
    public static Rect Move(Rect bounds, double dx, double dy, AbsoluteLayoutFlags flags, Size canvas)
    {
        var x = bounds.X + Scaled(dx, flags.HasFlag(AbsoluteLayoutFlags.XProportional), canvas.Width);
        var y = bounds.Y + Scaled(dy, flags.HasFlag(AbsoluteLayoutFlags.YProportional), canvas.Height);
        return new Rect(x, y, bounds.Width, bounds.Height);
    }

    /// <summary>Grows a rectangle by a drag of its bottom-right corner, honoring proportional size flags.</summary>
    public static Rect Resize(Rect bounds, double dx, double dy, AbsoluteLayoutFlags flags, Size canvas, double minimum)
    {
        var widthProportional = flags.HasFlag(AbsoluteLayoutFlags.WidthProportional);
        var heightProportional = flags.HasFlag(AbsoluteLayoutFlags.HeightProportional);
        var minWidth = widthProportional ? Scaled(minimum, true, canvas.Width) : minimum;
        var minHeight = heightProportional ? Scaled(minimum, true, canvas.Height) : minimum;
        var width = Math.Max(minWidth, bounds.Width + Scaled(dx, widthProportional, canvas.Width));
        var height = Math.Max(minHeight, bounds.Height + Scaled(dy, heightProportional, canvas.Height));
        return new Rect(bounds.X, bounds.Y, width, height);
    }

    public static Rect ToPixels(Rect bounds, AbsoluteLayoutFlags flags, Size canvas) => new(
        flags.HasFlag(AbsoluteLayoutFlags.XProportional) ? bounds.X * canvas.Width : bounds.X,
        flags.HasFlag(AbsoluteLayoutFlags.YProportional) ? bounds.Y * canvas.Height : bounds.Y,
        flags.HasFlag(AbsoluteLayoutFlags.WidthProportional) ? bounds.Width * canvas.Width : bounds.Width,
        flags.HasFlag(AbsoluteLayoutFlags.HeightProportional) ? bounds.Height * canvas.Height : bounds.Height);

    public static Rect Random(Random random, Size canvas, Size item)
    {
        var width = Math.Min(item.Width, Math.Max(0, canvas.Width));
        var height = Math.Min(item.Height, Math.Max(0, canvas.Height));
        var maxX = Math.Max(0, canvas.Width - width);
        var maxY = Math.Max(0, canvas.Height - height);
        return new Rect(random.NextDouble() * maxX, random.NextDouble() * maxY, width, height);
    }

    static double Scaled(double delta, bool proportional, double canvas) =>
        proportional ? (canvas == 0 ? 0 : delta / canvas) : delta;
}
