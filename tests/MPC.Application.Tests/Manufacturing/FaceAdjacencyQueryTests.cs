using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class FaceAdjacencyQueryTests
{
    [Fact]
    public void SharedEdge_ReportsOneNeighborAndUnsharedBoundaries()
    {
        var (selected, neighbor) = TrianglePair();
        FaceAdjacencyResult result = FaceAdjacencyQuery.Analyze(
            [selected, neighbor], selected);

        Assert.Equal(3, result.Boundaries.Count);
        Assert.Same(neighbor, Assert.Single(result.UniqueNeighbors));
        Assert.Single(result.Boundaries.Where(x => x.HasExactlyTwoFaceOwners));
        Assert.Equal(2, result.Boundaries.Count(x => !x.HasExactlyTwoFaceOwners));
        Assert.Same(neighbor, Assert.Single(
            Assert.Single(result.Boundaries.Where(x => x.HasExactlyTwoFaceOwners))
                .Neighbors));
        Assert.All(result.Boundaries, x => Assert.False(x.IsInnerBoundary));
    }

    [Fact]
    public void MissingSelectedFaceAndDuplicateScope_AreRejected()
    {
        var (selected, neighbor) = TrianglePair();
        Assert.Throws<ArgumentException>(() =>
            FaceAdjacencyQuery.Analyze([selected], neighbor));
        Assert.Throws<ArgumentException>(() =>
            FaceAdjacencyQuery.Analyze([selected, selected], selected));
    }

    [Fact]
    public void NoNeighbor_WhenOtherFaceHasIndependentEdges()
    {
        var (selected, _) = TrianglePair();
        var (other, _) = TrianglePair();
        var result = FaceAdjacencyQuery.Analyze([selected, other], selected);
        Assert.Empty(result.UniqueNeighbors);
        Assert.All(result.Boundaries, x => Assert.False(x.HasExactlyTwoFaceOwners));
    }

    private static (Face, Face) TrianglePair()
    {
        var a = new Vertex(0, 0, 0);
        var b = new Vertex(1, 0, 0);
        var c = new Vertex(0, 1, 0);
        var d = new Vertex(0, -1, 0);
        var curve = new Line3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));
        var shared = new Edge(a, b, curve);
        var bc = new Edge(b, c, curve);
        var ca = new Edge(c, a, curve);
        var ad = new Edge(a, d, curve);
        var db = new Edge(d, b, curve);
        var plane = new Plane3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1));
        var first = new Face(plane, new Loop([
            new EdgeUse(shared), new EdgeUse(bc), new EdgeUse(ca)]));
        var second = new Face(plane, new Loop([
            new EdgeUse(shared, EdgeUseDirection.Reversed),
            new EdgeUse(ad), new EdgeUse(db)]));
        return (first, second);
    }
}
