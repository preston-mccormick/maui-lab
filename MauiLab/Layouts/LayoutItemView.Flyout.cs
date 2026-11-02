#if IOS || MACCATALYST
using System.Runtime.CompilerServices;
using Microsoft.Maui.Platform;
using UIKit;

namespace MauiLab.Layouts;

partial class LayoutItemView
{
    partial void SuppressFlyoutPan()
    {
        if (Handler?.PlatformView is not UIView view)
            return;

        if (FlyoutPanGuard.TryInstall(view))
            return;

        // The handler can connect before the view is inside the shell, so the flyout's
        // pan is not an ancestor yet. Try once more when the view is on screen.
        Loaded -= InstallFlyoutGuard;
        Loaded += InstallFlyoutGuard;
    }

    void InstallFlyoutGuard(object? sender, EventArgs e)
    {
        Loaded -= InstallFlyoutGuard;
        if (Handler?.PlatformView is UIView view)
            FlyoutPanGuard.TryInstall(view);
    }
}

/// <summary>
/// The shell flyout pan accepts any touch that begins in the leading 10% of the screen.
/// An item that sits there, such as the broccoli, shares that touch with the pan, so
/// dragging the item also drags the flyout open. A touch that hits a draggable item is
/// declined; a swipe that starts on empty canvas still opens the flyout.
/// </summary>
static class FlyoutPanGuard
{
    static readonly ConditionalWeakTable<UIPanGestureRecognizer, object> Wrapped = new();

    public static bool TryInstall(UIView itemView)
    {
        var pan = FindFlyoutPan(itemView);
        if (pan == null || Wrapped.TryGetValue(pan, out _))
            return pan != null;

        var previous = pan.ShouldReceiveTouch;
        pan.ShouldReceiveTouch = (recognizer, touch) =>
        {
            if (previous != null && !previous(recognizer, touch))
                return false;

            return !TouchesDraggableItem(touch.View);
        };
        Wrapped.Add(pan, new object());
        return true;
    }

    static UIPanGestureRecognizer? FindFlyoutPan(UIView start)
    {
        UIPanGestureRecognizer? found = null;
        for (var view = start.Superview; view != null; view = view.Superview)
        {
            if (view.GestureRecognizers == null)
                continue;

            foreach (var recognizer in view.GestureRecognizers)
            {
                if (recognizer is UIPanGestureRecognizer pan)
                    found = pan;
            }
        }

        return found;
    }

    static bool TouchesDraggableItem(UIView? view)
    {
        for (var current = view; current != null; current = current.Superview)
        {
            if (current is MauiView { View: LayoutItemView { CanDrag: true } })
                return true;
        }

        return false;
    }
}
#endif
