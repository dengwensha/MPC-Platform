using MPC.Geometry.Contracts;

namespace MPC.Geometry.Primitives;

/// <summary>A B-spline basis and its unnormalised, ordered positive weights.</summary>
public sealed class RationalBSplineSurface3D : ISurface3D, IEquatable<RationalBSplineSurface3D>
{
    public BSplineSurface3D Basis { get; }
    public IReadOnlyList<IReadOnlyList<double>> Weights { get; }

    public RationalBSplineSurface3D(BSplineSurface3D basis, IEnumerable<IEnumerable<double>> weights)
    {
        ArgumentNullException.ThrowIfNull(basis);
        ArgumentNullException.ThrowIfNull(weights);
        var rows = new List<IReadOnlyList<double>>();
        foreach (IEnumerable<double> source in weights)
        {
            if (source is null) throw new ArgumentException("Weight rows cannot be null.", nameof(weights));
            double[] row = source.ToArray();
            if (row.Length != basis.ControlPointGrid[0].Count)
                throw new ArgumentException("Weights must match the control-point grid.", nameof(weights));
            if (row.Any(value => !double.IsFinite(value) || value <= 0.0))
                throw new ArgumentOutOfRangeException(nameof(weights), "Weights must be finite and positive.");
            rows.Add(Array.AsReadOnly(row));
        }
        if (rows.Count != basis.ControlPointGrid.Count)
            throw new ArgumentException("Weights must match the control-point grid.", nameof(weights));
        Basis = basis;
        Weights = Array.AsReadOnly(rows.ToArray());
    }

    public bool Equals(RationalBSplineSurface3D? other) =>
        other is not null && Basis.Equals(other.Basis) &&
        Weights.SelectMany(row => row).SequenceEqual(other.Weights.SelectMany(row => row));

    public override bool Equals(object? obj) => obj is RationalBSplineSurface3D other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Basis);
        foreach (double weight in Weights.SelectMany(row => row)) hash.Add(weight);
        return hash.ToHashCode();
    }

    public static bool operator ==(RationalBSplineSurface3D? left, RationalBSplineSurface3D? right) =>
        ReferenceEquals(left, right) || left is not null && left.Equals(right);

    public static bool operator !=(RationalBSplineSurface3D? left, RationalBSplineSurface3D? right) =>
        !(left == right);
}
