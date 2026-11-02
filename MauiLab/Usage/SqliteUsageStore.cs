using SQLite;

namespace MauiLab.Usage;

public sealed class SqliteUsageStore : IUsageStore, IDisposable
{
    static readonly object BatteryGate = new();
    static bool _batteriesReady;

    readonly Func<string> _pathFactory;
    readonly object _gate = new();
    SQLiteConnection? _connection;
    bool _memory;

    public SqliteUsageStore(string databasePath)
        : this(() => databasePath)
    {
    }

    public SqliteUsageStore(Func<string> pathFactory) => _pathFactory = pathFactory;

    public static SqliteUsageStore InMemory() => new(":memory:");

    public UsageEvent Append(UsageEvent usageEvent)
    {
        lock (_gate)
        {
            var db = Database();
            var sequence = db.ExecuteScalar<long>("SELECT IFNULL(MAX(Sequence), 0) FROM usage_event") + 1;
            var stored = usageEvent with { Sequence = sequence, UploadedUtc = null };
            db.Insert(UsageEventRow.From(stored));
            return stored;
        }
    }

    public IReadOnlyList<UsageEvent> GetAll()
    {
        lock (_gate)
        {
            return Database()
                .Query<UsageEventRow>("SELECT * FROM usage_event ORDER BY TimestampUtcTicks, Sequence")
                .Select(row => row.ToEvent())
                .ToList();
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            Database().DeleteAll<UsageEventRow>();
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            _connection?.Dispose();
            _connection = null;
        }
    }

    SQLiteConnection Database()
    {
        if (_connection != null)
            return _connection;

        EnsureBatteries();
        var path = _pathFactory();
        _memory = string.Equals(path, ":memory:", StringComparison.Ordinal);
        var flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex;
        _connection = new SQLiteConnection(path, flags);
        if (!_memory)
        {
            _connection.ExecuteScalar<string>("PRAGMA journal_mode=WAL;");
            _connection.ExecuteScalar<int>("PRAGMA busy_timeout=2000;");
        }

        _connection.CreateTable<UsageEventRow>();
        EnsureColumns(_connection);
        _connection.Execute(
            "CREATE INDEX IF NOT EXISTS ix_usage_event_timestamp ON usage_event (TimestampUtcTicks, Sequence);");
        _connection.Execute(
            "CREATE INDEX IF NOT EXISTS ix_usage_event_pending ON usage_event (UploadedUtcTicks);");
        return _connection;
    }

    static void EnsureColumns(SQLiteConnection db)
    {
        var names = db.GetTableInfo("usage_event")
            .Select(column => column.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        void Add(string name, string type)
        {
            if (!names.Contains(name))
                db.Execute($"ALTER TABLE usage_event ADD COLUMN {name} {type}");
        }

        Add("Width", "REAL");
        Add("Height", "REAL");
        Add("Mode", "TEXT");
        Add("ParentId", "TEXT");
        Add("Detail", "TEXT");
    }

    static void EnsureBatteries()
    {
        if (_batteriesReady)
            return;

        lock (BatteryGate)
        {
            if (_batteriesReady)
                return;

            SQLitePCL.Batteries_V2.Init();
            _batteriesReady = true;
        }
    }
}

[Table("usage_event")]
public sealed class UsageEventRow
{
    [PrimaryKey]
    public string Id { get; set; } = "";

    public long Sequence { get; set; }

    public long TimestampUtcTicks { get; set; }

    public string Kind { get; set; } = "";

    public string? SessionId { get; set; }

    public string? PageVisitId { get; set; }

    public string? Name { get; set; }

    public string? Route { get; set; }

    public long? ElapsedMilliseconds { get; set; }

    public long? ActiveMilliseconds { get; set; }

    public string? EndReason { get; set; }

    public long? UploadedUtcTicks { get; set; }

    public double? Width { get; set; }

    public double? Height { get; set; }

    public string? Mode { get; set; }

    public string? ParentId { get; set; }

    public string? Detail { get; set; }

    public static UsageEventRow From(UsageEvent usageEvent) => new()
    {
        Id = usageEvent.Id.ToString("D"),
        Sequence = usageEvent.Sequence,
        TimestampUtcTicks = usageEvent.Timestamp.ToUniversalTime().UtcTicks,
        Kind = usageEvent.Kind.ToString(),
        SessionId = usageEvent.SessionId?.ToString("D"),
        PageVisitId = usageEvent.PageVisitId?.ToString("D"),
        Name = usageEvent.Name,
        Route = usageEvent.Route,
        ElapsedMilliseconds = Milliseconds(usageEvent.Elapsed),
        ActiveMilliseconds = Milliseconds(usageEvent.Active),
        EndReason = usageEvent.EndReason,
        UploadedUtcTicks = usageEvent.UploadedUtc?.ToUniversalTime().UtcTicks,
        Width = usageEvent.Width,
        Height = usageEvent.Height,
        Mode = usageEvent.Mode,
        ParentId = usageEvent.ParentId?.ToString("D"),
        Detail = usageEvent.Detail
    };

    public UsageEvent ToEvent() => new(
        Guid.Parse(Id),
        Sequence,
        Enum.Parse<UsageEventKind>(Kind),
        new DateTimeOffset(TimestampUtcTicks, TimeSpan.Zero),
        SessionId is null ? null : Guid.Parse(SessionId),
        PageVisitId is null ? null : Guid.Parse(PageVisitId),
        Name,
        Route,
        Span(ElapsedMilliseconds),
        Span(ActiveMilliseconds),
        EndReason,
        UploadedUtcTicks is long ticks ? new DateTimeOffset(ticks, TimeSpan.Zero) : null,
        Width,
        Height,
        Mode,
        ParentId is null ? null : Guid.Parse(ParentId),
        Detail);

    static long? Milliseconds(TimeSpan? span) =>
        span is TimeSpan value ? value.Ticks / TimeSpan.TicksPerMillisecond : null;

    static TimeSpan? Span(long? milliseconds) =>
        milliseconds is long value ? TimeSpan.FromMilliseconds(value) : null;
}
