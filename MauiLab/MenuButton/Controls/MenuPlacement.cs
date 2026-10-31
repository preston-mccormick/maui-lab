namespace MauiLab.MenuButton.Controls;

/// <summary>
/// Where a menu lands relative to its button. The menu opens with its top on the
/// button and grows downward. It shifts only enough to keep about <see cref="EdgeClearance"/>
/// of space at the screen edges, and scrolls when the items are taller than that space.
/// </summary>
internal readonly record struct MenuPlacement(double X, double Y, double Width, double Height, bool Scroll)
{
    /// <summary>About 5mm, in device-independent units (160 per inch).</summary>
    internal const double EdgeClearance = 5d / 25.4d * 160d;

    internal static MenuPlacement Place(Rect button, Size content, Size window, double edge = EdgeClearance)
    {
        var inset = Math.Max(0, edge);
        var width = Math.Max(button.Width, content.Width);
        if (window.Width > 0)
            width = Math.Min(width, Math.Max(0, window.Width - inset * 2));

        // Stay on the button. The clearance only pulls the menu back in when it
        // would meet the far edge, or when the button itself is off screen.
        var x = button.X;
        if (window.Width > 0 && x + width > window.Width - inset)
            x = Math.Max(inset, window.Width - inset - width);
        if (x < 0)
            x = window.Width > 0 ? inset : 0;

        var availableHeight = window.Height > 0 ? Math.Max(0, window.Height - inset * 2) : content.Height;
        var scroll = window.Height > 0 && content.Height > availableHeight;
        var height = scroll ? availableHeight : content.Height;

        var y = button.Y;
        if (window.Height > 0 && y + height > window.Height - inset)
            y = Math.Max(inset, window.Height - inset - height);
        if (y < 0)
            y = window.Height > 0 ? inset : 0;

        return new MenuPlacement(x, y, width, height, scroll);
    }
}
