namespace MauiLab.Layouts;

// Paints a grid cell. Measure is zero so an Auto row or column is sized by the
// other children; arrange still stretches this view across the cell.
public class CellWash : BoxView
{
    protected override Size MeasureOverride(double widthConstraint, double heightConstraint) => Size.Zero;
}
