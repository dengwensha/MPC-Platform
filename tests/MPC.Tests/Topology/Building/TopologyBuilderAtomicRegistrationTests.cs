using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology.Building;
using MPC.Topology.Primitives;
using Xunit;

namespace MPC.Tests.Topology.Building;

public sealed class TopologyBuilderAtomicRegistrationTests
{
    private sealed class TestCurve3D : ICurve3D
    {
    }

    private sealed class TestSurface3D : ISurface3D
    {
    }

    [Fact]
    public void RegisterEdge_WithNullEdge_DoesNotChangeBuilder()
    {
        TopologyBuilder builder =
            new();

        Vertex existingVertex =
            CreateVertex(
                0.0,
                0.0,
                0.0);

        builder.RegisterVertex(
            existingVertex);

        Assert.Throws<ArgumentNullException>(
            () => builder.RegisterEdge(
                null!));

        Assert.Single(
            builder.Vertices);

        Assert.Empty(
            builder.Edges);

        Assert.Empty(
            builder.Loops);

        Assert.Empty(
            builder.Faces);

        Assert.Same(
            existingVertex,
            builder.Vertices.Single());

        Assert.Equal(
            TopologyBuilderState.Open,
            builder.State);
    }

    [Fact]
    public void RegisterFace_WithOuterLoopRepeatedAsInnerLoop_RestoresEmptyBuilder()
    {
        TopologyBuilder builder =
            new();

        Loop loop =
            CreateTriangleLoop(
                0.0,
                0.0);

        Face invalidFace =
            new(
                new TestSurface3D(),
                loop,
                new[]
                {
                    loop
                });

        Assert.Throws<InvalidOperationException>(
            () => builder.RegisterFace(
                invalidFace));

        Assert.Empty(
            builder.Vertices);

        Assert.Empty(
            builder.Edges);

        Assert.Empty(
            builder.Loops);

        Assert.Empty(
            builder.Faces);

        Assert.Equal(
            TopologyBuilderState.Open,
            builder.State);
    }

    [Fact]
    public void RegisterFace_WithDuplicateInnerLoop_RestoresEmptyBuilder()
    {
        TopologyBuilder builder =
            new();

        Loop outerLoop =
            CreateTriangleLoop(
                0.0,
                0.0);

        Loop innerLoop =
            CreateTriangleLoop(
                0.2,
                0.2);

        Face invalidFace =
            new(
                new TestSurface3D(),
                outerLoop,
                new[]
                {
                    innerLoop,
                    innerLoop
                });

        Assert.Throws<InvalidOperationException>(
            () => builder.RegisterFace(
                invalidFace));

        Assert.Empty(
            builder.Vertices);

        Assert.Empty(
            builder.Edges);

        Assert.Empty(
            builder.Loops);

        Assert.Empty(
            builder.Faces);

        Assert.Equal(
            TopologyBuilderState.Open,
            builder.State);
    }

    [Fact]
    public void RegisterFace_WithInvalidFace_PreservesExistingGraph()
    {
        TopologyBuilder builder =
            new();

        Face existingFace =
            CreateFace(
                0.0,
                0.0);

        builder.RegisterFace(
            existingFace);

        Vertex[] existingVertices =
            builder.Vertices.ToArray();

        Edge[] existingEdges =
            builder.Edges.ToArray();

        Loop[] existingLoops =
            builder.Loops.ToArray();

        Loop invalidLoop =
            CreateTriangleLoop(
                2.0,
                2.0);

        Face invalidFace =
            new(
                new TestSurface3D(),
                invalidLoop,
                new[]
                {
                    invalidLoop
                });

        Assert.Throws<InvalidOperationException>(
            () => builder.RegisterFace(
                invalidFace));

        Assert.Equal(
            3,
            builder.Vertices.Count);

        Assert.Equal(
            3,
            builder.Edges.Count);

        Assert.Single(
            builder.Loops);

        Assert.Single(
            builder.Faces);

        Assert.Same(
            existingFace,
            builder.Faces.Single());

        Assert.Equal(
            existingVertices,
            builder.Vertices);

        Assert.Equal(
            existingEdges,
            builder.Edges);

        Assert.Equal(
            existingLoops,
            builder.Loops);

        Assert.Equal(
            TopologyBuilderState.Open,
            builder.State);
    }

