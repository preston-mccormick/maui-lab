namespace MauiLab.Layouts;

public enum PanelEdge
{
    Bottom,
    Right
}

public static class LayoutPanelEdge
{
    public const double SideWidth = 800;

    public static PanelEdge ForWidth(double width) =>
        double.IsFinite(width) && width >= SideWidth ? PanelEdge.Right : PanelEdge.Bottom;
}
