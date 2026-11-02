namespace MauiLab.Layouts;

public partial class StackLayoutPage : ContentPage, ILayoutItemHost
{
    public StackLayoutPage()
    {
        InitializeComponent();
        var model = new StackLayoutPageModel(new TrackerLayoutUsage());
        ModePicker.ItemsSource = Enum.GetValues<StackMode>();
        StackModePicker.ItemsSource = Enum.GetValues<StackMode>();
        BindingContext = model;
        ModePicker.SelectedItem = model.Mode;
    }

    void OnModeChanged(object? sender, EventArgs e)
    {
        if (ModePicker.SelectedItem is StackMode mode && BindingContext is StackLayoutPageModel model)
            model.SetMode(mode);
    }

    void OnStackModeChanged(object? sender, EventArgs e)
    {
        if (StackModePicker.SelectedItem is StackMode mode && BindingContext is StackLayoutPageModel model)
            model.SetStackOrientation(mode == StackMode.Vertical);
    }

    void OnSpacingCommitted(object? sender, EventArgs e) =>
        (BindingContext as StackLayoutPageModel)?.CommitSpacing();

    void OnStackEdit(object? sender, FocusEventArgs e) =>
        (BindingContext as StackLayoutPageModel)?.BeginStackEdit();

    void OnStackSpacingCommitted(object? sender, EventArgs e) =>
        (BindingContext as StackLayoutPageModel)?.CommitStackSpacing();

    void OnAddOne(object? sender, EventArgs e) =>
        (BindingContext as StackLayoutPageModel)?.AddOne();

    void OnAddSet(object? sender, EventArgs e) =>
        (BindingContext as StackLayoutPageModel)?.AddSet();

    void OnAddStack(object? sender, EventArgs e) =>
        (BindingContext as StackLayoutPageModel)?.AddStack();

    void OnRemove(object? sender, EventArgs e)
    {
        if (BindingContext is StackLayoutPageModel model)
            model.Remove(model.Selected);
    }

    void OnCanvasTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Element canvas || BindingContext is not StackLayoutPageModel model || LayoutCanvas.HitsItem(e, canvas))
            return;

        model.ClearSelection();
        ItemLevers.BindingContext = null;
    }

    public void ItemTapped(LayoutItemView view, LayoutItem item)
    {
        if (BindingContext is not StackLayoutPageModel model)
            return;

        SelectIn(model.Items, item);
        model.Selected = item;
        ItemLevers.BindingContext = view;
        if (item.IsStack)
        {
            model.BeginStackEdit();
            StackModePicker.SelectedItem = item.IsVertical ? StackMode.Vertical : StackMode.Horizontal;
        }
    }

    static void SelectIn(IEnumerable<LayoutItem> items, LayoutItem item)
    {
        foreach (var other in items)
        {
            other.IsSelected = other == item;
            if (other.IsStack)
                SelectIn(other.Children, item);
        }
    }

    void OnInfoTapped(object? sender, EventArgs e)
    {
        if (sender is InfoControl info && BindingContext is StackLayoutPageModel model)
            model.ViewInfo(info.Title);
    }
}
