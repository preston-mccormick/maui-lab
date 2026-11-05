using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiLab.Collections;

public partial class SwatchItem : ObservableObject
{
    public SwatchItem(Color color, string name, double height, double width)
    {
        Color = color;
        Name = name;
        var varied = VariedSize(name);
        VariedWidth = varied.Width;
        VariedHeight = varied.Height;
        Height = height;
        Width = width;
    }

    public Color Color { get; }

    public string Name { get; }

    public double VariedWidth { get; }

    public double VariedHeight { get; }

    public Color NameInk => Color.GetLuminosity() > 0.45
        ? Microsoft.Maui.Graphics.Colors.Black
        : Microsoft.Maui.Graphics.Colors.White;

    [ObservableProperty]
    public partial bool IsPinned { get; set; }

    [ObservableProperty]
    public partial double Height { get; set; }

    [ObservableProperty]
    public partial double Width { get; set; }

    /// <summary>
    /// A stable size that does not follow the name's length.
    /// Width is 160, 200, 240, or 280. Height is 96, 132, 168, or 204.
    /// </summary>
    public static SwatchSize VariedSize(string name)
    {
        var hash = 2166136261u;
        foreach (var letter in name)
        {
            hash ^= letter;
            hash *= 16777619u;
        }

        var bucket = (int)(hash % 16);
        return new SwatchSize(Widths[bucket % 4], Heights[bucket / 4]);
    }

    static readonly double[] Widths = [160, 200, 240, 280];

    static readonly double[] Heights = [96, 132, 168, 204];
}

public readonly record struct SwatchSize(double Width, double Height);

public sealed class SwatchGroup : List<SwatchItem>
{
    public SwatchGroup(string letter, IEnumerable<SwatchItem> items)
        : base(items)
    {
        Letter = letter;
    }

    public string Letter { get; }
}
