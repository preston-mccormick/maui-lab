using System.Collections;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.Controls.Shapes;

namespace MauiLab.MenuButton.Controls;

/// <summary>
/// Show a context menu anchored to a view.
/// </summary>
public class ContextMenu
{
    const uint GrowDuration = 160;

    public static void Show(View anchor, IEnumerable items)
    {
        Show(CurrentPage(anchor), anchor, items);
    }

    public static void Show(Page page, View anchor, IEnumerable items)
    {
        var open = BuildMenu(page, anchor, items);
        page.ShowPopup(open.Menu, PopupOptions());
        PinPopupFrame(open.Menu, open.Anchor, open.Items);
    }

    public static async Task ShowAsync(View anchor, IEnumerable items)
    {
        await ShowAsync(CurrentPage(anchor), anchor, items);
    }

    public static async Task ShowAsync(Page page, View anchor, IEnumerable items)
    {
        var open = BuildMenu(page, anchor, items);
        var showing = page.ShowPopupAsync(open.Menu, PopupOptions());
        PinPopupFrame(open.Menu, open.Anchor, open.Items);
        await showing;
    }

    static PreparedMenu BuildMenu(Page page, View anchor, IEnumerable items)
    {
        var background = MenuBackground();
        var menu = new Menu
        {
            BackgroundColor = background,
            BeforeItemTap = () => _ = page.ClosePopupAsync(),
            Items = items,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            MinimumWidthRequest = Math.Max(0, anchor.Width),
            Opacity = 0
        };

        if (menu.Content is VisualElement content)
            content.BackgroundColor = background;

        return new PreparedMenu(menu, anchor, items);
    }

    // The toolkit copies Margin onto the popup frame and also leaves it on this view,
    // which shifts the rows out of the frame. Position the frame itself.
    static void PinPopupFrame(Menu menu, View anchor, IEnumerable items)
    {
        if (menu.Parent is View popup && popup.Parent is not null)
        {
            KeepMarginOnFrame(menu, popup, anchor, items);
            return;
        }

        menu.ParentChanged += OnMenuParentChanged;

        void OnMenuParentChanged(object? sender, EventArgs e)
        {
            if (menu.Parent is not View popup)
                return;

            if (popup.Parent is null)
            {
                popup.ParentChanged += (_, _) =>
                {
                    if (popup.Parent is not null)
                        KeepMarginOnFrame(menu, popup, anchor, items);
                };
                menu.ParentChanged -= OnMenuParentChanged;
                return;
            }

            menu.ParentChanged -= OnMenuParentChanged;
            KeepMarginOnFrame(menu, popup, anchor, items);
        }
    }

    static void KeepMarginOnFrame(Menu menu, View popup, View anchor, IEnumerable items)
    {
        VisualElement growTarget = menu;
        if (popup.Parent is Border frame)
        {
            frame.RemoveBinding(View.MarginProperty);
            frame.RemoveBinding(Border.StrokeThicknessProperty);
            frame.RemoveBinding(View.HorizontalOptionsProperty);
            frame.RemoveBinding(View.VerticalOptionsProperty);
            frame.StrokeThickness = 0;
            frame.StrokeShape = new RoundRectangle { CornerRadius = 8 };
            frame.HorizontalOptions = LayoutOptions.Start;
            frame.VerticalOptions = LayoutOptions.Start;
            frame.Padding = Thickness.Zero;
            frame.Opacity = 0;
            growTarget = frame;
            PlaceOn(menu, frame, anchor, items);
        }

        if (popup is ContentView popupContent)
        {
            popupContent.RemoveBinding(ContentView.PaddingProperty);
            popupContent.Padding = Thickness.Zero;
        }

        popup.RemoveBinding(View.MarginProperty);
        popup.RemoveBinding(CommunityToolkit.Maui.Views.Popup.MarginProperty);
        popup.Margin = Thickness.Zero;
        menu.Padding = Thickness.Zero;
        menu.Margin = Thickness.Zero;
        menu.Opacity = 1;
        if (growTarget == menu)
            growTarget.Opacity = 1;
    }

