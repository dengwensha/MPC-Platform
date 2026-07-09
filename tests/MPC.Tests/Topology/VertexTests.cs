namespace MPC.Tests.Topology;

using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

public sealed class VertexTests
{
    [Fact]
    public void Constructor_Should_Set_Position()
    {
        var position = new Point3D(1, 2, 3);

        var vertex = new Vertex(position);

        Assert.Equal(position, vertex.Position);
    }
}