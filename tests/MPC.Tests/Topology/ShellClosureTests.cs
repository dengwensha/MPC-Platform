using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Tests.Topology;

public sealed class ShellClosureTests
{
    private sealed class TestCurve3D :
        ICurve3D
    {
    }

    private sealed class TestSurface3D :
        ISurface3D
    {
    }

    [Fact]
    public void IsClosed_WithSingleOpenFace_ReturnsFalse()
    {
        Loop loop =
            CreateIndependentTriangleLoop(
                0.0);

        var shell =
            new Shell(
                [
                    CreateFace(
                        loop)
                ]);

        Assert.False(
            shell.IsClosed);
    }

    [Fact]
    public void IsClosed_WithClosedTetrahedron_ReturnsTrue()
    {
        IReadOnlyList<Face> faces =
            CreateTetrahedronFaces();

        var shell =
            new Shell(
                faces);

        Assert.True(
            shell.IsClosed);
    }

    [Fact]
    public void IsClosed_WhenEdgeIsUsedMoreThanTwice_ReturnsFalse()
    {
        List<Face> faces =
            CreateTetrahedronFaces()
                .ToList();

        Loop repeatedLoop =
            faces[0].OuterLoop;

        faces.Add(
            CreateFace(
                repeatedLoop));

        var shell =
            new Shell(
                faces);

        Assert.False(
            shell.IsClosed);
    }

    [Fact]
    public void IsClosed_IncludesInnerBoundaryEdges()
    {
        Loop sharedOuterLoop =
            CreateIndependentTriangleLoop(
                0.0);

        Loop innerLoop =
            CreateIndependentTriangleLoop(
                10.0);

        Face firstFace =
            new(
                new TestSurface3D(),
                sharedOuterLoop,
                [
                    innerLoop
                ]);

        Face secondFace =
            CreateFace(
                sharedOuterLoop);

        var shell =
            new Shell(
                [
                    firstFace,
                    secondFace
                ]);

        Assert.False(
            shell.IsClosed);
    }

    private static IReadOnlyList<Face>
        CreateTetrahedronFaces()
    {
        var curve =
            new TestCurve3D();

        Vertex a =
            CreateVertex(
                0.0,
                0.0,
                0.0);

        Vertex b =
            CreateVertex(
                1.0,
                0.0,
                0.0);

        Vertex c =
            CreateVertex(
                0.0,
                1.0,
                0.0);

        Vertex d =
            CreateVertex(
                0.0,
                0.0,
                1.0);

        Edge ab =
            new(
                a,
                b,
                curve);

        Edge ac =
            new(
                a,
                c,
                curve);

        Edge ad =
            new(
                a,
                d,
                curve);

        Edge bc =
            new(
                b,
                c,
                curve);

        Edge bd =
            new(
                b,
                d,
                curve);

        Edge cd =
            new(
                c,
                d,
                curve);

        Loop abc =
            CreateLoop(
                new EdgeUse(
                    ab,
                    EdgeUseDirection.Forward),
                new EdgeUse(
                    bc,
                    EdgeUseDirection.Forward),
                new EdgeUse(
                    ac,
                    EdgeUseDirection.Reversed));

        Loop adb =
            CreateLoop(
                new EdgeUse(
                    ad,
                    EdgeUseDirection.Forward),
                new EdgeUse(
                    bd,
                    EdgeUseDirection.Reversed),
                new EdgeUse(
                    ab,
                    EdgeUseDirection.Reversed));

        Loop bcd =
            CreateLoop(
                new EdgeUse(
                    bc,
                    EdgeUseDirection.Forward),
                new EdgeUse(
                    cd,
                    EdgeUseDirection.Forward),
                new EdgeUse(
                    bd,
                    EdgeUseDirection.Reversed));

        Loop acd =
            CreateLoop(
                new EdgeUse(
                    ac,
                    EdgeUseDirection.Forward),
                new EdgeUse(
                    cd,
                    EdgeUseDirection.Forward),
                new EdgeUse(
                    ad,
                    EdgeUseDirection.Reversed));

        return
        [
            CreateFace(
                abc),

            CreateFace(
                adb),

            CreateFace(
                bcd),

            CreateFace(
                acd)
        ];
    }

    private static Loop CreateIndependentTriangleLoop(
        double offset)
    {
        var curve =
            new TestCurve3D();

        Vertex first =
            CreateVertex(
                offset,
                0.0,
                0.0);

        Vertex second =
            CreateVertex(
                offset + 1.0,
                0.0,
                0.0);

        Vertex third =
            CreateVertex(
                offset,
                1.0,
                0.0);

        Edge firstEdge =
            new(
                first,
                second,
                curve);

        Edge secondEdge =
            new(
                second,
                third,
                curve);

        Edge thirdEdge =
            new(
                third,
                first,
                curve);

        return CreateLoop(
            new EdgeUse(
                firstEdge,
                EdgeUseDirection.Forward),
            new EdgeUse(
                secondEdge,
                EdgeUseDirection.Forward),
            new EdgeUse(
                thirdEdge,
                EdgeUseDirection.Forward));
    }

    private static Loop CreateLoop(
        params EdgeUse[] edgeUses)
    {
        return new Loop(
            edgeUses);
    }

    private static Face CreateFace(
        Loop outerLoop)
    {
        return new Face(
            new TestSurface3D(),
            outerLoop);
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
}