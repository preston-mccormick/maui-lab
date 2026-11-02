namespace MauiLab.Layouts;

public partial class RadialLayoutPage : ContentPage, ILayoutItemHost
{
    public RadialLayoutPage()
    {
        InitializeComponent();
        BindingContext = new RadialLayoutPageModel(new TrackerLayoutUsage());
    }

    void OnStartCommitted(object? sender, EventArgs e) =>
        (BindingContext as RadialLayoutPageModel)?.CommitStart();

    void OnSweepCommitted(object? sender, EventArgs e) =>
        (BindingContext as RadialLayoutPageModel)?.CommitSweep();

    void OnRadiusCommitted(object? sender, EventArgs e) =>
        (BindingContext as RadialLayoutPageModel)?.CommitRadius();

    void OnAngleCommitted(object? sender, EventArgs e) =>
        (BindingContext as RadialLayoutPageModel)?.CommitAngle(AngleEntry.Text);

    void OnAdd(object? sender, EventArgs e) =>
        (BindingContext as RadialLayoutPageModel)?.Add();

    void OnRemove(object? sender, EventArgs e)
    {
        if (BindingContext is RadialLayoutPageModel model)
            model.Remove(model.Selected);
    }

    void OnCanvasTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Element canvas || BindingContext is not RadialLayoutPageModel model || LayoutCanvas.HitsItem(e, canvas))
            return;

        model.ClearSelection();
        ItemLevers.BindingContext = null;
    }

    public void ItemTapped(LayoutItemView view, LayoutItem item)
    {
        if (BindingContext is not RadialLayoutPageModel model)
            return;

        foreach (var other in model.Items)
            other.IsSelected = other == item;
        model.Selected = item;
        ItemLevers.BindingContext = view;
    }

    void OnInfoTapped(object? sender, EventArgs e)
    {
        if (sender is InfoControl info && BindingContext is RadialLayoutPageModel model)
            model.ViewInfo(info.Title);
    }
}
