using Microsoft.Extensions.DependencyInjection;

namespace MauiLab.Usage;

static class UsageServices
{
    public static UsageTracker? Current =>
        IPlatformApplication.Current?.Services.GetService<UsageTracker>();
}
