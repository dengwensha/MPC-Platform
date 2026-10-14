using MPC.Geometry.Contracts;

namespace MPC.Geometry.Primitives;

/// <summary>A B-spline basis and its unnormalised, ordered positive weights.</summary>
public sealed class RationalBSplineCurve3D : ICurve3D, IEquatable<RationalBSplineCurve3D>
{
    public BSplineCurve3D Basis { get; }
    public IReadOnlyList<double> Weights { get; }

    public RationalBSplineCurve3D(BSplineCurve3D basis, IEnumerable<double> weights)
    {
        ArgumentNullException.ThrowIfNull(basis);
        ArgumentNullException.ThrowIfNull(weights);
        double[] values = weights.ToArray();
        if (values.Length != basis.ControlPoints.Count)
            throw new ArgumentException("Weights must match the control-point count.", nameof(weights));
        if (values.Any(value => !double.IsFinite(value) || value <= 0.0))
            throw new ArgumentOutOfRangeException(nameof(weights), "Weights must be finite and positive.");
        Basis = basis;
        Weights = Array.AsReadOnly(values);
    }

    public bool Equals(RationalBSplineCurve3D? other) =>
        other is not null && Basis.Equals(other.Basis) && Weights.SequenceEqual(other.Weights);

    public override bool Equals(object? obj) => obj is RationalBSplineCurve3D other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Basis);
        foreach (double weight in Weights) hash.Add(weight);
        return hash.ToHashCode();
    }

    public static bool operator ==(RationalBSplineCurve3D? left, RationalBSplineCurve3D? right) =>
        ReferenceEquals(left, right) || left is not null && left.Equals(right);

    public static bool operator !=(RationalBSplineCurve3D? left, RationalBSplineCurve3D? right) =>
        !(left == right);
}