    [Fact]
    public void FailedFaceRegistration_DoesNotAttachEdgesToVertices()
    {
        TopologyBuilder builder =
            new();

        Loop loop =
            CreateTriangleLoop(
                0.0,
                0.0);

        Vertex[] loopVertices =
            loop.EdgeUses
                .SelectMany(
                    edgeUse =>
                        new[]
                        {
                            edgeUse.Edge.StartVertex,
                            edgeUse.Edge.EndVertex
                        })
                .Distinct<Vertex>(
                    ReferenceEqualityComparer.Instance)
                .ToArray();

        Face invalidFace =
            new(
                new TestSurface3D(),
                loop,
                new[]
                {
                    loop
                });

        Assert.Throws<InvalidOperationException>(
            () => builder.RegisterFace(
                invalidFace));

        Assert.Empty(
            builder.Vertices);

        Assert.Empty(
            builder.Edges);

        Assert.Empty(
            builder.Loops);

        Assert.Empty(
            builder.Faces);

        foreach (Vertex vertex in loopVertices)
        {
            Assert.Empty(
                vertex.IncidentEdges);
        }

        Assert.Equal(
            TopologyBuilderState.Open,
            builder.State);
    }

    [Fact]
    public void RegisterFace_WithSameFaceTwice_DoesNotChangeGraph()
    {
        TopologyBuilder builder =
            new();

        Face face =
            CreateFace(
                0.0,
                0.0);

        builder.RegisterFace(
            face);

        builder.RegisterFace(
            face);

        Assert.Equal(
            3,
            builder.Vertices.Count);

        Assert.Equal(
            3,
            builder.Edges.Count);

        Assert.Single(
            builder.Loops);

        Assert.Single(
            builder.Faces);

        Assert.Equal(
            TopologyBuilderState.Open,
            builder.State);
    }

    [Fact]
    public void SuccessfulRegistration_CanStillBePublished()
    {
        TopologyBuilder builder =
            new();

        Face face =
            CreateFace(
                0.0,
                0.0);

        builder.RegisterFace(
            face);

        PublishedTopology result =
            builder.Publish();

        Assert.Equal(
            TopologyBuilderState.Published,
            builder.State);

        Assert.Equal(
            3,
            result.Vertices.Count);

        Assert.Equal(
            3,
            result.Edges.Count);

        Assert.Single(
            result.Loops);

        Assert.Single(
            result.Faces);

        Assert.Same(
            face,
            result.Faces[0]);

        foreach (Vertex vertex in result.Vertices)
        {
            Assert.Equal(
                2,
                vertex.IncidentEdges.Count);
        }
    }

    [Fact]
    public void SuccessfulRegistration_CanStillBeRolledBack()
    {
        TopologyBuilder builder =
            new();

        builder.RegisterFace(
            CreateFace(
                0.0,
                0.0));

        builder.Rollback();

        Assert.Equal(
            TopologyBuilderState.RolledBack,
            builder.State);

        Assert.Empty(
            builder.Vertices);

        Assert.Empty(
            builder.Edges);

        Assert.Empty(
            builder.Loops);

        Assert.Empty(
            builder.Faces);
    }

    private static Face CreateFace(
        double offsetX,
        double offsetY)
    {
        return new Face(
            new TestSurface3D(),
            CreateTriangleLoop(
                offsetX,
                offsetY));
    }

    private static Loop CreateTriangleLoop(
        double offsetX,
        double offsetY)
    {
        Vertex vertex1 =
            CreateVertex(
                offsetX,
                offsetY,
                0.0);

        Vertex vertex2 =
            CreateVertex(
                offsetX + 1.0,
                offsetY,
                0.0);

        Vertex vertex3 =
            CreateVertex(
                offsetX,
                offsetY + 1.0,
                0.0);

        Edge edge1 =
            CreateEdge(
                vertex1,
                vertex2);

        Edge edge2 =
            CreateEdge(
                vertex2,
                vertex3);

        Edge edge3 =
            CreateEdge(
                vertex3,
                vertex1);

        return new Loop(
            new[]
            {
                new EdgeUse(
                    edge1),

                new EdgeUse(
                    edge2),

                new EdgeUse(
                    edge3)
            });
    }

    private static Vertex CreateVertex(
        double x,
        double y,
        double z)
    {
        return new Vertex(
            new Point3D(
                x,
                y,
                z));
    }

    private static Edge CreateEdge(
        Vertex startVertex,
        Vertex endVertex)
    {
        return new Edge(
            startVertex,
            endVertex,
            new TestCurve3D());
    }
}