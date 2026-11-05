using MauiLab.Layouts;

namespace MauiLab.Collections;

public partial class CollectionViewPage : ContentPage
{
    bool _syncing;

    public CollectionViewPage()
    {
        InitializeComponent();
        OrientationPicker.ItemsSource = Enum.GetValues<ItemsLayoutOrientation>();
        SizingPicker.ItemsSource = Enum.GetValues<ItemSizingStrategy>();
        ScrollPicker.ItemsSource = Enum.GetValues<ScrollAnchor>();
        ItemsView.SizeChanged += (_, _) => FitToCanvas();
    }

    bool _loaded;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded)
            return;

        _loaded = true;
        var model = new CollectionViewPageModel(new TrackerLayoutUsage());
        BindingContext = model;
        CollectionTimings.Attach(model);
        _syncing = true;
        OrientationPicker.SelectedItem = model.Orientation;
        SizingPicker.SelectedItem = model.Sizing;
        ScrollPicker.SelectedItem = model.Scroll;
        _syncing = false;

        model.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(CollectionViewPageModel.Orientation)
                or nameof(CollectionViewPageModel.Span)
                or nameof(CollectionViewPageModel.HorizontalSpacing)
                or nameof(CollectionViewPageModel.VerticalSpacing)
                or nameof(CollectionViewPageModel.IsGrouped)
                or nameof(CollectionPageModel.Items)
                or nameof(CollectionViewPageModel.Groups))
                ApplyLayout();
        };
        model.Restored += (_, _) => Sync(model);
        ApplyLayout();
        FitToCanvas();
    }

    void Sync(CollectionViewPageModel model)
    {
        _syncing = true;
        OrientationPicker.SelectedItem = model.Orientation;
        SizingPicker.SelectedItem = model.Sizing;
        ScrollPicker.SelectedItem = model.Scroll;
        _syncing = false;
        FitToCanvas();
    }

    void FitToCanvas()
    {
        if (BindingContext is CollectionViewPageModel model)
            model.ApplyCanvasSize(ItemsView.Width, ItemsView.Height);
    }

    void ApplyLayout()
    {
        if (BindingContext is not CollectionViewPageModel model)
            return;

        ItemsView.IsGrouped = model.IsGrouped;
        ItemsView.ItemsSource = model.IsGrouped ? model.Groups : model.Items;
        ItemsView.ItemsLayout = new GridItemsLayout(Math.Max(1, model.Span), model.Orientation)
        {
            HorizontalItemSpacing = model.HorizontalSpacing,
            VerticalItemSpacing = model.VerticalSpacing
        };
    }

    void OnOrientationChanged(object? sender, EventArgs e)
    {
        if (!_syncing && OrientationPicker.SelectedItem is ItemsLayoutOrientation orientation && BindingContext is CollectionViewPageModel model)
        {
            model.SetOrientation(orientation);
            FitToCanvas();
        }
    }

    void OnSizingChanged(object? sender, EventArgs e)
    {
        if (!_syncing && SizingPicker.SelectedItem is ItemSizingStrategy sizing && BindingContext is CollectionViewPageModel model)
            model.SetSizing(sizing);
    }

    void OnScrollChanged(object? sender, EventArgs e)
    {
        if (!_syncing && ScrollPicker.SelectedItem is ScrollAnchor scroll && BindingContext is CollectionViewPageModel model)
            model.SetScroll(scroll);
    }

    void OnGroupingToggled(object? sender, ToggledEventArgs e)
    {
        if (BindingContext is CollectionViewPageModel model)
            model.SetGrouping(e.Value);
    }

    void OnSpanCommitted(object? sender, EventArgs e) =>
        (BindingContext as CollectionViewPageModel)?.CommitSpan();

    void OnHorizontalSpacingCommitted(object? sender, EventArgs e) =>
        (BindingContext as CollectionViewPageModel)?.CommitHorizontalSpacing();

    void OnVerticalSpacingCommitted(object? sender, EventArgs e) =>
        (BindingContext as CollectionViewPageModel)?.CommitVerticalSpacing();

    void OnAboutTapped(object? sender, EventArgs e)
    {
        if (BindingContext is CollectionPageModel model)
            model.ViewInfo(model.KindTitle);
    }

    void OnInfoTapped(object? sender, EventArgs e)
    {
        if (sender is InfoControl info && BindingContext is CollectionPageModel model)
            model.ViewInfo(info.Title);
    }
}
