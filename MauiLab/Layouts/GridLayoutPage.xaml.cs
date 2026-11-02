using System.ComponentModel;
using System.Globalization;

namespace MauiLab.Layouts;

public partial class GridLayoutPage : ContentPage
{
    const double WashOpacity = 0.25;
    bool _gridMeasureQueued;

    public GridLayoutPage()
    {
        InitializeComponent();
        var model = new GridLayoutPageModel(new TrackerLayoutUsage());
        model.PropertyChanged += OnModelPropertyChanged;
        BindingContext = model;
        RebuildWashes();
    }

    void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(GridLayoutPageModel.ColumnDefinitions)
            or nameof(GridLayoutPageModel.RowDefinitions)
            or nameof(GridLayoutPageModel.ColumnSpacing)
            or nameof(GridLayoutPageModel.RowSpacing))
        {
            RebuildWashes();
            QueueGridMeasure();
        }
    }

    void RebuildWashes()
    {
        if (BindingContext is not GridLayoutPageModel model)
            return;

        Washes.Children.Clear();
        for (var row = 0; row < model.Rows.Count; row++)
        {
            var wash = new CellWash { Color = model.Rows[row].Color, Opacity = WashOpacity };
            Grid.SetRow(wash, row);
            Grid.SetColumnSpan(wash, model.Columns.Count);
            Washes.Children.Add(wash);
        }

        for (var column = 0; column < model.Columns.Count; column++)
        {
            var wash = new CellWash { Color = model.Columns[column].Color, Opacity = WashOpacity };
            Grid.SetColumn(wash, column);
            Grid.SetRowSpan(wash, model.Rows.Count);
            Washes.Children.Add(wash);
        }
    }

    void OnCanvasTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Element canvas || BindingContext is not GridLayoutPageModel model || LayoutCanvas.HitsItem(e, canvas))
            return;

        model.ClearSelection();
        ItemLevers.BindingContext = null;
    }

    void OnChildTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not View view || view.BindingContext is not LayoutItem item || BindingContext is not GridLayoutPageModel model)
            return;

        foreach (var other in model.Children)
            other.IsSelected = other == item;
        model.Selected = item;
        ItemLevers.BindingContext = view;
    }

    void OnColumnSpacingCommitted(object? sender, EventArgs e) =>
        (BindingContext as GridLayoutPageModel)?.CommitColumnSpacing();

    void OnRowSpacingCommitted(object? sender, EventArgs e) =>
        (BindingContext as GridLayoutPageModel)?.CommitRowSpacing();

    void OnTrackValueCommitted(object? sender, EventArgs e)
    {
        if (sender is not BindableObject { BindingContext: GridTrack track } || BindingContext is not GridLayoutPageModel model)
            return;

        model.CommitTrack(track, model.Columns.Contains(track));
    }

    string? _placeSnapshot;

    void OnPlaceFocused(object? sender, FocusEventArgs e)
    {
        if (BindingContext is GridLayoutPageModel { Selected: LayoutItem item })
            _placeSnapshot = PlaceDetail(item);
    }

    void OnPlaceCommitted(object? sender, EventArgs e)
    {
        if (BindingContext is not GridLayoutPageModel { Selected: LayoutItem item } model)
            return;

        var detail = PlaceDetail(item);
        if (detail == _placeSnapshot)
            return;

        _placeSnapshot = detail;
        model.Place(item, item.Row, item.Column, item.RowSpan, item.ColumnSpan);
    }

    static string PlaceDetail(LayoutItem item) =>
        $"row {item.Row}, column {item.Column}, span {item.RowSpan} by {item.ColumnSpan}";

    void OnInfoTapped(object? sender, EventArgs e)
    {
        if (sender is InfoControl info && BindingContext is GridLayoutPageModel model)
            model.ViewInfo(info.Title);
    }

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

public sealed class NullVisibleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class NotNullVisibleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is not null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
