using System.Globalization;

namespace MauiLab.Layouts;

/// <summary>
/// Maps a slider position onto a numeric setting, with optional stops for an unset
/// value (usually -1) and for positive infinity.
/// </summary>
public readonly record struct NumberScale(
    double Minimum,
    double Maximum,
    double Step,
    bool HasUnset,
    double UnsetValue,
    bool HasUnbounded)
{
    public const double SliderEnd = 100;
    public const double Stop = 8;

    public double ToSlider(double value)
    {
        if (IsUnset(value))
            return 0;
        if (HasUnbounded && double.IsPositiveInfinity(value))
            return SliderEnd;

        var (low, high) = NumericEnds();
        var span = Maximum - Minimum;
        var t = span <= 0 ? 0 : (Math.Clamp(value, Minimum, Maximum) - Minimum) / span;
        return low + t * (high - low);
    }

    public double FromSlider(double slider)
    {
        var (low, high) = NumericEnds();
        if (HasUnset && slider < low)
            return UnsetValue;
        if (HasUnbounded && slider > high)
            return double.PositiveInfinity;

        var span = high - low;
        var t = span <= 0 ? 0 : (Math.Clamp(slider, low, high) - low) / span;
        return Quantize(Minimum + t * (Maximum - Minimum));
    }

    public string Format(double value)
    {
        if (IsUnset(value))
            return double.IsNaN(UnsetValue) ? "" : Trim(UnsetValue);
        if (double.IsPositiveInfinity(value))
            return "∞";
        if (double.IsNegativeInfinity(value))
            return "-∞";
        return Trim(value);
    }

    public bool TryParse(string? text, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            if (HasUnset && double.IsNaN(UnsetValue))
            {
                value = double.NaN;
                return true;
            }

            return false;
        }

        var trimmed = text.Trim();
        if (HasUnbounded && trimmed is "∞" or "+∞" or "Infinity" or "+Infinity" or "inf" or "+inf")
        {
            value = double.PositiveInfinity;
            return true;
        }

        return double.TryParse(trimmed, NumberStyles.Float, CultureInfo.CurrentCulture, out value)
            || double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    public bool IsUnset(double value) =>
        HasUnset && (double.IsNaN(UnsetValue) ? double.IsNaN(value) : value == UnsetValue);

    (double Low, double High) NumericEnds() =>
        (HasUnset ? Stop : 0, HasUnbounded ? SliderEnd - Stop : SliderEnd);

    double Quantize(double value)
    {
        if (Step <= 0)
            return value;

        var steps = Math.Round((value - Minimum) / Step, MidpointRounding.AwayFromZero);
        return Math.Clamp(Minimum + steps * Step, Minimum, Maximum);
    }

    public IReadOnlyList<double> TickValues()
    {
        var span = Maximum - Minimum;
        if (!(span > 0))
            return [Minimum];

        if (Step > 0)
        {
            var steps = (int)Math.Round(span / Step);
            if (steps is > 0 and <= 12)
            {
                var all = new double[steps + 1];
                for (var i = 0; i <= steps; i++)
                    all[i] = Minimum + i * Step;
                return all;
            }
        }

        var interval = NiceInterval(span / 4);
        var ticks = new List<double> { Minimum };
        var value = Math.Ceiling(Minimum / interval) * interval;
        if (value <= Minimum + interval * 0.01)
            value += interval;
        while (value < Maximum - interval * 0.01)
        {
            ticks.Add(value);
            value += interval;
        }

        ticks.Add(Maximum);
        return ticks;
    }

    static double NiceInterval(double raw)
    {
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(raw, 1e-9))));
        var normalized = raw / magnitude;
        var factor = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;
        return factor * magnitude;
    }

    static string Trim(double value) =>
        Math.Abs(value - Math.Round(value)) < 0.0001
            ? Math.Round(value).ToString("0", CultureInfo.CurrentCulture)
            : value.ToString("0.##", CultureInfo.CurrentCulture);
}
