using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiLab.Colors;
using MauiLab.Layouts;

namespace MauiLab.Collections;

public partial class CollectionPageModel : ObservableObject
{
    public const int FlexBatchSize = 40;
    public const double RowArrangementHeight = 44;
    public const double DefaultItemWidth = 180;
    public const double DefaultItemHeight = 120;
    public const double DefaultFontSize = 14;

    readonly bool[] _ready = [false];

    public CollectionPageModel(ILayoutUsage? usage = null)
    {
        Usage = usage ?? NoLayoutUsage.Instance;
        var catalogs = ColorCatalog.Load();
        Catalogs = OffersLongCatalog
            ? catalogs
            : catalogs.Where(catalog => catalog.Description != "12-letter names").ToList();
        SelectedCatalog = Catalogs[0];
        SortOptions = ColorSortOption.GetColorSortOptions();
        SelectedSort = SortOptions[0];
        CanvasColor = ThemeBackground();
        Fill(Build(), yieldTurn: false).GetAwaiter().GetResult();
        _ready[0] = true;
    }

    public ILayoutUsage Usage { get; }

    /// <summary>Collection View keeps the 3,099-name catalog. The other pages stop at the XKCD survey.</summary>
    protected virtual bool OffersLongCatalog => false;

    public IReadOnlyList<ColorListOption> Catalogs { get; }

    public ColorListOption SelectedCatalog { get; private set; }

    public List<ColorSortOption> SortOptions { get; }

    public ColorSortOption SelectedSort { get; private set; }

    public ObservableCollection<SwatchItem> Items { get; private set; } = [];

    [ObservableProperty]
    public partial SwatchArrangement Arrangement { get; set; } = SwatchArrangement.Card;

    public bool ShowRow => Arrangement == SwatchArrangement.Row;

    public bool ShowStack => Arrangement != SwatchArrangement.Row;

    public bool WidthApplies => Arrangement != SwatchArrangement.Varied;

    public bool HeightApplies => Arrangement == SwatchArrangement.Card;

    public virtual string KindTitle => "Collection";

    public virtual string KindSummary => "";

    public event EventHandler? Restored;

    [ObservableProperty]
    public partial double ItemWidth { get; set; } = DefaultItemWidth;

    [ObservableProperty]
    public partial double ItemHeight { get; set; } = DefaultItemHeight;

    [ObservableProperty]
    public partial double FontSize { get; set; } = DefaultFontSize;

    [ObservableProperty]
    public partial double Margin { get; set; }

