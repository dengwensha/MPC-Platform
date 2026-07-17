namespace MPC.Tests.Topology;

using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology;
using MPC.Topology.Primitives;

public sealed class FaceTests
{
    private sealed class TestCurve3D : ICurve3D
    {
    }

    private sealed class TestSurface3D : ISurface3D
    {
    }

    private static Loop CreateLoop()
    {
        var curve = new TestCurve3D();

        var v1 = new Vertex(new Point3D(0, 0, 0));
        var v2 = new Vertex(new Point3D(10, 0, 0));
        var v3 = new Vertex(new Point3D(10, 10, 0));

        var e1 = new Edge(v1, v2, curve);
        var e2 = new Edge(v2, v3, curve);
        var e3 = new Edge(v3, v1, curve);

        var edgeUse1 = new EdgeUse(
            e1,
            EdgeUseDirection.Forward);

        var edgeUse2 = new EdgeUse(
            e2,
            EdgeUseDirection.Forward);

        var edgeUse3 = new EdgeUse(
            e3,
            EdgeUseDirection.Forward);

        return new Loop(
            new[]
            {
                edgeUse1,
                edgeUse2,
                edgeUse3
            });
    }

    [Fact]
    public void Constructor_Should_Create_Face()
    {
        var surface = new TestSurface3D();
        var outerLoop = CreateLoop();

        var face = new Face(surface, outerLoop);

        Assert.NotNull(face);
        Assert.NotEqual(Guid.Empty, face.Id);
        Assert.Equal(surface, face.Surface);
        Assert.Equal(outerLoop, face.OuterLoop);
        Assert.Empty(face.InnerLoops);
        Assert.Equal(FaceOrientation.Forward, face.Orientation);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Surface_Is_Null()
    {
        var outerLoop = CreateLoop();

        Assert.Throws<ArgumentNullException>(() =>
            new Face(null!, outerLoop));
    }

    [Fact]
    public void Constructor_Should_Throw_When_OuterLoop_Is_Null()
    {
        var surface = new TestSurface3D();

        Assert.Throws<ArgumentNullException>(() =>
            new Face(surface, null!));
    }

    [Fact]
    public void Constructor_Should_Use_Empty_InnerLoops_When_InnerLoops_Is_Null()
    {
        var surface = new TestSurface3D();
        var outerLoop = CreateLoop();

        var face = new Face(surface, outerLoop);

        Assert.NotNull(face.InnerLoops);
        Assert.Empty(face.InnerLoops);
    }

    [Fact]
    public void Constructor_Should_Set_InnerLoops()
    {
        var surface = new TestSurface3D();
        var outerLoop = CreateLoop();
        var innerLoop = CreateLoop();

        var face = new Face(
            surface,
            outerLoop,
            new[] { innerLoop });

        Assert.Single(face.InnerLoops);
        Assert.Equal(innerLoop, face.InnerLoops[0]);
    }

    [Fact]
    public void Constructor_Should_Use_Forward_Orientation_By_Default()
    {
        var surface = new TestSurface3D();
        var outerLoop = CreateLoop();

        var face = new Face(surface, outerLoop);

        Assert.Equal(FaceOrientation.Forward, face.Orientation);
    }

    [Fact]
    public void Constructor_Should_Set_Orientation()
    {
        var surface = new TestSurface3D();
        var outerLoop = CreateLoop();

        var face = new Face(
            surface,
            outerLoop,
            null,
            FaceOrientation.Reversed);

        Assert.Equal(FaceOrientation.Reversed, face.Orientation);
    }

    [Fact]
    public void Constructor_Should_Throw_When_InnerLoops_Contains_Null()
    {
        var surface = new TestSurface3D();
        var outerLoop = CreateLoop();

        IEnumerable<Loop> innerLoops = new List<Loop>
        {
            null!
        };

        Assert.Throws<ArgumentNullException>(() =>
            new Face(
                surface,
                outerLoop,
                innerLoops));
    }
}