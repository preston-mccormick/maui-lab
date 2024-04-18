using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiCustomControls.ViewModels;

public partial class MenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string? image;

    [ObservableProperty]
    private string? text;

    [RelayCommand]
    private void Tap(object parameter)
    {
#if WINDOWS
        if (Application.Current?.MainPage is not Page page) return;
        page.DisplayAlert("Menu Item Tapped", Text, "OK");
#else       
        string message = $"Menu Item Tapped: {parameter ?? "<null>"}";
        Toast.Make(message, textSize: 42).Show();
#endif
    }
}