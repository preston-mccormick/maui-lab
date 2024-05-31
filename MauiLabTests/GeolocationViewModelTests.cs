using MauiLab.Geolocation;
using Xunit.Abstractions;

namespace MauiLabTests;

public class GeolocationViewModelTests(ITestOutputHelper output)
{
    GeolocationViewModel vm = new GeolocationViewModel(new MockGeolocation());

    [Fact]
    public async Task CurrentLocation()
    {
        await vm.Init();
        Assert.NotNull(vm.CurrentLocation);
        Assert.NotNull(vm.CurrentLocation?.Latitude);
        Assert.NotNull(vm.CurrentLocation?.Longitude);
        output.WriteLine($"Current Location: Latitude: {vm.CurrentLocation?.Latitude}, Longitude: {vm.CurrentLocation?.Longitude}");
    }
}
