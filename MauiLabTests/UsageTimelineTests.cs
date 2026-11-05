using System.Diagnostics;
using MauiLab.Usage;

namespace MauiLabTests;

public class UsageTimelineTests
{
    [Fact]
    public void Project_MatchesTheReferenceOnALargeGeneratedHistory()
    {
        var (events, now) = GenerateHistory(sessions: 60, minimumEvents: 4000);
        Assert.True(events.Count >= 4000, $"Generated only {events.Count} events.");

        var expected = LegacyTimeline.Project(events, now);
        var actual = UsageTimeline.Project(events, now);

        AssertSameRows(expected, actual);
    }

    [Fact]
    public void Project_IsMuchCheaperThanTheReference()
    {
        var (events, now) = GenerateHistory(sessions: 60, minimumEvents: 4000);

        // Warm up both paths so JIT cost does not count.
        LegacyTimeline.Project(events, now);
        UsageTimeline.Project(events, now);

        var legacy = Stopwatch.StartNew();
        LegacyTimeline.Project(events, now);
        legacy.Stop();

        var current = Stopwatch.StartNew();
        UsageTimeline.Project(events, now);
        current.Stop();

        Assert.True(
            current.Elapsed < legacy.Elapsed,
            $"Expected the new projection to be faster: new {current.ElapsedMilliseconds} ms, legacy {legacy.ElapsedMilliseconds} ms.");
    }

    [Fact]
    public void Project_WithNoEvents_IsEmpty()
    {
        Assert.Empty(UsageTimeline.Project([], UsageLab.Start));
    }

    static void AssertSameRows(IReadOnlyList<TimelineRow> expected, IReadOnlyList<TimelineRow> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (var i = 0; i < expected.Count; i++)
        {
            var left = expected[i];
            var right = actual[i];
            Assert.Equal(left.Id, right.Id);
            Assert.Equal(left.Depth, right.Depth);
            Assert.Equal(left.Title, right.Title);
            Assert.Equal(left.Detail, right.Detail);
            Assert.Equal(left.Marker, right.Marker);
            Assert.Equal(left.When, right.When);
            Assert.Equal(left.Category, right.Category);
            Assert.Equal(left.Rail, right.Rail);
        }
    }

    /// <summary>
    /// Drives the real tracker through many sessions: navigation, sleep and wake, rotations,
    /// menu interactions, permissions, a version bump, terminated sessions, and a reset part way through.
    /// </summary>
    static (IReadOnlyList<UsageEvent> Events, DateTimeOffset Now) GenerateHistory(int sessions, int minimumEvents)
    {
        var random = new Random(20261031);
        var clock = new ManualTimeProvider(UsageLab.Start);
        using var store = SqliteUsageStore.InMemory();
        var pages = new[] { ("Home", "//home"), ("Colors", "//colors"), ("Dialogs", "//experiments/dialogs"), ("Usage", "//usage") };

        var tracker = new UsageTracker(store, clock);
        tracker.NoteRelease("1.0 (1)", null, null);

        for (var session = 0; session < sessions || store.GetAll().Count < minimumEvents; session++)
        {
            var (name, route) = pages[random.Next(pages.Length)];
            tracker.Activate(name, route);
            tracker.NoteArea(390, 844, 390, 844, 0, 0);

            var steps = random.Next(10, 40);
            for (var step = 0; step < steps; step++)
            {
                clock.Advance(TimeSpan.FromSeconds(random.Next(1, 600)));
                switch (random.Next(8))
                {
                    case 0:
                    case 1:
                        (name, route) = pages[random.Next(pages.Length)];
                        tracker.Navigate(name, route);
                        break;
                    case 2:
                        tracker.Background();
                        clock.Advance(TimeSpan.FromMinutes(random.Next(1, 300)));
                        tracker.Activate(name, route);
                        break;
                    case 3:
                        var portrait = random.Next(2) == 0;
                        tracker.NoteArea(portrait ? 390 : 844, portrait ? 844 : 390, 390, 844, 0, 0);
                        break;
                    case 4:
                        tracker.BeginInteraction("Options", "Menu");
                        if (random.Next(2) == 0)
                            tracker.NoteChoice("Choice " + random.Next(5));
                        tracker.CompleteInteraction();
                        break;
                    case 5:
                        tracker.RecordPermission("Location", random.Next(2) == 0 ? "Granted" : "Denied");
                        break;
                    case 6:
                        tracker.BeginInteraction("Options", "Menu");
                        // Left open; the next navigation or close finishes it.
                        break;
                    case 7:
                        clock.Advance(TimeSpan.FromHours(random.Next(1, 30)));
                        break;
                }
            }

            if (session == sessions / 3)
            {
                tracker.Reset(name, route);
                clock.Advance(TimeSpan.FromMinutes(5));
            }

            if (session == sessions / 2)
                tracker.NoteRelease("1.1 (2)", null, null);

            clock.Advance(TimeSpan.FromSeconds(random.Next(1, 120)));
            if (random.Next(4) == 0)
            {
                // Simulate the process being killed: a new tracker reconciles the orphan on the next launch.
                tracker.Background();
                clock.Advance(TimeSpan.FromHours(random.Next(1, 12)));
                tracker = new UsageTracker(store, clock);
            }
            else
            {
                tracker.EndSession();
                clock.Advance(TimeSpan.FromMinutes(random.Next(1, 600)));
            }
        }

        tracker.Activate("Usage", "//usage");
        clock.Advance(TimeSpan.FromSeconds(90));
        return (store.GetAll(), clock.GetUtcNow());
    }

    [Fact]
    public void LayoutAction_SitsUnderThePage()
    {
        var (tracker, _, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("Grid Layout", "//layouts/grid");
            tracker.RecordAction("Added row", "4 rows");
            tracker.RecordAction("Viewed Cross axis", null);

            var rows = tracker.Project();
            var page = rows.Single(row => row.Title == "Navigated to Grid Layout page");
            var added = rows.Single(row => row.Title == "Added row");
            var viewed = rows.Single(row => row.Title == "Viewed Cross axis");

            Assert.Equal(page.Depth + 1, added.Depth);
            Assert.Equal(2, added.Depth);
            Assert.Equal("4 rows", added.Detail);
            Assert.Equal(UsageCategory.Action, added.Category);
            Assert.Equal("", viewed.Detail);
            Assert.True(rows.ToList().IndexOf(viewed) < rows.ToList().IndexOf(added));
            Assert.True(rows.ToList().IndexOf(added) < rows.ToList().IndexOf(page));
        }
    }

    [Fact]
    public void CollectionAction_SitsUnderListView()
    {
        var (tracker, _, store) = UsageLab.Open();
        using (store)
        {
            tracker.Activate("List View", "//collections/listview");
            tracker.RecordAction("Deleted swatch", "AliceBlue");
            tracker.RecordAction("Viewed Swipe", null);

            var rows = tracker.Project();
            var page = rows.Single(row => row.Title == "Navigated to List View page");
            var deleted = rows.Single(row => row.Title == "Deleted swatch");
            var viewed = rows.Single(row => row.Title == "Viewed Swipe");

            Assert.Equal(page.Depth + 1, deleted.Depth);
            Assert.Equal(2, deleted.Depth);
            Assert.Equal("AliceBlue", deleted.Detail);
            Assert.Equal(UsageCategory.Action, deleted.Category);
            Assert.Equal("", viewed.Detail);
            Assert.True(rows.ToList().IndexOf(deleted) < rows.ToList().IndexOf(page));
        }
    }
}
