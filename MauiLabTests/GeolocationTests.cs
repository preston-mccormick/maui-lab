using MauiLab.Geolocation;
using Xunit.Abstractions;

namespace MauiLabTests;

public class GeolocationTests(ITestOutputHelper output)
{
    [Fact]
    public void ConstructViewModel()
    {
        var vm = new GeolocationViewModel();
        output.WriteLine($"Testing 123");
    }
}