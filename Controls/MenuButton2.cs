using CommunityToolkit.Maui.Views;
using System.Collections;

namespace MauiCustomControls.Controls;

public class MenuButton2 : Button
{
    public static readonly BindableProperty ItemsProperty = BindableProperty.Create(
        nameof(Items),
        typeof(IEnumerable),
        typeof(MenuButton),
        null
    );

    public IEnumerable? Items
    {
        get => GetValue(ItemsProperty) as IEnumerable;
        set => SetValue(ItemsProperty, value);
    }

    public MenuButton2() : base()
    {
        Clicked += ButtonClicked;
    }

    private void ButtonClicked(object? sender, EventArgs e)
    {
        if (sender is not View view)
        {
            throw new ArgumentException("Sender is not a View", nameof(sender));
        };
        if (Application.Current?.MainPage is not Page page)
        {
            throw new InvalidOperationException("Could not find the current Page");
        };

        // Create the menu and popup
        Menu menu = new(Items);
        var popup = new Popup()
        {
            Anchor = view,
            Content = menu
        };

        // Close the popup when a menu item is tapped.
        menu.BeforeItemTap = () => popup.Close(null);

        page.ShowPopup(popup);
    }
}