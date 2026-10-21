using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Application.Manufacturing;

/// <summary>Unsigned local angle, not an oriented dihedral or concavity decision.</summary>
public static class PlanarCurvedFaceAngleQuery
{
    public static bool TryCalculate(
        IReadOnlyList<Face> faces, Face first, Face second, out double angleDegrees)
        => TryCalculate(faces, first, second, 1e-8, out angleDegrees);

    /// <summary>Uses the supplied linear tolerance for edge and curved-surface membership.</summary>
    public static bool TryCalculate(IReadOnlyList<Face> faces, Face first,
        Face second, double linearTolerance, out double angleDegrees)
    {
        if (!double.IsFinite(linearTolerance) || linearTolerance < 0)
            throw new ArgumentOutOfRangeException(nameof(linearTolerance));
        ArgumentNullException.ThrowIfNull(faces);
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        angleDegrees = 0;
        if (ReferenceEquals(first, second)) return false;

        Face planeFace, curvedFace;
        if (first.Surface is Plane3D &&
            (second.Surface is Cylinder3D || second.Surface is Cone3D))
            (planeFace, curvedFace) = (first, second);
        else if (second.Surface is Plane3D &&
            (first.Surface is Cylinder3D || first.Surface is Cone3D))
            (planeFace, curvedFace) = (second, first);
        else return false;

        var boundaries = FaceAdjacencyQuery.Analyze(faces, planeFace).Boundaries
            .Where(b => b.HasExactlyTwoFaceOwners &&
                b.Neighbors.Any(n => ReferenceEquals(n, curvedFace))).ToArray();
        if (boundaries.Length != 1 ||
            !CommonEdgeSampleQuery.TrySample(boundaries[0].Edge, linearTolerance, out var sample))
            return false;

        Vector3D normal = default;
        bool valid = curvedFace.Surface switch
        {
            Cylinder3D cylinder => CylinderLocalNormalQuery.TryEvaluate(
                cylinder, sample, linearTolerance, out normal),
            Cone3D cone => ConeLocalNormalQuery.TryEvaluate(cone, sample, linearTolerance, out normal),
            _ => false
        };
        if (!valid) return false;
        var plane = ((Plane3D)planeFace.Surface).Normal;
        double dot = plane.X * normal.X + plane.Y * normal.Y + plane.Z * normal.Z;
        angleDegrees = Math.Acos(Math.Clamp(dot, -1.0, 1.0)) * 180.0 / Math.PI;
        return true;
    }
}
