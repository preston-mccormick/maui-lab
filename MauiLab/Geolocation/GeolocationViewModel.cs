using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace MauiLab.Geolocation;
internal class GeolocationViewModel : ObservableObject
{
    Location? _currentLocation;
    Location? _lastLocation;

    public Location? LastLocation
    {
        get => _lastLocation;
        set => SetProperty(ref _lastLocation, value);
    }

    public Location? CurrentLocation
    {
        get => _currentLocation;
        set => SetProperty(ref _currentLocation, value);
    }

    public GeolocationViewModel()
    {
        Init().ConfigureAwait(false);
    }

    public async Task Init()
    {
        LastLocation = await GetCachedLocation();
        CurrentLocation = await GetCurrentLocation();
    }

    private async Task<Location?> GetCachedLocation()
    {
        try
        {
            IGeolocation gloc = Microsoft.Maui.Devices.Sensors.Geolocation.Default;
            Location? location = await gloc.GetLastKnownLocationAsync();
            Debug.WriteLine($"Last Known Location: Latitude: {location?.Latitude}, Longitude: {location?.Longitude}, Altitude: {location?.Altitude}");
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

    private CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
    private bool _isCheckingLocation;

    private async Task<Location?> GetCurrentLocation()
    {
        _isCheckingLocation = true;
        try
        {
            IGeolocation gloc = Microsoft.Maui.Devices.Sensors.Geolocation.Default;
            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(4));
            Location? location = await gloc.GetLocationAsync(request, _cancelTokenSource.Token);
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
        finally
        {
            _isCheckingLocation = false;
        }
        return null;
    }

    public void CancelRequest()
    {
        if (_isCheckingLocation && _cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested == false)
            _cancelTokenSource.Cancel();
    }
}