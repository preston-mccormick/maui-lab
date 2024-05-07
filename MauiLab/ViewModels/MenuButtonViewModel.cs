using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MauiCustomControls.ViewModels;

public partial class MenuButtonViewModel : ObservableObject
{
    public MenuButtonViewModel(string? text = null, params MenuItemViewModel[] items)
    {
        Items = new ObservableCollection<MenuItemViewModel>(items);
    }

    [ObservableProperty]
    private string? text;

    public ObservableCollection<MenuItemViewModel> Items { get; }
}
