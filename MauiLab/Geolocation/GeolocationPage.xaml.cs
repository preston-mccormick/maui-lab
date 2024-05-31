namespace MauiLab.Geolocation;

public partial class GeolocationPage : ContentPage
{
	public GeolocationPage()
	{
		InitializeComponent();

		IGeolocation location = Microsoft.Maui.Devices.Sensors.Geolocation.Default;
        BindingContext = new GeolocationViewModel(location);
    }
}