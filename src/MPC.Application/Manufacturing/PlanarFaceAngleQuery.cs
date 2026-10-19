using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Application.Manufacturing;

/// <summary>Unsigned angle of underlying planar surface normals, not a machining dihedral.</summary>
public static class PlanarFaceAngleQuery
{
    public static bool TryCalculate(
        IReadOnlyList<Face> faces,
        Face first,
        Face second,
        out double angleDegrees)
    {
        ArgumentNullException.ThrowIfNull(faces);
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        angleDegrees = 0;

        if (ReferenceEquals(first, second))
            return false;
        if (first.Surface is not Plane3D planeA ||
            second.Surface is not Plane3D planeB)
            return false;

        FaceAdjacencyResult adjacency = FaceAdjacencyQuery.Analyze(faces, first);
        if (!adjacency.Boundaries.Any(boundary =>
            boundary.HasExactlyTwoFaceOwners &&
            boundary.Neighbors.Any(face => ReferenceEquals(face, second))))
            return false;

        Vector3D a = planeA.Normal;
        Vector3D b = planeB.Normal;
        double dot = a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        double cosine = Math.Clamp(dot, -1.0, 1.0);
        angleDegrees = Math.Acos(cosine) * 180.0 / Math.PI;
        return true;
    }
}
