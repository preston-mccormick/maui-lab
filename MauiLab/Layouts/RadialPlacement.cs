namespace MauiLab.Layouts;

public readonly record struct RadialSlot(double? AngleDegrees);

public static class RadialPlacement
{
    public static IReadOnlyList<Point> Place(
        Point center,
        double radius,
        double startDegrees,
        double sweepDegrees,
        IReadOnlyList<double?> angleOverrides)
    {
        var even = angleOverrides.Count(angle => angle is null);
        var next = 0;
        var points = new Point[angleOverrides.Count];
        for (var index = 0; index < angleOverrides.Count; index++)
        {
            var angle = angleOverrides[index] ?? EvenAngle(startDegrees, sweepDegrees, even, next++);
            points[index] = PointOnArc(center, radius, angle);
        }

        return points;
    }

    static double EvenAngle(double start, double sweep, int count, int index)
    {
        if (count <= 1)
            return start;
        return start + index * sweep / (count - 1);
    }

    public static Point PointOnArc(Point center, double radius, double degreesClockwiseFromTop)
    {
        var radians = degreesClockwiseFromTop * Math.PI / 180d;
        return new Point(
            center.X + radius * Math.Sin(radians),
            center.Y - radius * Math.Cos(radians));
    }
}
