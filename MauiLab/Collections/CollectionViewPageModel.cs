using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiLab.Layouts;

namespace MauiLab.Collections;

public partial class CollectionViewPageModel : CollectionPageModel
{
    public CollectionViewPageModel(ILayoutUsage? usage = null)
        : base(usage)
    {
        OnPublished();
    }

    protected override bool OffersLongCatalog => true;

    public override string KindTitle => "CollectionView Grid Layout";

    public override string KindSummary =>
        "A virtualized grid. Grouping and a scroll anchor are built in. A grid stretches its cells to fill the row and has no edge padding, so spacing is not even space along the sides. Span stays automatic until you set it.";

    public ItemsLayoutOrientation Orientation { get; private set; } = ItemsLayoutOrientation.Vertical;

    public int Span { get; private set; } = 2;

    [ObservableProperty]
    public partial double SpanEdit { get; set; } = 2;

    int _recordedSpan = 2;

    [ObservableProperty]
    public partial double HorizontalSpacing { get; set; } = SwatchFit.PreferredGap;

    [ObservableProperty]
    public partial double VerticalSpacing { get; set; } = SwatchFit.PreferredGap;

    public ItemSizingStrategy Sizing { get; private set; } = ItemSizingStrategy.MeasureFirstItem;

    public bool IsGrouped { get; private set; }

    public ScrollAnchor Scroll { get; private set; } = ScrollAnchor.KeepOffset;

    public ItemsUpdatingScrollMode ScrollMode => Scroll switch
    {
        ScrollAnchor.KeepItem => ItemsUpdatingScrollMode.KeepItemsInView,
        ScrollAnchor.KeepLast => ItemsUpdatingScrollMode.KeepLastItemInView,
        _ => ItemsUpdatingScrollMode.KeepScrollOffset
    };

    public IReadOnlyList<SwatchGroup> Groups { get; private set; } = [];

    double _recordedHorizontal = SwatchFit.PreferredGap;
    double _recordedVertical = SwatchFit.PreferredGap;
    double _fittedCross = -1;
    bool _spanLocked;
    bool _spacingLocked;

    public void SetOrientation(ItemsLayoutOrientation orientation)
    {
        if (Orientation == orientation)
            return;

        Orientation = orientation;
        OnPropertyChanged(nameof(Orientation));
        Usage.Record("Changed orientation", orientation.ToString());
    }

    public void ApplyCanvasSize(double width, double height)
    {
        var cross = Orientation == ItemsLayoutOrientation.Horizontal ? height : width;
        if (cross <= 1 || Math.Abs(cross - _fittedCross) < 0.5)
            return;

        _fittedCross = cross;
        var fit = SwatchFit.Measure(cross);
        Quietly(() =>
        {
            if (!_spanLocked && Span != fit.Span)
            {
                SpanEdit = fit.Span;
                Span = fit.Span;
                _recordedSpan = fit.Span;
                OnPropertyChanged(nameof(Span));
            }

            if (!_spacingLocked)
            {
                if (HorizontalSpacing != fit.Gap)
                    HorizontalSpacing = fit.Gap;
                if (VerticalSpacing != fit.Gap)
                    VerticalSpacing = fit.Gap;
                _recordedHorizontal = fit.Gap;
                _recordedVertical = fit.Gap;
            }

            if (WidthApplies && !WidthLocked && ItemWidth != fit.CardWidth)
            {
                ItemWidth = fit.CardWidth;
                AcceptWidth(fit.CardWidth);
            }
        });
    }

    public void SetSpan(int span)
    {
        if (span < 1)
            return;

        _spanLocked = true;
        if (span == Span)
            return;

        Span = span;
        OnPropertyChanged(nameof(Span));
        Usage.Record("Changed span", span.ToString(CultureInfo.InvariantCulture));
        _recordedSpan = span;
    }

    partial void OnSpanEditChanged(double value)
    {
        if (!SuppressEditLock)
            _spanLocked = true;

        var span = (int)Math.Round(value);
        if (span < 1 || span == Span)
            return;

        Span = span;
        OnPropertyChanged(nameof(Span));
    }

