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

            var asleepRows = tracker.Project();
            Assert.Equal("Backgrounded", asleepRows[0].Title);
            var asleep = asleepRows.Single(row => row.Title == "Navigated to Home page");
            Assert.Contains("in progress", asleep.Detail);
            Assert.Contains("open 8h 2m", asleep.Detail);
            Assert.Contains("active 2m", asleep.Detail);

            tracker.Activate("Home", "//home");
            clock.Advance(TimeSpan.FromMinutes(3));

            var awake = tracker.Project().Single(row => row.Title == "Navigated to Home page");
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
            Assert.Equal("Navigated to Colors page", second.Project().First().Title);

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
            Assert.Equal("Navigated to Usage page", rows[0].Title);
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
                [
                    "Navigated to Usage page",
                    "Foregrounded",
                    "Backgrounded",
                    "Navigated to Colors page",
                    "Navigated to Home page",
                    "Session started",
                    "Installed"
                ],
                rows.Select(r => r.Title).ToArray());
            Assert.Equal([1, 2, 2, 1, 1, 0, 0], rows.Select(r => r.Depth).ToArray());
            Assert.Contains("in progress", rows[0].Detail);
            Assert.Equal("open 8h 5m · active 5m", rows[3].Detail);
            Assert.Equal("open 30s · active 30s", rows[4].Detail);
            Assert.Equal("", rows[1].Detail);
            Assert.Equal("", rows[5].Detail);
            Assert.Equal("", rows[^1].Detail);
            Assert.Equal(UsageCategory.Navigation, rows[0].Category);
            Assert.Equal(UsageCategory.Lifecycle, rows[1].Category);
            Assert.Equal(UsageTimeline.FormatWhen(UsageLab.Start, clock.UtcNow), rows[5].When);
            Assert.Equal(UsageTimeline.FormatWhen(UsageLab.Start.AddSeconds(30).AddMinutes(2).AddHours(8), clock.UtcNow), rows[1].When);
            Assert.Matches(@"\d:\d\d:\d\d", rows[1].When);
            Assert.False(rows[1].Rail.LineFromTop);
            Assert.True(rows[2].Rail.CurveToLane == 1);
            Assert.False(string.IsNullOrEmpty(rows[0].When));
            Assert.False(string.IsNullOrEmpty(rows[^1].When));
            Assert.Equal("Installed", rows[^1].Title);
        }
    }

    [Fact]
    public void PopupRoute_DoesNotSplitTheVisit()
    {
        var (tracker, _, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Menu Button", "//menubutton", "MenuButtonPage");
            tracker.Navigate(null, "D_FAULT_PopupPage45", "PopupPage");
            tracker.Activate(null, "//PopupPageabc", "PopupPage");

            Assert.Single(store.GetAll(), e => e.Kind == UsageEventKind.PageOpened);
            Assert.DoesNotContain(store.GetAll(), e => e.Kind == UsageEventKind.PageClosed);
            Assert.Equal("Navigated to Menu Button page", tracker.Project().First().Title);
        }
    }

    [Fact]
    public void WindowArea_RecordsStartSizeAndLaterChange()
    {
        var (tracker, clock, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Home", "//home");
            tracker.NoteArea(1024, 768, 1024, 768, 0, 0);
            tracker.NoteArea(1024.4, 768, 1024, 768, 0, 0);
            clock.Advance(TimeSpan.FromMinutes(1));
            tracker.NoteArea(1250, 750, 1440, 900, 40, 40);

            var areas = store.GetAll().Where(e => e.Kind == UsageEventKind.AreaChanged).ToList();
            Assert.Equal(2, areas.Count);
            Assert.Equal(1024, areas[0].Width);
            Assert.Equal(768, areas[0].Height);
            Assert.Equal("Full screen", areas[0].Mode);
            Assert.Equal("Windowed", areas[1].Mode);

            var rows = tracker.Project();
            Assert.Equal("Windowed", rows[0].Title);
            Assert.Equal("1250 × 750", rows[0].Detail);
            Assert.Equal(2, rows[0].Depth);
            Assert.Equal(UsageCategory.Screen, rows[0].Category);
            Assert.Equal("Full screen", rows[1].Title);
            Assert.Equal("1024 × 768", rows[1].Detail);
            Assert.Equal("Navigated to Home page", rows[2].Title);
            var pageId = store.GetAll().Single(e => e.Kind == UsageEventKind.PageOpened).PageVisitId;
            Assert.All(
                store.GetAll().Where(e => e.Kind == UsageEventKind.AreaChanged),
                areaEvent => Assert.Equal(pageId, areaEvent.PageVisitId));
        }
    }

    [Fact]
    public void AmbiguousCompactWidth_StoresSizeWithoutMode()
    {
        var (tracker, _, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Home", "//home");
            tracker.NoteArea(341, 800, 1024, 800, 0, 0);

            var area = Assert.Single(store.GetAll(), e => e.Kind == UsageEventKind.AreaChanged);
            Assert.Equal(341, area.Width);
            Assert.Equal(800, area.Height);
            Assert.Null(area.Mode);
            Assert.Equal(area.PageVisitId, store.GetAll().Single(e => e.Kind == UsageEventKind.PageOpened).PageVisitId);
            var row = tracker.Project().Single(item => item.Title == "Screen");
            Assert.Equal("341 × 800", row.Detail);
            Assert.Equal(2, row.Depth);
        }
    }

    [Fact]
    public void MenuChoice_NestsButtonMenuAndSelection()
    {
        var (tracker, _, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Menu Button", "//menubutton");
            tracker.BeginInteraction("Top Left", "Menu");
            tracker.NoteChoice("Mine");
            tracker.CompleteInteraction();

            var rows = tracker.Project();
            Assert.Equal(
                ["Mine", "Menu", "Top Left", "Navigated to Menu Button page", "Session started", "Installed"],
                rows.Select(row => row.Title).ToArray());
            Assert.Equal([4, 3, 2, 1, 0, 0], rows.Select(row => row.Depth).ToArray());
            Assert.Equal(UsageCategory.Action, rows[0].Category);
            Assert.All(rows.Take(3), row => Assert.Equal("", row.Detail));
        }
    }

    [Fact]
    public void PromptDismiss_RecordsDismissedUnderTheButton()
    {
        var (tracker, _, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Dialogs", "//dialogs");
            tracker.BeginInteraction("Show Prompt", "Prompt");
            tracker.CompleteInteraction();
            tracker.BeginInteraction("Show ActionSheet", "Action sheet");
            tracker.NoteChoice("Copy");
            tracker.CompleteInteraction();

            var rows = tracker.Project();
            Assert.Equal(
                [
                    "Copy",
                    "Action sheet",
                    "Show ActionSheet",
                    "Dismissed",
                    "Prompt",
                    "Show Prompt",
                    "Navigated to Dialogs page",
                    "Session started",
                    "Installed"
                ],
                rows.Select(row => row.Title).ToArray());
            Assert.Equal([4, 3, 2, 4, 3, 2, 1, 0, 0], rows.Select(row => row.Depth).ToArray());
        }
    }

    [Fact]
    public void Permission_NestsAboveTheOpenPage()
    {
        var (tracker, _, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Geolocation", "//geolocation");
            tracker.RecordPermission("Location", "Denied");

            var rows = tracker.Project();
            Assert.Equal("Location", rows[0].Title);
            Assert.Equal("Denied", rows[0].Detail);
            Assert.Equal(2, rows[0].Depth);
            Assert.Equal(UsageCategory.Permission, rows[0].Category);
            Assert.Equal("Navigated to Geolocation page", rows[1].Title);
        }
    }

    [Fact]
    public void NoteRelease_RecordsInstallTimeAndVersionChange()
    {
        var (tracker, clock, store) = UsageLab.Open();
        using (store)
        {
            var installed = UsageLab.Start.AddDays(-10);
            var updated = UsageLab.Start.AddDays(-2);
            tracker.NoteRelease("1.0 (1)", installed, updated);
            tracker.Activate("Home", "//home");
            tracker.NoteRelease("1.0 (1)", installed, updated);
            clock.Advance(TimeSpan.FromMinutes(1));
            tracker.NoteRelease("1.1 (2)", null, null);

            var events = store.GetAll();
            var install = Assert.Single(events, e => e.Kind == UsageEventKind.Installed);
            Assert.Equal(installed, install.Timestamp);
            Assert.Equal("1.0 (1)", install.Name);
            var updates = events.Where(e => e.Kind == UsageEventKind.Updated).ToList();
            Assert.Equal(2, updates.Count);
            Assert.Equal(updated, updates[0].Timestamp);
            Assert.Equal("1.1 (2)", updates[1].Name);

            var rows = tracker.Project();
            Assert.Equal("Updated", rows[0].Title);
            Assert.Equal("1.1 (2)", rows[0].Detail);
            Assert.Equal("Installed", rows[^1].Title);
            Assert.Equal("1.0 (1)", rows[^1].Detail);
        }
    }
}
