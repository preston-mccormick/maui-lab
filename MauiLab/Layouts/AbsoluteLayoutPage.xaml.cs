using Microsoft.Maui.Layouts;

namespace MauiLab.Layouts;

public partial class AbsoluteLayoutPage : ContentPage, ILayoutItemHost
{
    const double MinimumSide = 16;

    readonly FlagChoice[] _flags =
    [
        new("Absolute", AbsoluteLayoutFlags.None),
        new("Position proportional", AbsoluteLayoutFlags.PositionProportional),
        new("Size proportional", AbsoluteLayoutFlags.SizeProportional),
        new("All proportional", AbsoluteLayoutFlags.All)
    ];

    Rect _panOrigin;
    Point _grab;
    bool _dragging;
    bool _resizing;

    public AbsoluteLayoutPage()
    {
        InitializeComponent();
        var model = new AbsoluteLayoutPageModel(new TrackerLayoutUsage());
        BackgroundPicker.ItemsSource = Enum.GetValues<CanvasBackground>();
        FlagPicker.ItemsSource = _flags;
        FlagPicker.ItemDisplayBinding = new Binding(nameof(FlagChoice.Name));
        BindingContext = model;
        KindPicker.ItemsSource = model.Illustrations;
        KindPicker.ItemDisplayBinding = new Binding(nameof(VeggieChoice.Name));
        KindPicker.SelectedItem = model.Illustrations[0];
        BackgroundPicker.SelectedItem = model.Background;
    }

    void OnCanvasSize(object? sender, EventArgs e)
    {
        if (sender is VisualElement canvas && canvas.Width > 0 && canvas.Height > 0 && BindingContext is AbsoluteLayoutPageModel model)
            model.CanvasSize = new Size(canvas.Width, canvas.Height);
    }

    void OnBackgroundChanged(object? sender, EventArgs e)
    {
        if (BackgroundPicker.SelectedItem is CanvasBackground background && BindingContext is AbsoluteLayoutPageModel model)
            model.SetBackground(background);
    }

    void OnAdd(object? sender, EventArgs e)
    {
        if (KindPicker.SelectedItem is VeggieChoice illustration && BindingContext is AbsoluteLayoutPageModel model)
            model.Add(illustration, randomBounds: false);
    }

    void OnAddRandom(object? sender, EventArgs e) =>
        (BindingContext as AbsoluteLayoutPageModel)?.AddRandom();

    void OnRemove(object? sender, EventArgs e)
    {
        if (BindingContext is AbsoluteLayoutPageModel model)
            model.Remove(model.Selected);
    }

    void OnFlagsChanged(object? sender, EventArgs e)
    {
        if (FlagPicker.SelectedItem is FlagChoice choice && BindingContext is AbsoluteLayoutPageModel model)
            model.SetFlags(choice.Flags);
    }

    void OnItemEdit(object? sender, FocusEventArgs e) =>
        (BindingContext as AbsoluteLayoutPageModel)?.BeginItemEdit();

    void OnRotationCommitted(object? sender, EventArgs e) =>
        (BindingContext as AbsoluteLayoutPageModel)?.CommitRotation();

    void OnOpacityCommitted(object? sender, EventArgs e) =>
        (BindingContext as AbsoluteLayoutPageModel)?.CommitOpacity();

    void OnZCommitted(object? sender, EventArgs e) =>
        (BindingContext as AbsoluteLayoutPageModel)?.CommitZ();

    void OnPositionCommitted(object? sender, EventArgs e) =>
        (BindingContext as AbsoluteLayoutPageModel)?.CommitPosition();

    void OnSizeCommitted(object? sender, EventArgs e) =>
        (BindingContext as AbsoluteLayoutPageModel)?.CommitSize();

    void OnClipToggled(object? sender, ToggledEventArgs e) =>
        (BindingContext as AbsoluteLayoutPageModel)?.SetClip(e.Value);

    void OnCanvasTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not AbsoluteLayoutPageModel model)
            return;
        if (e.GetPosition(Canvas) is Point at && model.HitsItem(at))
            return;

        model.ClearSelection();
    }

    public void ItemTapped(LayoutItemView view, LayoutItem item)
    {
        if (BindingContext is AbsoluteLayoutPageModel model)
            Select(model, view, item);
    }

    void OnDrag(object? sender, LongPressingEventArgs e)
    {
        if (sender is not LayoutItemView view || view.Item is not LayoutItem item || BindingContext is not AbsoluteLayoutPageModel model)
            return;

        switch (e.Status)
        {
            case GestureStatus.Started:
                if (e.GetPosition(Canvas) is not Point start)
                    return;
                _grab = start;
                _panOrigin = item.Bounds;
                _dragging = true;
                Select(model, view, item);
                break;
            case GestureStatus.Running when _dragging:
                if (e.GetPosition(Canvas) is not Point at)
                    return;
                var moved = AbsolutePlacement.Move(_panOrigin, at.X - _grab.X, at.Y - _grab.Y, item.Flags, model.CanvasSize);
                model.DragTo(item, moved.X, moved.Y);
                break;
            case GestureStatus.Completed when _dragging:
                _dragging = false;
                if (item.Bounds.Location != _panOrigin.Location)
                    model.CommitMove(item);
                break;
            case GestureStatus.Canceled when _dragging:
                _dragging = false;
                model.DragTo(item, _panOrigin.X, _panOrigin.Y);
                break;
        }
    }

    void OnResize(object? sender, PanUpdatedEventArgs e)
    {
        if (BindingContext is not AbsoluteLayoutPageModel { Selected: LayoutItem item } model)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _resizing = true;
                _dragging = false;
                _panOrigin = item.Bounds;
                model.BeginItemEdit();
                break;
            case GestureStatus.Running when _resizing:
                var grown = AbsolutePlacement.Resize(_panOrigin, e.TotalX, e.TotalY, item.Flags, model.CanvasSize, MinimumSide);
                item.Bounds = grown;
                break;
            case GestureStatus.Completed or GestureStatus.Canceled when _resizing:
                _resizing = false;
                model.CommitSize();
                break;
        }
    }

    void Select(AbsoluteLayoutPageModel model, View view, LayoutItem item)
    {
        foreach (var other in model.Items)
            other.IsSelected = other == item;
        model.Selected = item;
        model.BeginItemEdit();
        ItemLevers.BindingContext = view;
        FlagPicker.SelectedItem = _flags.FirstOrDefault(choice => choice.Flags == item.Flags);
    }

    void OnInfoTapped(object? sender, EventArgs e)
    {
        if (sender is InfoControl info && BindingContext is AbsoluteLayoutPageModel model)
            model.ViewInfo(info.Title);
    }

    sealed record FlagChoice(string Name, AbsoluteLayoutFlags Flags);
}
