namespace MPC.Tests.Topology;

using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

public sealed class EdgeTests
{
    private sealed class TestCurve3D : ICurve3D
    {
    }

    [Fact]
    public void Constructor_Should_Set_StartVertex_EndVertex_And_Curve()
    {
        var start = new Vertex(new Point3D(0, 0, 0));
        var end = new Vertex(new Point3D(10, 0, 0));
        var curve = new TestCurve3D();

        var edge = new Edge(start, end, curve);

        Assert.Equal(start, edge.StartVertex);
        Assert.Equal(end, edge.EndVertex);
        Assert.Equal(curve, edge.Curve);
    }
}