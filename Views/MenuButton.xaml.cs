using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
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

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(MenuButton),
        string.Empty);

    [TypeConverter(typeof(ImageSourceConverter))]
    public ImageSource Icon
    {
        get => (ImageSource)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public MenuButton()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private void ButtonClicked(object sender, EventArgs e)
    {
        if (sender is not Button button) return;
        if (App.Current?.MainPage is not Page page) return;

        ViewModels.MenuItemViewModel[] items = [
            new ViewModels.MenuItemViewModel() { Text = "Mine", Icon = "steelpick.png" },
            new ViewModels.MenuItemViewModel() { Text = "Farm", Icon = "stonehoe.png" },
            new ViewModels.MenuItemViewModel() { Text = "Fight", Icon = "diamondsword.png" },
            new ViewModels.MenuItemViewModel() { Text = "Mine", Icon = "steelpick.png" },
            new ViewModels.MenuItemViewModel() { Text = "Learn", Icon = "book.png" },
            new ViewModels.MenuItemViewModel() { Text = "Eat", Icon = "apple.png" }];

        BindableMenu menu = new(items);
        var popup = new Popup()
        {
            Anchor = button,
            Content = menu
        };
        page.ShowPopup(popup);
    }
}