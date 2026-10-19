using MPC.Application.Manufacturing;
using MPC.Geometry.Primitives;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Application.Tests.Manufacturing;

public sealed class SignedLocalDihedralQueryTests
{
    [Fact]
    public void OrthogonalPlanePair_HasSignedQuarterTurn()
    {
        var a=new Vertex(new Point3D(0,0,0));
        var b=new Vertex(new Point3D(1,0,0));
        var c=new Vertex(new Point3D(0,1,0));
        var d=new Vertex(new Point3D(0,0,1));
        var line=new Line3D(new Point3D(0,0,0),new Vector3D(1,0,0));
        var shared=new Edge(a,b,line);
        var face1=new Face(new Plane3D(new Point3D(0,0,0),new Vector3D(0,0,1)),
            new Loop([new EdgeUse(shared),
                new EdgeUse(new Edge(b,c,line)),
                new EdgeUse(new Edge(c,a,line))]));
        var face2=new Face(new Plane3D(new Point3D(0,0,0),new Vector3D(0,1,0)),
            new Loop([new EdgeUse(shared,EdgeUseDirection.Reversed),
                new EdgeUse(new Edge(a,d,line)),
                new EdgeUse(new Edge(d,b,line))]));
        Assert.True(SignedLocalDihedralQuery.TryCalculate([face1,face2],face1,face2,out double angle));
        Assert.Equal(-90.0,angle,8);
        Assert.True(SignedLocalDihedralQuery.TryCalculate([face1,face2],face2,face1,out double reverse));
        Assert.Equal(-90.0,reverse,8);
    }
}
