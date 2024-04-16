using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using MauiCustomControls.ViewModels;
using System.Collections;
using System.ComponentModel;
using System.Windows.Input;

namespace MauiCustomControls.Views;

public partial class MenuButton : ContentView
{
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(ImageSource),
        typeof(MenuItem),
        default(ImageSource)
    );

    public static readonly BindableProperty ItemsProperty = BindableProperty.Create(
        nameof(Items),
        typeof(IEnumerable),
        typeof(MenuButton),
        null
    );

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(MenuButton),
        string.Empty
    );

    [TypeConverter(typeof(ImageSourceConverter))]
    public ImageSource? Icon
    {
        get => GetValue(IconProperty) as ImageSource;
        set => SetValue(IconProperty, value);
    }

    public IEnumerable? Items
    {
        get => GetValue(ItemsProperty) as IEnumerable;
        set => SetValue(ItemsProperty, value);
    }

    public string? Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public MenuButton()
    {
        InitializeComponent();
    }

    private void ButtonClicked(object sender, EventArgs e)
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