using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiLab.Usage;

public partial class UsagePageModel : ObservableObject
{
    readonly UsageTracker _tracker;
    IDispatcher? _dispatcher;
    IDispatcherTimer? _timer;

    public UsagePageModel(UsageTracker tracker)
    {
        _tracker = tracker;
        _tracker.Changed += OnChanged;
    }

    public ObservableCollection<TimelineRow> Rows { get; } = [];

    public void StartClock(IDispatcher dispatcher)
    {
        StopClock();
        _dispatcher = dispatcher;
        Refresh();
        _timer = dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.IsRepeating = true;
        _timer.Tick += OnTick;
        _timer.Start();
    }

    public void StopClock()
    {
        if (_timer == null)
            return;

        _timer.Tick -= OnTick;
        _timer.Stop();
        _timer = null;
    }

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

    void OnTick(object? sender, EventArgs e) => Refresh();

    void OnChanged(object? sender, EventArgs e)
    {
        if (_dispatcher?.IsDispatchRequired == true)
            _dispatcher.Dispatch(Refresh);
        else
            Refresh();
    }

    void Refresh()
    {
        var latest = _tracker.Project();
        if (latest.Count == Rows.Count)
        {
            var same = true;
            for (var i = 0; i < latest.Count; i++)
            {
                if (latest[i].Id != Rows[i].Id)
                {
                    same = false;
                    break;
                }
            }

            if (same)
            {
                for (var i = 0; i < latest.Count; i++)
                    Rows[i].Detail = latest[i].Detail;
                return;
            }
        }

        Rows.Clear();
        foreach (var row in latest)
            Rows.Add(row);
    }
}
