using MPC.Geometry.Primitives;
using MPC.Topology;
using MPC.Topology.Building;
using MPC.Topology.Primitives;

namespace MPC.Tests.Topology.Building;

public sealed class FaceBuilderTests
{
    [Fact]
    public void Constructor_WhenSurfaceIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => new FaceBuilder(
                null!));
    }

    [Fact]
    public void Build_WhenOuterLoopIsAssigned_ReturnsFace()
    {
        Plane3D surface =
            CreateSurface();

        Loop outerLoop =
            CreateTriangleLoop(
                xOffset:
                    0.0,
                yOffset:
                    0.0);

        var builder =
            new FaceBuilder(
                surface);

        builder.SetOuterLoop(
            outerLoop);

        Face face =
            builder.Build();

        Assert.NotNull(
            face);

        Assert.Same(
            surface,
            face.Surface);

        Assert.Same(
            outerLoop,
            face.OuterLoop);

        Assert.Empty(
            face.InnerLoops);

        Assert.Equal(
            FaceOrientation.Forward,
            face.Orientation);

        Assert.True(
            builder.IsBuilt);
    }

    [Fact]
    public void Build_WhenOuterLoopIsMissing_ThrowsInvalidOperationException()
    {
        Plane3D surface =
            CreateSurface();

        var builder =
            new FaceBuilder(
                surface);

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => builder.Build());

        Assert.Equal(
            "A face cannot be built without an outer loop.",
            exception.Message);

        Assert.False(
            builder.IsBuilt);
    }

    [Fact]
    public void SetOuterLoop_WhenOuterLoopWasAlreadyAssigned_ThrowsInvalidOperationException()
    {
        Plane3D surface =
            CreateSurface();

        Loop firstOuterLoop =
            CreateTriangleLoop(
                xOffset:
                    0.0,
                yOffset:
                    0.0);

        Loop secondOuterLoop =
            CreateTriangleLoop(
                xOffset:
                    10.0,
                yOffset:
                    0.0);

        var builder =
            new FaceBuilder(
                surface);

        builder.SetOuterLoop(
            firstOuterLoop);

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => builder.SetOuterLoop(
                    secondOuterLoop));

        Assert.Equal(
            "A face outer loop has already been assigned.",
            exception.Message);

        Assert.Same(
            firstOuterLoop,
            builder.OuterLoop);
    }

    [Fact]
    public void AddInnerLoop_WhenSameLoopIsAddedTwice_ThrowsInvalidOperationException()
    {
        Plane3D surface =
            CreateSurface();

        Loop innerLoop =
            CreateTriangleLoop(
                xOffset:
                    2.0,
                yOffset:
                    2.0);

        var builder =
            new FaceBuilder(
                surface);

        builder.AddInnerLoop(
            innerLoop);

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => builder.AddInnerLoop(
                    innerLoop));

        Assert.Equal(
            "The same inner loop cannot be added more than once.",
            exception.Message);

        Loop storedInnerLoop =
            Assert.Single(
                builder.InnerLoops);

        Assert.Same(
            innerLoop,
            storedInnerLoop);
    }

    [Fact]
    public void AddInnerLoop_WhenLoopIsOuterLoop_ThrowsInvalidOperationException()
    {
        Plane3D surface =
            CreateSurface();

        Loop outerLoop =
            CreateTriangleLoop(
                xOffset:
                    0.0,
                yOffset:
                    0.0);

        var builder =
            new FaceBuilder(
                surface);

        builder.SetOuterLoop(
            outerLoop);

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => builder.AddInnerLoop(
                    outerLoop));

        Assert.Equal(
            "A face outer loop cannot also be added as an inner loop.",
            exception.Message);

        Assert.Empty(
            builder.InnerLoops);
    }

    [Fact]
    public void SetOuterLoop_WhenLoopWasAlreadyAddedAsInnerLoop_ThrowsInvalidOperationException()
    {
        Plane3D surface =
            CreateSurface();

        Loop loop =
            CreateTriangleLoop(
                xOffset:
                    0.0,
                yOffset:
                    0.0);

        var builder =
            new FaceBuilder(
                surface);

        builder.AddInnerLoop(
            loop);

        InvalidOperationException exception =
            Assert.Throws<InvalidOperationException>(
                () => builder.SetOuterLoop(
                    loop));

        Assert.Equal(
            "A face inner loop cannot also be assigned as its outer loop.",
            exception.Message);

        Assert.Null(
            builder.OuterLoop);
    }

    [Fact]
    public void Build_PreservesInnerLoopOrderAndOrientation()
    {
        Plane3D surface =
            CreateSurface();

        Loop outerLoop =
            CreateTriangleLoop(
                xOffset:
                    0.0,
                yOffset:
                    0.0);

        Loop firstInnerLoop =
            CreateTriangleLoop(
                xOffset:
                    2.0,
                yOffset:
                    2.0);

        Loop secondInnerLoop =
            CreateTriangleLoop(
                xOffset:
                    4.0,
                yOffset:
                    4.0);

        var builder =
            new FaceBuilder(
                surface);

        builder
            .SetOuterLoop(
                outerLoop)
            .AddInnerLoop(
                firstInnerLoop)
            .AddInnerLoop(
                secondInnerLoop)
            .SetOrientation(
                FaceOrientation.Reversed);

        Face face =
            builder.Build();

        Assert.Equal(
            FaceOrientation.Reversed,
            face.Orientation);

        Assert.Equal(
            2,
            face.InnerLoops.Count);

        Assert.Same(
            firstInnerLoop,
            face.InnerLoops[0]);

        Assert.Same(
            secondInnerLoop,
            face.InnerLoops[1]);
    }

    [Fact]
    public void Builder_AfterBuild_CannotBeModifiedOrBuiltAgain()
    {
        Plane3D surface =
            CreateSurface();

        Loop outerLoop =
            CreateTriangleLoop(
                xOffset:
                    0.0,
                yOffset:
                    0.0);

        Loop innerLoop =
            CreateTriangleLoop(
                xOffset:
                    2.0,
                yOffset:
                    2.0);

        var builder =
            new FaceBuilder(
                surface);

        builder.SetOuterLoop(
            outerLoop);

        Face face =
            builder.Build();

        Assert.NotNull(
            face);

        Assert.Throws<InvalidOperationException>(
            () => builder.SetOuterLoop(
                CreateTriangleLoop(
                    xOffset:
                        10.0,
                    yOffset:
                        0.0)));

        Assert.Throws<InvalidOperationException>(
            () => builder.AddInnerLoop(
                innerLoop));

        Assert.Throws<InvalidOperationException>(
            () => builder.SetOrientation(
                FaceOrientation.Forward));

        Assert.Throws<InvalidOperationException>(
            () => builder.Build());
    }

    private static Plane3D CreateSurface()
    {
        return new Plane3D(
            new Point3D(
                0.0,
                0.0,
                0.0),
            new Vector3D(
                0.0,
                0.0,
                1.0));
    }

    private static Loop CreateTriangleLoop(
        double xOffset,
        double yOffset)
    {
        var firstVertex =
            new Vertex(
                xOffset,
                yOffset,
                0.0);

        var secondVertex =
            new Vertex(
                xOffset + 1.0,
                yOffset,
                0.0);

        var thirdVertex =
            new Vertex(
                xOffset,
                yOffset + 1.0,
                0.0);

        var firstEdge =
            new Edge(
                firstVertex,
                secondVertex,
                new Line3D(
                    firstVertex.Position,
                    new Vector3D(
                        1.0,
                        0.0,
                        0.0)));

        double diagonalComponent =
            1.0 /
            Math.Sqrt(
                2.0);

        var secondEdge =
            new Edge(
                secondVertex,
                thirdVertex,
                new Line3D(
                    secondVertex.Position,
                    new Vector3D(
                        -diagonalComponent,
                        diagonalComponent,
                        0.0)));

        var thirdEdge =
            new Edge(
                thirdVertex,
                firstVertex,
                new Line3D(
                    thirdVertex.Position,
                    new Vector3D(
                        0.0,
                        -1.0,
                        0.0)));

        return new Loop(
            [
                new EdgeUse(
                    firstEdge),

                new EdgeUse(
                    secondEdge),

                new EdgeUse(
                    thirdEdge)
            ]);
    }
}