using MauiCustomControls.Views;
using CommunityToolkit.Maui.Views;
using MauiCustomControls.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace MauiCustomControls.Pages;

public partial class MenuButtonPage : ContentPage
{
    public MenuButtonPage()
    {
        InitializeComponent();

        MenuItemViewModel[] items = [
            new MenuItemViewModel() { Text = "Mine", Icon = "steelpick.png" },
            new MenuItemViewModel() { Text = "Farm", Icon = "stonehoe.png" },
            new MenuItemViewModel() { Text = "Fight", Icon = "diamondsword.png" },
            new MenuItemViewModel() { Text = "Mine", Icon = "steelpick.png" },
            new MenuItemViewModel() { Text = "Learn", Icon = "book.png" },
            new MenuItemViewModel() { Text = "Eat", Icon = "apple.png" }];
        var viewModel = new MenuButtonViewModel("Bound Menu", items);

        BindingContext = viewModel;

        Menu2.BeforeItemTap = () => Menu2.IsVisible = false;
    }

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