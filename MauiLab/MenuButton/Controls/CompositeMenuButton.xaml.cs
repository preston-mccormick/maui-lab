using CommunityToolkit.Maui.Views;
using System.Collections;
using System.ComponentModel;

namespace MauiLab.MenuButton.Controls;

/// <summary>
/// The composite menu button inferior in most ways to the derived version, but it is more
/// analgous to how this will have to be done with SyncFusion and there are other scenarios
/// where the composite approach adds flexibility.
///
/// The main downside is that any functionality that you want to expose in nested controls
/// requires replicating the properties on the composite control.
/// </summary>
public partial class CompositeMenuButton : ContentView
{
    public static readonly BindableProperty ImageProperty = BindableProperty.Create(
        nameof(Image), typeof(ImageSource), typeof(CompositeMenuButton), default(ImageSource),
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is not CompositeMenuButton menuButton) return;
            if (newValue is not ImageSource icon) return;
            menuButton.Button.ImageSource = icon;
        });

    public static readonly BindableProperty ItemsProperty = BindableProperty.Create(
        nameof(Items), typeof(IEnumerable), typeof(CompositeMenuButton), null);

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CompositeMenuButton), string.Empty,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is not CompositeMenuButton menuButton) return;
            if (newValue is not string text) return;
            menuButton.Button.Text = text;
        });

    [TypeConverter(typeof(ImageSourceConverter))]
    public ImageSource? Image
    {
        get => GetValue(ImageProperty) as ImageSource;
        set => SetValue(ImageProperty, value);
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
    public CompositeMenuButton()
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