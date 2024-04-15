namespace MauiCustomControls.Pages;

public partial class MenuButton : ContentPage
{
	public MenuButton()
	{
		InitializeComponent();
	}

    private void OnButtonClicked(object sender, EventArgs e)
    {
		Menu.IsVisible = !Menu.IsVisible;
    }
}