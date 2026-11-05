using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiLab.Layouts;

namespace MauiLab.Collections;

public partial class LinearCollectionPageModel : CollectionPageModel
{
    public LinearCollectionPageModel(ILayoutUsage? usage = null)
        : base(usage)
    {
    }

    protected override bool OffersLongCatalog => true;

    public override string KindTitle => "CollectionView Linear Layout";

    public override string KindSummary =>
        "A virtualized CollectionView in one line, the replacement for ListView. Pin and Copy are on the leading edge. Delete is on the trailing edge: the right side of a vertical list, and a swipe up when the list is horizontal. A mouse drag on Mac Catalyst reveals the same edges. Measure first shares the first item's size.";

    public ItemsLayoutOrientation Orientation { get; private set; } = ItemsLayoutOrientation.Vertical;

    /// <summary>A vertical list swipes left and right. A horizontal list swipes up and down, so the swipe never follows the scroll.</summary>
    public bool UsesSideSwipe => Orientation == ItemsLayoutOrientation.Vertical;

    [ObservableProperty]
    public partial double Spacing { get; set; }

    public double ItemSpacing => double.IsFinite(Spacing) && Spacing > 0 ? Spacing : 0;

    public ItemSizingStrategy Sizing { get; private set; } = ItemSizingStrategy.MeasureFirstItem;

    public ScrollAnchor Scroll { get; private set; } = ScrollAnchor.KeepOffset;

    public ItemsUpdatingScrollMode ScrollMode => Scroll switch
    {
        ScrollAnchor.KeepItem => ItemsUpdatingScrollMode.KeepItemsInView,
        ScrollAnchor.KeepLast => ItemsUpdatingScrollMode.KeepLastItemInView,
        _ => ItemsUpdatingScrollMode.KeepScrollOffset
    };

    public bool OfferPin { get; private set; } = true;

    public bool OfferCopy { get; private set; }

    public bool OfferDelete { get; private set; } = true;

    public event EventHandler? ActionsChanged;

    double _recordedSpacing;

    public void SetOrientation(ItemsLayoutOrientation orientation)
    {
        if (Orientation == orientation)
            return;

        Orientation = orientation;
        OnPropertyChanged(nameof(Orientation));
        OnPropertyChanged(nameof(UsesSideSwipe));
        Usage.Record("Changed orientation", orientation.ToString());
    }

    public void CommitSpacing()
    {
        var next = ItemSpacing;
        if (next != Spacing)
            Spacing = next;
        if (next == _recordedSpacing)
            return;

        Usage.Record("Changed spacing", $"{Format(_recordedSpacing)} to {Format(next)}");
        _recordedSpacing = next;
    }

    public void SetSizing(ItemSizingStrategy sizing)
    {
        if (Sizing == sizing)
            return;

        Sizing = sizing;
        OnPropertyChanged(nameof(Sizing));
        Usage.Record("Changed sizing", sizing == ItemSizingStrategy.MeasureAllItems ? "Measure all" : "Measure first");
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

    public void SetOfferPin(bool on) => SetOffer(OfferPin, on, value => OfferPin = value, nameof(OfferPin), "Changed pin action");

    public void SetOfferCopy(bool on) => SetOffer(OfferCopy, on, value => OfferCopy = value, nameof(OfferCopy), "Changed copy action");

    public void SetOfferDelete(bool on) => SetOffer(OfferDelete, on, value => OfferDelete = value, nameof(OfferDelete), "Changed delete action");

    public void Pin(SwatchItem item)
    {
        item.IsPinned = !item.IsPinned;
        Usage.Record(item.IsPinned ? "Pinned swatch" : "Unpinned swatch", item.Name);
    }

    public void Copy(SwatchItem item)
    {
        var index = Items.IndexOf(item);
        if (index < 0)
            return;

        Items.Insert(index + 1, new SwatchItem(item.Color, item.Name, item.Height, item.Width));
        Usage.Record("Copied swatch", item.Name);
    }

    public void Delete(SwatchItem item)
    {
        if (!Items.Remove(item))
            return;

        Usage.Record("Deleted swatch", item.Name);
    }

    protected override void RestoreDefaults()
    {
        base.RestoreDefaults();
        var actionsChanged = !OfferPin || OfferCopy || !OfferDelete;
        Orientation = ItemsLayoutOrientation.Vertical;
        Spacing = 0;
        _recordedSpacing = 0;
        Sizing = ItemSizingStrategy.MeasureFirstItem;
        Scroll = ScrollAnchor.KeepOffset;
        OfferPin = true;
        OfferCopy = false;
        OfferDelete = true;
        OnPropertyChanged(nameof(Orientation));
        OnPropertyChanged(nameof(UsesSideSwipe));
        OnPropertyChanged(nameof(ItemSpacing));
        OnPropertyChanged(nameof(Sizing));
        OnPropertyChanged(nameof(Scroll));
        OnPropertyChanged(nameof(ScrollMode));
        OnPropertyChanged(nameof(OfferPin));
        OnPropertyChanged(nameof(OfferCopy));
        OnPropertyChanged(nameof(OfferDelete));
        if (actionsChanged)
            ActionsChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnSpacingChanged(double value) => OnPropertyChanged(nameof(ItemSpacing));

    void SetOffer(bool current, bool next, Action<bool> assign, string property, string name)
    {
        if (current == next)
            return;

        assign(next);
        OnPropertyChanged(property);
        Usage.Record(name, next ? "On" : "Off");
        ActionsChanged?.Invoke(this, EventArgs.Empty);
    }

    static string Format(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
