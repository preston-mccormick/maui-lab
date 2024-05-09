namespace MauiLab.Colors;

public class ColorModel : IComparable<ColorModel>
{
    public ColorModel(Color color, string name)
    {
        Color = color;
        Name = name;
    }

    public Color Color { get;  }

    public string Name { get; }

    /// <summary>
    /// Default sort by name.
    /// </summary>
    public int CompareTo(ColorModel? other)
    {
        if (other == null) return 1;
        else return Comparer<string>.Default.Compare(Name, other.Name);
    }
}