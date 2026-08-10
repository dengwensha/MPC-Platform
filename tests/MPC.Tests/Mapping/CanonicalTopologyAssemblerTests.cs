using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping;

public sealed class CanonicalTopologyAssemblerTests
{
    [Fact]
    public void Assemble_OuterShell_PublishesReachableTopology()
    {
        TestTopology topology =
            CreateSingleTriangleSolid();

        var assembler =
            new CanonicalTopologyAssembler();

        CanonicalTopologyAssemblyResult result =
            assembler.Assemble(
                topology.Solid);

        Assert.NotNull(
            result.Topology);

        Assert.Single(
            result.Topology.Faces);

        Assert.Single(
            result.Topology.Loops);

        Assert.Equal(
            3,
            result.Topology.Edges.Count);

        Assert.Equal(
            3,
            result.Topology.Vertices.Count);
    }

    [Fact]
    public void Assemble_PreservesOriginalSolidInstance()
    {
        TestTopology topology =
            CreateSingleTriangleSolid();

        var assembler =
            new CanonicalTopologyAssembler();

        CanonicalTopologyAssemblyResult result =
            assembler.Assemble(
                topology.Solid);

        Assert.Same(
            topology.Solid,
            result.Solid);
    }

    [Fact]
    public void Assemble_PublishesOriginalCanonicalGraphInstances()
    {
        TestTopology topology =
            CreateSingleTriangleSolid();

        var assembler =
            new CanonicalTopologyAssembler();

        CanonicalTopologyAssemblyResult result =
            assembler.Assemble(
                topology.Solid);

        Assert.Same(
            topology.Face,
            Assert.Single(
                result.Topology.Faces));

        Assert.Same(
            topology.Loop,
            Assert.Single(
                result.Topology.Loops));

        Assert.Contains(
            result.Topology.Vertices,
            vertex =>
                ReferenceEquals(
                    vertex,
                    topology.Vertex1));

        Assert.Contains(
            result.Topology.Vertices,
            vertex =>
                ReferenceEquals(
                    vertex,
                    topology.Vertex2));

        Assert.Contains(
            result.Topology.Vertices,
            vertex =>
                ReferenceEquals(
                    vertex,
                    topology.Vertex3));

        Assert.Contains(
            result.Topology.Edges,
            edge =>
                ReferenceEquals(
                    edge,
                    topology.Edge1));

        Assert.Contains(
            result.Topology.Edges,
            edge =>
                ReferenceEquals(
                    edge,
                    topology.Edge2));

        Assert.Contains(
            result.Topology.Edges,
            edge =>
                ReferenceEquals(
                    edge,
                    topology.Edge3));
    }

    [Fact]
    public void Assemble_Publish_AttachesIncidentEdgesToVertices()
    {
        TestTopology topology =
            CreateSingleTriangleSolid();

        Assert.Empty(
            topology.Vertex1.IncidentEdges);

        Assert.Empty(
            topology.Vertex2.IncidentEdges);

        Assert.Empty(
            topology.Vertex3.IncidentEdges);

        var assembler =
            new CanonicalTopologyAssembler();

        assembler.Assemble(
            topology.Solid);

        Assert.Equal(
            2,
            topology.Vertex1.IncidentEdges.Count);

        Assert.Equal(
            2,
            topology.Vertex2.IncidentEdges.Count);

        Assert.Equal(
            2,
            topology.Vertex3.IncidentEdges.Count);

        Assert.Contains(
            topology.Edge1,
            topology.Vertex1.IncidentEdges);

        Assert.Contains(
            topology.Edge3,
            topology.Vertex1.IncidentEdges);

        Assert.Contains(
            topology.Edge1,
            topology.Vertex2.IncidentEdges);

        Assert.Contains(
            topology.Edge2,
            topology.Vertex2.IncidentEdges);

        Assert.Contains(
            topology.Edge2,
            topology.Vertex3.IncidentEdges);

        Assert.Contains(
            topology.Edge3,
            topology.Vertex3.IncidentEdges);
    }

    [Fact]
    public void Assemble_InnerShell_PublishesReachableFaces()
    {
        TestTopology outerTopology =
            CreateSingleTriangleSolid();

        TestTopology innerTopology =
            CreateSingleTriangleSolid();

        var solid =
            new Solid(
                outerTopology.Shell,
                [
                    innerTopology.Shell
                ]);

        var assembler =
            new CanonicalTopologyAssembler();

        CanonicalTopologyAssemblyResult result =
            assembler.Assemble(
                solid);

        Assert.Equal(
            2,
            result.Topology.Faces.Count);

        Assert.Contains(
            result.Topology.Faces,
            face =>
                ReferenceEquals(
                    face,
                    outerTopology.Face));

        Assert.Contains(
            result.Topology.Faces,
            face =>
                ReferenceEquals(
                    face,
                    innerTopology.Face));
    }

