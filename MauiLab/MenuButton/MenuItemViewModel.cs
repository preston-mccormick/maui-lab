using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiLab.ViewModels;

public partial class MenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _image;

    [ObservableProperty]
    private string? _text;

    [RelayCommand]
    private void Tap(object parameter)
    {
#if WINDOWS
        if (Application.Current?.MainPage is not Page page) return;
        page.DisplayAlert("Menu Item Tapped", Text, "OK");
#else       
        string message = $"Menu Item Tapped: {Text}";
        Toast.Make(message, textSize: 42).Show();
#endif
    }
}