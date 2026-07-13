namespace MPC.Geometry.Primitives;

public sealed record Arc3D
{
    public Circle3D Circle { get; }

    public double StartAngle { get; }

    public double EndAngle { get; }

    public ArcDirection Direction { get; }

    public Arc3D(
        Circle3D circle,
        double startAngle,
        double endAngle,
        ArcDirection direction)
    {
        if (!double.IsFinite(startAngle))
        {
            throw new ArgumentOutOfRangeException(
                nameof(startAngle),
                startAngle,
                "Start angle must be a finite number.");
        }

        if (!double.IsFinite(endAngle))
        {
            throw new ArgumentOutOfRangeException(
                nameof(endAngle),
                endAngle,
                "End angle must be a finite number.");
        }

        if (startAngle == endAngle)
        {
            throw new ArgumentException(
                "Start angle and end angle must be different.",
                nameof(endAngle));
        }

        if (!Enum.IsDefined(direction))
        {
            throw new ArgumentOutOfRangeException(
                nameof(direction),
                direction,
                "Arc direction is not valid.");
        }

        Circle = circle;
        StartAngle = startAngle;
        EndAngle = endAngle;
        Direction = direction;
    }
}