namespace MauiLab.Usage;

static class PlatformInstall
{
    public static (DateTimeOffset? FirstInstall, DateTimeOffset? LastUpdate) Read()
    {
#if ANDROID
        try
        {
            var context = Platform.AppContext;
            var packageName = context.PackageName;
            if (string.IsNullOrEmpty(packageName))
                return (null, null);

            var info = context.PackageManager?.GetPackageInfo(packageName, Android.Content.PM.PackageInfoFlags.MetaData);
            if (info == null)
                return (null, null);

            return (FromUnix(info.FirstInstallTime), FromUnix(info.LastUpdateTime));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Usage tracking: {ex}");
            return (null, null);
        }
#else
        return (null, null);
#endif
    }

#if ANDROID
    static DateTimeOffset? FromUnix(long millis)
    {
        if (millis <= 0)
            return null;

        try
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(millis);
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
#endif
}
