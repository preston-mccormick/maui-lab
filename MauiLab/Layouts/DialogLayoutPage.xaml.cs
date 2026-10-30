using System.ComponentModel;
using MauiLab.Colors;

namespace MauiLab.Layouts;

public partial class DialogLayoutPage : ContentPage
{
    bool _gridMeasureQueued;

    public DialogLayoutPage()
    {
        InitializeComponent();
        BindingContext = new DialogLayoutPageModel()
            {
                Title = "Colors",
                Colors = [.. ColorModel.GetNamedColors()]
            };

        Panel.PropertyChanged += OnLayoutPropertyChanged;
        CollectionView.PropertyChanged += OnLayoutPropertyChanged;
    }

    private void OnLayoutPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(VisualElement.WidthRequest)
            or nameof(VisualElement.HeightRequest)
            or nameof(VisualElement.MinimumWidthRequest)
            or nameof(VisualElement.MaximumWidthRequest)
            or nameof(VisualElement.MinimumHeightRequest)
            or nameof(VisualElement.MaximumHeightRequest)
            or nameof(View.HorizontalOptions)
            or nameof(View.VerticalOptions)
            or nameof(VisualElement.IsVisible))
        {
            QueueGridMeasure();
        }
    }

    // The grid fills the page, so the Auto column can change without the grid's
    // own bounds changing. Measure and arrange it here; otherwise the column
    // keeps the widest size it has already recorded.
    private void QueueGridMeasure()
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
