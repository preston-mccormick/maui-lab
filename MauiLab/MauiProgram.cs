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

    private static void HandleFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
    {
        try
        {
            string errorType = e.Exception.GetType().Name;
            Debug.WriteLine($"{errorType}: {e.Exception.Message}");
            Debug.WriteLine(e.Exception);

            if (App.Current == null)
            {
                MauiApp.CreateBuilder().Build();
                if (App.Current is Application app)
                {
                    app.MainPage = new ContentPage()
                    {
                        BackgroundColor = Microsoft.Maui.Graphics.Colors.Pink
                    };
                }
            }

            if (App.Current?.MainPage is Page mainPage)
            {
                StringBuilder sb = new(e.Exception.Message);
                sb.AppendLine();
                sb.Append("The appliaction will now close.");
                mainPage.DisplayAlert($"Error: {errorType}", sb.ToString(), "Close App");
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in the error handler!");
            Debug.WriteLine(ex);
        }
    }
}