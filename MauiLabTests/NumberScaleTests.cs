using MauiLab.Layouts;

namespace MauiLabTests;

public class NumberScaleTests
{
    static NumberScale Requests() => new(0, 400, 0, true, -1, true);

    [Fact]
    public void UnsetStopIsNegativeOne()
    {
        var scale = Requests();

        Assert.Equal(-1, scale.FromSlider(0));
        Assert.Equal(0, scale.ToSlider(-1));
        Assert.Equal("-1", scale.Format(-1));
    }

    [Fact]
    public void UnboundedStopIsInfinity()
    {
        var scale = Requests();

        Assert.Equal(double.PositiveInfinity, scale.FromSlider(100));
        Assert.Equal(100, scale.ToSlider(double.PositiveInfinity));
        Assert.Equal("∞", scale.Format(double.PositiveInfinity));
        Assert.True(scale.TryParse("∞", out var parsed));
        Assert.True(double.IsPositiveInfinity(parsed));
    }

    [Fact]
    public void EndsOfTheRangeStayFinite()
    {
        var scale = Requests();

        Assert.Equal(0, scale.FromSlider(NumberScale.Stop));
        Assert.Equal(400, scale.FromSlider(NumberScale.SliderEnd - NumberScale.Stop));
    }

    [Fact]
    public void MiddleMapsToTheMiddleOfTheRange()
    {
        var scale = Requests();

        Assert.Equal(200, scale.FromSlider(50), precision: 3);
    }

    [Fact]
    public void StepSnapsToWholeNumbers()
    {
        var scale = new NumberScale(0, 12, 1, false, -1, false);

        Assert.Equal(6, scale.FromSlider(50));
    }

    [Fact]
    public void StepOneRoundsAFractionalPosition()
    {
        var scale = new NumberScale(0, 80, 1, false, -1, false);

        Assert.Equal(6, scale.FromSlider(7));
    }

    [Fact]
    public void BlankUnsetParsesEmptyAsNaN()
    {
        var scale = new NumberScale(0, 360, 0, true, double.NaN, false);

        Assert.True(scale.TryParse("", out var value));
        Assert.True(double.IsNaN(value));
        Assert.Equal("", scale.Format(double.NaN));
        Assert.Equal(0, scale.ToSlider(double.NaN));
        Assert.True(double.IsNaN(scale.FromSlider(0)));
    }
}
