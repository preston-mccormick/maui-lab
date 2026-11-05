using MauiLab.Colors;

namespace MauiLabTests;

public class ColorCatalogTests
{
    [Fact]
    public void LoadIsCachedAndNamesTheLongListsBeforeReadingThem()
    {
        var first = ColorCatalog.Load();
        var again = ColorCatalog.Load();

        Assert.Same(first, again);
        Assert.Equal("XKCD color survey (949)", first[1].Name);
        Assert.Equal("12-letter names (3,099)", first[2].Name);
        Assert.Equal(949, first[1].Colors.Count);
        Assert.Equal(3099, first[2].Colors.Count);
    }
}
