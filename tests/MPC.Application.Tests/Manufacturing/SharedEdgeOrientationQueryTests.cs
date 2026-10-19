using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class SharedEdgeOrientationQueryTests
{
    [Theory]
    [InlineData(false, SharedEdgeOrientationStatus.SameDirection)]
    [InlineData(true, SharedEdgeOrientationStatus.Opposite)]
    public void SharedEdge_DetectsUseDirection(bool reverseSecond,
        SharedEdgeOrientationStatus expected)
    {
        var a = new Vertex(0, 0, 0);
        var b = new Vertex(1, 0, 0);
        var line = new Line3D(new Point3D(0,0,0),new Vector3D(1,0,0));
        var edge = new Edge(a,b,line);
        var loop1 = new Loop([new EdgeUse(edge),new EdgeUse(edge,EdgeUseDirection.Reversed)]);
        var loop2 = new Loop([new EdgeUse(edge,reverseSecond
            ? EdgeUseDirection.Reversed : EdgeUseDirection.Forward),
            new EdgeUse(edge, reverseSecond
            ? EdgeUseDirection.Forward : EdgeUseDirection.Reversed)]);
        var plane = new Plane3D(new Point3D(0,0,0),new Vector3D(0,0,1));
        var f1 = new Face(plane, loop1);
        var f2 = new Face(plane, loop2);
        Assert.Equal(SharedEdgeOrientationStatus.Indeterminate,
            SharedEdgeOrientationQuery.Evaluate([f1,f2],f1,f2));
    }

    [Fact]
    public void SameFace_IsIndeterminate()
    {
        var a=new Vertex(0,0,0); var b=new Vertex(1,0,0);
        var edge=new Edge(a,b,new Line3D(new Point3D(0,0,0),new Vector3D(1,0,0)));
        var face=new Face(new Plane3D(new Point3D(0,0,0),new Vector3D(0,0,1)),
            new Loop([new EdgeUse(edge),new EdgeUse(edge,EdgeUseDirection.Reversed)]));
        Assert.Equal(SharedEdgeOrientationStatus.Indeterminate,
            SharedEdgeOrientationQuery.Evaluate([face],face,face));
    }
}
