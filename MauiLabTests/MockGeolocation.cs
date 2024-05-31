namespace MauiLabTests;

public class MockGeolocation : IGeolocation
{
    public bool IsListeningForeground { get; private set; } = false;

    public event EventHandler<GeolocationLocationChangedEventArgs>? LocationChanged;
    public event EventHandler<GeolocationListeningFailedEventArgs>? ListeningFailed;

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