using MauiLab.Layouts;
using Microsoft.Maui.Layouts;

namespace MauiLab.Collections;

public partial class FlexItemsPage : ContentPage
{
    bool _syncing;

    public FlexItemsPage()
    {
        InitializeComponent();
        DirectionPicker.ItemsSource = Enum.GetValues<FlexDirection>();
        WrapPicker.ItemsSource = Enum.GetValues<FlexWrap>();
        JustifyPicker.ItemsSource = Enum.GetValues<FlexJustify>();
        AlignItemsPicker.ItemsSource = Enum.GetValues<FlexAlignItems>();
        AlignContentPicker.ItemsSource = Enum.GetValues<FlexAlignContent>();
    }

    bool _loaded;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded)
            return;

        // This bottom tab is created when it is shown. Skip a stray appear while another page is current.
        if (Shell.Current?.CurrentPage is Page current && current != this)
            return;

        _loaded = true;
        var model = new FlexItemsPageModel(new TrackerLayoutUsage());
        BindingContext = model;
        CollectionTimings.Attach(model);
        _syncing = true;
        DirectionPicker.SelectedItem = model.Direction;
        WrapPicker.SelectedItem = model.Wrap;
        JustifyPicker.SelectedItem = model.JustifyContent;
        AlignItemsPicker.SelectedItem = model.AlignItems;
        AlignContentPicker.SelectedItem = model.AlignContent;
        _syncing = false;
        model.Restored += (_, _) => Sync(model);
        FitToCanvas();
    }

    void Sync(FlexItemsPageModel model)
    {
        _syncing = true;
        DirectionPicker.SelectedItem = model.Direction;
        WrapPicker.SelectedItem = model.Wrap;
        JustifyPicker.SelectedItem = model.JustifyContent;
        AlignItemsPicker.SelectedItem = model.AlignItems;
        AlignContentPicker.SelectedItem = model.AlignContent;
        _syncing = false;
        FitToCanvas();
    }

    void OnCanvasSizeChanged(object? sender, EventArgs e) => FitToCanvas();

    void FitToCanvas()
    {
        if (BindingContext is FlexItemsPageModel model)
            model.ApplyCanvasWidth(ItemsScroll.Width);
    }

    void OnDirectionChanged(object? sender, EventArgs e)
    {
        if (!_syncing && DirectionPicker.SelectedItem is FlexDirection direction && BindingContext is FlexItemsPageModel model)
            model.SetDirection(direction);
    }

    void OnWrapChanged(object? sender, EventArgs e)
    {
        if (!_syncing && WrapPicker.SelectedItem is FlexWrap wrap && BindingContext is FlexItemsPageModel model)
            model.SetWrap(wrap);
    }

    void OnJustifyChanged(object? sender, EventArgs e)
    {
        if (!_syncing && JustifyPicker.SelectedItem is FlexJustify justify && BindingContext is FlexItemsPageModel model)
            model.SetJustify(justify);
    }

    void OnAlignItemsChanged(object? sender, EventArgs e)
    {
        if (!_syncing && AlignItemsPicker.SelectedItem is FlexAlignItems align && BindingContext is FlexItemsPageModel model)
            model.SetAlignItems(align);
    }

    void OnAlignContentChanged(object? sender, EventArgs e)
    {
        if (!_syncing && AlignContentPicker.SelectedItem is FlexAlignContent align && BindingContext is FlexItemsPageModel model)
            model.SetAlignContent(align);
    }

    void OnBatchesToggled(object? sender, ToggledEventArgs e)
    {
        if (BindingContext is CollectionPageModel model)
            model.SetLoadsInBatches(e.Value);
    }

    void OnPaddingCommitted(object? sender, EventArgs e) =>
        (BindingContext as FlexItemsPageModel)?.CommitPadding();

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
