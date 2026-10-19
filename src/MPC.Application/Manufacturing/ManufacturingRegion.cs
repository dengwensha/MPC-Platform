using MPC.Geometry.Primitives;
using MPC.Mapping.Step;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Application.Manufacturing;

// V1: one mapped planar face, no stock or machining assumptions.
public sealed class ManufacturingRegion
{
    public string DocumentFingerprint { get; }
    public string SourceFaceId { get; }
    public Guid CanonicalFaceId { get; }
    public Point3D Origin { get; }
    public Vector3D SurfaceNormal { get; }
    public FaceOrientation Orientation { get; }
    public int OuterBoundaryEdgeCount { get; }
    public int InnerBoundaryCount { get; }

    private ManufacturingRegion(string fingerprint, string sourceId, Face face, Plane3D plane)
    {
        DocumentFingerprint = fingerprint;
        SourceFaceId = sourceId;
        CanonicalFaceId = face.Id;
        Origin = plane.Origin;
        SurfaceNormal = plane.Normal;
        Orientation = face.Orientation;
        OuterBoundaryEdgeCount = face.OuterLoop.EdgeUses.Count;
        InnerBoundaryCount = face.InnerLoops.Count;
    }

    // The caller must supply a SHA-256 hash of the exact imported STEP bytes.
    // The operation must have completed a successful Map first.
    internal static bool TryCreate(
        StepCanonicalMappingOperation operation,
        string documentSha256,
        string sourceFaceId,
        out ManufacturingRegion? region)
    {
        ArgumentNullException.ThrowIfNull(operation);
        region = null;
        if (string.IsNullOrWhiteSpace(documentSha256)
            || documentSha256.Length != 64
            || !documentSha256.All(Uri.IsHexDigit)
            || string.IsNullOrWhiteSpace(sourceFaceId))
            return false;

        if (!operation.FaceIdentities.TryGetFace(sourceFaceId, out Face? face)
            || face is null || face.Surface is not Plane3D plane)
            return false;

        region = new ManufacturingRegion(
            documentSha256.ToUpperInvariant(), sourceFaceId, face, plane);
        return true;
    }
}
