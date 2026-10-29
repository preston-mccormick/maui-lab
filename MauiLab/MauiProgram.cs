using CommunityToolkit.Maui;
using DotNet.Meteor.HotReload.Plugin;
using MauiLab.Services;
using MauiLab.Services.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;

[assembly: InternalsVisibleTo("MauiLabTests")]
namespace MauiLab;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        AppDomain.CurrentDomain.FirstChanceException += HandleFirstChanceException;
        AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            // The toolkit API is supported only on OS targets. Plain net11.0 has no OS, so this call warns (CA1416) there.
#if HAS_TARGET_PLATFORM
            .UseMauiCommunityToolkit()
#endif
#if DEBUG
            .EnableHotReload()
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        RegisterServices(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IGeolocation>(
            Microsoft.Maui.Devices.Sensors.Geolocation.Default);

        services.AddSingleton<ILog, DebugLogger>();
    }

    private static void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Debug.WriteLine($"UnhandledException: {ex.Message}");
        }
    }

    private static void HandleFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
    {
        Debug.WriteLine($"FirstChanceException: {e.Exception.Message}");
    }
}