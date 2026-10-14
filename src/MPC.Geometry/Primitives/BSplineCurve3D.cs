using MPC.Geometry.Contracts;

namespace MPC.Geometry.Primitives;

public sealed class BSplineCurve3D : ICurve3D, IEquatable<BSplineCurve3D>
{
    public int Degree { get; }
    public IReadOnlyList<Point3D> ControlPoints { get; }
    public IReadOnlyList<int> Multiplicities { get; }
    public IReadOnlyList<double> Knots { get; }

    public BSplineCurve3D(int degree, IEnumerable<Point3D> controlPoints,
        IEnumerable<int> multiplicities, IEnumerable<double> knots)
    {
        ArgumentNullException.ThrowIfNull(controlPoints);
        ArgumentNullException.ThrowIfNull(multiplicities);
        ArgumentNullException.ThrowIfNull(knots);
        if (degree < 1) throw new ArgumentOutOfRangeException(nameof(degree));
        Point3D[] points = controlPoints.ToArray();
        if (points.Length <= degree)
            throw new ArgumentException("Expected degree plus one control points.", nameof(controlPoints));
        if (points.Any(p => !double.IsFinite(p.X) || !double.IsFinite(p.Y) || !double.IsFinite(p.Z)))
            throw new ArgumentOutOfRangeException(nameof(controlPoints), "Control points must be finite.");
        int[] counts = multiplicities.ToArray();
        double[] values = knots.ToArray();
        if (values.Length < 2 || values.Any(x => !double.IsFinite(x)) ||
            values.Skip(1).Where((x, i) => x <= values[i]).Any())
            throw new ArgumentException("Distinct knots must be finite and strictly increasing.", nameof(knots));
        if (counts.Length != values.Length ||
            counts.Where((x, i) => x <= 0 || x > (i == 0 || i == counts.Length - 1 ? degree + 1 : degree)).Any() ||
            counts.Sum(x => (long)x) != points.Length + (long)degree + 1)
            throw new ArgumentException("Invalid degree-based knot multiplicities.", nameof(multiplicities));
        Degree = degree;
        ControlPoints = Array.AsReadOnly(points);
        Multiplicities = Array.AsReadOnly(counts);
        Knots = Array.AsReadOnly(values);
    }

    public bool Equals(BSplineCurve3D? other) => other is not null && Degree == other.Degree &&
        ControlPoints.SequenceEqual(other.ControlPoints) && Multiplicities.SequenceEqual(other.Multiplicities) &&
        Knots.SequenceEqual(other.Knots);

    public override bool Equals(object? obj) => obj is BSplineCurve3D curve && Equals(curve);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Degree);
        foreach (Point3D point in ControlPoints) hash.Add(point);
        foreach (int count in Multiplicities) hash.Add(count);
        foreach (double knot in Knots) hash.Add(knot);
        return hash.ToHashCode();
    }

    public static bool operator ==(BSplineCurve3D? left, BSplineCurve3D? right) =>
        ReferenceEquals(left, right) || left is not null && left.Equals(right);
    public static bool operator !=(BSplineCurve3D? left, BSplineCurve3D? right) => !(left == right);
}
