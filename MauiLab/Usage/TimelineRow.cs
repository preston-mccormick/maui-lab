using System.ComponentModel;

namespace MauiLab.Usage;

public sealed class TimelineRow : INotifyPropertyChanged
{
    string _detail;

    public TimelineRow(string id, int depth, string title, string detail, string marker, string when)
    {
        Id = id;
        Depth = depth;
        Title = title;
        _detail = detail;
        Marker = marker;
        When = when;
    }

    public string Id { get; }

    public int Depth { get; }

    public string Title { get; }

    /// <summary>Open, Page, Activity, Session, or Root. The row template picks a dot color from this.</summary>
    public string Marker { get; }

    /// <summary>Local time shown in the right-hand column. Not indented with the row.</summary>
    public string When { get; }

    public bool HasDetail => _detail.Length > 0;

    public Thickness Indent => new(Depth * 22, 0, 0, 0);

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
