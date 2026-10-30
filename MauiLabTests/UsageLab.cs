using MauiLab.Usage;

namespace MauiLabTests;

static class UsageLab
{
    public static readonly DateTimeOffset Start = new(2026, 10, 1, 18, 0, 0, TimeSpan.Zero);

    public static (UsageTracker Tracker, ManualTimeProvider Clock, SqliteUsageStore Store) Open()
    {
        var clock = new ManualTimeProvider(Start);
        var store = SqliteUsageStore.InMemory();
        return (new UsageTracker(store, clock), clock, store);
    }

    public static UsageEvent Event(DateTimeOffset timestamp, UsageEventKind kind = UsageEventKind.Backgrounded) =>
        new(
            Guid.NewGuid(),
            Sequence: 0,
            kind,
            timestamp,
            SessionId: null,
            PageVisitId: null,
            Name: null,
            Route: null,
            Elapsed: null,
            Active: null,
            EndReason: null,
            UploadedUtc: null);
}
