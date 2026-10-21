using MPC.Geometry.Primitives;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Application.Manufacturing;

/// <summary>A signed local angle, not a concavity or material-side decision.</summary>
public static class SignedLocalDihedralQuery
{
    public static bool TryCalculate(IReadOnlyList<Face> faces, Face first, Face second,
        out double angleDegrees)
    {
        ArgumentNullException.ThrowIfNull(faces);
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        angleDegrees = 0;
        if (SharedEdgeOrientationQuery.Evaluate(faces, first, second)
            != SharedEdgeOrientationStatus.Opposite) return false;
        // F3 supports only planar pairs and verified straight shared edges.
        if (first.Surface is not Plane3D a || second.Surface is not Plane3D b)
            return false;
        var candidates = FaceAdjacencyQuery.Analyze(faces, first).Boundaries
            .Where(x => x.HasExactlyTwoFaceOwners &&
                x.Neighbors.Any(n => ReferenceEquals(n, second))).ToArray();
        if (candidates.Length != 1 || candidates[0].Edge.Curve is not Line3D ||
            !CommonEdgeSampleQuery.TrySample(candidates[0].Edge, out _))
            return false;
        Edge edge = candidates[0].Edge;
        Point3D p = edge.StartVertex.Position, q = edge.EndVertex.Position;
        // Both endpoints of the straight shared edge must lie on both supporting
        // planes. Perpendicular tangents alone do not prove plane membership.
        if (!OnPlane(a, p) || !OnPlane(a, q) ||
            !OnPlane(b, p) || !OnPlane(b, q)) return false;
        double tx=q.X-p.X, ty=q.Y-p.Y, tz=q.Z-p.Z;
        double length=Math.Sqrt(tx*tx+ty*ty+tz*tz);
        if (!double.IsFinite(length) || length<=1e-12) return false;
        tx/=length; ty/=length; tz/=length;
        // Canonical edge direction defines the angle sign. It is not
        // automatically the outward material orientation.
        if (!FirstUseForward(first, edge, out bool forward)) return false;
        if (!forward) { tx=-tx; ty=-ty; tz=-tz; }
        double ax=a.Normal.X, ay=a.Normal.Y, az=a.Normal.Z;
        double bx=b.Normal.X, by=b.Normal.Y, bz=b.Normal.Z;
        if(first.Orientation==FaceOrientation.Reversed) {ax=-ax;ay=-ay;az=-az;}
        if(second.Orientation==FaceOrientation.Reversed) {bx=-bx;by=-by;bz=-bz;}
        double crossX=ay*bz-az*by,crossY=az*bx-ax*bz,crossZ=ax*by-ay*bx;
        double sin=tx*crossX+ty*crossY+tz*crossZ;
        double dot=Math.Clamp(ax*bx+ay*by+az*bz,-1.0,1.0);
        // The two plane normals must be perpendicular to the edge tangent.
        if(Math.Abs(tx*ax+ty*ay+tz*az)>1e-8 ||
           Math.Abs(tx*bx+ty*by+tz*bz)>1e-8) return false;
        angleDegrees=Math.Atan2(sin,dot)*180.0/Math.PI;
        return double.IsFinite(angleDegrees);
    }

    private static bool OnPlane(Plane3D plane, Point3D point)
    {
        double dx = point.X - plane.Origin.X;
        double dy = point.Y - plane.Origin.Y;
        double dz = point.Z - plane.Origin.Z;
        double residual = dx * plane.Normal.X +
            dy * plane.Normal.Y + dz * plane.Normal.Z;
        return double.IsFinite(residual) && Math.Abs(residual) <= 1e-8;
    }

    private static bool FirstUseForward(Face face, Edge edge, out bool forward)
    {
        forward=false; int count=0;
        foreach(var loop in new[]{face.OuterLoopUse}.Concat(face.InnerLoopUses))
            foreach(var use in loop.Loop.EdgeUses)
                if(ReferenceEquals(use.Edge,edge))
                {
                    count++;
                    forward=(use.Direction==EdgeUseDirection.Forward)==
                        (loop.Direction==LoopUseDirection.Forward);
                }
        return count==1;
    }
}
