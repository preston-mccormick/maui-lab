namespace MauiLab.Usage;

/// <summary>
/// Best-effort window mode from the app's rectangle compared with the full screen.
/// No platform API names Slide Over, so an ambiguous rectangle stays unlabeled.
/// </summary>
public static class UsageArea
{
    public static string? Classify(
        double width,
        double height,
        double screenWidth,
        double screenHeight,
        double x,
        double y)
    {
        if (!Finite(width) || !Finite(height) || !Finite(screenWidth) || !Finite(screenHeight) || !Finite(x) || !Finite(y))
            return null;
        if (width < 1 || height < 1 || screenWidth < 1 || screenHeight < 1)
            return null;

        const double edge = 24;
        var nearFullHeight = height >= screenHeight - 80;
        var nearFullWidth = Math.Abs(width - screenWidth) <= edge;
        var atOrigin = x <= edge && y <= edge;
        if (nearFullWidth && nearFullHeight && atOrigin)
            return "Full screen";

        var edgeAligned = x <= edge || Math.Abs(x + width - screenWidth) <= edge;
        var splitWidth = IsSplitFraction(width, screenWidth);
        var compact = width is >= 280 and <= 420;
        if (nearFullHeight && edgeAligned && splitWidth && compact)
            return null;
        if (nearFullHeight && edgeAligned && splitWidth)
            return "Split View";
        if (nearFullHeight && compact && !splitWidth)
            return "Slide Over";
        if (!nearFullWidth || !nearFullHeight)
            return "Windowed";

        return null;
    }

    static bool IsSplitFraction(double width, double screenWidth)
    {
        foreach (var fraction in new[] { 1d / 3, 0.5, 2d / 3 })
        {
            var expected = screenWidth * fraction;
            if (Math.Abs(width - expected) <= Math.Max(32, screenWidth * 0.05))
                return true;
        }

        return false;
    }

    static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
}
