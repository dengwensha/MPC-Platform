using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class CommonEdgeSampleQueryTests
{
    [Fact]
    public void Line_ReturnsMidpointOnCurve()
    {
        var edge = new Edge(new Vertex(0, 0, 0), new Vertex(4, 0, 0),
            new Line3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0)));
        Assert.True(CommonEdgeSampleQuery.TrySample(edge, out var point));
        Assert.Equal(2, point.X);
        Assert.Equal(0, point.Y);
        Assert.Equal(0, point.Z);
    }

    [Fact]
    public void Circle_UsesValidatedEndpoint_NotChordMidpoint()
    {
        var edge = new Edge(new Vertex(2, 0, 0), new Vertex(0, 2, 0),
            new Circle3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1), 2));
        Assert.True(CommonEdgeSampleQuery.TrySample(edge, out var point));
        Assert.Equal(2, point.X);
        Assert.Equal(0, point.Y);
    }

    [Fact]
    public void InvalidCurveMembership_IsRejected()
    {
        var edge = new Edge(new Vertex(2, 0, 1), new Vertex(0, 2, 0),
            new Circle3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1), 2));
        Assert.False(CommonEdgeSampleQuery.TrySample(edge, out _));
    }

    [Fact]
    public void UnsupportedCurve_IsRejected()
    {
        var edge = new Edge(new Vertex(0, 0, 0), new Vertex(1, 0, 0),
            new Circle3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1), 2));
        Assert.False(CommonEdgeSampleQuery.TrySample(edge, out _));
    }
}
