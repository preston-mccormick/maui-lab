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
        var viewModel = new MenuButtonViewModel("Bound Menu", items);

        BindingContext = viewModel;
    }
}