using System.ComponentModel;

namespace MauiLab.Collections;

public sealed class SwipeSwatch : SwipeView
{
    readonly LinearCollectionPageModel _model;
    readonly SwatchView _card;
    SwatchItem? _item;

    public SwipeSwatch(LinearCollectionPageModel model)
    {
        _model = model;
        _card = new SwatchView { PaintHost = model };
        Content = _card;
        _model.ActionsChanged += (_, _) => Rebuild();
        _model.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(LinearCollectionPageModel.UsesSideSwipe))
                Rebuild();
        };
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_item != null)
            _item.PropertyChanged -= OnItemChanged;

        _item = BindingContext as SwatchItem;
        _card.BindingContext = BindingContext;
        if (_item != null)
            _item.PropertyChanged += OnItemChanged;
        Rebuild();
    }

    void OnItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SwatchItem.IsPinned))
            return;

        // Replacing the Mac menu while it is closing can leave the card hidden. Wait until it has closed.
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), () =>
        {
            _card.Opacity = 1;
            Rebuild();
        });
    }

    void Rebuild()
    {
        LeftItems = new SwipeItems();
        RightItems = new SwipeItems();
        TopItems = new SwipeItems();
        BottomItems = new SwipeItems();
        if (_item == null)
        {
            FlyoutBase.SetContextFlyout(_card, null);
            return;
        }

        var keep = new SwipeItems { Mode = SwipeMode.Reveal, SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close };
        var remove = new SwipeItems { Mode = SwipeMode.Reveal, SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close };
        var menu = OperatingSystem.IsMacCatalyst() ? new MenuFlyout() : null;
        var any = false;
        any |= Offer(keep, menu, _item.IsPinned ? "Unpin" : "Pin", () => _model.Pin(_item), _model.OfferPin, "#007AFF");
        any |= Offer(keep, menu, "Copy", () => _model.Copy(_item), _model.OfferCopy, "#8E8E93");
        any |= Offer(remove, menu, "Delete", () => _model.Delete(_item), _model.OfferDelete, "#FF3B30");
        if (_model.UsesSideSwipe)
        {
            if (keep.Count > 0)
                LeftItems = keep;
            if (remove.Count > 0)
                RightItems = remove;
        }
        else
        {
            if (keep.Count > 0)
                TopItems = keep;
            if (remove.Count > 0)
                BottomItems = remove;
        }

        FlyoutBase.SetContextFlyout(_card, any ? menu : null);
    }

    bool Offer(SwipeItems items, MenuFlyout? menu, string text, Action invoke, bool offered, string color)
    {
        if (!offered || _item == null)
            return false;

        var swipe = new SwipeItem
        {
            Text = text,
            BackgroundColor = Color.FromArgb(color)
        };
        swipe.Invoked += (_, _) =>
        {
            invoke();
            _card.Opacity = 1;
        };
        items.Add(swipe);
        if (menu == null)
            return true;

        var item = new MenuFlyoutItem { Text = text };
        item.Clicked += (_, _) =>
        {
            invoke();
            _card.Opacity = 1;
            Close();
        };
        menu.Add(item);
        return true;
    }
}
