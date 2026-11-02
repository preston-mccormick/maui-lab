using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Layouts;
using System.Collections.ObjectModel;

namespace MauiLab.Layouts;

/// <summary>One veggie illustration. <see cref="File"/> is the image resource, <see cref="Name"/> is the picker label.</summary>
public sealed record VeggieChoice(string Name, string File);

public enum CanvasBackground
{
    Solid,
    Gradient,
    Image
}

public partial class AbsoluteLayoutPageModel : ObservableObject
{
    public AbsoluteLayoutPageModel(ILayoutUsage? usage = null, Random? random = null)
    {
        Usage = usage ?? NoLayoutUsage.Instance;
        Random = random ?? Random.Shared;
        Items.Add(Illustration("broccoli", new Rect(24, 24, 96, 96)));
        Items.Add(Illustration("red-bell-pepper", new Rect(150, 36, 110, 110)));
        Items.Add(Illustration("corn", new Rect(40, 170, 120, 120)));
        Items.Add(Illustration("mushroom", new Rect(180, 180, 100, 100)));
    }

    /// <summary>File names, without extension, of the transparent veggie SVGs.</summary>
    public static readonly string[] IllustrationFiles =
    [
        "amanita-muscaria", "beet", "broccoli", "cabbage", "cauliflower",
        "chili-pepper", "corn", "ginger", "green-bell-pepper", "lettuce",
        "mushroom", "mushrooms", "okra", "orange-bell-pepper", "potato",
        "red-bell-pepper", "red-onion", "zuccini"
    ];

    public VeggieChoice[] Illustrations { get; } = IllustrationFiles.Select(Choice).ToArray();

    /// <summary>
    /// An absolute item gets its size from <see cref="LayoutItem.Bounds"/> alone. An explicit
    /// WidthRequest/HeightRequest would keep the view a fixed box centered in the bounds, so a
    /// resize would look like a slide instead of growth.
    /// </summary>
    LayoutItem Illustration(string file, Rect bounds) =>
        LayoutItem.Create(LayoutItemKind.Image, Items.Count, bounds, -1, -1, Resource(file));

    /// <summary>MAUI image names cannot contain hyphens, so the resource uses underscores.</summary>
    static string Resource(string file) => $"{file.Replace('-', '_')}.png";

    static VeggieChoice Choice(string file) =>
        new(string.Join(' ', file.Split('-').Select(word => char.ToUpper(word[0]) + word[1..])), $"{file}.png");

    public ILayoutUsage Usage { get; }

    public Random Random { get; }

    public ObservableCollection<LayoutItem> Items { get; } = [];

    public CanvasBackground[] Backgrounds { get; } = Enum.GetValues<CanvasBackground>();

    [ObservableProperty]
    public partial CanvasBackground Background { get; set; } = CanvasBackground.Solid;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    public partial LayoutItem? Selected { get; set; }

    [ObservableProperty]
    public partial Size CanvasSize { get; set; } = new(400, 600);

    public bool HasSelection => Selected != null;

    /// <summary>Pixel rectangle of the resize handle, centered on the selected item's bottom-right corner.</summary>
    [ObservableProperty]
    public partial Rect HandleBounds { get; set; }

    public const double HandleSize = 24;

    partial void OnSelectedChanged(LayoutItem? oldValue, LayoutItem? newValue)
    {
        if (oldValue != null)
            oldValue.PropertyChanged -= OnSelectedItemChanged;
        if (newValue != null)
            newValue.PropertyChanged += OnSelectedItemChanged;
        UpdateHandle();
    }

    partial void OnCanvasSizeChanged(Size value) => UpdateHandle();

