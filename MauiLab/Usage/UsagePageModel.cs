using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiLab.Usage;

public partial class UsagePageModel : ObservableObject
{
    readonly UsageTracker _tracker;
    IDispatcher? _dispatcher;
    IDispatcherTimer? _timer;
    bool _refreshing;
    bool _refreshQueued;

    public UsagePageModel(UsageTracker tracker)
    {
        _tracker = tracker;
        _tracker.Changed += OnChanged;
    }

    /// <summary>
    /// Swapped wholesale only on first load and after a reset. Otherwise patched in place by
    /// <see cref="TimelineRowDiff"/> so the list keeps its cells.
    /// </summary>
    [ObservableProperty]
    public partial ObservableCollection<TimelineRow> Rows { get; set; } = [];

    public void StartClock(IDispatcher dispatcher)
    {
        StopClock();
        _dispatcher = dispatcher;

        // On appearing, replace the list in one step rather than diffing. The model outlives the
        // page, so Rows still holds the last visit; diffing would insert everything that happened
        // since then at the top, one animated insert at a time. A fresh collection raises a single
        // reset and the list draws current. The projection is linear, so inline costs milliseconds.
        // Refreshes while the page is visible still go through RequestRefresh and patch in place.
        Rows = new ObservableCollection<TimelineRow>(_tracker.Project());

        _timer = dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.IsRepeating = true;
        _timer.Tick += OnTick;
        _timer.Start();
    }

    public void StopClock()
    {
        _dispatcher = null;
        if (_timer == null)
            return;

        _timer.Tick -= OnTick;
        _timer.Stop();
        _timer = null;
    }

    /// <summary>
    /// Rebuilds every cell in one reset. Used when the theme changes, because the row
    /// template binds its colours once per cell.
    /// </summary>
    public void RebuildCells() => Rows = new ObservableCollection<TimelineRow>(Rows);

    [RelayCommand]
    async Task Reset()
    {
        var shell = Shell.Current;
        if (shell == null)
            return;

        var confirm = await shell.DisplayAlertAsync(
            "Reset history",
            "This clears the usage log and starts over.",
            "Reset",
            "Cancel");
        if (!confirm)
            return;

        _tracker.Reset(shell.CurrentPage?.Title, shell.CurrentState?.Location.ToString());
    }

    void OnTick(object? sender, EventArgs e) => RequestRefresh();

    void OnChanged(object? sender, EventArgs e)
    {
        // Only the visible page refreshes. StartClock refreshes when it next appears.
        var dispatcher = _dispatcher;
        if (dispatcher == null)
            return;

        if (dispatcher.IsDispatchRequired)
            dispatcher.Dispatch(RequestRefresh);
        else
            RequestRefresh();
    }

    /// <summary>
    /// Runs on the UI thread. Overlapping requests, such as a tick, a tracker change and a
    /// rotation landing together, collapse into one projection plus at most one follow-up.
    /// </summary>
    async void RequestRefresh()
    {
        if (_refreshing)
        {
            _refreshQueued = true;
            return;
        }

        _refreshing = true;
        try
        {
            do
            {
                _refreshQueued = false;
                var events = _tracker.Snapshot();
                var now = _tracker.Now;
                var latest = await Task.Run(() => UsageTimeline.Project(events, now));
                Rows = TimelineRowDiff.Apply(Rows, latest);
            }
            while (_refreshQueued && _dispatcher != null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Usage refresh: {ex}");
        }
        finally
        {
            _refreshing = false;
        }
    }
}
