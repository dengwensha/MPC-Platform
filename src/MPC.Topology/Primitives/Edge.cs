namespace MPC.Topology.Primitives;

using MPC.Geometry.Contracts;

public sealed class Edge
{
    public Vertex StartVertex { get; }
    public Vertex EndVertex { get; }
    public ICurve3D Curve { get; }

    public Edge(Vertex startVertex, Vertex endVertex, ICurve3D curve)
    {
        StartVertex = startVertex;
        EndVertex = endVertex;
        Curve = curve;
    }
}