using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiLab.Layouts;

public enum FieldLayout
{
    Flex,
    Uniform,
    Columns
}

public partial class CompositeLayoutPageModel : ObservableObject
{
    double _recordedWidth = 320;

    public CompositeLayoutPageModel(ILayoutUsage? usage = null)
    {
        Usage = usage ?? NoLayoutUsage.Instance;
    }

    public ILayoutUsage Usage { get; }

    public FieldLayout[] Layouts { get; } = Enum.GetValues<FieldLayout>();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFlex))]
    [NotifyPropertyChangedFor(nameof(IsUniform))]
    [NotifyPropertyChangedFor(nameof(IsColumns))]
    public partial FieldLayout Layout { get; set; } = FieldLayout.Flex;

    [ObservableProperty]
    public partial double Width { get; set; } = 320;

    [ObservableProperty]
    public partial bool ShowDescription { get; set; }

    [ObservableProperty]
    public partial bool ShowValidation { get; set; }

    public bool IsFlex => Layout == FieldLayout.Flex;

    public bool IsUniform => Layout == FieldLayout.Uniform;

    public bool IsColumns => Layout == FieldLayout.Columns;

    public void SetLayout(FieldLayout layout)
    {
        if (Layout == layout)
            return;

        Layout = layout;
        Usage.Record("Switched layout", layout.ToString());
    }

    public void SetDescription(bool show)
    {
        if (ShowDescription == show)
            return;

        ShowDescription = show;
        Usage.Record(show ? "Showed description" : "Hid description", null);
    }

    public void SetValidation(bool show)
    {
        if (ShowValidation == show)
            return;

        ShowValidation = show;
        Usage.Record(show ? "Showed validation" : "Hid validation", null);
    }

    public void CommitWidth()
    {
        if (!double.IsFinite(Width) || Width < 0)
            Width = 0;
        if (_recordedWidth == Width)
            return;

        Usage.Record("Changed width", $"{Format(_recordedWidth)} to {Format(Width)}");
        _recordedWidth = Width;
    }

    public void ViewInfo(string title) => Usage.Record($"Viewed {title}", null);

    static string Format(double value) => value.ToString("0.##");
}
