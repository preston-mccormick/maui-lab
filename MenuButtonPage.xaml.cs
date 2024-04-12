namespace MauiCustomControls;

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