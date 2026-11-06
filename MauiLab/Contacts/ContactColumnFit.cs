namespace MauiLab.Contacts;

public static class ContactColumnFit
{
    public const double MinimumCardWidth = 300;
    public const double Gap = 16;

    public static int Columns(double width)
    {
        if (width <= 0)
            return 1;

        return Math.Max(1, (int)Math.Floor((width + Gap) / (MinimumCardWidth + Gap)));
    }
}
