using MauiLab.FormControls;

namespace MauiLabTests;

public class FormSamplesTests
{
    readonly IReadOnlyList<FormControlsViewModel> samples = FormSamples.Create();

    [Fact]
    public void FiveSamplesInSpecOrder()
    {
        Assert.Equal(
            [FormSamples.Empty, FormSamples.Short, FormSamples.Long, FormSamples.Valid, FormSamples.Invalid],
            samples.Select(s => s.Name).ToArray());
    }

    [Fact]
    public void Empty_HasNullsAndZeros()
    {
        var empty = samples[0];
        Assert.Null(empty.FullName);
        Assert.Null(empty.Required);
        Assert.Null(empty.Email);
        Assert.Null(empty.Letters);
        Assert.Null(empty.Choice);
        Assert.Null(empty.Date);
        Assert.Null(empty.Time12);
        Assert.Null(empty.RadioChoice);
        Assert.False(empty.Box1);
        Assert.False(empty.Switch2);
        Assert.Equal(0, empty.SliderValue);
        Assert.Equal(0, empty.StepperValue);
        Assert.Equal(0, empty.Stars);
        Assert.Equal(0, empty.PressCount);
    }

    [Fact]
    public void Invalid_HasTheSpecifiedBadValues()
    {
        var invalid = samples[4];
        Assert.Equal("   ", invalid.Required);
        Assert.Equal("not-an-email", invalid.Email);
        Assert.Equal("0", invalid.Number);
        Assert.Equal("Yes", invalid.Exact);
        Assert.Equal("abcdef", invalid.Digit);
        Assert.Equal("not a url", invalid.Url);
        Assert.Equal("abc123", invalid.Letters);
        Assert.Null(invalid.Choice);
    }

    [Fact]
    public void Valid_PassesEveryRule()
    {
        var valid = samples[3];
        Assert.False(string.IsNullOrWhiteSpace(valid.Required));
        Assert.Equal("ada@example.com", valid.Email);
        Assert.Equal("7", valid.Number);
        Assert.Equal("yes", valid.Exact);
        Assert.Contains(valid.Digit!, char.IsDigit);
        Assert.True(Uri.IsWellFormedUriString(valid.Url, UriKind.Absolute));
        Assert.Matches("^[A-Za-z]+$", valid.Letters);
        Assert.Equal("Green", valid.Choice);
        Assert.False(valid.Box1);
        Assert.True(valid.Box2);
        Assert.True(valid.DisabledBox);
        Assert.False(valid.Switch1);
        Assert.True(valid.Switch2);
        Assert.True(valid.DisabledSwitch);
    }

    [Fact]
    public void Long_FillsSliderAndRatings()
    {
        var longSample = samples[2];
        Assert.Equal(100, longSample.SliderValue);
        Assert.Equal(5, longSample.Stars);
        Assert.Equal(5, longSample.Hearts);
        Assert.Equal(5, longSample.ReadOnlyRating);
        Assert.True(longSample.ReadOnlyText!.Length > 250);
    }

    [Fact]
    public void ConstrainedDates_NeverExceedToday()
    {
        foreach (var sample in samples)
        {
            if (sample.ConstrainedDate is { } date)
                Assert.True(date <= sample.Today);
        }
    }

    [Fact]
    public void Press_IncrementsOnlyItsOwnForm()
    {
        var empty = samples[0];
        var shortSample = samples[1];

        empty.PressCommand.Execute(null);
        empty.PressCommand.Execute(null);

        Assert.Equal(2, empty.PressCount);
        Assert.Equal(0, shortSample.PressCount);
    }
}
