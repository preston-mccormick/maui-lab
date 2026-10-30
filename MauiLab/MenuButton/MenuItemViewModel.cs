using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiLab.ViewModels;

public partial class MenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string? Image { get; set; }

    [ObservableProperty]
    public partial string? Text { get; set; }

    [RelayCommand]
    private void Tap(object parameter)
    {
#if WINDOWS
        if (Application.Current?.MainPage is not Page page) return;
        page.DisplayAlert("Menu Item Tapped", Text, "OK");
#else       
        string message = Text ?? String.Empty;
        Toast.Make(message, textSize: 42).Show();
#endif
    }
}