using CommunityToolkit.Mvvm.ComponentModel;
using MauiLab.Colors;
using System.Collections.ObjectModel;

namespace MauiLab.Layouts;

public partial class DialogLayoutPageModel : ObservableObject
{
    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private ObservableCollection<ColorModel> _colors = new();
}
