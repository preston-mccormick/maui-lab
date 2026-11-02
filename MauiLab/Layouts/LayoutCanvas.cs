namespace MauiLab.Layouts;

/// <summary>
/// A tap on the layout itself also reaches a tap on an item, because the item's gesture
/// recognizer does not stop the ancestor's. This asks whether the tap landed on an item
/// so the canvas tap can clear the selection only when it missed.
/// </summary>
static class LayoutCanvas
{
    public static bool HitsItem(TappedEventArgs tap, Element canvas)
    {
        if (canvas is not IVisualTreeElement root)
            return false;

        foreach (var element in Descendants(root))
        {
            if (element is not VisualElement view || view.BindingContext is not LayoutItem)
                continue;
            if (view is not (LayoutItemView or Border) || !view.IsVisible || view.Width <= 0 || view.Height <= 0)
                continue;
            if (tap.GetPosition(view) is Point point
                && point.X >= 0 && point.Y >= 0
                && point.X < view.Width && point.Y < view.Height)
                return true;
        }

        return false;
    }

    static IEnumerable<IVisualTreeElement> Descendants(IVisualTreeElement root)
    {
        foreach (var child in root.GetVisualChildren())
        {
            yield return child;
            foreach (var nested in Descendants(child))
                yield return nested;
        }
    }
}
