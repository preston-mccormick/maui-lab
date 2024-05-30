using CommunityToolkit.Maui.Views;
using System.Collections;

namespace MauiLab.MenuButton.Controls;

/// <summary>
/// Show a context menu anchored to a view.
/// </summary>
public class ContextMenu
{
    public static void Show(View anchor, IEnumerable items)
    {
        if (Application.Current?.MainPage is not Page page)
        {
            throw new InvalidOperationException("Could not find the current Page");
        };
        Show(page, anchor, items);
    }

    public static void Show(Page page, View anchor, IEnumerable items)
    {
        Popup popup = BuildPopup(anchor, items);
        page.ShowPopup(popup);
    }

    public static async Task ShowAsync(View anchor, IEnumerable items)
    {
        if (Application.Current?.MainPage is not Page page)
        {
            throw new InvalidOperationException("Could not find the current Page");
        };
        await ShowAsync(page, anchor, items);
    }

    public static async Task ShowAsync(Page page, View anchor, IEnumerable items)
    {
        Popup popup = BuildPopup(anchor, items);
        await page.ShowPopupAsync(popup);
    }

    private static Popup BuildPopup(View anchor, IEnumerable items)
    {
        // Create the menu and popup
        var menu = new Menu(items);
        var popup = new Popup()
        {
            Anchor = anchor,
            Content = menu
        };

        // Close popup menu item is tapped
        menu.BeforeItemTap = () => popup.Close();
        return popup;
    }
}