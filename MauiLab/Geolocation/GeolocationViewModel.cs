using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiLab.Usage;
using System.Diagnostics;

namespace MauiLab.Geolocation;

public partial class GeolocationViewModel : ObservableObject
{
    private readonly IGeolocation _service;
    private Location? _lastLocation;
    private bool _isListening;

    public Location? LastLocation
    {
        get => _lastLocation;
        set
        {
            if (SetProperty(ref _lastLocation, value))
            {
                OnPropertyChanged(nameof(HasLocation));
                OpenMapsCommand.NotifyCanExecuteChanged();
                Debug.WriteLine($"LastLocation Changed: {_lastLocation}");
            }
        }
    }

    public bool HasLocation => _lastLocation is not null;

    public bool IsListening
    {
        get => _isListening;
        private set
        {
            if (SetProperty(ref _isListening, value))
                OnPropertyChanged(nameof(ListeningStatus));
        }
    }

    public string ListeningStatus => IsListening ? "Listening" : "Stopped";

    public GeolocationViewModel(IGeolocation geolocation)
    {
        _service = geolocation;
        _isListening = geolocation.IsListeningForeground;
    }

    public bool CanStart() => !_service.IsListeningForeground;

    public bool CanStop() => _service.IsListeningForeground;

    public bool CanUpdate() => !_service.IsListeningForeground;

    public bool CanOpenMaps() => _lastLocation is not null;

    void RefreshCommands()
    {
        IsListening = _service.IsListeningForeground;
        StartListeningCommand.NotifyCanExecuteChanged();
        StopListeningCommand.NotifyCanExecuteChanged();
        UpdateLocationCommand.NotifyCanExecuteChanged();
    }

    private void OnListeningFailed(object? sender, GeolocationListeningFailedEventArgs e)
    {
        Debug.WriteLine($"ListeningFailed: {e.Error}");
        RefreshCommands();
    }

    private void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
    {
        LastLocation = e.Location;
    }

    [RelayCommand(AllowConcurrentExecutions = false, CanExecute = "CanStart")]
    public Task StartListening() => Track("Start Listening", "Location", StartListeningCore);

    async Task<string?> StartListeningCore()
    {
        try
        {
            if (!await EnsureLocationPermission())
                return null;

            LastLocation = await _service.GetLastKnownLocationAsync();

            if (_service.IsListeningForeground)
                return "Started";

            _service.LocationChanged += OnLocationChanged;
            _service.ListeningFailed += OnListeningFailed;

            var request = new GeolocationListeningRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(0.5));
            if (await _service.StartListeningForegroundAsync(request))
            {
                Debug.WriteLine("Started listening to geolocation");
                RefreshCommands();
                return "Started";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            await AppShell.Current.DisplayAlertAsync(ex.GetType().Name, ex.Message, "Close");
        }

        await StopListeningCore();
        return "Failed";
    }

    [RelayCommand(CanExecute = "CanStop")]
    public Task StopListening() => Track("Stop Listening", "Location", StopListeningCore);

    async Task<string?> StopListeningCore()
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
            RefreshCommands();
            return "Stopped";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            await AppShell.Current.DisplayAlertAsync(ex.GetType().Name, ex.Message, "Close");
            return "Failed";
        }
    }

    [RelayCommand(CanExecute = "CanUpdate")]
    public Task UpdateLocation() => Track("Update", "Location", UpdateLocationCore);

    [RelayCommand(CanExecute = "CanOpenMaps")]
    public Task OpenMaps() => Track("Open in Maps", "Maps", OpenMapsCore);

    async Task<string?> OpenMapsCore()
    {
        if (_lastLocation is not Location location)
            return null;

        try
        {
            await Map.Default.OpenAsync(location);
            return $"{location.Latitude:F4}, {location.Longitude:F4}";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            await AppShell.Current.DisplayAlertAsync(ex.GetType().Name, ex.Message, "Close");
            return "Failed";
        }
    }

    async Task<string?> UpdateLocationCore()
    {
        try
        {
            if (!await EnsureLocationPermission())
                return null;

            var location = await _service.GetLocationAsync();
            location ??= await _service.GetLastKnownLocationAsync();

            LastLocation = location;
            return location is null
                ? null
                : $"{location.Latitude:F4}, {location.Longitude:F4}";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            await AppShell.Current.DisplayAlertAsync(ex.GetType().Name, ex.Message, "Close");
            return "Failed";
        }
    }

    static async Task Track(string button, string surface, Func<Task<string?>> action)
    {
        var usage = UsageServices.Current;
        usage?.BeginInteraction(button, surface);
        try
        {
            var choice = await action();
            if (choice != null)
                usage?.NoteChoice(choice);
        }
        finally
        {
            usage?.CompleteInteraction();
        }
    }

    static async Task<bool> EnsureLocationPermission()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status == PermissionStatus.Granted)
                return true;

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            UsageServices.Current?.RecordPermission("Location", status.ToString());
            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Usage tracking: {ex}");
            return true;
        }
    }
}