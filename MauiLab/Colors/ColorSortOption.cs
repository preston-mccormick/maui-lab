using System.ComponentModel;

namespace MauiLab.Colors;

public class ColorSortOption
{
    public ListSortDirection Direction { get; set; }

    public string Name { get; set; }

    public ColorSortType SortType { get; set; }

    public static List<ColorSortOption> GetColorSortOptions()
    {
        return new List<ColorSortOption> {
                new ColorSortOption
                {
                    Name = "Name (ascending)",
                    SortType = ColorSortType.Name,
                    Direction = ListSortDirection.Ascending
                },
                new ColorSortOption
                {
                    Name = "Name (descending)",
                    SortType = ColorSortType.Name,
                    Direction = ListSortDirection.Descending
                },
                new ColorSortOption
                {
                    Name = "Luminosity (light to dark)",
                    SortType = ColorSortType.Luminocity,
                    Direction = ListSortDirection.Descending
                },
                new ColorSortOption
                {
                    Name = "Luminosity (dark to light)",
                    SortType = ColorSortType.Luminocity,
                    Direction = ListSortDirection.Ascending
                },
                new ColorSortOption
                {
                    Name = "Hue (ascending)",
                    SortType = ColorSortType.Hue,
                    Direction = ListSortDirection.Ascending
                },
                new ColorSortOption
                {
                    Name = "Hue (descending)",
                    SortType = ColorSortType.Hue,
                    Direction = ListSortDirection.Descending
                },
                new ColorSortOption
                {
                    Name = "Saturation (dull to intense)",
                    SortType = ColorSortType.Saturation,
                    Direction = ListSortDirection.Ascending
                },
                new ColorSortOption
                {
                    Name = "Saturation (intense to dull)",
                    SortType = ColorSortType.Saturation,
                    Direction = ListSortDirection.Descending
                },
            };
    }
}