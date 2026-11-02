using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using GraphicsColors = Microsoft.Maui.Graphics.Colors;

namespace MauiLab.Layouts;

public partial class GridLayoutPageModel : ObservableObject
{
    const double GoldenAngle = 137.508;
    const double RowHueOffset = 60;

    static readonly Color[] ColumnSeeds = [GraphicsColors.Red, GraphicsColors.Green, GraphicsColors.Blue];
    static readonly Color[] RowSeeds = [GraphicsColors.Magenta, GraphicsColors.Yellow, GraphicsColors.Cyan];

    int _columnsCreated;
    int _rowsCreated;
    double _recordedColumnSpacing = 48;
    double _recordedRowSpacing = 48;
    readonly HashSet<GridTrack> _watched = [];
    readonly Dictionary<GridTrack, string> _trackDetail = [];
    bool _ready;

    public GridLayoutPageModel(ILayoutUsage? usage = null)
    {
        Usage = usage ?? NoLayoutUsage.Instance;
        Columns.Add(CreateColumn(GridUnitType.Star, 1));
        Columns.Add(CreateColumn(GridUnitType.Star, 4));
        Columns.Add(CreateColumn(GridUnitType.Star, 1));
        Rows.Add(CreateRow(GridUnitType.Star, 1));
        Rows.Add(CreateRow(GridUnitType.Star, 4));
        Rows.Add(CreateRow(GridUnitType.Star, 1));
        Columns.CollectionChanged += (_, _) => RebuildDefinitions();
        Rows.CollectionChanged += (_, _) => RebuildDefinitions();
        RebuildDefinitions();
        foreach (var column in Columns)
            RememberTrack(column, column: true);
        foreach (var row in Rows)
            RememberTrack(row, column: false);
        _ready = true;
    }

    public ILayoutUsage Usage { get; }

    public ObservableCollection<GridTrack> Columns { get; } = [];

    public ObservableCollection<GridTrack> Rows { get; } = [];

    public ObservableCollection<LayoutItem> Children { get; } = [];

    [ObservableProperty]
    public partial LayoutItem? Selected { get; set; }

    public ColumnDefinitionCollection ColumnDefinitions { get; private set; } = [];

    public RowDefinitionCollection RowDefinitions { get; private set; } = [];

    [ObservableProperty]
    public partial double ColumnSpacing { get; set; } = 48;

