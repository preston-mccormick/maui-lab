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

        window.Activated += (_, _) => Safe(() =>
        {
            var (name, route) = PageOf(window);
            _usage.Activate(name, route);
        });
        window.Resumed += (_, _) => Safe(() =>
        {
            var (name, route) = PageOf(window);
            _usage.Activate(name, route);
        });
        window.Deactivated += (_, _) => Safe(_usage.Background);
        window.Stopped += (_, _) => Safe(_usage.Background);
        window.Destroying += (_, _) => Safe(_usage.EndSession);

        if (window.Page is Shell shell)
        {
            shell.Navigated += (_, _) => Safe(() =>
                _usage.Navigate(shell.CurrentPage?.Title, shell.CurrentState?.Location.ToString()));
        }

        return window;
    }

    static (string? Name, string? Route) PageOf(Window window)
    {
        if (window.Page is Shell shell)
            return (shell.CurrentPage?.Title, shell.CurrentState?.Location.ToString());
        return (window.Page?.Title, null);
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
