using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class CommonEdgeMultiSampleQueryTests
{
    [Fact]
    public void ValidStraightEdge_HasThreeOrderedInteriorSamples()
    {
        var edge = new Edge(new Vertex(0, 0, 0), new Vertex(4, 0, 0),
            new Line3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0)));
        Assert.True(CommonEdgeMultiSampleQuery.TrySample(edge, out var points));
        Assert.Equal(3, points.Count);
        Assert.Equal(1, points[0].X, 12);
        Assert.Equal(2, points[1].X, 12);
        Assert.Equal(3, points[2].X, 12);
    }

    [Fact]
    public void OffCurveEndpoint_IsRejected()
    {
        var edge = new Edge(new Vertex(0, 1, 0), new Vertex(4, 0, 0),
            new Line3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0)));
        Assert.False(CommonEdgeMultiSampleQuery.TrySample(edge, out var points));
        Assert.Empty(points);
    }

    [Fact]
    public void DegenerateEdge_IsRejected()
    {
        var edge = new Edge(new Vertex(0, 0, 0), new Vertex(0, 0, 0),
            new Line3D(new Point3D(0, 0, 0), new Vector3D(1, 0, 0)));
        Assert.False(CommonEdgeMultiSampleQuery.TrySample(edge, out _));
    }

    [Fact]
    public void CircularEdge_WithoutTrimParameters_IsRejected()
    {
        var edge = new Edge(new Vertex(1, 0, 0), new Vertex(0, 1, 0),
            new Circle3D(new Point3D(0, 0, 0), new Vector3D(0, 0, 1), 1));
        Assert.False(CommonEdgeMultiSampleQuery.TrySample(edge, out var points));
        Assert.Empty(points);
    }
}