    public Thickness ItemMargin => new(Finite(Margin));

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTimingNotice))]
    public partial string TimingText { get; set; } = "";

    public bool HasTimingNotice => TimingText.Length > 0;

    [ObservableProperty]
    public partial bool ShowTimings { get; set; }

    [ObservableProperty]
    public partial Color CanvasColor { get; set; }

    public event Func<Task>? CardChanged;

    /// <summary>When off, a long list is replaced in one step. Flex and Uniform start with this on.</summary>
    public bool LoadsInBatches { get; private set; } = true;

    public void SetLoadsInBatches(bool on)
    {
        if (LoadsInBatches == on)
            return;

        LoadsInBatches = on;
        OnPropertyChanged(nameof(LoadsInBatches));
        Record("Changed batch loading", on ? "On" : "Off");
    }

    double _recordedWidth = DefaultItemWidth;
    double _recordedHeight = DefaultItemHeight;
    double _recordedFont = DefaultFontSize;
    double _recordedMargin;

    public async Task SetCatalog(ColorListOption catalog)
    {
        SelectedCatalog = catalog;
        OnPropertyChanged(nameof(SelectedCatalog));
        await Measure($"Catalog: {catalog.Description}", () => Fill(Build(), yieldTurn: true));
        Record("Swapped list", catalog.Description);
    }

    public async Task SetSort(ColorSortOption sort)
    {
        if (sort == SelectedSort)
            return;

        SelectedSort = sort;
        OnPropertyChanged(nameof(SelectedSort));
        await Measure($"Sort: {sort.Name}", () => Fill(Build(), yieldTurn: true));
        Record("Sorted", sort.Name);
    }

    public async Task SetArrangement(SwatchArrangement arrangement)
    {
        if (Arrangement == arrangement)
            return;

        Arrangement = arrangement;
        OnPropertyChanged(nameof(ShowRow));
        OnPropertyChanged(nameof(ShowStack));
        OnPropertyChanged(nameof(WidthApplies));
        OnPropertyChanged(nameof(HeightApplies));
        var watch = Stopwatch.StartNew();
        ApplySizes();
        await NotifyCardChanged();
        watch.Stop();
        Present($"Arrangement: {arrangement}", watch.Elapsed);
        Record("Changed arrangement", arrangement.ToString());
    }

    public bool WidthLocked { get; private set; }

    public bool MarginLocked { get; private set; }

    public void CommitWidth()
    {
        WidthLocked = true;
        CommitSize(ref _recordedWidth, ItemWidth, value => ItemWidth = value, "Changed item width");
    }

    public void CommitHeight()
    {
        var next = Finite(ItemHeight);
        if (next != ItemHeight)
            ItemHeight = next;
        if (next == _recordedHeight)
            return;

        Record("Changed item height", $"{Format(_recordedHeight)} to {Format(next)}");
        _recordedHeight = next;
        if (Arrangement == SwatchArrangement.Card)
            ApplyHeights();
        _ = NotifyCardChanged();
    }

    public void CommitFont()
    {
        var next = Math.Clamp(Finite(FontSize), 8, 40);
        if (next != FontSize)
            FontSize = next;
        if (next == _recordedFont)
            return;

        Present($"Font: {Format(next)}", TimeSpan.Zero);
        Record("Changed font", $"{Format(_recordedFont)} to {Format(next)}");
        _recordedFont = next;
        _ = NotifyCardChanged();
    }

    public void CommitMargin()
    {
        MarginLocked = true;
        var next = Finite(Margin);
        if (next != Margin)
            Margin = next;
        if (next == _recordedMargin)
            return;

        OnPropertyChanged(nameof(ItemMargin));
        Record("Changed margin", $"{Format(_recordedMargin)} to {Format(next)}");
        _recordedMargin = next;
        _ = NotifyCardChanged();
    }

    public void SetTimings(bool on)
    {
        if (ShowTimings == on)
            return;

        AdoptTimings(on);
        Record("Changed timings", on ? "On" : "Off");
        CollectionTimings.Publish(on);
    }

    /// <summary>Applies the shared timings switch without recording another action.</summary>
    public void AdoptTimings(bool on)
    {
        if (ShowTimings == on)
            return;

        ShowTimings = on;
        if (!on)
            TimingText = "";
    }

    public void DismissTiming() => TimingText = "";

    public async Task Reset()
    {
        var watch = Stopwatch.StartNew();
        RestoreDefaults();
        await Fill(Build(), yieldTurn: false);
        Restored?.Invoke(this, EventArgs.Empty);
        watch.Stop();
        Present("Reset", watch.Elapsed);
        Record("Reset", null);
    }

    public void Paint(SwatchItem item) => CanvasColor = item.Color;

    public void ViewInfo(string title) => Record($"Viewed {title}", null);

    partial void OnShowTimingsChanged(bool value)
    {
        if (!value)
            TimingText = "";
    }

    partial void OnItemWidthChanged(double value)
    {
        if (!SuppressEditLock)
            WidthLocked = true;
        if (Arrangement != SwatchArrangement.Varied)
            ApplySharedWidth();
    }

    partial void OnMarginChanged(double value)
    {
        OnPropertyChanged(nameof(ItemMargin));
        if (!SuppressEditLock)
            MarginLocked = true;
    }

    protected bool SuppressEditLock { get; private set; }

    protected void Quietly(Action edit)
    {
        var prior = SuppressEditLock;
        SuppressEditLock = true;
        try
        {
            edit();
        }
        finally
        {
            SuppressEditLock = prior;
        }
    }

    protected void AcceptWidth(double width) => _recordedWidth = width;

    protected void AcceptMargin(double margin) => _recordedMargin = margin;

    protected virtual Task Fill(IReadOnlyList<SwatchItem> next, bool yieldTurn)
    {
        _ = yieldTurn;
        Items = new ObservableCollection<SwatchItem>(next);
        OnPropertyChanged(nameof(Items));
        OnPublished();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Replaces <see cref="Items"/> in groups, returning to the dispatcher between groups
    /// so a long list can paint and accept input while it fills.
    /// </summary>
    protected async Task FillGradually(IReadOnlyList<SwatchItem> next, bool yieldTurn)
    {
        if (yieldTurn)
            await NextFrame();

        while (Items.Count > 0)
        {
            var count = Math.Min(FlexBatchSize, Items.Count);
            for (var i = 0; i < count; i++)
                Items.RemoveAt(Items.Count - 1);
            if (yieldTurn)
                await NextFrame();
        }

        for (var start = 0; start < next.Count; start += FlexBatchSize)
        {
            var end = Math.Min(start + FlexBatchSize, next.Count);
            for (var i = start; i < end; i++)
                Items.Add(next[i]);
            if (yieldTurn)
                await NextFrame();
        }

        OnPublished();
    }

    static Task NextFrame()
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null)
            return Task.Delay(1);

        var done = new TaskCompletionSource();
        dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(1), () => done.TrySetResult());
        return done.Task;
    }

    protected virtual void OnPublished()
    {
    }

    protected virtual void RestoreDefaults()
    {
        SelectedCatalog = Catalogs[0];
        SelectedSort = SortOptions[0];
        Arrangement = SwatchArrangement.Card;
        Quietly(() =>
        {
            WidthLocked = false;
            MarginLocked = false;
            ItemWidth = DefaultItemWidth;
            ItemHeight = DefaultItemHeight;
            FontSize = DefaultFontSize;
            Margin = 0;
        });
        AcceptWidth(DefaultItemWidth);
        AcceptMargin(0);
        _recordedHeight = DefaultItemHeight;
        _recordedFont = DefaultFontSize;
        CanvasColor = ThemeBackground();
        OnPropertyChanged(nameof(SelectedCatalog));
        OnPropertyChanged(nameof(SelectedSort));
        OnPropertyChanged(nameof(Arrangement));
        OnPropertyChanged(nameof(ShowRow));
        OnPropertyChanged(nameof(ShowStack));
        OnPropertyChanged(nameof(WidthApplies));
        OnPropertyChanged(nameof(HeightApplies));
    }

    protected void ApplyHeights()
    {
        foreach (var item in Items)
            item.Height = HeightFor(item.Name);
    }

    void ApplySizes()
    {
        foreach (var item in Items)
        {
            var height = HeightFor(item.Name);
            if (item.Height != height)
                item.Height = height;
            var width = WidthFor(item);
            if (item.Width != width)
                item.Width = width;
        }
    }

    void ApplySharedWidth()
    {
        var width = Finite(ItemWidth);
        foreach (var item in Items)
        {
            if (item.Width != width)
                item.Width = width;
        }
    }

    List<SwatchItem> Build()
    {
        var colors = SelectedCatalog.Colors.ToList();
        Sort(colors, SelectedSort);
        return colors.ConvertAll(color => new SwatchItem(color.Color, color.Name, HeightFor(color.Name), WidthFor(color.Name)));
    }

    double WidthFor(string name) =>
        Arrangement == SwatchArrangement.Varied ? SwatchItem.VariedSize(name).Width : Finite(ItemWidth);

    double WidthFor(SwatchItem item) =>
        Arrangement == SwatchArrangement.Varied ? item.VariedWidth : Finite(ItemWidth);

    double HeightFor(string name) => Arrangement switch
    {
        SwatchArrangement.Row => RowArrangementHeight,
        SwatchArrangement.Varied => SwatchItem.VariedSize(name).Height,
        _ => Finite(ItemHeight)
    };

    async Task Measure(string headline, Func<Task> work)
    {
        var watch = Stopwatch.StartNew();
        await work();
        watch.Stop();
        Present(headline, watch.Elapsed);
    }

    void Present(string headline, TimeSpan elapsed)
    {
        if (!_ready[0] || !ShowTimings)
            return;

        TimingText = headline + "\n" + elapsed.TotalSeconds.ToString("0.000", CultureInfo.InvariantCulture) + " s";
    }

    void Record(string name, string? detail)
    {
        if (_ready[0])
            Usage.Record(name, detail);
    }

    void CommitSize(ref double recorded, double current, Action<double> assign, string name)
    {
        var next = Finite(current);
        assign(next);
        if (next == recorded)
            return;

        Record(name, $"{Format(recorded)} to {Format(next)}");
        recorded = next;
        _ = NotifyCardChanged();
    }

    async Task NotifyCardChanged()
    {
        var handlers = CardChanged;
        if (handlers == null)
            return;

        foreach (Func<Task> handler in handlers.GetInvocationList())
            await handler();
    }

    static double Finite(double value) => double.IsFinite(value) && value > 0 ? value : value == 0 ? 0 : 0;

    static string Format(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    static void Sort(List<ColorModel> colors, ColorSortOption option)
    {
        var direction = option.Direction == ListSortDirection.Ascending ? 1 : -1;
        colors.Sort((a, b) => direction * Compare(a, b, option.SortType));
    }

    static int Compare(ColorModel a, ColorModel b, ColorSortType sortType) => sortType switch
    {
        ColorSortType.Luminocity => a.Color.GetLuminosity().CompareTo(b.Color.GetLuminosity()),
        ColorSortType.Saturation => a.Color.GetSaturation().CompareTo(b.Color.GetSaturation()),
        ColorSortType.Hue => a.Color.GetHue().CompareTo(b.Color.GetHue()),
        _ => string.Compare(a.Name, b.Name, StringComparison.CurrentCulture)
    };

    static Color ThemeBackground()
    {
        if (Application.Current == null)
            return Microsoft.Maui.Graphics.Colors.White;

        var dark = Application.Current.RequestedTheme == AppTheme.Dark;
        var key = dark ? "DarkBackground" : "LightBackground";
        if (Application.Current.Resources.TryGetValue(key, out var value) && value is Color color)
            return color;

        return dark ? Color.FromArgb("#222222") : Microsoft.Maui.Graphics.Colors.White;
    }
}
