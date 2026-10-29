using MauiLab.Colors;
using System.Collections.ObjectModel;

namespace MauiLab.Layouts;

public partial class DialogLayoutPage : ContentPage
{
    public DialogLayoutPage()
    {
        InitializeComponent();
        BindingContext = new DialogLayoutPageModel()
            {
                Title = "Colors",
                Colors = new ObservableCollection<ColorModel>(ColorModel.GetNamedColors())
            };
    }

    private void Button_Clicked(object? sender, EventArgs e)
    {
        CollectionView.IsVisible = !CollectionView.IsVisible;
        LayoutGrid.Arrange(LayoutGrid.Bounds);
    }
}