    static void PlaceOn(Menu menu, Border frame, View anchor, IEnumerable items)
    {
        if (frame.Parent is not VisualElement overlay)
            return;

        var placed = false;
        overlay.SizeChanged += OnSized;
        Apply();

        void OnSized(object? sender, EventArgs e) => Apply();

        void Apply()
        {
            if (placed || overlay.Width <= 0 || overlay.Height <= 0)
                return;

            placed = true;
            overlay.SizeChanged -= OnSized;

            var bounds = new Size(overlay.Width, overlay.Height);
            var content = MeasuredSize(menu, items, bounds);
            var origin = PositionIn(anchor, overlay);
            var button = new Rect(origin.X, origin.Y, Math.Max(0, anchor.Width), Math.Max(0, anchor.Height));
            var placement = MenuPlacement.Place(button, content, bounds);

            menu.WidthRequest = placement.Width;
            if (menu.Content is View stack)
            {
                stack.WidthRequest = placement.Width;
                if (placement.Scroll)
                {
                    menu.Content = new ScrollView
                    {
                        Content = stack,
                        BackgroundColor = menu.BackgroundColor,
                        HeightRequest = placement.Height,
                        WidthRequest = placement.Width,
                        Orientation = ScrollOrientation.Vertical,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill
                    };
                    menu.HeightRequest = placement.Height;
                }
            }

            frame.Margin = new Thickness(placement.X, placement.Y, 0, 0);
            // Margin is applied inside the popup page, whose platform view can sit
            // lower than those coordinates (safe area, default popup inset). Nudge
            // until the frame's real top matches the placement, then grow.
            Settle(0);

            void Settle(int attempt)
            {
                if (attempt > 6)
                {
                    GrowFromButton(frame, button, placement);
                    return;
                }

                if (frame.Width <= 0 || frame.Height <= 0)
                {
                    frame.Dispatcher.Dispatch(() => Settle(attempt + 1));
                    return;
                }

                var actual = PositionIn(frame, overlay);
                var dx = placement.X - actual.X;
                var dy = placement.Y - actual.Y;
                if (Math.Abs(dx) < 1 && Math.Abs(dy) < 1)
                {
                    GrowFromButton(frame, button, placement);
                    return;
                }

                frame.Margin = new Thickness(frame.Margin.Left + dx, frame.Margin.Top + dy, 0, 0);
                frame.Dispatcher.Dispatch(() => Settle(attempt + 1));
            }
        }
    }

    static void GrowFromButton(VisualElement target, Rect button, MenuPlacement placement)
    {
        // Leave the layer anchor at its default center. Moving AnchorX/AnchorY on iOS
        // shifts the view even after the scale returns to 1, which drops the menu
        // below the button. Translation keeps the button fixed while the scale runs.
        if (button.Width <= 0 || button.Height <= 0 || placement.Width <= 0 || placement.Height <= 0)
        {
            target.Opacity = 1;
            target.ScaleX = 1;
            target.ScaleY = 1;
            target.TranslationX = 0;
            target.TranslationY = 0;
            return;
        }

        var scaleX = Math.Clamp(button.Width / placement.Width, 0.01, 1);
        var scaleY = Math.Clamp(button.Height / placement.Height, 0.01, 1);
        var translationX = (1 - scaleX) * (button.X + button.Width / 2 - (placement.X + placement.Width / 2));
        var translationY = (1 - scaleY) * (button.Y + button.Height / 2 - (placement.Y + placement.Height / 2));

        target.ScaleX = scaleX;
        target.ScaleY = scaleY;
        target.TranslationX = translationX;
        target.TranslationY = translationY;
        target.Opacity = 1;

        var animation = new Animation();
        animation.Add(0, 1, new Animation(v => target.ScaleX = v, scaleX, 1, Easing.CubicOut));
        animation.Add(0, 1, new Animation(v => target.ScaleY = v, scaleY, 1, Easing.CubicOut));
        animation.Add(0, 1, new Animation(v => target.TranslationX = v, translationX, 0, Easing.CubicOut));
        animation.Add(0, 1, new Animation(v => target.TranslationY = v, translationY, 0, Easing.CubicOut));
        animation.Commit(target, "MenuGrow", 16, GrowDuration, Easing.Linear, (_, _) =>
        {
            target.ScaleX = 1;
            target.ScaleY = 1;
            target.TranslationX = 0;
            target.TranslationY = 0;
        });
    }

    static PopupOptions PopupOptions() => new()
    {
        CanBeDismissedByTappingOutsideOfPopup = true,
        PageOverlayColor = Microsoft.Maui.Graphics.Colors.Transparent,
        Shape = new RoundRectangle
        {
            CornerRadius = 8,
            StrokeThickness = 0
        }
    };

    static Color MenuBackground()
    {
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var key = dark ? "DarkBackground" : "LightBackground";
        if (Application.Current?.Resources.TryGetValue(key, out var value) is true && value is Color color)
        {
            return color;
        }

        return dark ? Color.FromArgb("#222222") : Microsoft.Maui.Graphics.Colors.White;
    }

    static Size MeasuredSize(Menu menu, IEnumerable items, Size window)
    {
        if (menu.Content is not VisualElement stack)
            return EstimatedSize(items);

        try
        {
            var measured = stack.Measure(double.PositiveInfinity, double.PositiveInfinity);
            if (measured.Width <= 0 || measured.Height <= 0 || double.IsNaN(measured.Width) || double.IsNaN(measured.Height))
                return EstimatedSize(items);

            if (window.Width > 0 && measured.Width > window.Width)
            {
                var capped = stack.Measure(window.Width, double.PositiveInfinity);
                if (capped.Height > 0 && !double.IsNaN(capped.Height))
                    return new Size(window.Width, capped.Height);
            }

            return measured;
        }
        catch (Exception)
        {
            return EstimatedSize(items);
        }
    }