    void OnSelectedItemChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(LayoutItem.Bounds) or nameof(LayoutItem.Flags))
            UpdateHandle();
    }

    void UpdateHandle()
    {
        if (Selected == null)
        {
            HandleBounds = Rect.Zero;
            return;
        }

        var pixels = AbsolutePlacement.ToPixels(Selected.Bounds, Selected.Flags, CanvasSize);
        HandleBounds = new Rect(pixels.Right - HandleSize / 2, pixels.Bottom - HandleSize / 2, HandleSize, HandleSize);
    }

    public void Add(VeggieChoice illustration, bool randomBounds)
    {
        var size = new Size(96, 96);
        var bounds = randomBounds
            ? AbsolutePlacement.Random(Random, CanvasSize, size)
            : new Rect(24, 24, size.Width, size.Height);
        Items.Add(Illustration(Path.GetFileNameWithoutExtension(illustration.File), bounds));
        Usage.Record("Added item", Path.GetFileNameWithoutExtension(illustration.File));
    }

    /// <summary>Adds a random illustration at a random rectangle inside the canvas.</summary>
    public void AddRandom() =>
        Add(Illustrations[Random.Next(Illustrations.Length)], randomBounds: true);

    public void ClearSelection()
    {
        foreach (var item in Items)
            item.IsSelected = false;
        Selected = null;
    }

    /// <summary>True when a canvas point lands on an item or on the resize handle.</summary>
    public bool HitsItem(Point point) =>
        (HasSelection && HandleBounds.Contains(point))
        || Items.Any(item => AbsolutePlacement.ToPixels(item.Bounds, item.Flags, CanvasSize).Contains(point));

    public void Remove(LayoutItem? item)
    {
        if (item == null || !Items.Remove(item))
            return;

        if (Selected == item)
            Selected = null;
        Usage.Record("Removed item", Path.GetFileNameWithoutExtension(item.ImageName).Replace('_', '-'));
    }

    public void DragTo(LayoutItem item, double x, double y) =>
        item.Bounds = new Rect(x, y, item.Bounds.Width, item.Bounds.Height);

    public void CommitMove(LayoutItem item) =>
        Usage.Record("Moved item", $"{Format(item.Bounds.X)}, {Format(item.Bounds.Y)}");

    public void SetClip(bool clipped)
    {
        if (Selected == null || Selected.IsClipped == clipped)
            return;

        Selected.IsClipped = clipped;
        Usage.Record("Changed clip", clipped ? "ellipse" : "none");
    }

    /// <summary>Snapshots the selected item so a later commit records only a real change.</summary>
    public void BeginItemEdit()
    {
        if (Selected == null)
            return;

        _rotation = Selected.Rotation;
        _opacity = Selected.Opacity;
        _z = Selected.ZIndex;
        _bounds = Selected.Bounds;
    }

    public void CommitRotation() => Commit(Selected?.Rotation, _rotation, "Changed rotation", value => _rotation = value);

    public void CommitOpacity() => Commit(Selected?.Opacity, _opacity, "Changed opacity", value => _opacity = value);

    public void CommitZ()
    {
        if (Selected == null || Selected.ZIndex == _z)
            return;

        _z = Selected.ZIndex;
        Usage.Record("Changed z order", _z.ToString());
    }

    public void CommitPosition()
    {
        if (Selected == null || Selected.Bounds.Location == _bounds.Location)
            return;

        _bounds = Selected.Bounds;
        CommitMove(Selected);
    }

    public void CommitSize()
    {
        if (Selected == null || Selected.Bounds.Size == _bounds.Size)
            return;

        _bounds = Selected.Bounds;
        Usage.Record("Resized item", $"{Format(_bounds.Width)} by {Format(_bounds.Height)}");
    }

    public void SetFlags(AbsoluteLayoutFlags flags)
    {
        if (Selected == null || Selected.Flags == flags)
            return;

        Selected.Flags = flags;
        Usage.Record("Changed flags", flags.ToString());
    }

    public void SetBackground(CanvasBackground background)
    {
        if (Background == background)
            return;

        var bounds = Items.Select(item => item.Bounds).ToArray();
        Background = background;
        for (var index = 0; index < Items.Count; index++)
            Items[index].Bounds = bounds[index];
        Usage.Record("Changed background", background.ToString().ToLowerInvariant());
    }

    public void ViewInfo(string title) => Usage.Record($"Viewed {title}", null);

    double _rotation = double.NaN;
    double _opacity = double.NaN;
    int _z = int.MinValue;
    Rect _bounds;

    void Commit(double? next, double previous, string name, Action<double> remember)
    {
        if (Selected == null || next is not double value || value == previous)
            return;

        remember(value);
        Usage.Record(name, Format(value));
    }

    static string Format(double value) => value.ToString("0.##");
}