    [Fact]
    public void Assemble_SameFaceReferencedByMultipleShells_RegistersFaceOnce()
    {
        TestTopology topology =
            CreateSingleTriangleSolid();

        var duplicateShell =
            new Shell(
                [
                    topology.Face
                ]);

        var solid =
            new Solid(
                topology.Shell,
                [
                    duplicateShell
                ]);

        var assembler =
            new CanonicalTopologyAssembler();

        CanonicalTopologyAssemblyResult result =
            assembler.Assemble(
                solid);

        Assert.Single(
            result.Topology.Faces);

        Assert.Same(
            topology.Face,
            Assert.Single(
                result.Topology.Faces));

        Assert.Single(
            result.Topology.Loops);

        Assert.Equal(
            3,
            result.Topology.Edges.Count);

        Assert.Equal(
            3,
            result.Topology.Vertices.Count);
    }

    [Fact]
    public void Assemble_InvalidTopology_DoesNotPublishIncidentEdges()
    {
        TestTopology topology =
            CreateSingleTriangleSolid();

        var invalidFace =
            new Face(
                topology.Face.Surface,
                topology.Loop,
                [
                    topology.Loop
                ]);

        var invalidShell =
            new Shell(
                [
                    invalidFace
                ]);

        var solid =
            new Solid(
                invalidShell);

        var assembler =
            new CanonicalTopologyAssembler();

        Assert.Throws<InvalidOperationException>(
            () =>
                assembler.Assemble(
                    solid));

        Assert.Empty(
            topology.Vertex1.IncidentEdges);

        Assert.Empty(
            topology.Vertex2.IncidentEdges);

        Assert.Empty(
            topology.Vertex3.IncidentEdges);
    }

    private static TestTopology
        CreateSingleTriangleSolid()
    {
        var vertex1 =
            new Vertex(
                0.0,
                0.0,
                0.0);

        var vertex2 =
            new Vertex(
                1.0,
                0.0,
                0.0);

        var vertex3 =
            new Vertex(
                0.0,
                1.0,
                0.0);

        var edge1 =
            new Edge(
                vertex1,
                vertex2,
                new Line3D(
                    vertex1.Position,
                    new Vector3D(
                        1.0,
                        0.0,
                        0.0)));

        double inverseSqrtTwo =
            1.0 /
            Math.Sqrt(
                2.0);

        var edge2 =
            new Edge(
                vertex2,
                vertex3,
                new Line3D(
                    vertex2.Position,
                    new Vector3D(
                        -inverseSqrtTwo,
                        inverseSqrtTwo,
                        0.0)));

        var edge3 =
            new Edge(
                vertex3,
                vertex1,
                new Line3D(
                    vertex3.Position,
                    new Vector3D(
                        0.0,
                        -1.0,
                        0.0)));

        var edgeUse1 =
            new EdgeUse(
                edge1,
                EdgeUseDirection.Forward);

        var edgeUse2 =
            new EdgeUse(
                edge2,
                EdgeUseDirection.Forward);

        var edgeUse3 =
            new EdgeUse(
                edge3,
                EdgeUseDirection.Forward);

        var loop =
            new Loop(
                [
                    edgeUse1,
                    edgeUse2,
                    edgeUse3
                ]);

        var surface =
            new Plane3D(
                new Point3D(
                    0.0,
                    0.0,
                    0.0),
                new Vector3D(
                    0.0,
                    0.0,
                    1.0));

        var face =
            new Face(
                surface,
                loop);

        var shell =
            new Shell(
                [
                    face
                ]);

        var solid =
            new Solid(
                shell);

        return new TestTopology(
            vertex1,
            vertex2,
            vertex3,
            edge1,
            edge2,
            edge3,
            loop,
            face,
            shell,
            solid);
    }

    private sealed record TestTopology(
        Vertex Vertex1,
        Vertex Vertex2,
        Vertex Vertex3,
        Edge Edge1,
        Edge Edge2,
        Edge Edge3,
        Loop Loop,
        Face Face,
        Shell Shell,
        Solid Solid);
}