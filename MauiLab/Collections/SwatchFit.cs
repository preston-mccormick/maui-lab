namespace MauiLab.Collections;

/// <summary>
/// The color swatches' fit: preferred cards of 180, a preferred gap of 36, and at least two columns.
/// The gap shrinks to 8 before the card narrows.
/// </summary>
public readonly record struct SwatchFit(int Span, double CardWidth, double Gap, double Side)
{
    public const double PreferredCardWidth = 180;
    public const double PreferredGap = 36;
    public const double MinimumGap = 8;

    public double Margin => Gap / 2;

    /// <summary>The horizontal inset the color page puts on the scroll view, outside the card margin.</summary>
    public double Padding
    {
        get
        {
            var horizontal = Math.Max(0, Side - Gap / 2);
            if (horizontal >= 1)
                horizontal -= 0.5;
            return horizontal;
        }
    }

    public static SwatchFit Measure(double width)
    {
        var span = Math.Max(2, (int)Math.Floor((width + PreferredGap) / (PreferredCardWidth + PreferredGap)));
        var card = PreferredCardWidth;
        var gap = PreferredGap;
        var side = SideOf(width, span, card, gap);

        if (side < 0)
        {
            var gapForPreferredCards = (width - span * card) / (span - 1);
            if (gapForPreferredCards >= MinimumGap)
                gap = gapForPreferredCards;
            else
            {
                gap = MinimumGap;
                card = (width - (span - 1) * gap) / span;
            }

            side = SideOf(width, span, card, gap);
        }

        if (side < gap / 2)
        {
            var gapMatchingHalfSide = (width - span * card) / span;
            if (gapMatchingHalfSide >= MinimumGap && gapMatchingHalfSide <= gap)
                gap = gapMatchingHalfSide;
            else
            {
                gap = MinimumGap;
                card = (width - span * gap) / span;
            }

            side = SideOf(width, span, card, gap);
        }

        if (card < 1)
            card = 1;
        if (side < 0)
            side = 0;

        return new SwatchFit(span, card, gap, side);
    }

    static double SideOf(double width, int span, double card, double gap) =>
        (width - span * card - (span - 1) * gap) / 2;
}
