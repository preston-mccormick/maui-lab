using MauiLab.Colors;

namespace MauiLab.Collections;

public partial class CollectionDataSection : ContentView
{
    CollectionPageModel? _model;
    bool _syncing;

    public CollectionDataSection()
    {
        InitializeComponent();
        CatalogPicker.ItemDisplayBinding = new Binding(nameof(ColorListOption.Name));
        SortPicker.ItemDisplayBinding = new Binding(nameof(ColorSortOption.Name));
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_model != null)
            _model.Restored -= OnRestored;

        _model = BindingContext as CollectionPageModel;
        if (_model == null)
            return;

        _model.Restored += OnRestored;
        Sync();
    }

    void OnRestored(object? sender, EventArgs e) => Sync();

    void Sync()
    {
        if (_model == null)
            return;

        _syncing = true;
        CatalogPicker.SelectedItem = _model.SelectedCatalog;
        SortPicker.SelectedItem = _model.SelectedSort;
        _syncing = false;
    }

    void OnCatalogChanged(object? sender, EventArgs e)
    {
        if (!CatalogPicker.IsFocused)
            ApplyCatalog();
    }

    void OnCatalogClosed(object? sender, FocusEventArgs e) => ApplyCatalog();

    void OnSortChanged(object? sender, EventArgs e)
    {
        if (!SortPicker.IsFocused)
            ApplySort();
    }

    void OnSortClosed(object? sender, FocusEventArgs e) => ApplySort();

    void ApplyCatalog()
    {
        if (_syncing || BindingContext is not CollectionPageModel model)
            return;
        if (CatalogPicker.SelectedItem is not ColorListOption catalog || catalog == model.SelectedCatalog)
            return;

        _ = model.SetCatalog(catalog);
    }

    void ApplySort()
    {
        if (_syncing || BindingContext is not CollectionPageModel model)
            return;
        if (SortPicker.SelectedItem is ColorSortOption sort)
            _ = model.SetSort(sort);
    }
}
