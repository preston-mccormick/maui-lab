using System.Globalization;
using System.Text.Json;

namespace MauiLab.Colors;

public sealed class ColorListOption
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public required IReadOnlyList<ColorModel> Colors { get; init; }
}

public static class ColorCatalog
{
    // XKCD color survey is CC0. The large list is color-name-list names of at most 12 letters (MIT).
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IReadOnlyList<ColorListOption> Load()
    {
        var small = ColorModel.GetNamedColors();
        var medium = LoadEmbedded("MauiLab.Colors.Data.xkcd-colors.json", properCase: true);
        var large = LoadEmbedded("MauiLab.Colors.Data.short-colors.json", properCase: false);
        return
        [
            new ColorListOption { Name = Label("HTML/CSS colors", small.Count), Description = "HTML/CSS colors", Colors = small },
            new ColorListOption { Name = Label("XKCD color survey", medium.Count), Description = "XKCD color survey", Colors = medium },
            new ColorListOption { Name = Label("12-letter names", large.Count), Description = "12-letter names", Colors = large },
        ];
    }

    private static string Label(string description, int count) =>
        string.Create(CultureInfo.InvariantCulture, $"{description} ({count:N0})");

    private static List<ColorModel> LoadEmbedded(string resourceName, bool properCase)
    {
        var assembly = typeof(ColorCatalog).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing color catalog '{resourceName}'.");
        var entries = JsonSerializer.Deserialize<List<NamedColorEntry>>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"Color catalog '{resourceName}' was empty.");

        var colors = new List<ColorModel>(entries.Count);
        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Name) || string.IsNullOrWhiteSpace(entry.Hex))
                continue;

            var name = properCase ? ToProperCase(entry.Name) : entry.Name;
            colors.Add(new ColorModel(Color.FromArgb(entry.Hex), name));
        }

        colors.Sort();
        return colors;
    }

    // Capitalize each word. An apostrophe does not start a new word, so "robin's egg" stays "Robin's Egg".
    private static string ToProperCase(string name)
    {
        var chars = name.ToCharArray();
        var capitalize = true;
        for (var i = 0; i < chars.Length; i++)
        {
            if (char.IsLetter(chars[i]))
            {
                chars[i] = capitalize
                    ? char.ToUpper(chars[i], CultureInfo.InvariantCulture)
                    : char.ToLower(chars[i], CultureInfo.InvariantCulture);
                capitalize = false;
            }
            else
            {
                capitalize = chars[i] is not ('\'' or '\u2019');
            }
        }

        return new string(chars);
    }

    private sealed class NamedColorEntry
    {
        public string? Name { get; set; }
        public string? Hex { get; set; }
    }
}
