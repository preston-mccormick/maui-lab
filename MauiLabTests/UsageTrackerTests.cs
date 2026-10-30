using MauiLab.Usage;

namespace MauiLabTests;

public class UsageTrackerTests
{
    [Fact]
    public void SleepOnPage_RecordsElapsedAndActiveSeparately()
    {
        var (tracker, clock, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Home", "//home");
            clock.Advance(TimeSpan.FromMinutes(2));
            tracker.Background();
            clock.Advance(TimeSpan.FromHours(8));

            var asleep = tracker.Project().First();
            Assert.Equal("Home", asleep.Title);
            Assert.Contains("in progress", asleep.Detail);
            Assert.Contains("open 8h 2m", asleep.Detail);
            Assert.Contains("active 2m", asleep.Detail);

            tracker.Activate("Home", "//home");
            clock.Advance(TimeSpan.FromMinutes(3));

            var awake = tracker.Project().First();
            Assert.Equal("Home", awake.Title);
            Assert.Contains("in progress", awake.Detail);
            Assert.Contains("open 8h 5m", awake.Detail);
            Assert.Contains("active 5m", awake.Detail);

            tracker.Navigate("Colors", "//colors");

            var closed = Assert.Single(store.GetAll(), e => e.Kind == UsageEventKind.PageClosed && e.Name == "Home");
            Assert.Equal(TimeSpan.FromHours(8) + TimeSpan.FromMinutes(5), closed.Elapsed);
            Assert.Equal(TimeSpan.FromMinutes(5), closed.Active);
        }
    }

    [Fact]
    public void RepeatedSleepCycles_SumOnlyForegroundSlices()
    {
        var (tracker, clock, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Home", "//home");
            clock.Advance(TimeSpan.FromMinutes(2));
            tracker.Background();
            clock.Advance(TimeSpan.FromHours(1));
            tracker.Activate("Home", "//home");
            clock.Advance(TimeSpan.FromMinutes(2));
            tracker.Background();
            clock.Advance(TimeSpan.FromHours(1));
            tracker.Activate("Home", "//home");
            clock.Advance(TimeSpan.FromMinutes(1));
            tracker.Navigate("Colors", "//colors");

            var closed = Assert.Single(store.GetAll(), e => e.Kind == UsageEventKind.PageClosed && e.Name == "Home");
            Assert.Equal(TimeSpan.FromHours(2) + TimeSpan.FromMinutes(5), closed.Elapsed);
            Assert.Equal(TimeSpan.FromMinutes(5), closed.Active);
            Assert.Equal(2, store.GetAll().Count(e => e.Kind == UsageEventKind.Backgrounded));
            Assert.Equal(2, store.GetAll().Count(e => e.Kind == UsageEventKind.Foregrounded));
        }
    }

    [Fact]
    public void StayingForeground_ActiveEqualsElapsed()
    {
        var (tracker, clock, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Home", "//home");
            clock.Advance(TimeSpan.FromMinutes(4));
            tracker.Navigate("Colors", "//colors");

            var closed = Assert.Single(store.GetAll(), e => e.Kind == UsageEventKind.PageClosed);
            Assert.Equal(TimeSpan.FromMinutes(4), closed.Elapsed);
            Assert.Equal(closed.Elapsed, closed.Active);
        }
    }

    [Fact]
    public void RepeatedBackground_RecordsOneEvent()
    {
        var (tracker, _, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Home", "//home");
            tracker.Background();
            tracker.Background();

            Assert.Single(store.GetAll(), e => e.Kind == UsageEventKind.Backgrounded);
        }
    }

    [Fact]
    public void ColdStart_DoesNotRecordForegrounded()
    {
        var (tracker, _, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Home", "//home");
            tracker.Activate("Home", "//home");

            var events = store.GetAll();
            Assert.Single(events, e => e.Kind == UsageEventKind.Installed);
            Assert.Single(events, e => e.Kind == UsageEventKind.SessionStarted);
            Assert.Single(events, e => e.Kind == UsageEventKind.PageOpened);
            Assert.DoesNotContain(events, e => e.Kind == UsageEventKind.Foregrounded);
        }
    }

    [Fact]
    public void Resume_RecordsForegroundWithoutNewSession()
    {
        var (tracker, _, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Home", "//home");
            tracker.Background();
            tracker.Activate("Home", "//home");

            var events = store.GetAll();
            Assert.Single(events, e => e.Kind == UsageEventKind.SessionStarted);
            Assert.Single(events, e => e.Kind == UsageEventKind.Foregrounded);
            Assert.Single(events, e => e.Kind == UsageEventKind.PageOpened);
        }
    }

