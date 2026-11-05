using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using MauiLab.Layouts;
using Microsoft.Maui.Layouts;

namespace MauiLab.Collections;

public partial class FlexItemsPageModel : CollectionPageModel
{
    public FlexItemsPageModel(ILayoutUsage? usage = null)
        : base(usage)
    {
    }

    public override string KindTitle => "FlexView";

    public override string KindSummary =>
        "Justification can put equal space on the sides and between items. Wrap, direction, and alignment are all live. Every item is created, so a long list is slow. A long catalog is added 40 at a time.";

    public FlexDirection Direction { get; private set; } = FlexDirection.Row;

    public FlexWrap Wrap { get; private set; } = FlexWrap.Wrap;

    public FlexJustify JustifyContent { get; private set; } = FlexJustify.Start;

    public FlexAlignItems AlignItems { get; private set; } = FlexAlignItems.Start;

    public FlexAlignContent AlignContent { get; private set; } = FlexAlignContent.Start;

    [ObservableProperty]
    public partial double Padding { get; set; }

    public Thickness LayoutPadding => new(double.IsFinite(Padding) && Padding > 0 ? Padding : 0);

    double _recordedPadding;
    double _fittedWidth = -1;
    bool _paddingLocked;

    public void ApplyCanvasWidth(double width)
    {
        if (width <= 1 || Math.Abs(width - _fittedWidth) < 0.5)
            return;

        _fittedWidth = width;
        var fit = SwatchFit.Measure(width);
        Quietly(() =>
        {
            if (WidthApplies && !WidthLocked && ItemWidth != fit.CardWidth)
            {
                ItemWidth = fit.CardWidth;
                AcceptWidth(fit.CardWidth);
            }

            if (!MarginLocked && Margin != fit.Margin)
            {
                Margin = fit.Margin;
                AcceptMargin(fit.Margin);
            }

            if (!_paddingLocked && Padding != fit.Padding)
            {
                Padding = fit.Padding;
                _recordedPadding = fit.Padding;
            }
        });
    }

    public void SetDirection(FlexDirection direction) =>
        Set(Direction == direction, () => Direction = direction, nameof(Direction), "Changed direction", direction.ToString());

    public void SetWrap(FlexWrap wrap) =>
        Set(Wrap == wrap, () => Wrap = wrap, nameof(Wrap), "Changed wrap", wrap.ToString());

    public void SetJustify(FlexJustify justify) =>
        Set(JustifyContent == justify, () => JustifyContent = justify, nameof(JustifyContent), "Changed justification", justify.ToString());

    public void SetAlignItems(FlexAlignItems align) =>
        Set(AlignItems == align, () => AlignItems = align, nameof(AlignItems), "Changed item alignment", align.ToString());

    public void SetAlignContent(FlexAlignContent align) =>
        Set(AlignContent == align, () => AlignContent = align, nameof(AlignContent), "Changed content alignment", align.ToString());

    public void CommitPadding()
    {
        _paddingLocked = true;
        var next = double.IsFinite(Padding) && Padding > 0 ? Padding : 0;
        if (next != Padding)
            Padding = next;
        OnPropertyChanged(nameof(LayoutPadding));
        if (next == _recordedPadding)
            return;

        Usage.Record("Changed padding", $"{Format(_recordedPadding)} to {Format(next)}");
        _recordedPadding = next;
    }

    protected override void RestoreDefaults()
    {
        base.RestoreDefaults();
        Direction = FlexDirection.Row;
        Wrap = FlexWrap.Wrap;
        JustifyContent = FlexJustify.Start;
        AlignItems = FlexAlignItems.Start;
        AlignContent = FlexAlignContent.Start;
        Quietly(() =>
        {
            _paddingLocked = false;
            Padding = 0;
        });
        _recordedPadding = 0;
        _fittedWidth = -1;
        OnPropertyChanged(nameof(Direction));
        OnPropertyChanged(nameof(Wrap));
        OnPropertyChanged(nameof(JustifyContent));
        OnPropertyChanged(nameof(AlignItems));
        OnPropertyChanged(nameof(AlignContent));
    }

    protected override Task Fill(IReadOnlyList<SwatchItem> next, bool yieldTurn) =>
        LoadsInBatches ? FillGradually(next, yieldTurn) : base.Fill(next, yieldTurn);

    void Set(bool unchanged, Action assign, string property, string name, string detail)
    {
        if (unchanged)
            return;

        assign();
        OnPropertyChanged(property);
        Usage.Record(name, detail);
    }

    partial void OnPaddingChanged(double value)
    {
        if (!SuppressEditLock)
            _paddingLocked = true;
        OnPropertyChanged(nameof(LayoutPadding));
    }

    static string Format(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