    static Size EstimatedSize(IEnumerable items)
    {
        var count = 0;
        var longest = 0;
        foreach (var item in items)
        {
            count++;
            var text = item?.GetType().GetProperty("Text")?.GetValue(item) as string;
            if (text is not null)
                longest = Math.Max(longest, text.Length);
        }

        const double rowHeight = 38;
        const double verticalPadding = 8;
        // Icon, spacing, row padding, and about 12 points per character. Used when the
        // stack cannot be measured yet; the popup is then sized from this estimate.
        var width = 56 + longest * 12;
        return new Size(width, verticalPadding + Math.Max(count, 1) * rowHeight);
    }

    static Page CurrentPage(VisualElement anchor)
    {
        if (anchor.Window?.Page is { } page)
            return page;

        var windows = Application.Current?.Windows;
        if (windows is { Count: > 0 } && windows[0].Page is { } windowPage)
            return windowPage;

        throw new InvalidOperationException("Could not find the current Page");
    }

    static Size WindowSize(VisualElement anchor)
    {
        var display = DeviceDisplay.MainDisplayInfo;
        var density = Math.Max(display.Density, 1);
        var screen = new Size(display.Width / density, display.Height / density);

        if (anchor.Window is not { Width: > 0, Height: > 0 } window)
            return screen;

        var size = new Size(window.Width, window.Height);
        if (size.Width > screen.Width * 1.5 || size.Height > screen.Height * 1.5)
            size = new Size(size.Width / density, size.Height / density);

        return size;
    }

    static Point PositionIn(VisualElement from, VisualElement to)
    {
        if (from.Handler?.PlatformView is not null && to.Handler?.PlatformView is not null)
        {
#if IOS || MACCATALYST
            var point = ((UIKit.UIView)from.Handler.PlatformView)
                .ConvertPointToView(CoreGraphics.CGPoint.Empty, (UIKit.UIView)to.Handler.PlatformView);
            return new Point(point.X, point.Y);
#elif ANDROID
            var fromView = (Android.Views.View)from.Handler.PlatformView;
            var toView = (Android.Views.View)to.Handler.PlatformView;
            var fromLocation = new int[2];
            var toLocation = new int[2];
            fromView.GetLocationOnScreen(fromLocation);
            toView.GetLocationOnScreen(toLocation);
            var density = Math.Max(DeviceDisplay.MainDisplayInfo.Density, 1);
            return new Point((fromLocation[0] - toLocation[0]) / density, (fromLocation[1] - toLocation[1]) / density);
#elif WINDOWS
            var point = ((Microsoft.UI.Xaml.UIElement)from.Handler.PlatformView)
                .TransformToVisual((Microsoft.UI.Xaml.UIElement)to.Handler.PlatformView)
                .TransformPoint(new Windows.Foundation.Point(0, 0));
            return new Point(point.X, point.Y);
#endif
        }

        var fromWindow = WindowPosition(from);
        var toWindow = WindowPosition(to);
        return new Point(fromWindow.X - toWindow.X, fromWindow.Y - toWindow.Y);
    }

    static Point WindowPosition(VisualElement view)
    {
        if (view.Handler?.PlatformView is null)
            return new Point(view.X, view.Y);

#if IOS || MACCATALYST
        var native = (UIKit.UIView)view.Handler.PlatformView;
        if (native.Window is UIKit.UIView window)
        {
            var point = native.ConvertPointToView(CoreGraphics.CGPoint.Empty, window);
            return new Point(point.X, point.Y);
        }

        return new Point(view.X, view.Y);
#elif ANDROID
        var native = (Android.Views.View)view.Handler.PlatformView;
        var location = new int[2];
        native.GetLocationOnScreen(location);

        var originX = 0;
        var originY = 0;
        var content = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity
            ?.FindViewById(Android.Resource.Id.Content);
        if (content is not null)
        {
            var contentLocation = new int[2];
            content.GetLocationOnScreen(contentLocation);
            originX = contentLocation[0];
            originY = contentLocation[1];
        }

        var density = DeviceDisplay.MainDisplayInfo.Density;
        return new Point((location[0] - originX) / density, (location[1] - originY) / density);
#elif WINDOWS
        var native = (Microsoft.UI.Xaml.UIElement)view.Handler.PlatformView;
        var point = native.TransformToVisual(null).TransformPoint(new Windows.Foundation.Point(0, 0));
        return new Point(point.X, point.Y);
#else
        return new Point(view.X, view.Y);
#endif
    }

    readonly record struct PreparedMenu(Menu Menu, View Anchor, IEnumerable Items);
}