    [Fact]
    public void KilledWhileBackgrounded_ClosesAtBackgroundTime()
    {
        var (first, clock, store) = UsageLab.Open();
        using (store)
        {
            first.Activate("Home", "//home");
            clock.Advance(TimeSpan.FromMinutes(2));
            first.Background();
            var backgroundAt = clock.UtcNow;
            clock.Advance(TimeSpan.FromHours(8));

            var second = new UsageTracker(store, clock);
            second.Activate("Colors", "//colors");

            var events = store.GetAll();
            var closed = Assert.Single(events, e => e.Kind == UsageEventKind.PageClosed);
            Assert.Equal("Home", closed.Name);
            Assert.Equal(backgroundAt, closed.Timestamp);
            Assert.Equal(TimeSpan.FromMinutes(2), closed.Elapsed);
            Assert.Equal(TimeSpan.FromMinutes(2), closed.Active);

            var ended = Assert.Single(events, e => e.Kind == UsageEventKind.SessionEnded);
            Assert.Equal(UsageEndReason.Terminated, ended.EndReason);
            Assert.Equal(backgroundAt, ended.Timestamp);

            Assert.Equal(2, events.Count(e => e.Kind == UsageEventKind.SessionStarted));
            Assert.Single(events, e => e.Kind == UsageEventKind.Installed);
            Assert.Equal("Colors", second.Project().First().Title);

            var rows = second.Project();
            Assert.Contains(rows, r => r.Title == "Session started" && r.Detail.Contains("terminated"));
            Assert.Contains(rows, r => r.Title == "Session started" && !r.Detail.Contains("terminated"));
        }
    }

    [Fact]
    public void ClosingTheWindow_EndsTheSessionCleanly()
    {
        var (tracker, clock, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Home", "//home");
            clock.Advance(TimeSpan.FromMinutes(1));
            tracker.EndSession();
            tracker.Activate("Home", "//home");

            var events = store.GetAll();
            var ended = Assert.Single(events, e => e.Kind == UsageEventKind.SessionEnded);
            Assert.Equal(UsageEndReason.Clean, ended.EndReason);
            Assert.Equal(TimeSpan.FromMinutes(1), ended.Elapsed);
            Assert.Equal(TimeSpan.FromMinutes(1), ended.Active);
            Assert.Single(events, e => e.Kind == UsageEventKind.Installed);
            Assert.Equal(2, events.Count(e => e.Kind == UsageEventKind.SessionStarted));
        }
    }

    [Fact]
    public void Reset_ReplacesHistoryWithResetRoot()
    {
        var (tracker, _, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Home", "//home");
            tracker.Navigate("Colors", "//colors");
            tracker.Reset("Usage", "//usage");

            var events = store.GetAll();
            Assert.Equal(UsageEventKind.HistoryReset, events[0].Kind);
            Assert.DoesNotContain(events, e => e.Kind == UsageEventKind.Installed);
            Assert.DoesNotContain(events, e => e.Name == "Colors" || e.Name == "Home");

            var rows = tracker.Project();
            Assert.Equal("Usage", rows[0].Title);
            Assert.Equal(1, rows[0].Depth);
            Assert.Equal("Open", rows[0].Marker);
            Assert.Equal("Session started", rows[1].Title);
            Assert.Equal("History reset", rows[^1].Title);
            Assert.Equal(0, rows[^1].Depth);
            Assert.Equal("Root", rows[^1].Marker);
        }
    }

    [Fact]
    public void NavigatingToTheSamePage_DoesNotSplitTheVisit()
    {
        var (tracker, _, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Home", null);
            tracker.Navigate("Home", "//home");

            Assert.Single(store.GetAll(), e => e.Kind == UsageEventKind.PageOpened);
            Assert.DoesNotContain(store.GetAll(), e => e.Kind == UsageEventKind.PageClosed);
        }
    }

    [Fact]
    public void Timeline_IsNewestFirstAndNested()
    {
        var (tracker, clock, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Home", "//home");
            clock.Advance(TimeSpan.FromSeconds(30));
            tracker.Navigate("Colors", "//colors");
            clock.Advance(TimeSpan.FromMinutes(2));
            tracker.Background();
            clock.Advance(TimeSpan.FromHours(8));
            tracker.Activate("Colors", "//colors");
            clock.Advance(TimeSpan.FromMinutes(3));
            tracker.Navigate("Usage", "//usage");

            var rows = tracker.Project();
            Assert.Equal(
                ["Usage", "Colors", "Foregrounded", "Backgrounded", "Home", "Session started", "Installed"],
                rows.Select(r => r.Title).ToArray());
            Assert.Equal([1, 1, 2, 2, 1, 0, 0], rows.Select(r => r.Depth).ToArray());
            Assert.Contains("in progress", rows[0].Detail);
            Assert.Equal("open 8h 5m · active 5m", rows[1].Detail);
            Assert.Equal("open 30s · active 30s", rows[4].Detail);
            Assert.Equal("Installed", rows[^1].Title);
        }
    }
}
