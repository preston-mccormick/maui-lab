using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Layouts;

namespace MauiLab.Layouts;

public partial class LayoutPlayground : ContentView
{
    public const double PanelWidth = 320;
    public const double PanelHeight = 280;
    public const double CollapsedStripWidth = 44;

    public static double BottomPanelHeight { get; private set; } = PanelHeight;

    static event Action? SplitChanged;

    bool _watchingSplit;
    double _panStart;

    public LayoutPlayground()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    void OnLoaded(object? sender, EventArgs e)
    {
        if (_watchingSplit)
            return;

        _watchingSplit = true;
        SplitChanged += ApplySharedSplit;
    }

    void OnUnloaded(object? sender, EventArgs e)
    {
        if (!_watchingSplit)
            return;

        _watchingSplit = false;
        SplitChanged -= ApplySharedSplit;
    }

    void ApplySharedSplit() => ApplyDock();

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

    public View? Toolbar
    {
        get => (View?)GetValue(ToolbarProperty);
        set => SetValue(ToolbarProperty, value);
    }

    public static readonly BindableProperty ToolbarProperty =
        BindableProperty.Create(nameof(Toolbar), typeof(View), typeof(LayoutPlayground),
            propertyChanged: (bindable, _, value) =>
            {
                var playground = (LayoutPlayground)bindable;
                playground.ToolbarHost.Content = value as View;
                playground.OnPropertyChanged(nameof(ShowToolbar));
            });

    public string AboutTitle
    {
        get => (string)GetValue(AboutTitleProperty);
        set => SetValue(AboutTitleProperty, value);
    }

    public static readonly BindableProperty AboutTitleProperty =
        BindableProperty.Create(nameof(AboutTitle), typeof(string), typeof(LayoutPlayground), "");

    public string AboutText
    {
        get => (string)GetValue(AboutTextProperty);
        set => SetValue(AboutTextProperty, value);
    }

    public static readonly BindableProperty AboutTextProperty =
        BindableProperty.Create(nameof(AboutText), typeof(string), typeof(LayoutPlayground), "",
            propertyChanged: (bindable, _, _) => ((LayoutPlayground)bindable).OnPropertyChanged(nameof(ShowAbout)));

    /// <summary>Raised when the header info button is tapped, before the banner is shown.</summary>
    public event EventHandler? AboutTapped;

    public bool ShowAbout => IsExpanded && AboutText.Length > 0;

    public bool ShowToolbar => IsExpanded && Toolbar != null;

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
        playground.OnPropertyChanged(nameof(ShowAbout));
        playground.OnPropertyChanged(nameof(ShowToolbar));
        playground.ApplyDock();
    }

    void OnHeaderTapped(object? sender, TappedEventArgs e) => IsExpanded = !IsExpanded;

    void OnAboutTapped(object? sender, EventArgs e) => AboutTapped?.Invoke(this, EventArgs.Empty);

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
            SplitGrip.IsVisible = false;
            SplitGrip.HeightRequest = 0;
            return;
        }

        PanelBorder.WidthRequest = right ? PanelWidth : -1;
        PanelBorder.HeightRequest = right ? -1 : BottomPanelHeight;
        var grip = !right && IsExpanded;
        SplitGrip.IsVisible = grip;
        SplitGrip.HeightRequest = grip ? 28 : 0;
    }

    void OnSplitPan(object? sender, PanUpdatedEventArgs e)
    {
        if (!IsExpanded || LayoutPanelEdge.ForWidth(Width) != PanelEdge.Bottom)
            return;

        if (e.StatusType == GestureStatus.Started)
        {
            _panStart = BottomPanelHeight;
            SetSplitCursor(grabbing: true);
        }
        else if (e.StatusType == GestureStatus.Running)
        {
            SetBottomPanelHeight(_panStart - e.TotalY);
            SetSplitCursor(grabbing: true);
        }
        else if (e.StatusType is GestureStatus.Completed or GestureStatus.Canceled)
            SetSplitCursor(grabbing: false);
    }

    void OnSplitEntered(object? sender, PointerEventArgs e) => SetSplitCursor(grabbing: false);

    void OnSplitExited(object? sender, PointerEventArgs e) => SetSplitCursor(grabbing: false, clear: true);

    void SetSplitCursor(bool grabbing, bool clear = false)
    {
#if MACCATALYST
        if (clear)
            AppKit.NSCursor.ArrowCursor.Set();
        else if (grabbing)
            AppKit.NSCursor.ClosedHandCursor.Set();
        else
            AppKit.NSCursor.OpenHandCursor.Set();
#else
        _ = grabbing;
        _ = clear;
#endif
    }

    void SetBottomPanelHeight(double height)
    {
        var max = Math.Max(160, Height - 120);
        var next = Math.Clamp(height, 160, max);
        if (Math.Abs(next - BottomPanelHeight) < 0.5)
            return;

        BottomPanelHeight = next;
        SplitChanged?.Invoke();
    }
}
