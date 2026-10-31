namespace MauiLab.Usage;

/// <summary>
/// Turns window and shell signals into a usage log.
/// A session stays open across backgrounding. Coming back is a foreground event, not a new session.
/// </summary>
public sealed class UsageTracker
{
    readonly IUsageStore _store;
    readonly TimeProvider _clock;
    readonly object _gate = new();
    readonly List<UsageEvent> _events = [];

    bool _loaded;
    bool _processSessionStarted;
    bool _foreground;
    Guid? _sessionId;
    OpenVisit? _open;
    (string? Name, string? Route)? _pending;
    Guid? _surfaceId;
    string? _pendingChoice;
    double? _areaWidth;
    double? _areaHeight;

    public UsageTracker(IUsageStore store, TimeProvider clock)
    {
        _store = store;
        _clock = clock;
    }

    public event EventHandler? Changed;

    public void Activate(string? pageName, string? route, string? pageType = null)
    {
        if (IsToolkitPopup(route, pageType))
        {
            pageName = null;
            route = null;
        }

        var changed = false;
        lock (_gate)
        {
            EnsureLoaded();
            var now = _clock.GetUtcNow();
            var name = Norm(pageName);
            var path = Norm(route);

            if (!_processSessionStarted)
            {
                ReconcileOrphans();
                if (_events.Count == 0)
                    Append(UsageEventKind.Installed, now, null, null, null, null, null, null, null);

                if (name == null && path == null && _pending is { } pending)
                {
                    name = pending.Name;
                    path = pending.Route;
                }

                _pending = null;
                BeginSession(now);
                OpenPage(now, name, path);
                changed = true;
            }
            else
            {
                if (!_foreground)
                {
                    _foreground = true;
                    if (_open != null)
                    {
                        Append(
                            UsageEventKind.Foregrounded,
                            now,
                            _sessionId,
                            _open.Id,
                            _open.Name,
                            _open.Route,
                            null,
                            null,
                            null);
                    }

                    changed = true;
                }

                if ((name != null || path != null) && !IsOpenPage(name, path))
                {
                    CloseOpenPage(now);
                    OpenPage(now, name, path);
                    changed = true;
                }
            }
        }

        if (changed)
            Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Deactivate and stop both call this. The second call, while already backgrounded, is ignored.
    /// </summary>
    public void Background()
    {
        var changed = false;
        lock (_gate)
        {
            if (!_processSessionStarted || !_foreground)
                return;

            _foreground = false;
            Append(
                UsageEventKind.Backgrounded,
                _clock.GetUtcNow(),
                _sessionId,
                _open?.Id,
                _open?.Name,
                _open?.Route,
                null,
                null,
                null);
            changed = true;
        }

        if (changed)
            Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Navigate(string? pageName, string? route, string? pageType = null)
    {
        if (IsToolkitPopup(route, pageType))
            return;

        var name = Norm(pageName);
        var path = Norm(route);
        if (name == null && path == null)
            return;

        var changed = false;
        lock (_gate)
        {
            EnsureLoaded();
            if (!_processSessionStarted)
            {
                _pending = (name, path);
                return;
            }

            if (IsOpenPage(name, path))
                return;

            var now = _clock.GetUtcNow();
            CloseOpenPage(now);
            OpenPage(now, name, path);
            changed = true;
        }

        if (changed)
            Changed?.Invoke(this, EventArgs.Empty);
    }

    public void NoteArea(double width, double height, double screenWidth, double screenHeight, double x, double y)
    {
        var changed = false;
        lock (_gate)
        {
            EnsureLoaded();
            if (!_processSessionStarted || _sessionId is not Guid sessionId)
                return;
            if (!double.IsFinite(width) || !double.IsFinite(height) || width < 1 || height < 1)
                return;
            if (_areaWidth is double previousWidth &&
                _areaHeight is double previousHeight &&
                Math.Abs(previousWidth - width) < 1 &&
                Math.Abs(previousHeight - height) < 1)
                return;

            _areaWidth = width;
            _areaHeight = height;
            Append(
                UsageEventKind.AreaChanged,
                _clock.GetUtcNow(),
                sessionId,
                _open?.Id,
                null,
                null,
                null,
                null,
                null,
                width: width,
                height: height,
                mode: UsageArea.Classify(width, height, screenWidth, screenHeight, x, y));
            changed = true;
        }

        if (changed)
            Changed?.Invoke(this, EventArgs.Empty);
    }

    public void BeginInteraction(string control, string surface)
    {
        var changed = false;
        lock (_gate)
        {
            EnsureLoaded();
            if (!_processSessionStarted || _open == null || _sessionId is not Guid sessionId)
                return;

            var now = _clock.GetUtcNow();
            FinishInteraction(now);
            var button = Append(
                UsageEventKind.ControlPressed,
                now,
                sessionId,
                _open.Id,
                Norm(control) ?? "Button",
                null,
                null,
                null,
                null);
            var opened = Append(
                UsageEventKind.SurfaceOpened,
                now,
                sessionId,
                _open.Id,
                Norm(surface) ?? "Menu",
                null,
                null,
                null,
                null,
                parentId: button.Id);
            _surfaceId = opened.Id;
            _pendingChoice = null;
            changed = true;
        }

        if (changed)
            Changed?.Invoke(this, EventArgs.Empty);
    }

    public void NoteChoice(string choice)
    {
        lock (_gate)
            _pendingChoice = choice;
    }

    public void CompleteInteraction()
    {
        var changed = false;
        lock (_gate)
        {
            if (_surfaceId == null)
                return;

            FinishInteraction(_clock.GetUtcNow());
            changed = true;
        }

        if (changed)
            Changed?.Invoke(this, EventArgs.Empty);
    }

    public void NoteRelease(string version, DateTimeOffset? firstInstall, DateTimeOffset? lastUpdate)
    {
        var changed = false;
        lock (_gate)
        {
            EnsureLoaded();
            var name = Norm(version);
            if (_events.Count == 0)
            {
                Append(
                    UsageEventKind.Installed,
                    firstInstall ?? _clock.GetUtcNow(),
                    null,
                    null,
                    name,
                    null,
                    null,
                    null,
                    null);
                changed = true;
            }

            if (lastUpdate is DateTimeOffset updated &&
                firstInstall is DateTimeOffset installed &&
                updated > installed.AddMinutes(1) &&
                !_events.Any(e => e.Kind == UsageEventKind.Updated && e.Timestamp == updated))
            {
                Append(UsageEventKind.Updated, updated, null, null, name, null, null, null, null);
                changed = true;
            }

            var recorded = _events
                .Where(e => (e.Kind == UsageEventKind.Installed || e.Kind == UsageEventKind.Updated) && e.Name != null)
                .OrderByDescending(e => e.Timestamp)
                .ThenByDescending(e => e.Sequence)
                .Select(e => e.Name)
                .FirstOrDefault();
            var versionChanged = recorded != null && recorded != name;
            var unnamedInstall = recorded == null && _events.Any(e => e.Kind == UsageEventKind.Installed);
            if (name != null && (versionChanged || unnamedInstall) &&
                !_events.Any(e => e.Kind == UsageEventKind.Updated && e.Name == name))
            {
                Append(UsageEventKind.Updated, _clock.GetUtcNow(), null, null, name, null, null, null, null);
                changed = true;
            }
        }

        if (changed)
            Changed?.Invoke(this, EventArgs.Empty);
    }

    public void RecordPermission(string name, string result)
    {
        var changed = false;
        lock (_gate)
        {
            EnsureLoaded();
            if (!_processSessionStarted || _sessionId is not Guid sessionId)
                return;

            Append(
                UsageEventKind.PermissionRequested,
                _clock.GetUtcNow(),
                sessionId,
                _open?.Id,
                Norm(name) ?? "Permission",
                Norm(result) ?? "Unknown",
                null,
                null,
                null);
            changed = true;
        }

        if (changed)
            Changed?.Invoke(this, EventArgs.Empty);
    }

    public void EndSession()
    {
        var changed = false;
        lock (_gate)
        {
            if (!_processSessionStarted || _sessionId is not Guid sessionId)
                return;

            var now = _clock.GetUtcNow();
            var start = _events.First(e => e.Kind == UsageEventKind.SessionStarted && e.SessionId == sessionId);
            CloseOpenPage(now);
            Append(
                UsageEventKind.SessionEnded,
                now,
                sessionId,
                null,
                null,
                null,
                UsageMath.Elapsed(start.Timestamp, now),
                SessionActive(sessionId),
                UsageEndReason.Clean);
            _processSessionStarted = false;
            _sessionId = null;
            _foreground = false;
            _open = null;
            changed = true;
        }

        if (changed)
            Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Reset(string? pageName, string? route)
    {
        lock (_gate)
        {
            EnsureLoaded();
            var now = _clock.GetUtcNow();
            _store.Clear();
            _events.Clear();
            _open = null;
            _sessionId = null;
            _pending = null;
            _surfaceId = null;
            _pendingChoice = null;
            _areaWidth = null;
            _areaHeight = null;
            _processSessionStarted = false;
            _foreground = false;
            Append(UsageEventKind.HistoryReset, now, null, null, null, null, null, null, null);
            BeginSession(now);
            OpenPage(now, Norm(pageName), Norm(route));
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>The clock the tracker stamps events with. Pair with <see cref="Snapshot"/> to project off-thread.</summary>
    public DateTimeOffset Now => _clock.GetUtcNow();

    /// <summary>A copy of every event, taken under the lock. Safe to project on another thread.</summary>
    public IReadOnlyList<UsageEvent> Snapshot()
    {
        lock (_gate)
        {
            EnsureLoaded();
            return _events.ToArray();
        }
    }

    public IReadOnlyList<TimelineRow> Project() => UsageTimeline.Project(Snapshot(), Now);

    void ReconcileOrphans()
    {
        var openSessions = _events
            .Where(e => e.Kind == UsageEventKind.SessionStarted && e.SessionId is Guid)
            .Where(start => !_events.Any(e => e.Kind == UsageEventKind.SessionEnded && e.SessionId == start.SessionId))
            .ToList();

        foreach (var start in openSessions)
        {
            var sessionId = start.SessionId!.Value;
            var ordered = _events
                .Where(e => e.SessionId == sessionId)
                .OrderBy(e => e.Timestamp)
                .ThenBy(e => e.Sequence)
                .ToList();
            var asOf = TerminationTime(ordered);
            CloseStoredPages(sessionId, asOf);
            Append(
                UsageEventKind.SessionEnded,
                asOf,
                sessionId,
                null,
                null,
                null,
                UsageMath.Elapsed(start.Timestamp, asOf),
                SessionActive(sessionId),
                UsageEndReason.Terminated);
        }
    }

    static DateTimeOffset TerminationTime(List<UsageEvent> ordered)
    {
        var signal = ordered.LastOrDefault(e => e.Kind is
            UsageEventKind.Backgrounded or
            UsageEventKind.Foregrounded or
            UsageEventKind.PageOpened or
            UsageEventKind.PageClosed or
            UsageEventKind.SessionStarted);

        if (signal?.Kind == UsageEventKind.Backgrounded)
            return signal.Timestamp;

        return ordered[^1].Timestamp;
    }

    void CloseStoredPages(Guid sessionId, DateTimeOffset asOf)
    {
        var opens = _events
            .Where(e => e.Kind == UsageEventKind.PageOpened && e.SessionId == sessionId)
            .ToList();

        foreach (var open in opens)
        {
            if (open.PageVisitId is not Guid visitId)
                continue;
            if (_events.Any(e => e.Kind == UsageEventKind.PageClosed && e.PageVisitId == visitId))
                continue;

            var lifecycle = _events.Where(e => e.PageVisitId == visitId);
            Append(
                UsageEventKind.PageClosed,
                asOf,
                sessionId,
                visitId,
                open.Name,
                open.Route,
                UsageMath.Elapsed(open.Timestamp, asOf),
                UsageMath.ActiveTime(open.Timestamp, asOf, lifecycle),
                null);
        }
    }

    void BeginSession(DateTimeOffset now)
    {
        _sessionId = Guid.NewGuid();
        _open = null;
        _surfaceId = null;
        _pendingChoice = null;
        _areaWidth = null;
        _areaHeight = null;
        _processSessionStarted = true;
        _foreground = true;
        Append(UsageEventKind.SessionStarted, now, _sessionId, null, null, null, null, null, null);
    }

    void OpenPage(DateTimeOffset now, string? name, string? route)
    {
        if (name == null && route == null)
            return;

        var id = Guid.NewGuid();
        _open = new OpenVisit(id, name, route, now);
        Append(UsageEventKind.PageOpened, now, _sessionId, id, name, route, null, null, null);
        if (!_foreground)
        {
            Append(UsageEventKind.Backgrounded, now, _sessionId, id, name, route, null, null, null);
        }
    }

    void CloseOpenPage(DateTimeOffset asOf)
    {
        if (_open is not OpenVisit page || _sessionId is not Guid sessionId)
            return;

        FinishInteraction(asOf);
        var lifecycle = _events.Where(e => e.PageVisitId == page.Id);
        Append(
            UsageEventKind.PageClosed,
            asOf,
            sessionId,
            page.Id,
            page.Name,
            page.Route,
            UsageMath.Elapsed(page.OpenedAt, asOf),
            UsageMath.ActiveTime(page.OpenedAt, asOf, lifecycle),
            null);
        _open = null;
    }

    TimeSpan SessionActive(Guid sessionId) =>
        _events
            .Where(e => e.Kind == UsageEventKind.PageClosed && e.SessionId == sessionId)
            .Aggregate(TimeSpan.Zero, (sum, e) => sum + (e.Active ?? TimeSpan.Zero));

    bool IsOpenPage(string? name, string? route)
    {
        if (_open == null)
            return false;
        if (name != null && _open.Name != null)
            return name == _open.Name;
        return name == _open.Name && route == _open.Route;
    }

    void EnsureLoaded()
    {
        if (_loaded)
            return;

        _events.AddRange(_store.GetAll());
        _loaded = true;
    }

    void FinishInteraction(DateTimeOffset now)
    {
        if (_surfaceId is not Guid surface || _sessionId is not Guid sessionId)
            return;

        Append(
            UsageEventKind.SelectionMade,
            now,
            sessionId,
            _open?.Id,
            _pendingChoice ?? "Dismissed",
            null,
            null,
            null,
            null,
            parentId: surface);
        _surfaceId = null;
        _pendingChoice = null;
    }

    UsageEvent Append(
        UsageEventKind kind,
        DateTimeOffset timestamp,
        Guid? sessionId,
        Guid? pageVisitId,
        string? name,
        string? route,
        TimeSpan? elapsed,
        TimeSpan? active,
        string? endReason,
        Guid? parentId = null,
        double? width = null,
        double? height = null,
        string? mode = null)
    {
        var stored = _store.Append(new UsageEvent(
            Guid.NewGuid(),
            Sequence: 0,
            kind,
            timestamp,
            sessionId,
            pageVisitId,
            name,
            route,
            elapsed,
            active,
            endReason,
            UploadedUtc: null,
            width,
            height,
            mode,
            parentId));
        _events.Add(stored);
        return stored;
    }

    static bool IsToolkitPopup(string? route, string? pageType)
    {
        if (pageType != null && pageType.StartsWith("PopupPage", StringComparison.Ordinal))
            return true;
        return route != null && route.Contains("PopupPage", StringComparison.OrdinalIgnoreCase);
    }

    static string? Norm(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    sealed record OpenVisit(Guid Id, string? Name, string? Route, DateTimeOffset OpenedAt);
}
