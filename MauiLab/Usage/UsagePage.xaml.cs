using Microsoft.Extensions.DependencyInjection;

namespace MauiLab.Usage;

public partial class UsagePage : ContentPage
{
    public UsagePage()
    {
        InitializeComponent();
        BindingContext = Services.GetRequiredService<UsagePageModel>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (Application.Current is Application app)
        {
            app.RequestedThemeChanged -= OnThemeChanged;
            app.RequestedThemeChanged += OnThemeChanged;
        }

        // Shell updates CurrentState before the page appears but raises Navigated only after it
        // has, so App's Navigated handler would add this page's own row once the list is already
        // on screen. Record the visit here first; the later Navigated call sees the page open and
        // does nothing.
        UsageServices.Current?.Navigate(Title, Shell.Current?.CurrentState?.Location.ToString(), GetType().Name);

        if (BindingContext is UsagePageModel model)
            model.StartClock(Dispatcher);
    }

    protected override void OnDisappearing()
    {
        if (Application.Current is Application app)
            app.RequestedThemeChanged -= OnThemeChanged;
        if (BindingContext is UsagePageModel model)
            model.StopClock();
        base.OnDisappearing();
    }

    /// <summary>Row colours bind once per cell, so a theme change rebuilds the cells.</summary>
    void OnThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        if (BindingContext is UsagePageModel model)
            model.RebuildCells();
    }

    static IServiceProvider Services =>
        IPlatformApplication.Current?.Services
        ?? Application.Current?.Handler?.MauiContext?.Services
        ?? throw new InvalidOperationException("Application services are not available.");
}
