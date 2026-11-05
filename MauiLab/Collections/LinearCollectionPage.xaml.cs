using MauiLab.Layouts;

namespace MauiLab.Collections;

public partial class LinearCollectionPage : ContentPage
{
    bool _syncing;
    bool _loaded;

    public LinearCollectionPage()
    {
        InitializeComponent();
        OrientationPicker.ItemsSource = Enum.GetValues<ItemsLayoutOrientation>();
        SizingPicker.ItemsSource = Enum.GetValues<ItemSizingStrategy>();
        ScrollPicker.ItemsSource = Enum.GetValues<ScrollAnchor>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded)
            return;

        if (Shell.Current?.CurrentPage is Page current && current != this)
            return;

        _loaded = true;
        var model = new LinearCollectionPageModel(new TrackerLayoutUsage());
        BindingContext = model;
        CollectionTimings.Attach(model);
        ItemsView.ItemTemplate = new DataTemplate(() => new SwipeSwatch(model));
        Sync(model);
        model.Restored += (_, _) => Sync(model);
        model.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(LinearCollectionPageModel.Orientation)
                or nameof(LinearCollectionPageModel.ItemSpacing)
                or nameof(CollectionPageModel.Items))
                ApplyLayout();
        };
        ApplyLayout();
    }

    void Sync(LinearCollectionPageModel model)
    {
        _syncing = true;
        OrientationPicker.SelectedItem = model.Orientation;
        SizingPicker.SelectedItem = model.Sizing;
        ScrollPicker.SelectedItem = model.Scroll;
        _syncing = false;
        ApplyLayout();
    }

    void ApplyLayout()
    {
        if (BindingContext is not LinearCollectionPageModel model)
            return;

        ItemsView.ItemsSource = model.Items;
        ItemsView.ItemsLayout = new LinearItemsLayout(model.Orientation) { ItemSpacing = model.ItemSpacing };
    }

    void OnOrientationChanged(object? sender, EventArgs e)
    {
        if (!_syncing && OrientationPicker.SelectedItem is ItemsLayoutOrientation orientation && BindingContext is LinearCollectionPageModel model)
            model.SetOrientation(orientation);
    }

    void OnSizingChanged(object? sender, EventArgs e)
    {
        if (!_syncing && SizingPicker.SelectedItem is ItemSizingStrategy sizing && BindingContext is LinearCollectionPageModel model)
            model.SetSizing(sizing);
    }

    void OnScrollChanged(object? sender, EventArgs e)
    {
        if (!_syncing && ScrollPicker.SelectedItem is ScrollAnchor scroll && BindingContext is LinearCollectionPageModel model)
            model.SetScroll(scroll);
    }

    void OnSpacingCommitted(object? sender, EventArgs e) =>
        (BindingContext as LinearCollectionPageModel)?.CommitSpacing();

    void OnPinToggled(object? sender, ToggledEventArgs e)
    {
        if (BindingContext is LinearCollectionPageModel model)
            model.SetOfferPin(e.Value);
    }

    void OnCopyToggled(object? sender, ToggledEventArgs e)
    {
        if (BindingContext is LinearCollectionPageModel model)
            model.SetOfferCopy(e.Value);
    }

    void OnDeleteToggled(object? sender, ToggledEventArgs e)
    {
        if (BindingContext is LinearCollectionPageModel model)
            model.SetOfferDelete(e.Value);
    }

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
