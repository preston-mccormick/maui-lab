namespace MauiLab.Usage;

public interface IUsageStore
{
    /// <summary>Inserts one row and returns it with the store-assigned sequence.</summary>
    UsageEvent Append(UsageEvent usageEvent);

    /// <summary>All rows, oldest first.</summary>
    IReadOnlyList<UsageEvent> GetAll();

    void Clear();
}