    [ObservableProperty]
    public partial double RowSpacing { get; set; } = 48;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExpandGlyph))]
    public partial bool IsExpanded { get; set; } = true;

    public string ExpandGlyph => IsExpanded ? "▾" : "▸";

    [RelayCommand]
    void ToggleExpanded() => IsExpanded = !IsExpanded;

    [RelayCommand]
    void AddColumn()
    {
        Columns.Add(CreateColumn(GridUnitType.Star, 1));
        RemoveColumnCommand.NotifyCanExecuteChanged();
        Usage.Record("Added column", $"{Columns.Count} columns");
    }

    [RelayCommand(CanExecute = nameof(CanRemoveColumn))]
    void RemoveColumn(GridTrack? track)
    {
        if (track == null || !CanRemoveColumn(track))
            return;

        Columns.Remove(track);
        RemoveColumnCommand.NotifyCanExecuteChanged();
        Usage.Record("Removed column", $"{Columns.Count} columns");
    }

    bool CanRemoveColumn(GridTrack? track) => Columns.Count > 1;

    [RelayCommand]
    void AddRow()
    {
        Rows.Add(CreateRow(GridUnitType.Star, 1));
        RemoveRowCommand.NotifyCanExecuteChanged();
        Usage.Record("Added row", $"{Rows.Count} rows");
    }

    [RelayCommand(CanExecute = nameof(CanRemoveRow))]
    void RemoveRow(GridTrack? track)
    {
        if (track == null || !CanRemoveRow(track))
            return;

        Rows.Remove(track);
        RemoveRowCommand.NotifyCanExecuteChanged();
        Usage.Record("Removed row", $"{Rows.Count} rows");
    }

    bool CanRemoveRow(GridTrack? track) => Rows.Count > 1;

    [RelayCommand]
    void AddChild()
    {
        Children.Add(new LayoutItem(LayoutItemKind.Rectangle, $"Child {Children.Count + 1}", GraphicsColors.Orange));
        Usage.Record("Added child", null);
    }

    [RelayCommand]
    void RemoveChild(LayoutItem? item)
    {
        if (item == null || !Children.Remove(item))
            return;

        if (Selected == item)
            Selected = null;
        Usage.Record("Removed child", null);
    }

    public void ClearSelection()
    {
        foreach (var child in Children)
            child.IsSelected = false;
        Selected = null;
    }

    public void Place(LayoutItem item, int row, int column, int rowSpan, int columnSpan)
    {
        item.Row = row;
        item.Column = column;
        item.RowSpan = Math.Max(1, rowSpan);
        item.ColumnSpan = Math.Max(1, columnSpan);
        Usage.Record("Placed child", $"row {row}, column {column}, span {rowSpan} by {columnSpan}");
    }

    public void CommitColumnSpacing() => CommitSpacing(ColumnSpacing, ref _recordedColumnSpacing, "Changed column spacing");

    public void CommitRowSpacing() => CommitSpacing(RowSpacing, ref _recordedRowSpacing, "Changed row spacing");

    public void CommitTrack(GridTrack track, bool column)
    {
        var list = column ? (IList<GridTrack>)Columns : Rows;
        if (list.IndexOf(track) < 0)
            return;

        var detail = TrackDetail(track, list.IndexOf(track));
        if (_trackDetail.TryGetValue(track, out var previous) && previous == detail)
            return;

        _trackDetail[track] = detail;
        if (_ready)
            Usage.Record(column ? "Changed column" : "Changed row", detail);
    }

    void RememberTrack(GridTrack track, bool column)
    {
        var list = column ? (IList<GridTrack>)Columns : Rows;
        _trackDetail[track] = TrackDetail(track, list.IndexOf(track));
    }

    static string TrackDetail(GridTrack track, int index) =>
        track.Unit == GridUnitType.Auto
            ? $"{index} Auto"
            : $"{index} {track.Unit} {Format(track.Value)}";

    public void ViewInfo(string title) => Usage.Record($"Viewed {title}", null);

    void CommitSpacing(double value, ref double recorded, string name)
    {
        if (recorded == value)
            return;

        Usage.Record(name, $"{Format(recorded)} to {Format(value)}");
        recorded = value;
    }

    static string Format(double value) => value.ToString("0.##");

    partial void OnColumnSpacingChanged(double value)
    {
        if (!double.IsFinite(value) || value < 0)
            ColumnSpacing = 0;
    }

    partial void OnRowSpacingChanged(double value)
    {
        if (!double.IsFinite(value) || value < 0)
            RowSpacing = 0;
    }

    void RebuildDefinitions()
    {
        WatchTracks();
        var columns = new ColumnDefinitionCollection();
        foreach (var column in Columns)
            columns.Add(new ColumnDefinition(column.ToGridLength()));
        ColumnDefinitions = columns;

        var rows = new RowDefinitionCollection();
        foreach (var row in Rows)
            rows.Add(new RowDefinition(row.ToGridLength()));
        RowDefinitions = rows;
        OnPropertyChanged(nameof(ColumnDefinitions));
        OnPropertyChanged(nameof(RowDefinitions));
    }

    void WatchTracks()
    {
        foreach (var track in Columns.Concat(Rows))
        {
            if (!_watched.Add(track))
                continue;
            track.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName is nameof(GridTrack.Unit) or nameof(GridTrack.Value))
                    RebuildDefinitions();
                if (args.PropertyName == nameof(GridTrack.Unit))
                    CommitTrack(track, Columns.Contains(track));
            };
        }
    }

    GridTrack CreateColumn(GridUnitType unit, double value) =>
        new(unit, value, NextColor(Columns, ColumnSeeds, ref _columnsCreated, hueOffset: 0));

    GridTrack CreateRow(GridUnitType unit, double value) =>
        new(unit, value, NextColor(Rows, RowSeeds, ref _rowsCreated, hueOffset: RowHueOffset));

    static Color NextColor(IReadOnlyList<GridTrack> existing, Color[] seeds, ref int created, double hueOffset)
    {
        for (var attempt = 0; attempt < 360; attempt++)
        {
            var index = created++;
            var color = index < seeds.Length
                ? seeds[index]
                : ColorFromHue(hueOffset + index * GoldenAngle);

            if (existing.All(track => !ColorsMatch(track.Color, color)))
                return color;
        }

        return ColorFromHue(hueOffset + created * GoldenAngle);
    }

    static Color ColorFromHue(double hue)
    {
        var wrapped = ((hue % 360) + 360) % 360;
        return Color.FromHsla((float)(wrapped / 360d), 0.75f, 0.55f);
    }

    static bool ColorsMatch(Color left, Color right) =>
        Math.Abs(left.Red - right.Red) < 0.02 &&
        Math.Abs(left.Green - right.Green) < 0.02 &&
        Math.Abs(left.Blue - right.Blue) < 0.02;
}
