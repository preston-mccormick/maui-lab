namespace MauiCustomControls.Geolocation;

public partial class GeolocationPage : ContentPage
{
	public GeolocationPage()
	{
		InitializeComponent();
        BindingContext = new GeolocationViewModel();
    }
}