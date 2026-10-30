using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MauiLab.ViewModels;

public partial class MenuButtonViewModel : ObservableObject
{
    public MenuButtonViewModel(string? text = null, params MenuItemViewModel[] items)
    {
        Text = text;
        Items = [.. items];
    }

    [ObservableProperty]
    public partial string? Text { get; set; }

    public ObservableCollection<MenuItemViewModel> Items { get; }
}
