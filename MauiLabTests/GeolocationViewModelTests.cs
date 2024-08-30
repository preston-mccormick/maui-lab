using MauiLab.Geolocation;
using Xunit.Abstractions;

namespace MauiLabTests;

public class GeolocationViewModelTests(ITestOutputHelper output)
{
    GeolocationViewModel vm = new GeolocationViewModel(new MockGeolocation());

    [Fact]
    public async Task UpdateLocation()
    {
        await vm.UpdateLocation();
        Assert.NotNull(vm.LastLocation);
        Assert.NotNull(vm.LastLocation?.Latitude);
        Assert.NotNull(vm.LastLocation?.Longitude);
        output.WriteLine($"Current Location: Latitude: {vm.LastLocation?.Latitude}, Longitude: {vm.LastLocation?.Longitude}");
    }
}
