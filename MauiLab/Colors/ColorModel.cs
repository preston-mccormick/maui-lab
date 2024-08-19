using System.Reflection;

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

    public static List<ColorModel> GetNamedColors()
    {
        var swatches = new List<ColorModel>();

        Type mauiColors = typeof(Microsoft.Maui.Graphics.Colors);
        Type mauiColor = typeof(Color);
        var fields = mauiColors.GetFields(BindingFlags.Static | BindingFlags.Public);
        foreach (var field in fields)
        {
            if (field.FieldType == mauiColor)
            {
                object? value = field.GetValue(null);
                if (value == null) continue;
                var color = (Color)value;
                var swatch = new ColorModel(color, field.Name);
                swatches.Add(swatch);
            }
        }

        swatches.Sort();

        return swatches;
    }

}