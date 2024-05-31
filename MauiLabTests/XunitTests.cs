using MauiLab.Geolocation;
using Xunit.Abstractions;

namespace MauiLabTests;

public class XunitTests(ITestOutputHelper output)
{
    [Fact]
    public void Output()
    {
        output.WriteLine($"Testing 123");
    }
}