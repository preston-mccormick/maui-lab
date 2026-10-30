using MauiLab.ViewModels;

namespace MauiLab.MenuButton;

public partial class MenuButtonPage : ContentPage
{
    public MenuButtonPage()
    {
        InitializeComponent();

        MenuItemViewModel[] items = [
            new MenuItemViewModel() { Text = "Mine", Image = "steelpick.png" },
            new MenuItemViewModel() { Text = "Farm", Image = "stonehoe.png" },
            new MenuItemViewModel() { Text = "Fight", Image = "diamondsword.png" },
            new MenuItemViewModel() { Text = "Mine", Image = "steelpick.png" },
            new MenuItemViewModel() { Text = "Learn", Image = "book.png" },
            new MenuItemViewModel() { Text = "Eat", Image = "apple.png" },
            new MenuItemViewModel() { Text = "Mine", Image = "steelpick.png" },
            new MenuItemViewModel() { Text = "Farm", Image = "stonehoe.png" },
            new MenuItemViewModel() { Text = "Fight", Image = "diamondsword.png" },
            new MenuItemViewModel() { Text = "Mine", Image = "steelpick.png" },
            new MenuItemViewModel() { Text = "Learn", Image = "book.png" },
            new MenuItemViewModel() { Text = "Eat", Image = "apple.png" }
            ];
        TopLeft.BindingContext = new MenuButtonViewModel("Top Left", items);
        MiddleLeft.BindingContext = new MenuButtonViewModel("Middle Left", items);
        BottomLeft.BindingContext = new MenuButtonViewModel("Bottom Left", items);
        TopCenter.BindingContext = new MenuButtonViewModel("Top Center", items);
        MiddleCenter.BindingContext = new MenuButtonViewModel("Middle Center", items);
        BottomCenter.BindingContext = new MenuButtonViewModel("Bottom Center", items);
        TopRight.BindingContext = new MenuButtonViewModel("Top Right", items);
        MiddleRight.BindingContext = new MenuButtonViewModel("Middle Right", items);
        BottomRight.BindingContext = new MenuButtonViewModel("Bottom Right", items);
    }
}