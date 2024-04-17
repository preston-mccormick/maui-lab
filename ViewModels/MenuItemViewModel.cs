using CommunityToolkit.Maui.Alerts;
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
    private void Tap(object parameter)
    {
        string message = $"Menu Item Tapped: {parameter ?? "<null>"}";
        Toast.Make(message).Show();
    }
}