namespace MPC.Topology.Primitives;

using MPC.Geometry.Primitives;

public sealed class Vertex
{
    public Point3D Position { get; }

    public Vertex(double x, double y, double z)
    {
        Position = new Point3D(x, y, z);
    }

    public Vertex(Point3D position)
    {
        Position = position;
    }
}