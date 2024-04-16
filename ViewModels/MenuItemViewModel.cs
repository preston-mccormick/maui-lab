using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiCustomControls.ViewModels;

public partial class MenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string? icon;

    [ObservableProperty]
    private string? text;

    [RelayCommand]
    private void Tap()
    {
        if (Application.Current?.MainPage is not Page page) return;
        page.DisplayAlert("Menu Item Tapped", Text, "OK");
    }
}