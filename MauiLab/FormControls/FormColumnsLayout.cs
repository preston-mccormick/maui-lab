using Microsoft.Maui.Layouts;

namespace MauiLab.FormControls;

/// <summary>
/// Flows its children into equal-width columns. The column count comes from
/// <see cref="FormColumnCount.ForWidth"/> of the width this layout is given, so each group
/// measures its own variant area rather than the window. Each row is as tall as its tallest
/// child, so a cell that grows (a validation message, an auto-sizing editor) pushes the next
/// row down instead of overlapping it.
/// </summary>
/// <remarks>
/// The toolkit's <c>UniformItemsLayout</c> is not used here because its manager measures only
/// the first child, once, and gives every cell that size.
/// </remarks>
public class FormColumnsLayout : Layout
{
    protected override ILayoutManager CreateLayoutManager() => new FormColumnsLayoutManager(this);
}

public class FormColumnsLayoutManager(FormColumnsLayout layout) : LayoutManager(layout)
{
    public override Size Measure(double widthConstraint, double heightConstraint)
    {
        var padding = layout.Padding;
        var width = widthConstraint - padding.HorizontalThickness;
        var columns = FormColumnCount.ForWidth(width);
        var columnWidth = double.IsFinite(width) ? width / columns : double.PositiveInfinity;

        double totalHeight = 0, rowHeight = 0, widest = 0;
        var index = 0;
        foreach (var child in VisibleChildren())
        {
            var size = child.Measure(columnWidth, double.PositiveInfinity);
            rowHeight = Math.Max(rowHeight, size.Height);
            widest = Math.Max(widest, size.Width);
            if (++index % columns == 0)
            {
                totalHeight += rowHeight;
                rowHeight = 0;
            }
        }
        totalHeight += rowHeight;

        var measuredWidth = double.IsFinite(width) ? width : widest * columns;
        return new Size(measuredWidth + padding.HorizontalThickness, totalHeight + padding.VerticalThickness);
    }

    public override Size ArrangeChildren(Rect bounds)
    {
        var padding = layout.Padding;
        var width = bounds.Width - padding.HorizontalThickness;
        var columns = FormColumnCount.ForWidth(width);
        var columnWidth = width / columns;

        var children = VisibleChildren().ToList();
        var y = bounds.Top + padding.Top;
        for (var first = 0; first < children.Count; first += columns)
        {
            var row = children.Skip(first).Take(columns).ToList();
            var rowHeight = row.Max(child => child.Measure(columnWidth, double.PositiveInfinity).Height);
            for (var column = 0; column < row.Count; column++)
            {
                var x = bounds.Left + padding.Left + column * columnWidth;
                row[column].Arrange(new Rect(x, y, columnWidth, rowHeight));
            }
            y += rowHeight;
        }

        return new Size(bounds.Width, y - bounds.Top + padding.Bottom);
    }

    IEnumerable<IView> VisibleChildren() => layout.Where(static child => child.Visibility == Visibility.Visible);
}
