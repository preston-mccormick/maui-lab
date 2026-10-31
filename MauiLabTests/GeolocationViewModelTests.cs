using MauiLab.Geolocation;
using Xunit.Abstractions;

namespace MauiLabTests;

public class GeolocationViewModelTests(ITestOutputHelper output)
{
    readonly GeolocationViewModel vm = new(new MockGeolocation());

    [Fact]
    public async Task UpdateLocation()
    {
        await vm.UpdateLocation();
        Assert.NotNull(vm.LastLocation);
        Assert.NotNull(vm.LastLocation?.Latitude);
        Assert.NotNull(vm.LastLocation?.Longitude);
        output.WriteLine($"Current Location: Latitude: {vm.LastLocation?.Latitude}, Longitude: {vm.LastLocation?.Longitude}");
    }

    [Fact]
    public async Task UpdateEnabledOnlyWhenStopped()
    {
        Assert.True(vm.UpdateLocationCommand.CanExecute(null));

        await vm.StartListening();
        Assert.False(vm.UpdateLocationCommand.CanExecute(null));

        await vm.StopListening();
        Assert.True(vm.UpdateLocationCommand.CanExecute(null));
    }

    [Fact]
    public async Task OpenMapsEnabledWhenLocationIsKnown()
    {
        Assert.False(vm.OpenMapsCommand.CanExecute(null));

        await vm.UpdateLocation();

        Assert.True(vm.OpenMapsCommand.CanExecute(null));
    }
}
