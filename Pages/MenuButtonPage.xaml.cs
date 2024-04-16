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
    }

    private void PopupMenuButtonClicked(object sender, EventArgs e)
    {
        if (sender is not View view) return;
        var menu = new StaticMenuPopup()
        {
            Anchor = view,
        };
        this.ShowPopup(menu);
    }

    private void BindButtonClicked(object sender, EventArgs e)
    {
        Menu1.IsVisible = !Menu1.IsVisible;
    }


    public ICommand MenuItemCommand => new AsyncRelayCommand<string>(async (item) =>
    {
        await DisplayAlert("Menu Item Tapped", item, "OK");
    });

    private void OnButtonClicked(object sender, EventArgs e)
    {
        Menu2.IsVisible = !Menu2.IsVisible;
    }
}