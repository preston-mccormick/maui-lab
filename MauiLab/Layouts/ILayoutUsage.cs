namespace MauiLab.Layouts;

public interface ILayoutUsage
{
    void Record(string name, string? detail);
}

public sealed class NoLayoutUsage : ILayoutUsage
{
    public static NoLayoutUsage Instance { get; } = new();

    public void Record(string name, string? detail)
    {
    }
}

public sealed class ListLayoutUsage : ILayoutUsage
{
    public List<(string Name, string? Detail)> Actions { get; } = [];

    public void Record(string name, string? detail) => Actions.Add((name, detail));
}

public sealed class TrackerLayoutUsage : ILayoutUsage
{
    public void Record(string name, string? detail)
    {
        try
        {
            Usage.UsageServices.Current?.RecordAction(name, detail);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine(ex);
        }
    }
}
