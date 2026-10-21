using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class SignedLocalDihedralOracleTests
{
    [Theory]
    [InlineData(30.0)]
    [InlineData(60.0)]
    [InlineData(90.0)]
    [InlineData(120.0)]
    [InlineData(150.0)]
    public void PlaneRotation_MatchesIndependentAtan2Oracle(double degrees)
    {
        var (first, second) = Pair(degrees, false, false);
        Assert.True(SignedLocalDihedralQuery.TryCalculate([first,second],
            first, second, out double angle));
        Assert.InRange(angle, degrees - 1e-8, degrees + 1e-8);
        Assert.True(SignedLocalDihedralQuery.TryCalculate([first,second],
            second, first, out double reverse));
        Assert.InRange(reverse, degrees - 1e-8, degrees + 1e-8);
    }

    [Fact]
    public void ReversedSecondFaceOrientation_ChangesSignedRotation()
    {
        var (first, second) = Pair(90, true, false);
        Assert.True(SignedLocalDihedralQuery.TryCalculate([first,second],
            first, second, out double actual));
        Assert.Equal(-90, actual, 8);
    }

    [Fact]
    public void SameDirectionEdgeUses_AreNotSupported()
    {
        var (first, second) = Pair(90, false, true);
        Assert.False(SignedLocalDihedralQuery.TryCalculate([first,second],
            first, second, out _));
    }

    private static (Face, Face) Pair(double degrees, bool reversed, bool sameDirection)
    {
        double radians=degrees*Math.PI/180;
        var a=new Vertex(0,0,0);
        var b=new Vertex(1,0,0);
        var c=new Vertex(0,1,0);
        var d=new Vertex(0,0,1);
        var tangent=new Line3D(new Point3D(0,0,0),new Vector3D(1,0,0));
        var shared=new Edge(a,b,tangent);
        var first=new Face(new Plane3D(new Point3D(0,0,0),
            new Vector3D(0,0,1)),
            new Loop([
                new EdgeUse(shared),
                new EdgeUse(new Edge(b,c,tangent)),
                new EdgeUse(new Edge(c,a,tangent))
            ]));
        var normal = new Vector3D(0,-Math.Sin(radians),Math.Cos(radians));
        var second=new Face(new Plane3D(new Point3D(0,0,0),normal),
            sameDirection
              ? new Loop([
                  new EdgeUse(shared),
                  new EdgeUse(new Edge(b,d,tangent)),
                  new EdgeUse(new Edge(d,a,tangent))
                ])
              : new Loop([
                  new EdgeUse(shared,EdgeUseDirection.Reversed),
                  new EdgeUse(new Edge(a,d,tangent)),
                  new EdgeUse(new Edge(d,b,tangent))
                ]),
            orientation: reversed ? FaceOrientation.Reversed : FaceOrientation.Forward);
        return (first,second);
    }
}
