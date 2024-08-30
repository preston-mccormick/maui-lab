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
    public async Task StartListening()
    {
        try
        {
            LastLocation = await _service.GetLastKnownLocationAsync();

            if (_service.IsListeningForeground) return;

            _service.LocationChanged += OnLocationChanged;
            _service.ListeningFailed += OnListeningFailed;

            bool success = false;
            var request = new GeolocationListeningRequest();
            success = await _service.StartListeningForegroundAsync(request);

            if (success)
            {
                Debug.WriteLine("Started listening to geolocation");
                StartListeningCommand.NotifyCanExecuteChanged();
                StopListeningCommand.NotifyCanExecuteChanged();
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            await AppShell.Current.DisplayAlert(ex.GetType().Name, ex.Message, "Close");
        }

        // Failed
        await StopListening();
    }

    [RelayCommand(CanExecute = "CanStop")]
    public async Task StopListening()
    {
        try
        {
            _service.LocationChanged -= OnLocationChanged;
            _service.ListeningFailed -= OnListeningFailed;
            if (_service.IsListeningForeground)
            {
                _service.StopListeningForeground();
                Debug.WriteLine("Stopped listening to geolocation");
            }
            StartListeningCommand.NotifyCanExecuteChanged();
            StopListeningCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            await AppShell.Current.DisplayAlert(ex.GetType().Name, ex.Message, "Close");
        }
    }

    [RelayCommand]
    public async Task UpdateLocation()
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
            await AppShell.Current.DisplayAlert(ex.GetType().Name, ex.Message, "Close");
        }
    }
}