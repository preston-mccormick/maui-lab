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

    public GridLayoutPageModel()
    {
        Columns.Add(CreateColumn(GridUnitType.Star, 1));
        Columns.Add(CreateColumn(GridUnitType.Star, 4));
        Columns.Add(CreateColumn(GridUnitType.Star, 1));
        Rows.Add(CreateRow(GridUnitType.Star, 1));
        Rows.Add(CreateRow(GridUnitType.Star, 4));
        Rows.Add(CreateRow(GridUnitType.Star, 1));
    }

    public ObservableCollection<GridTrack> Columns { get; } = [];

    public ObservableCollection<GridTrack> Rows { get; } = [];

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
    }

    [RelayCommand(CanExecute = nameof(CanRemoveColumn))]
    void RemoveColumn(GridTrack? track)
    {
        if (track == null || !CanRemoveColumn(track))
            return;

        Columns.Remove(track);
        RemoveColumnCommand.NotifyCanExecuteChanged();
    }

    bool CanRemoveColumn(GridTrack? track) => Columns.Count > 1;

    [RelayCommand]
    void AddRow()
    {
        Rows.Add(CreateRow(GridUnitType.Star, 1));
        RemoveRowCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanRemoveRow))]
    void RemoveRow(GridTrack? track)
    {
        if (track == null || !CanRemoveRow(track))
            return;

        Rows.Remove(track);
        RemoveRowCommand.NotifyCanExecuteChanged();
    }

    bool CanRemoveRow(GridTrack? track) => Rows.Count > 1;

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
