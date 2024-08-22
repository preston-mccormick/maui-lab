using CommunityToolkit.Maui;
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
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
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