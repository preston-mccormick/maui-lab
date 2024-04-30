using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Devices.Sensors;

namespace MauiCustomControls.Geolocation;
internal class GeolocationViewModel : ObservableObject
{
    Location? _lastLocation;
    public Location? LastLocation
    {
        get => _lastLocation;
        set => SetProperty(ref _lastLocation, value);
    }

    public GeolocationViewModel()
    {
        Init().ConfigureAwait(false);
    }

    public async Task Init()
    {
        LastLocation = await GetCachedLocation();
    }

    public async Task<Location?> GetCachedLocation()
    {
        try
        {
            IGeolocation gloc = Microsoft.Maui.Devices.Sensors.Geolocation.Default;
            Location? location = await gloc.GetLastKnownLocationAsync();
            Debug.WriteLine($"Latitude: {location?.Latitude}, Longitude: {location?.Longitude}, Altitude: {location?.Altitude}");
            return location;
        }
        catch (FeatureNotSupportedException fnsEx)
        {
            // Handle not supported on device exception
            Debug.WriteLine(fnsEx);
        }
        catch (FeatureNotEnabledException fneEx)
        {
            // Handle not enabled on device exception
            Debug.WriteLine(fneEx);
        }
        catch (PermissionException pEx)
        {
            // Handle permission exception
            Debug.WriteLine(pEx);
        }
        return null;
    }
}
