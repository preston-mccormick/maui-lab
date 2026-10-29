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
    const double Gap = 4;

    public static void Show(View anchor, IEnumerable items)
    {
        Show(CurrentPage(anchor), anchor, items);
    }

    public static void Show(Page page, View anchor, IEnumerable items)
    {
        var menu = BuildMenu(page, anchor, items);
        page.ShowPopup(menu, PopupOptions());
        PinPopupFrame(menu);
    }

    public static async Task ShowAsync(View anchor, IEnumerable items)
    {
        await ShowAsync(CurrentPage(anchor), anchor, items);
    }

    public static async Task ShowAsync(Page page, View anchor, IEnumerable items)
    {
        var menu = BuildMenu(page, anchor, items);
        var showing = page.ShowPopupAsync(menu, PopupOptions());
        PinPopupFrame(menu);
        await showing;
    }

    static Menu BuildMenu(Page page, View anchor, IEnumerable items)
    {
        var background = MenuBackground();
        var menu = new Menu
        {
            BackgroundColor = background,
            BeforeItemTap = () => _ = page.ClosePopupAsync(),
            Items = items,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start
        };

        if (menu.Content is VisualElement content)
        {
            content.BackgroundColor = background;
        }

        var size = EstimatedSize(items);
        var origin = Place(anchor, size);
        menu.Margin = new Thickness(origin.X, origin.Y, -origin.X, -origin.Y);
        return menu;
    }

    // The toolkit copies Margin onto the popup frame and also leaves it on this view,
    // which shifts the rows out of the frame. Keep the margin on the frame only.
    static void PinPopupFrame(View menu)
    {
        if (menu.Parent is View popup && popup.Parent is not null)
        {
            KeepMarginOnFrame(menu, popup);
            return;
        }

        menu.ParentChanged += OnMenuParentChanged;

        void OnMenuParentChanged(object? sender, EventArgs e)
        {
            if (menu.Parent is not View popup)
            {
                return;
            }

            if (popup.Parent is null)
            {
                popup.ParentChanged += (_, _) =>
                {
                    if (popup.Parent is not null)
                    {
                        KeepMarginOnFrame(menu, popup);
                    }
                };
                menu.ParentChanged -= OnMenuParentChanged;
                return;
            }

            menu.ParentChanged -= OnMenuParentChanged;
            KeepMarginOnFrame(menu, popup);
        }
    }

    static void KeepMarginOnFrame(View menu, View popup)
    {
        var position = menu.Margin;
        if (popup.Parent is Border frame)
        {
            frame.RemoveBinding(View.MarginProperty);
            frame.RemoveBinding(Border.StrokeThicknessProperty);
            frame.Margin = position;
            frame.StrokeThickness = 0;
            frame.StrokeShape = new RoundRectangle { CornerRadius = 8 };
        }

        popup.RemoveBinding(View.MarginProperty);
        popup.RemoveBinding(CommunityToolkit.Maui.Views.Popup.MarginProperty);
        popup.Margin = Thickness.Zero;
        menu.Margin = Thickness.Zero;
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

    static Size EstimatedSize(IEnumerable items)
    {
        var count = 0;
        var longest = 0;
        foreach (var item in items)
        {
            count++;
            var text = item?.GetType().GetProperty("Text")?.GetValue(item) as string;
            if (text is not null)
            {
                longest = Math.Max(longest, text.Length);
            }
        }

        const double rowHeight = 38;
        const double verticalPadding = 8;
        // Icon, spacing, row padding, and about 12 points per character. Used only to
        // keep a menu near the right edge on screen; the popup itself sizes to its rows.
        var width = 56 + longest * 12;
        return new Size(width, verticalPadding + Math.Max(count, 1) * rowHeight);
    }

    static Page CurrentPage(VisualElement anchor)
    {
        if (anchor.Window?.Page is { } page)
        {
            return page;
        }

        var windows = Application.Current?.Windows;
        if (windows is { Count: > 0 } && windows[0].Page is { } windowPage)
        {
            return windowPage;
        }

        throw new InvalidOperationException("Could not find the current Page");
    }

    static Point Place(View anchor, Size menuSize)
    {
        var origin = WindowPosition(anchor);
        var window = WindowSize(anchor);
        var x = origin.X;
        var below = origin.Y + anchor.Height + Gap;
        var above = origin.Y - menuSize.Height - Gap;
        var y = below + menuSize.Height <= window.Height ? below : Math.Max(0, above);

        if (x + menuSize.Width > window.Width)
        {
            x = window.Width - menuSize.Width;
        }

        return new Point(Math.Max(0, x), Math.Max(0, y));
    }

    static Size WindowSize(VisualElement anchor)
    {
        var display = DeviceDisplay.MainDisplayInfo;
        var density = Math.Max(display.Density, 1);
        var screen = new Size(display.Width / density, display.Height / density);

        if (anchor.Window is not { Width: > 0, Height: > 0 } window)
        {
            return screen;
        }

        var size = new Size(window.Width, window.Height);
        if (size.Width > screen.Width * 1.5 || size.Height > screen.Height * 1.5)
        {
            size = new Size(size.Width / density, size.Height / density);
        }

        return size;
    }

    static Point WindowPosition(VisualElement view)
    {
        if (view.Handler?.PlatformView is null)
        {
            return new Point(view.X, view.Y);
        }

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
}
