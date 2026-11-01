namespace MauiLab.FormControls;

/// <summary>
/// Column count for a group's variant area. Measured from the area, not the window,
/// so an inset group breaks at the same widths as a full-width one.
/// </summary>
public static class FormColumnCount
{
    public const double TwoColumnWidth = 640;
    public const double ThreeColumnWidth = 960;

    public static int ForWidth(double width)
    {
        if (double.IsNaN(width) || double.IsInfinity(width))
            return 1;
        if (width >= ThreeColumnWidth)
            return 3;
        if (width >= TwoColumnWidth)
            return 2;
        return 1;
    }
}
