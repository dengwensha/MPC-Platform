using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public sealed class Line3DTests
{
    [Fact]
    public void Constructor_ShouldStoreProvidedValues()
    {
        var origin = new Point3D(0.0, 0.0, 0.0);
        var direction = new Vector3D(1.0, 0.0, 0.0);

        var line = new Line3D(origin, direction);

        Assert.Equal(origin, line.Origin);
        Assert.Equal(direction, line.Direction);
    }

    [Fact]
    public void Constructor_ShouldAcceptUnitDirectionWithinTolerance()
    {
        var component = Math.Sqrt(0.5);
        var direction = new Vector3D(component, component, 0.0);

        var line = new Line3D(
            new Point3D(1.0, 2.0, 3.0),
            direction);

        Assert.Equal(direction, line.Direction);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOriginContainsNaN()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Line3D(
                new Point3D(double.NaN, 0.0, 0.0),
                new Vector3D(1.0, 0.0, 0.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOriginContainsInfinity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Line3D(
                new Point3D(0.0, double.PositiveInfinity, 0.0),
                new Vector3D(1.0, 0.0, 0.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDirectionContainsNaN()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Line3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(double.NaN, 0.0, 0.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDirectionContainsInfinity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Line3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, double.NegativeInfinity, 0.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDirectionIsZeroVector()
    {
        Assert.Throws<ArgumentException>(() =>
            new Line3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 0.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDirectionIsNotUnitVector()
    {
        Assert.Throws<ArgumentException>(() =>
            new Line3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(2.0, 0.0, 0.0)));
    }

    [Fact]
    public void EqualLines_ShouldBeValueEqual()
    {
        var first = new Line3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(1.0, 0.0, 0.0));

        var second = new Line3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(1.0, 0.0, 0.0));

        Assert.Equal(first, second);
    }
}