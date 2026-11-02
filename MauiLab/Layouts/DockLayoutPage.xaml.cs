namespace MauiLab.Layouts;

public partial class DockLayoutPage : ContentPage, ILayoutItemHost
{
    public DockLayoutPage()
    {
        InitializeComponent();
        BindingContext = new DockLayoutPageModel(new TrackerLayoutUsage());
    }

    void OnExpandLastToggled(object? sender, ToggledEventArgs e) =>
        (BindingContext as DockLayoutPageModel)?.SetExpandLast(e.Value);

    void OnHorizontalCommitted(object? sender, EventArgs e) =>
        (BindingContext as DockLayoutPageModel)?.CommitHorizontalSpacing();

    void OnVerticalCommitted(object? sender, EventArgs e) =>
        (BindingContext as DockLayoutPageModel)?.CommitVerticalSpacing();

    void OnAdd(object? sender, EventArgs e) =>
        (BindingContext as DockLayoutPageModel)?.Add();

    void OnRemove(object? sender, EventArgs e)
    {
        if (BindingContext is DockLayoutPageModel model)
            model.Remove(model.Selected);
    }

    void OnEdgeChanged(object? sender, EventArgs e)
    {
        if (EdgePicker.SelectedItem is LayoutDock edge && BindingContext is DockLayoutPageModel { Selected: LayoutItem item } model)
            model.SetEdge(item, edge);
    }

    void OnCanvasTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Element canvas || BindingContext is not DockLayoutPageModel model || LayoutCanvas.HitsItem(e, canvas))
            return;

        model.ClearSelection();
        ItemLevers.BindingContext = null;
    }

    public void ItemTapped(LayoutItemView view, LayoutItem item)
    {
        if (BindingContext is not DockLayoutPageModel model)
            return;

        foreach (var other in model.Items)
            other.IsSelected = other == item;
        model.Selected = item;
        ItemLevers.BindingContext = view;
        EdgePicker.SelectedItem = item.Dock;
    }

    void OnInfoTapped(object? sender, EventArgs e)
    {
        if (sender is InfoControl info && BindingContext is DockLayoutPageModel model)
            model.ViewInfo(info.Title);
    }
}
