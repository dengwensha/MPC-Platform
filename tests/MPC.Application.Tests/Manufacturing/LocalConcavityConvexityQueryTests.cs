using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class LocalConcavityConvexityQueryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BooleanClaimCannotGrantMaterialSideClassification(bool claimedVerified)
    {
        var point = new Vertex(0, 0, 0);
        var next = new Vertex(1, 0, 0);
        var line = new Line3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0));
        var edge = new Edge(point, next, line);
        var returnEdge = new Edge(next, point, line);
        var loop = new Loop([new EdgeUse(edge), new EdgeUse(returnEdge)]);
        var face = new Face(new Plane3D(
            new Point3D(0, 0, 0), new Vector3D(0, 0, 1)), loop);
        Assert.Equal(LocalDihedralShape.Indeterminate,
            LocalConcavityConvexityQuery.Evaluate([face], face, face,
                claimedVerified, 0.5));
    }

    [Fact]
    public void InvalidTolerance_IsRejected()
    {
        var face = new Face(new Plane3D(
            new Point3D(0, 0, 0), new Vector3D(0, 0, 1)),
            new Loop([
                new EdgeUse(new Edge(new Vertex(0,0,0), new Vertex(1,0,0),
                    new Line3D(new Point3D(0,0,0), new Vector3D(1,0,0)))),
                new EdgeUse(new Edge(new Vertex(1,0,0), new Vertex(0,0,0),
                    new Line3D(new Point3D(0,0,0), new Vector3D(1,0,0))))
            ]));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            LocalConcavityConvexityQuery.Evaluate([face], face, face, false, 90));
    }
}
