using MauiLab.Layouts;

namespace MauiLab.Collections;

public partial class CollectionItemSection : ContentView
{
    CollectionPageModel? _model;
    bool _syncing;

    public CollectionItemSection()
    {
        InitializeComponent();
        ArrangementPicker.ItemsSource = Enum.GetValues<SwatchArrangement>();
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
        ArrangementPicker.SelectedItem = _model.Arrangement;
        _syncing = false;
    }

    void OnArrangementChanged(object? sender, EventArgs e)
    {
        if (!ArrangementPicker.IsFocused)
            ApplyArrangement();
    }

    void OnArrangementClosed(object? sender, FocusEventArgs e) => ApplyArrangement();

    void ApplyArrangement()
    {
        if (_syncing || BindingContext is not CollectionPageModel model)
            return;
        if (ArrangementPicker.SelectedItem is SwatchArrangement arrangement)
            _ = model.SetArrangement(arrangement);
    }

    void OnWidthCommitted(object? sender, EventArgs e) =>
        (BindingContext as CollectionPageModel)?.CommitWidth();

    void OnHeightCommitted(object? sender, EventArgs e) =>
        (BindingContext as CollectionPageModel)?.CommitHeight();

    void OnFontCommitted(object? sender, EventArgs e) =>
        (BindingContext as CollectionPageModel)?.CommitFont();

    void OnMarginCommitted(object? sender, EventArgs e) =>
        (BindingContext as CollectionPageModel)?.CommitMargin();

    void OnInfoTapped(object? sender, EventArgs e)
    {
        if (sender is InfoControl info && BindingContext is CollectionPageModel model)
            model.ViewInfo(info.Title);
    }
}
