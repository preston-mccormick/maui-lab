using System.Globalization;
using MauiLab.Layouts;

namespace MauiLab.Collections;

public partial class UniformItemsPageModel : CollectionPageModel
{
    double _fittedWidth = -1;

    public UniformItemsPageModel(ILayoutUsage? usage = null)
        : base(usage)
    {
    }

    public override string KindTitle => "UniformItemsView";

    public override string KindSummary =>
        "Every cell copies the first item's size, and Fit packs in as many of that size as the canvas holds. A later item's own size is ignored for placement, and a larger card paints outside that cell and overlaps its neighbors. Items past the row maximum are not shown, and nothing is virtualized.";

    public int? MaxColumns { get; private set; }

    public int? MaxRows { get; private set; }

    public int AppliedMaxColumns => MaxColumns ?? int.MaxValue;

    public int AppliedMaxRows => MaxRows ?? int.MaxValue;

    protected override Task Fill(IReadOnlyList<SwatchItem> next, bool yieldTurn) =>
        LoadsInBatches ? FillGradually(next, yieldTurn) : base.Fill(next, yieldTurn);

    /// <summary>
    /// Matches the responsive swatch dimensions until the user takes control of
    /// width or margin. Returns whether the stamped layout needs to be replaced.
    /// </summary>
    public bool ApplyCanvasWidth(double width)
    {
        if (width <= 1 || Math.Abs(width - _fittedWidth) < 0.5)
            return false;

        _fittedWidth = width;
        var fit = SwatchFit.Measure(width);
        var changed = false;
        Quietly(() =>
        {
            if (WidthApplies && !WidthLocked && ItemWidth != fit.CardWidth)
            {
                ItemWidth = fit.CardWidth;
                AcceptWidth(fit.CardWidth);
                changed = true;
            }

            if (!MarginLocked && Margin != fit.Margin)
            {
                Margin = fit.Margin;
                AcceptMargin(fit.Margin);
                changed = true;
            }
        });
        return changed;
    }

    protected override void RestoreDefaults()
    {
        base.RestoreDefaults();
        MaxColumns = null;
        MaxRows = null;
        _fittedWidth = -1;
        OnPropertyChanged(nameof(MaxColumns));
        OnPropertyChanged(nameof(MaxRows));
        OnPropertyChanged(nameof(AppliedMaxColumns));
        OnPropertyChanged(nameof(AppliedMaxRows));
    }

    public void SetMaxColumns(int? value) => Set(value, MaxColumns, next => MaxColumns = next, nameof(MaxColumns), "Changed max columns");

    public void SetMaxRows(int? value) => Set(value, MaxRows, next => MaxRows = next, nameof(MaxRows), "Changed max rows");

    void Set(int? value, int? current, Action<int?> assign, string property, string name)
    {
        if (value is < 1)
            return;
        if (value == current)
            return;

        var previous = Label(current);
        assign(value);
        OnPropertyChanged(property);
        OnPropertyChanged(property == nameof(MaxColumns) ? nameof(AppliedMaxColumns) : nameof(AppliedMaxRows));
        Usage.Record(name, $"{previous} to {Label(value)}");
    }

    static string Label(int? value) => value?.ToString(CultureInfo.InvariantCulture) ?? "Fit";
}
