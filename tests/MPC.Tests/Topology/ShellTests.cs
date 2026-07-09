using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Tests.Topology;

public sealed class ShellTests
{
    [Fact]
    public void Constructor_WithNullFaces_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Shell(null!));
    }

    [Fact]
    public void Constructor_WithEmptyFaces_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Shell(Array.Empty<Face>()));
    }

    [Fact]
    public void Constructor_WithNullFace_ThrowsArgumentException()
    {
        var faces = new Face?[] { null };

        Assert.Throws<ArgumentException>(() => new Shell(faces!));
    }

    [Fact]
    public void Constructor_WithSingleFace_CreatesShell()
    {
        var face = CreateSimpleFace();

        var shell = new Shell(new[] { face });

        Assert.Equal(1, shell.Count);
        Assert.Same(face, shell.Faces[0]);
    }

    [Fact]
    public void Constructor_WithMultipleFaces_CreatesShell()
    {
        var face1 = CreateSimpleFace();
        var face2 = CreateSimpleFace();

        var shell = new Shell(new[] { face1, face2 });

        Assert.Equal(2, shell.Count);
        Assert.Same(face1, shell.Faces[0]);
        Assert.Same(face2, shell.Faces[1]);
    }

    [Fact]
    public void Faces_IsReadOnlySnapshot()
    {
        var face = CreateSimpleFace();
        var source = new List<Face> { face };

        var shell = new Shell(source);

        source.Clear();

        Assert.Equal(1, shell.Count);
        Assert.Same(face, shell.Faces[0]);
    }

    [Fact]
    public void Shell_CanBeEnumerated()
    {
        var face1 = CreateSimpleFace();
        var face2 = CreateSimpleFace();

        var shell = new Shell(new[] { face1, face2 });

        var enumeratedFaces = shell.ToList();

        Assert.Equal(2, enumeratedFaces.Count);
        Assert.Same(face1, enumeratedFaces[0]);
        Assert.Same(face2, enumeratedFaces[1]);
    }

    private static Face CreateSimpleFace()
    {
        var p1 = new Point3D(0, 0, 0);
        var p2 = new Point3D(1, 0, 0);
        var p3 = new Point3D(1, 1, 0);
        var p4 = new Point3D(0, 1, 0);

        var v1 = new Vertex(p1);
        var v2 = new Vertex(p2);
        var v3 = new Vertex(p3);
        var v4 = new Vertex(p4);

        var e1 = new Edge(v1, v2, new Line3D(p1, p2));
        var e2 = new Edge(v2, v3, new Line3D(p2, p3));
        var e3 = new Edge(v3, v4, new Line3D(p3, p4));
        var e4 = new Edge(v4, v1, new Line3D(p4, p1));

        var loop = new Loop(new[] { e1, e2, e3, e4 });

        return new Face(new TestSurface(), loop);
    }

    private sealed class TestSurface : ISurface3D
    {
    }
}