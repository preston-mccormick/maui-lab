namespace MauiLab.FormControls;

public partial class FormControlsPage : ContentPage
{
    public FormControlsPage()
    {
        InitializeComponent();
        BindingContext = new FormGallery();
    }
}