    public void CommitSpan()
    {
        _spanLocked = true;
        var span = Math.Max(1, (int)Math.Round(SpanEdit));
        if (span == _recordedSpan)
            return;

        Span = span;
        OnPropertyChanged(nameof(Span));
        Usage.Record("Changed span", span.ToString(CultureInfo.InvariantCulture));
        _recordedSpan = span;
    }

    public void CommitHorizontalSpacing()
    {
        _spacingLocked = true;
        CommitGap(ref _recordedHorizontal, HorizontalSpacing, value => HorizontalSpacing = value, "Changed horizontal spacing");
    }

    public void CommitVerticalSpacing()
    {
        _spacingLocked = true;
        CommitGap(ref _recordedVertical, VerticalSpacing, value => VerticalSpacing = value, "Changed vertical spacing");
    }

    partial void OnHorizontalSpacingChanged(double value)
    {
        if (!SuppressEditLock)
            _spacingLocked = true;
    }

    partial void OnVerticalSpacingChanged(double value)
    {
        if (!SuppressEditLock)
            _spacingLocked = true;
    }

    public void SetSizing(ItemSizingStrategy sizing)
    {
        if (Sizing == sizing)
            return;

        Sizing = sizing;
        OnPropertyChanged(nameof(Sizing));
        Usage.Record("Changed sizing", sizing == ItemSizingStrategy.MeasureAllItems ? "Measure all" : "Measure first");
    }

    public void SetGrouping(bool grouped)
    {
        if (IsGrouped == grouped)
            return;

        IsGrouped = grouped;
        OnPropertyChanged(nameof(IsGrouped));
        Usage.Record("Changed grouping", grouped ? "On" : "Off");
    }

    public void SetScroll(ScrollAnchor scroll)
    {
        if (Scroll == scroll)
            return;

        Scroll = scroll;
        OnPropertyChanged(nameof(Scroll));
        OnPropertyChanged(nameof(ScrollMode));
        Usage.Record("Changed scroll", scroll switch
        {
            ScrollAnchor.KeepItem => "Keep item",
            ScrollAnchor.KeepLast => "Keep last",
            _ => "Keep offset"
        });
    }

    protected override void RestoreDefaults()
    {
        base.RestoreDefaults();
        Orientation = ItemsLayoutOrientation.Vertical;
        Quietly(() =>
        {
            _spanLocked = false;
            _spacingLocked = false;
            Span = 2;
            SpanEdit = 2;
            HorizontalSpacing = SwatchFit.PreferredGap;
            VerticalSpacing = SwatchFit.PreferredGap;
        });
        _recordedSpan = 2;
        _recordedHorizontal = SwatchFit.PreferredGap;
        _recordedVertical = SwatchFit.PreferredGap;
        _fittedCross = -1;
        Sizing = ItemSizingStrategy.MeasureFirstItem;
        IsGrouped = false;
        Scroll = ScrollAnchor.KeepOffset;
        OnPropertyChanged(nameof(Orientation));
        OnPropertyChanged(nameof(Span));
        OnPropertyChanged(nameof(Sizing));
        OnPropertyChanged(nameof(IsGrouped));
        OnPropertyChanged(nameof(Scroll));
        OnPropertyChanged(nameof(ScrollMode));
    }

    protected override void OnPublished()
    {
        Groups = BuildGroups();
        OnPropertyChanged(nameof(Groups));
    }

    List<SwatchGroup> BuildGroups()
    {
        var groups = new List<SwatchGroup>();
        SwatchGroup? current = null;
        foreach (var item in Items)
        {
            var letter = item.Name.Length == 0 ? "#" : char.ToUpperInvariant(item.Name[0]).ToString();
            if (current == null || current.Letter != letter)
            {
                current = new SwatchGroup(letter, []);
                groups.Add(current);
            }

            current.Add(item);
        }

        return groups;
    }

    void CommitGap(ref double recorded, double current, Action<double> assign, string name)
    {
        var next = double.IsFinite(current) && current > 0 ? current : 0;
        assign(next);
        if (next == recorded)
            return;

        Usage.Record(name, $"{recorded.ToString("0.##", CultureInfo.InvariantCulture)} to {next.ToString("0.##", CultureInfo.InvariantCulture)}");
        recorded = next;
    }
}
