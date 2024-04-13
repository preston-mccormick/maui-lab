namespace MauiCustomControls.Pages;

public partial class MenuButtonPage : ContentPage
{
	public MenuButtonPage()
	{
		InitializeComponent();
	}

    private void OnButtonClicked(object sender, EventArgs e)
    {
		Menu.IsVisible = !Menu.IsVisible;
    }
}