using Microsoft.Maui.Layouts;

namespace MauiLab.Layouts;

public partial class FlexLayoutPage : ContentPage, ILayoutItemHost
{
    bool _syncingAlignSelf;

    public FlexLayoutPage()
    {
        InitializeComponent();
        var model = new FlexLayoutPageModel(new TrackerLayoutUsage());
        DirectionPicker.ItemsSource = Enum.GetValues<FlexDirection>();
        WrapPicker.ItemsSource = Enum.GetValues<FlexWrap>();
        JustifyPicker.ItemsSource = Enum.GetValues<FlexJustify>();
        AlignItemsPicker.ItemsSource = Enum.GetValues<FlexAlignItems>();
        AlignContentPicker.ItemsSource = Enum.GetValues<FlexAlignContent>();
        AlignSelfPicker.ItemsSource = Enum.GetValues<FlexAlignSelf>();
        BindingContext = model;
        DirectionPicker.SelectedItem = model.Direction;
        WrapPicker.SelectedItem = model.Wrap;
        JustifyPicker.SelectedItem = model.JustifyContent;
        AlignItemsPicker.SelectedItem = model.AlignItems;
        AlignContentPicker.SelectedItem = model.AlignContent;
        SyncAlignSelf(model);
    }

    void OnDirectionChanged(object? sender, EventArgs e)
    {
        if (DirectionPicker.SelectedItem is FlexDirection direction && BindingContext is FlexLayoutPageModel model)
            model.SetDirection(direction);
    }

    void OnWrapChanged(object? sender, EventArgs e)
    {
        if (WrapPicker.SelectedItem is FlexWrap wrap && BindingContext is FlexLayoutPageModel model)
            model.SetWrap(wrap);
    }

    void OnJustifyChanged(object? sender, EventArgs e)
    {
        if (JustifyPicker.SelectedItem is FlexJustify justify && BindingContext is FlexLayoutPageModel model)
            model.SetJustify(justify);
    }

    void OnAlignItemsChanged(object? sender, EventArgs e)
    {
        if (AlignItemsPicker.SelectedItem is FlexAlignItems align && BindingContext is FlexLayoutPageModel model)
            model.SetAlignItems(align);
    }

    void OnAlignContentChanged(object? sender, EventArgs e)
    {
        if (AlignContentPicker.SelectedItem is FlexAlignContent align && BindingContext is FlexLayoutPageModel model)
            model.SetAlignContent(align);
    }

    void OnAlignSelfChanged(object? sender, EventArgs e)
    {
        if (!_syncingAlignSelf && AlignSelfPicker.SelectedItem is FlexAlignSelf align && BindingContext is FlexLayoutPageModel model)
            model.SetAlignSelf(align);
    }

    void OnBasisPercentToggled(object? sender, ToggledEventArgs e) =>
        (BindingContext as FlexLayoutPageModel)?.SetBasisIsPercent(e.Value);

    void OnAddOne(object? sender, EventArgs e) => (BindingContext as FlexLayoutPageModel)?.AddOne();

    void OnAddSet(object? sender, EventArgs e) => (BindingContext as FlexLayoutPageModel)?.AddSet();

    void OnAddMixed(object? sender, EventArgs e) => (BindingContext as FlexLayoutPageModel)?.AddMixed();

    void OnRemove(object? sender, EventArgs e)
    {
        if (BindingContext is FlexLayoutPageModel model)
        {
            model.Remove(model.Selected);
            ItemLevers.BindingContext = null;
            SyncAlignSelf(model);
        }
    }

    void OnItemEdit(object? sender, FocusEventArgs e) =>
        (BindingContext as FlexLayoutPageModel)?.BeginItemEdit();

    void OnGrowCommitted(object? sender, EventArgs e) => (BindingContext as FlexLayoutPageModel)?.CommitGrow();

    void OnShrinkCommitted(object? sender, EventArgs e) => (BindingContext as FlexLayoutPageModel)?.CommitShrink();

    void OnBasisCommitted(object? sender, EventArgs e) => (BindingContext as FlexLayoutPageModel)?.CommitBasis();

    void OnOrderCommitted(object? sender, EventArgs e) => (BindingContext as FlexLayoutPageModel)?.CommitOrder();

    void OnMarginCommitted(object? sender, EventArgs e) => (BindingContext as FlexLayoutPageModel)?.CommitMargin();

    void OnPaddingCommitted(object? sender, EventArgs e) => (BindingContext as FlexLayoutPageModel)?.CommitPadding();

    void OnCanvasTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Element canvas || BindingContext is not FlexLayoutPageModel model || LayoutCanvas.HitsItem(e, canvas))
            return;

        model.ClearSelection();
        ItemLevers.BindingContext = null;
        SyncAlignSelf(model);
    }

    public void ItemTapped(LayoutItemView view, LayoutItem item)
    {
        if (BindingContext is not FlexLayoutPageModel model)
            return;

        // Tapping the selected item again goes back to editing all items.
        var next = model.Selected == item ? null : item;
        foreach (var other in model.Items)
            other.IsSelected = other == next;
        model.Selected = next;
        ItemLevers.BindingContext = next == null ? null : view;
        SyncAlignSelf(model);
    }

    void SyncAlignSelf(FlexLayoutPageModel model)
    {
        _syncingAlignSelf = true;
        AlignSelfPicker.SelectedItem = model.EditTarget.AlignSelf;
        _syncingAlignSelf = false;
    }

    void OnInfoTapped(object? sender, EventArgs e)
    {
        if (sender is InfoControl info && BindingContext is FlexLayoutPageModel model)
            model.ViewInfo(info.Title);
    }
}
