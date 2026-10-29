using System.Collections;

namespace MauiLab.MenuButton.Controls;

/// <summary>
/// Deriving from button retains all the default button capabilites. However the Click event
/// and Command property should probably not be used.
///
/// To gain the default button style, add ApplyToDerivedTypes="True" to the implicit button
/// style and any other button styles that you might want to use on this control.
/// </summary>
public class MenuButton : Button
{
    public static readonly BindableProperty ItemsProperty = BindableProperty.Create(
        nameof(Items),
        typeof(IEnumerable),
        typeof(MenuButton),
        null
    );

    /// <summary>
    /// Objects representing the menu items.
    /// The menu items must use the hard-coded names for the properties Text, Image, TapCommand, and TapCommandParameter for the binding to work.
    /// </summary>
    /// <remarks>Could concievably make these property names configurable, but it would add an extra layer of reflection for questionable gain.</remarks>
    public IEnumerable? Items
    {
        get => GetValue(ItemsProperty) as IEnumerable;
        set => SetValue(ItemsProperty, value);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public MenuButton() : base()
    {
        Clicked += ButtonClicked;
    }

    /// <summary>
    /// Generates the menu from the Items collection and displays it in a popup anchored to the button.
    /// The popup will close when a menu item is tapped or when tapping outside of the popup.
    /// </summary>
    private async void ButtonClicked(object? sender, EventArgs e)
    {
        if (sender is not View view)
        {
            throw new ArgumentException("Sender is not a View", nameof(sender));
        };
        if (Items is null)
        {
            throw new InvalidOperationException("Items collection is null");
        }

        await ContextMenu.ShowAsync(view, Items);
    }
}