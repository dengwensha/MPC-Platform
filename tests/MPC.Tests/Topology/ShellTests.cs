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
        var p1 = new Point3D(0.0, 0.0, 0.0);
        var p2 = new Point3D(1.0, 0.0, 0.0);
        var p3 = new Point3D(1.0, 1.0, 0.0);
        var p4 = new Point3D(0.0, 1.0, 0.0);

        var v1 = new Vertex(p1);
        var v2 = new Vertex(p2);
        var v3 = new Vertex(p3);
        var v4 = new Vertex(p4);

        var e1 = CreateEdge(v1, v2);
        var e2 = CreateEdge(v2, v3);
        var e3 = CreateEdge(v3, v4);
        var e4 = CreateEdge(v4, v1);

        var loop = new Loop(new[] { e1, e2, e3, e4 });

        return new Face(new TestSurface(), loop);
    }

    private static Edge CreateEdge(Vertex start, Vertex end)
    {
        var direction = CreateUnitDirection(
            start.Position,
            end.Position);

        return new Edge(
            start,
            end,
            new Line3D(start.Position, direction));
    }

    private static Vector3D CreateUnitDirection(
        Point3D start,
        Point3D end)
    {
        var x = end.X - start.X;
        var y = end.Y - start.Y;
        var z = end.Z - start.Z;

        var length = Math.Sqrt(
            x * x +
            y * y +
            z * z);

        if (length == 0.0)
        {
            throw new ArgumentException(
                "Cannot create a line direction from identical points.");
        }

        return new Vector3D(
            x / length,
            y / length,
            z / length);
    }

    private sealed class TestSurface : ISurface3D
    {
    }
}