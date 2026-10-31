using System.Collections.Specialized;
using System.ComponentModel;
using GraphicsColors = Microsoft.Maui.Graphics.Colors;

namespace MauiLab.Layouts;

public partial class GridLayoutPage : ContentPage
{
    const double WashOpacity = 0.25;

    readonly List<GridTrack> _tracks = [];
    bool _gridMeasureQueued;

    public GridLayoutPage()
    {
        InitializeComponent();
        var model = new GridLayoutPageModel();
        Attach(model);
        BindingContext = model;
        RebuildGrid();
    }

    void Attach(GridLayoutPageModel model)
    {
        model.PropertyChanged += OnModelPropertyChanged;
        model.Columns.CollectionChanged += OnTracksChanged;
        model.Rows.CollectionChanged += OnTracksChanged;
        WatchTracks(model);
    }

    void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(GridLayoutPageModel.ColumnSpacing)
            or nameof(GridLayoutPageModel.RowSpacing))
        {
            RebuildGrid();
            QueueGridMeasure();
        }
    }

    void OnTracksChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (BindingContext is GridLayoutPageModel model)
            WatchTracks(model);

        RebuildGrid();
        QueueGridMeasure();
    }

    void OnTrackPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(GridTrack.Unit)
            or nameof(GridTrack.Value)
            or nameof(GridTrack.Color))
        {
            RebuildGrid();
            QueueGridMeasure();
        }
    }

    void WatchTracks(GridLayoutPageModel model)
    {
        foreach (var track in _tracks)
            track.PropertyChanged -= OnTrackPropertyChanged;

        _tracks.Clear();
        _tracks.AddRange(model.Columns);
        _tracks.AddRange(model.Rows);

        foreach (var track in _tracks)
            track.PropertyChanged += OnTrackPropertyChanged;
    }

    void RebuildGrid()
    {
        if (BindingContext is not GridLayoutPageModel model)
            return;

        var columns = model.Columns;
        var rows = model.Rows;
        if (columns.Count == 0 || rows.Count == 0)
            return;

        LayoutGrid.ColumnSpacing = FiniteOrZero(model.ColumnSpacing);
        LayoutGrid.RowSpacing = FiniteOrZero(model.RowSpacing);

        var columnDefinitions = new ColumnDefinitionCollection();
        foreach (var column in columns)
            columnDefinitions.Add(new ColumnDefinition(column.ToGridLength()));

        var rowDefinitions = new RowDefinitionCollection();
        foreach (var row in rows)
            rowDefinitions.Add(new RowDefinition(row.ToGridLength()));

        LayoutGrid.ColumnDefinitions = columnDefinitions;
        LayoutGrid.RowDefinitions = rowDefinitions;

        LayoutGrid.Children.Clear();

        // Rows first, then columns, so each cell is the pair of washes.
        for (var row = 0; row < rows.Count; row++)
        {
            var wash = new CellWash
            {
                Color = rows[row].Color,
                Opacity = WashOpacity,
            };
            Grid.SetRow(wash, row);
            Grid.SetColumnSpan(wash, columns.Count);
            LayoutGrid.Children.Add(wash);
        }

        for (var column = 0; column < columns.Count; column++)
        {
            var wash = new CellWash
            {
                Color = columns[column].Color,
                Opacity = WashOpacity,
            };
            Grid.SetColumn(wash, column);
            Grid.SetRowSpan(wash, rows.Count);
            LayoutGrid.Children.Add(wash);
        }

        // CellWash measures to zero, so the label is what an Auto track sizes to.
        for (var row = 0; row < rows.Count; row++)
        {
            for (var column = 0; column < columns.Count; column++)
            {
                var label = new Label
                {
                    Text = $"{row},{column}",
                    FontSize = 14,
                    BackgroundColor = GraphicsColors.Transparent,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    InputTransparent = true,
                };
                Grid.SetRow(label, row);
                Grid.SetColumn(label, column);
                LayoutGrid.Children.Add(label);
            }
        }
    }

    static double FiniteOrZero(double value) =>
        double.IsFinite(value) ? Math.Max(0, value) : 0;

    // The grid fills the page, so an Auto track can change size without the
    // grid's own bounds changing. Measure and arrange it here.
    void QueueGridMeasure()
    {
        if (_gridMeasureQueued)
            return;

        _gridMeasureQueued = true;
        Dispatcher.Dispatch(() =>
        {
            _gridMeasureQueued = false;

            var bounds = LayoutGrid.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                LayoutGrid.InvalidateMeasure();
                return;
            }

            ((IView)LayoutGrid).Measure(bounds.Width, bounds.Height);
            ((IView)LayoutGrid).Arrange(bounds);
        });
    }
}
