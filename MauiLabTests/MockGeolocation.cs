namespace MauiLabTests;

public class MockGeolocation : IGeolocation
{
    public bool IsListeningForeground { get; private set; } = false;

    public bool IsEnabled => true;

    // Required by IGeolocation. This mock never raises them.
#pragma warning disable CS0067
    public event EventHandler<GeolocationLocationChangedEventArgs>? LocationChanged;
    public event EventHandler<GeolocationListeningFailedEventArgs>? ListeningFailed;
#pragma warning restore CS0067

    public Task<Location?> GetLastKnownLocationAsync()
    {
        return Task.FromResult((Location?)new Location(47.673988, -122.121513));
    }

    public Task<Location?> GetLocationAsync(GeolocationRequest request, CancellationToken cancelToken)
    {
        return Task.FromResult((Location?)new Location(47.673988, -122.121513));
    }

    public Task<bool> StartListeningForegroundAsync(GeolocationListeningRequest request)
    {
        IsListeningForeground = true;
        return Task.FromResult(true);
    }

    public void StopListeningForeground()
    {
        IsListeningForeground = false;
        return;
    }
}