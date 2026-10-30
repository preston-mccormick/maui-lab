using CommunityToolkit.Mvvm.ComponentModel;
using MauiLab.Colors;
using System.Collections.ObjectModel;

namespace MauiLab.Layouts;

public partial class DialogLayoutPageModel : ObservableObject
{
    [ObservableProperty]
    public partial string? Title { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ColorModel> Colors { get; set; } = [];
}
