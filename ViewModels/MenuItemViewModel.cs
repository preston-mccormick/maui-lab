using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiCustomControls.ViewModels;

public partial class MenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string? text;

    [ObservableProperty]
    private string? icon;

    [RelayCommand]
    private void Tap()
    {
        if (App.Current?.MainPage is not Page page) return;
        page.DisplayAlert("Menu Item Tapped", Text, "OK");
    }
}
