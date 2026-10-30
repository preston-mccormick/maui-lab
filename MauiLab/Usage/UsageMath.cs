namespace MauiLab.Usage;

static class UsageMath
{
    public static TimeSpan Elapsed(DateTimeOffset openedAt, DateTimeOffset asOf)
    {
        var elapsed = asOf - openedAt;
        return elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
    }

    /// <summary>
    /// Foreground time from <paramref name="openedAt"/> through <paramref name="asOf"/>.
    /// A page starts in the foreground. Each background ends a slice; each foreground starts the next.
    /// </summary>
    public static TimeSpan ActiveTime(
        DateTimeOffset openedAt,
        DateTimeOffset asOf,
        IEnumerable<UsageEvent> lifecycle)
    {
        if (asOf <= openedAt)
            return TimeSpan.Zero;

        var active = TimeSpan.Zero;
        DateTimeOffset? slice = openedAt;
        foreach (var entry in lifecycle
                     .Where(e => e.Timestamp >= openedAt && e.Timestamp <= asOf)
                     .Where(e => e.Kind is UsageEventKind.Backgrounded or UsageEventKind.Foregrounded)
                     .OrderBy(e => e.Timestamp)
                     .ThenBy(e => e.Sequence))
        {
            if (entry.Kind == UsageEventKind.Backgrounded && slice is DateTimeOffset start)
            {
                active += entry.Timestamp - start;
                slice = null;
            }
            else if (entry.Kind == UsageEventKind.Foregrounded && slice is null)
            {
                slice = entry.Timestamp;
            }
        }

        if (slice is DateTimeOffset open && open < asOf)
            active += asOf - open;

        return active < TimeSpan.Zero ? TimeSpan.Zero : active;
    }
}
