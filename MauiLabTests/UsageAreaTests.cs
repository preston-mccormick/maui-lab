using MauiLab.Usage;

namespace MauiLabTests;

public class UsageAreaTests
{
    [Fact]
    public void Classify_NamesTheRectangleWhenItIsClear()
    {
        Assert.Equal("Full screen", UsageArea.Classify(390, 844, 390, 844, 0, 0));
        Assert.Equal("Split View", UsageArea.Classify(683, 1024, 1366, 1024, 0, 0));
        Assert.Equal("Slide Over", UsageArea.Classify(320, 1000, 1366, 1024, 1040, 0));
        Assert.Equal("Windowed", UsageArea.Classify(1250, 750, 1440, 900, 40, 40));
    }

    [Fact]
    public void Classify_LeavesCompactSplitUnset()
    {
        Assert.Null(UsageArea.Classify(341, 800, 1024, 800, 0, 0));
    }
}
