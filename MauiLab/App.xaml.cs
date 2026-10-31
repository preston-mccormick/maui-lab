using System.Diagnostics;
using MauiLab.Usage;

namespace MauiLab;

public partial class App : Application
{
    readonly UsageTracker _usage;

    public App(UsageTracker usage)
    {
        _usage = usage;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell())
        {
            Width = 1250,
            Height = 750
        };

        window.Activated += (_, _) => Safe(() => OnActive(window));
        window.Resumed += (_, _) => Safe(() => OnActive(window));
        window.Deactivated += (_, _) => Safe(_usage.Background);
        window.Stopped += (_, _) => Safe(_usage.Background);
        window.Destroying += (_, _) => Safe(_usage.EndSession);
        window.SizeChanged += (_, _) => Safe(() => RecordArea(window));

        if (window.Page is Shell shell)
        {
            shell.Navigated += (_, _) => Safe(() =>
            {
                var page = PageOf(window);
                _usage.Navigate(page.Name, page.Route, page.TypeName);
            });
        }

        return window;
    }

    void OnActive(Window window)
    {
        var page = PageOf(window);
        _usage.Activate(page.Name, page.Route, page.TypeName);
        RecordArea(window);
    }

    void RecordArea(Window window)
    {
        var display = DeviceDisplay.MainDisplayInfo;
        var density = display.Density <= 0 ? 1 : display.Density;
        _usage.NoteArea(
            window.Width,
            window.Height,
            display.Width / density,
            display.Height / density,
            window.X,
            window.Y);
    }

    static (string? Name, string? Route, string? TypeName) PageOf(Window window)
    {
        if (window.Page is Shell shell)
            return (shell.CurrentPage?.Title, shell.CurrentState?.Location.ToString(), shell.CurrentPage?.GetType().Name);
        return (window.Page?.Title, null, window.Page?.GetType().Name);
    }

    static void Safe(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Usage tracking: {ex}");
        }
    }
}
