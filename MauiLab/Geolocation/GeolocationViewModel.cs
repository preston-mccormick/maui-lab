using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace MauiLab.Geolocation;

public partial class GeolocationViewModel : ObservableObject
{
    private readonly IGeolocation _service;
    private Location? _lastLocation;

    public Location? LastLocation
    {
        get => _lastLocation;
        set
        {
            if (SetProperty(ref _lastLocation, value))
            {

                Debug.WriteLine($"LastLocation Changed: {_lastLocation}");
            }
        }
    }

    public GeolocationViewModel(IGeolocation geolocation)
    {
        _service = geolocation;
    }

    public bool CanStart() => !_service.IsListeningForeground;

    public bool CanStop() => _service.IsListeningForeground;

    private void OnListeningFailed(object? sender, GeolocationListeningFailedEventArgs e)
    {
        Debug.WriteLine($"ListeningFailed: {e.Error}");
        StartListeningCommand.NotifyCanExecuteChanged();
        StopListeningCommand.NotifyCanExecuteChanged();
    }

    private void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
    {
        LastLocation = e.Location;
    }

    [RelayCommand(AllowConcurrentExecutions = false, CanExecute = "CanStart")]
    private async Task StartListening()
    {
        LastLocation = await _service.GetLastKnownLocationAsync();
        if (!_service.IsListeningForeground)
        {
            _service.LocationChanged += OnLocationChanged;
            _service.ListeningFailed += OnListeningFailed;
            var request = new GeolocationListeningRequest(
                accuracy: GeolocationAccuracy.Default);
            bool success = false;
            try
            {
                success = await _service.StartListeningForegroundAsync(request);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            }

            if (success)
            {
                Debug.WriteLine("Started listening to geolocation");
            }
            else
            {
                _service.LocationChanged -= OnLocationChanged;
                _service.ListeningFailed -= OnListeningFailed;
                Debug.WriteLine("Failed to start listening to geolocation");
            }
        }
        StartListeningCommand.NotifyCanExecuteChanged();
        StopListeningCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = "CanStop")]
    private void StopListening()
    {
        try
        {
            _service.LocationChanged -= OnLocationChanged;
            _service.ListeningFailed -= OnListeningFailed;
            if (_service.IsListeningForeground)
            {
                _service.StopListeningForeground();
            }
            StartListeningCommand.NotifyCanExecuteChanged();
            StopListeningCommand.NotifyCanExecuteChanged();
            Debug.WriteLine("Stopped listening to geolocation");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.GetType().Name}: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task UpdateLocation()
    {
        try
        {
            var location = await _service.GetLocationAsync();

            if (location == null)
            {
                location = await _service.GetLastKnownLocationAsync();
            }

            LastLocation = location;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.GetType().Name}: {ex.Message}");
        }
    }
}