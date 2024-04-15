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


        ViewModels.MenuItemViewModel[] items = [
            new ViewModels.MenuItemViewModel() { Text = "Mine", Icon = "steelpick.png" },
            new ViewModels.MenuItemViewModel() { Text = "Farm", Icon = "stonehoe.png" },
            new ViewModels.MenuItemViewModel() { Text = "Fight", Icon = "diamondsword.png" },
            new ViewModels.MenuItemViewModel() { Text = "Mine", Icon = "steelpick.png" },
            new ViewModels.MenuItemViewModel() { Text = "Learn", Icon = "book.png" },
            new ViewModels.MenuItemViewModel() { Text = "Eat", Icon = "apple.png" }];
        var viewModel = new ViewModels.MenuButtonViewModel("Bound Menu", items);
        // Binding at page level does not work!
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