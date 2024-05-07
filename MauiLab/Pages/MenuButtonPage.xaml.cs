using MauiLab.Views;
using MauiLab.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;

namespace MauiLab.Pages;

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

        Menu2.BeforeItemTap = () => Menu2.IsVisible = false;
    }


    #region Popup Prototype

    private void PopupMenuButtonClicked(object sender, EventArgs e)
    {
        var popup = new StaticMenuPopup();
        popup.Anchor = sender as View;
        if (Application.Current?.MainPage is Page page)
        {
            page.ShowPopup(popup);
        }
    }

    #endregion

    #region Menu ProtoType

    private void BindableMenuButtonClicked(object sender, EventArgs e)
    {
        Menu2.IsVisible = !Menu2.IsVisible;
    }

    #endregion

    #region MenuItem Prototype

    public ICommand MenuItemCommand => new AsyncRelayCommand<string>(async (item) =>
    {
        Menu1.IsVisible = false;
        await DisplayAlert("Menu Item Tapped", item, "OK");
    });

    private void MenuButtonClicked(object sender, EventArgs e)
    {
        Menu1.IsVisible = !Menu1.IsVisible;
    }

    #endregion

}