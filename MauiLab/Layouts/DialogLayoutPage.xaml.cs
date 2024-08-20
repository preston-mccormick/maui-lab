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
}