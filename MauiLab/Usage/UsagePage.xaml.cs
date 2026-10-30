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
        if (BindingContext is UsagePageModel model)
            model.StartClock(Dispatcher);
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is UsagePageModel model)
            model.StopClock();
        base.OnDisappearing();
    }

    static IServiceProvider Services =>
        IPlatformApplication.Current?.Services
        ?? Application.Current?.Handler?.MauiContext?.Services
        ?? throw new InvalidOperationException("Application services are not available.");
}
