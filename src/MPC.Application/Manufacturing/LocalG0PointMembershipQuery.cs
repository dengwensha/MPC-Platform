using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Application.Manufacturing;

public enum LocalG0Status
{
    Indeterminate,
    CoincidentAtSample,
    MismatchAtSample
}

/// <summary>Evidence of underlying analytic surface membership at one common-edge witness.
/// Does not prove trimmed-face membership or continuity along the full boundary.</summary>
public static class LocalG0PointMembershipQuery
{
    public static LocalG0Status Evaluate(
        IReadOnlyList<Face> faces, Face first, Face second,
        double linearTolerance, out Point3D sample)
    {
        ArgumentNullException.ThrowIfNull(faces);
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        if (!double.IsFinite(linearTolerance) || linearTolerance < 0)
            throw new ArgumentOutOfRangeException(nameof(linearTolerance));
        sample = default;
        if (ReferenceEquals(first, second)) return LocalG0Status.Indeterminate;
        var boundaries = FaceAdjacencyQuery.Analyze(faces, first).Boundaries
            .Where(b => b.HasExactlyTwoFaceOwners &&
                b.Neighbors.Any(n => ReferenceEquals(n, second))).ToArray();
        if (boundaries.Length != 1 ||
            !CommonEdgeSampleQuery.TrySample(boundaries[0].Edge, out var witness))
            return LocalG0Status.Indeterminate;
        if (!TryResidual(first, witness, out double a) ||
            !TryResidual(second, witness, out double b))
            return LocalG0Status.Indeterminate;
        sample = witness;
        return a <= linearTolerance && b <= linearTolerance
            ? LocalG0Status.CoincidentAtSample
            : LocalG0Status.MismatchAtSample;
    }

    private static bool TryResidual(Face face, Point3D p, out double residual)
    {
        residual = 0;
        switch (face.Surface)
        {
            case Plane3D plane:
            {
                var n = plane.Normal;
                residual = Math.Abs((p.X - plane.Origin.X) * n.X +
                    (p.Y - plane.Origin.Y) * n.Y +
                    (p.Z - plane.Origin.Z) * n.Z);
                break;
            }
            case Cylinder3D cylinder:
            {
                var axis = cylinder.AxisDirection;
                double x = p.X - cylinder.AxisOrigin.X;
                double y = p.Y - cylinder.AxisOrigin.Y;
                double z = p.Z - cylinder.AxisOrigin.Z;
                double h = x * axis.X + y * axis.Y + z * axis.Z;
                residual = Math.Abs(Math.Sqrt(
                    Math.Pow(x - h * axis.X, 2) +
                    Math.Pow(y - h * axis.Y, 2) +
                    Math.Pow(z - h * axis.Z, 2)) - cylinder.Radius);
                break;
            }
            case Cone3D cone:
            {
                var axis = cone.AxisDirection;
                double x = p.X - cone.Apex.X;
                double y = p.Y - cone.Apex.Y;
                double z = p.Z - cone.Apex.Z;
                double h = x * axis.X + y * axis.Y + z * axis.Z;
                if (h <= 0) return false;
                double radial = Math.Sqrt(
                    Math.Pow(x - h * axis.X, 2) +
                    Math.Pow(y - h * axis.Y, 2) +
                    Math.Pow(z - h * axis.Z, 2));
                residual = Math.Abs(radial - h * Math.Tan(cone.SemiAngle)) *
                    Math.Cos(cone.SemiAngle);
                break;
            }
            default:
                return false;
        }
        return double.IsFinite(residual);
    }
}
