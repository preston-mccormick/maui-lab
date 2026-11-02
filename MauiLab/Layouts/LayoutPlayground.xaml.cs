using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Layouts;

namespace MauiLab.Layouts;

public partial class LayoutPlayground : ContentView
{
    public const double PanelWidth = 320;
    public const double PanelHeight = 280;
    public const double CollapsedStripWidth = 44;

    public LayoutPlayground()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(LayoutPlayground), "");

    public View? Canvas
    {
        get => (View?)GetValue(CanvasProperty);
        set => SetValue(CanvasProperty, value);
    }

    public static readonly BindableProperty CanvasProperty =
        BindableProperty.Create(nameof(Canvas), typeof(View), typeof(LayoutPlayground),
            propertyChanged: (bindable, _, value) => ((LayoutPlayground)bindable).CanvasHost.Content = value as View);

    public View? Panel
    {
        get => (View?)GetValue(PanelProperty);
        set => SetValue(PanelProperty, value);
    }

    public static readonly BindableProperty PanelProperty =
        BindableProperty.Create(nameof(Panel), typeof(View), typeof(LayoutPlayground),
            propertyChanged: (bindable, _, value) => ((LayoutPlayground)bindable).PanelHost.Content = value as View);

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly BindableProperty IsExpandedProperty =
        BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(LayoutPlayground), true, propertyChanged: OnIsExpandedChanged);

    public string ExpandGlyph => IsExpanded ? "▾" : "▸";

    /// <summary>The title hides when the panel is a collapsed strip on the right, so the strip stays thin.</summary>
    public bool ShowTitle => IsExpanded || LayoutPanelEdge.ForWidth(Width) == PanelEdge.Bottom;

    public bool HasInfo => InfoTitle.Length > 0;

    public string InfoTitle { get; private set; } = "";

    public string InfoText { get; private set; } = "";

    /// <summary>Shows a dismissible banner at the top of the page. It stays until the user closes it or taps another info control.</summary>
    public void ShowInfo(string title, string text)
    {
        InfoTitle = title;
        InfoText = text;
        OnPropertyChanged(nameof(InfoTitle));
        OnPropertyChanged(nameof(InfoText));
        OnPropertyChanged(nameof(HasInfo));
    }

    public void DismissInfo() => ShowInfo("", "");

    static void OnIsExpandedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var playground = (LayoutPlayground)bindable;
        playground.OnPropertyChanged(nameof(ExpandGlyph));
        playground.OnPropertyChanged(nameof(ShowTitle));
        playground.ApplyDock();
    }

    void OnHeaderTapped(object? sender, TappedEventArgs e) => IsExpanded = !IsExpanded;

    void OnDismissInfo(object? sender, EventArgs e) => DismissInfo();

    void OnSizeChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(ShowTitle));
        ApplyDock();
    }

    void ApplyDock()
    {
        var edge = LayoutPanelEdge.ForWidth(Width);
        var right = edge == PanelEdge.Right;
        DockLayout.SetDockPosition(PanelBorder, right ? DockPosition.Right : DockPosition.Bottom);
        if (!IsExpanded)
        {
            PanelBorder.WidthRequest = right ? CollapsedStripWidth : -1;
            PanelBorder.HeightRequest = -1;
            return;
        }

        PanelBorder.WidthRequest = right ? PanelWidth : -1;
        PanelBorder.HeightRequest = right ? -1 : PanelHeight;
    }
}
