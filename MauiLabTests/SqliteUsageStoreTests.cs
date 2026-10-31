using MauiLab.Usage;
using SQLite;

namespace MauiLabTests;

public class SqliteUsageStoreTests
{
    [Fact]
    public void InsertsComeBackInTimestampOrderWithUploadPending()
    {
        using var store = SqliteUsageStore.InMemory();
        var at = UsageLab.Start;
        var later = UsageLab.Event(at.AddHours(2));
        var earlier = UsageLab.Event(at);
        var middle = UsageLab.Event(at.AddHours(1));

        store.Append(later);
        store.Append(earlier);
        store.Append(middle);

        var all = store.GetAll();

        Assert.Equal(3, all.Count);
        Assert.Equal(earlier.Id, all[0].Id);
        Assert.Equal(middle.Id, all[1].Id);
        Assert.Equal(later.Id, all[2].Id);
        Assert.All(all, e => Assert.Null(e.UploadedUtc));
        Assert.Equal(2, all[0].Sequence);
        Assert.Equal(3, all[1].Sequence);
        Assert.Equal(1, all[2].Sequence);
    }

    [Fact]
    public void FileDatabase_SurvivesReopen()
    {
        var path = Path.Combine(Path.GetTempPath(), $"mauilab-usage-{Guid.NewGuid():N}.db");
        try
        {
            Guid id;
            using (var store = new SqliteUsageStore(path))
            {
                id = store.Append(UsageLab.Event(UsageLab.Start)).Id;
            }

            using var reopened = new SqliteUsageStore(path);
            var all = reopened.GetAll();
            var row = Assert.Single(all);
            Assert.Equal(id, row.Id);
            Assert.Null(row.UploadedUtc);
        }
        finally
        {
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                var file = path + suffix;
                if (File.Exists(file))
                    File.Delete(file);
            }
        }
    }

    [Fact]
    public void ExistingDatabase_GainsAreaColumns()
    {
        var path = Path.Combine(Path.GetTempPath(), $"mauilab-usage-{Guid.NewGuid():N}.db");
        try
        {
            SQLitePCL.Batteries_V2.Init();
            using (var db = new SQLiteConnection(path))
            {
                db.Execute("""
                    CREATE TABLE usage_event (
                        Id TEXT PRIMARY KEY,
                        Sequence INTEGER NOT NULL,
                        TimestampUtcTicks INTEGER NOT NULL,
                        Kind TEXT NOT NULL,
                        SessionId TEXT,
                        PageVisitId TEXT,
                        Name TEXT,
                        Route TEXT,
                        ElapsedMilliseconds INTEGER,
                        ActiveMilliseconds INTEGER,
                        EndReason TEXT,
                        UploadedUtcTicks INTEGER
                    );
                    """);
            }

            using var store = new SqliteUsageStore(path);
            var parent = Guid.NewGuid();
            store.Append(UsageLab.Event(UsageLab.Start) with
            {
                Width = 320,
                Height = 700,
                ParentId = parent
            });

            var read = Assert.Single(store.GetAll());
            Assert.Equal(320, read.Width);
            Assert.Equal(700, read.Height);
            Assert.Null(read.Mode);
            Assert.Equal(parent, read.ParentId);
        }
        finally
        {
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                var file = path + suffix;
                if (File.Exists(file))
                    File.Delete(file);
            }
        }
    }
}
