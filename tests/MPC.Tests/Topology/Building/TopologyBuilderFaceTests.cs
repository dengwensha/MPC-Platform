using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology;
using MPC.Topology.Building;
using MPC.Topology.Primitives;
using Xunit;

namespace MPC.Tests.Topology.Building;

public sealed class TopologyBuilderFaceTests
{
    private sealed class TestCurve3D : ICurve3D
    {
    }

    private sealed class TestSurface3D : ISurface3D
    {
    }

    [Fact]
    public void Constructor_CreatesBuilderWithEmptyFaceCollection()
    {
        TopologyBuilder builder =
            new();

        Assert.Empty(
            builder.Faces);
    }

    [Fact]
    public void RegisterFace_WithValidFace_RegistersFace()
    {
        TopologyBuilder builder =
            new();

        Face face =
            CreateFace();

        builder.RegisterFace(
            face);

        Assert.Single(
            builder.Faces);

        Assert.Same(
            face,
            builder.Faces.Single());
    }

    [Fact]
    public void RegisterFace_WithSameFaceTwice_DoesNotDuplicateFace()
    {
        TopologyBuilder builder =
            new();

        Face face =
            CreateFace();

        builder.RegisterFace(
            face);

        builder.RegisterFace(
            face);

        Assert.Single(
            builder.Faces);
    }

    [Fact]
    public void RegisterFace_WithNullFace_ThrowsArgumentNullException()
    {
        TopologyBuilder builder =
            new();

        Assert.Throws<ArgumentNullException>(
            () => builder.RegisterFace(
                null!));
    }

    [Fact]
    public void RegisterFace_RegistersOuterLoop()
    {
        TopologyBuilder builder =
            new();

        Loop outerLoop =
            CreateTriangleLoop(
                0.0,
                0.0);

        Face face =
            new(
                new TestSurface3D(),
                outerLoop);

        builder.RegisterFace(
            face);

        Assert.Single(
            builder.Loops);

        Assert.Contains(
            builder.Loops,
            registeredLoop =>
                ReferenceEquals(
                    registeredLoop,
                    outerLoop));
    }

    [Fact]
    public void RegisterFace_RegistersOuterLoopEdgesAndVertices()
    {
        TopologyBuilder builder =
            new();

        Loop outerLoop =
            CreateTriangleLoop(
                0.0,
                0.0);

        Face face =
            new(
                new TestSurface3D(),
                outerLoop);

        builder.RegisterFace(
            face);

        Assert.Equal(
            3,
            builder.Edges.Count);

        Assert.Equal(
            3,
            builder.Vertices.Count);
    }

    [Fact]
    public void RegisterFace_RegistersInnerLoops()
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

        Face face =
            new(
                new TestSurface3D(),
                outerLoop,
                new[]
                {
                    innerLoop
                });

        builder.RegisterFace(
            face);

        Assert.Equal(
            2,
            builder.Loops.Count);

        Assert.Contains(
            builder.Loops,
            registeredLoop =>
                ReferenceEquals(
                    registeredLoop,
                    outerLoop));

        Assert.Contains(
            builder.Loops,
            registeredLoop =>
                ReferenceEquals(
                    registeredLoop,
                    innerLoop));
    }

    [Fact]
    public void RegisterFace_RegistersInnerLoopEdgesAndVertices()
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

        Face face =
            new(
                new TestSurface3D(),
                outerLoop,
                new[]
                {
                    innerLoop
                });

        builder.RegisterFace(
            face);

        Assert.Equal(
            6,
            builder.Edges.Count);

        Assert.Equal(
            6,
            builder.Vertices.Count);
    }

    [Fact]
    public void RegisterFaces_WithSharedLoop_DoesNotDuplicateLoop()
    {
        TopologyBuilder builder =
            new();

        Loop sharedLoop =
            CreateTriangleLoop(
                0.0,
                0.0);

        Face firstFace =
            new(
                new TestSurface3D(),
                sharedLoop);

        Face secondFace =
            new(
                new TestSurface3D(),
                sharedLoop,
                orientation:
                    FaceOrientation.Reversed);

        builder.RegisterFace(
            firstFace);

        builder.RegisterFace(
            secondFace);

        Assert.Equal(
            2,
            builder.Faces.Count);

        Assert.Single(
            builder.Loops);

        Assert.Equal(
            3,
            builder.Edges.Count);

        Assert.Equal(
            3,
            builder.Vertices.Count);
    }

    [Fact]
    public void RegisterFace_WithOuterLoopRepeatedAsInnerLoop_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder =
            new();

        Loop loop =
            CreateTriangleLoop(
                0.0,
                0.0);

        Face face =
            new(
                new TestSurface3D(),
                loop,
                new[]
                {
                    loop
                });

        Assert.Throws<InvalidOperationException>(
            () => builder.RegisterFace(
                face));

        Assert.Empty(
            builder.Faces);
    }

    [Fact]
    public void RegisterFace_WithDuplicateInnerLoop_ThrowsInvalidOperationException()
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

        Face face =
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
                face));

        Assert.Empty(
            builder.Faces);
    }

    [Fact]
    public void Verify_WithRegisteredFace_DoesNotThrow()
    {
        TopologyBuilder builder =
            new();

        Face face =
            CreateFace();

        builder.RegisterFace(
            face);

        Exception? exception =
            Record.Exception(
                builder.Verify);

        Assert.Null(
            exception);
    }

    [Fact]
    public void RegisterFace_DoesNotAttachEdgesBeforePublish()
    {
        TopologyBuilder builder =
            new();

        Face face =
            CreateFace();

        builder.RegisterFace(
            face);

        foreach (Vertex vertex in builder.Vertices)
        {
            Assert.Empty(
                vertex.IncidentEdges);
        }
    }

    [Fact]
    public void Publish_WithFace_AttachesEdgesToVertices()
    {
        TopologyBuilder builder =
            new();

        Face face =
            CreateFace();

        builder.RegisterFace(
            face);

        builder.Publish();

        foreach (Vertex vertex in builder.Vertices)
        {
            Assert.Equal(
                2,
                vertex.IncidentEdges.Count);
        }
    }

    [Fact]
    public void Publish_WithFace_ReturnsPublishedFace()
    {
        TopologyBuilder builder =
            new();

        Face face =
            CreateFace();

        builder.RegisterFace(
            face);

        PublishedTopology result =
            builder.Publish();

        Assert.Single(
            result.Faces);

        Assert.Same(
            face,
            result.Faces[0]);
    }

    [Fact]
    public void Rollback_WithFace_ClearsFaceCollection()
    {
        TopologyBuilder builder =
            new();

        Face face =
            CreateFace();

        builder.RegisterFace(
            face);

        builder.Rollback();

        Assert.Empty(
            builder.Faces);

        Assert.Empty(
            builder.Loops);

        Assert.Empty(
            builder.Edges);

        Assert.Empty(
            builder.Vertices);
    }

    [Fact]
    public void RegisterFace_AfterPublish_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder =
            new();

        builder.Publish();

        Face face =
            CreateFace();

        Assert.Throws<InvalidOperationException>(
            () => builder.RegisterFace(
                face));
    }

    [Fact]
    public void RegisterFace_AfterRollback_ThrowsInvalidOperationException()
    {
        TopologyBuilder builder =
            new();

        builder.Rollback();

        Face face =
            CreateFace();

        Assert.Throws<InvalidOperationException>(
            () => builder.RegisterFace(
                face));
    }

    private static Face CreateFace()
    {
        return new Face(
            new TestSurface3D(),
            CreateTriangleLoop(
                0.0,
                0.0));
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