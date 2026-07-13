using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public sealed class Cylinder3DTests
{
    [Fact]
    public void Constructor_ShouldStoreProvidedValues()
    {
        var origin = new Point3D(1, 2, 3);
        var direction = new Vector3D(0, 0, 1);

        var cylinder = new Cylinder3D(
            origin,
            direction,
            25.0);

        Assert.Equal(origin, cylinder.AxisOrigin);
        Assert.Equal(direction, cylinder.AxisDirection);
        Assert.Equal(25.0, cylinder.Radius);
    }

    [Fact]
    public void Constructor_ShouldAcceptUnitDirectionWithinTolerance()
    {
        var component = Math.Sqrt(0.5);

        var direction = new Vector3D(
            component,
            component,
            0);

        var cylinder = new Cylinder3D(
            new Point3D(),
            direction,
            10);

        Assert.Equal(direction, cylinder.AxisDirection);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOriginContainsNaN()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Cylinder3D(
                new Point3D(double.NaN, 0, 0),
                new Vector3D(0, 0, 1),
                10));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDirectionIsNotUnit()
    {
        Assert.Throws<ArgumentException>(() =>
            new Cylinder3D(
                new Point3D(),
                new Vector3D(0, 0, 2),
                10));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRadiusIsZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Cylinder3D(
                new Point3D(),
                new Vector3D(0, 0, 1),
                0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRadiusIsNotFinite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Cylinder3D(
                new Point3D(),
                new Vector3D(0, 0, 1),
                double.PositiveInfinity));
    }

    [Fact]
    public void EqualCylinders_ShouldBeValueEqual()
    {
        var first = new Cylinder3D(
            new Point3D(1, 2, 3),
            new Vector3D(0, 0, 1),
            25);

        var second = new Cylinder3D(
            new Point3D(1, 2, 3),
            new Vector3D(0, 0, 1),
            25);

        Assert.Equal(first, second);
    }
}