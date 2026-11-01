using MauiLab.FormControls;

namespace MauiLabTests;

public class FormColumnCountTests
{
    [Theory]
    [InlineData(639, 1)]
    [InlineData(640, 2)]
    [InlineData(959, 2)]
    [InlineData(960, 3)]
    [InlineData(1400, 3)]
    [InlineData(0, 1)]
    [InlineData(-10, 1)]
    public void ForWidth_BreaksAt640And960(double width, int expected) =>
        Assert.Equal(expected, FormColumnCount.ForWidth(width));

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void ForWidth_NonFiniteIsOneColumn(double width) =>
        Assert.Equal(1, FormColumnCount.ForWidth(width));
}
