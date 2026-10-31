using System.ComponentModel;

namespace MauiLab.Usage;

public sealed class TimelineRow : INotifyPropertyChanged
{
    string _detail;
    TimelineRail _rail;

    public TimelineRow(string id, int depth, string title, string detail, string marker, string when, string category)
    {
        Id = id;
        Depth = depth;
        Title = title;
        _detail = detail;
        Marker = marker;
        When = when;
        Category = category;
        _rail = new TimelineRail(depth, category, false, false, null, []);
    }

    public string Id { get; }

    public int Depth { get; }

    public string Title { get; }

    /// <summary>Open, Page, Activity, Session, or Root. The row template picks a dot color from this.</summary>
    public string Marker { get; }

    /// <summary>Local time shown in the right-hand column. Not indented with the row.</summary>
    public string When { get; }

    public string Category { get; }

    /// <summary>Connector geometry. Changes when a neighbour is inserted or removed, so it notifies.</summary>
    public TimelineRail Rail
    {
        get => _rail;
        set
        {
            if (_rail.Equals(value))
                return;
            _rail = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rail)));
        }
    }

    public bool HasDetail => _detail.Length > 0;

    public Thickness TextIndent => new(30 + Depth * 22, 0, 0, 0);

    public Thickness Indent => TextIndent;

    public string Detail
    {
        get => _detail;
        set
        {
            if (_detail == value)
                return;
            _detail = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Detail)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
