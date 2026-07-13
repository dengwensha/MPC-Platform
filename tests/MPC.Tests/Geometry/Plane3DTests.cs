using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public sealed class Plane3DTests
{
    [Fact]
    public void Constructor_ShouldStoreProvidedValues()
    {
        var origin = new Point3D(1.0, 2.0, 3.0);
        var normal = new Vector3D(0.0, 0.0, 1.0);

        var plane = new Plane3D(origin, normal);

        Assert.Equal(origin, plane.Origin);
        Assert.Equal(normal, plane.Normal);
    }

    [Fact]
    public void Constructor_ShouldAcceptUnitNormalWithinTolerance()
    {
        var component = Math.Sqrt(0.5);
        var normal = new Vector3D(component, component, 0.0);

        var plane = new Plane3D(
            new Point3D(0.0, 0.0, 0.0),
            normal);

        Assert.Equal(normal, plane.Normal);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOriginXIsNotFinite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Plane3D(
                new Point3D(double.NaN, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOriginYIsNotFinite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Plane3D(
                new Point3D(0.0, double.PositiveInfinity, 0.0),
                new Vector3D(0.0, 0.0, 1.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOriginZIsNotFinite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Plane3D(
                new Point3D(0.0, 0.0, double.NegativeInfinity),
                new Vector3D(0.0, 0.0, 1.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNormalContainsNaN()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Plane3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(double.NaN, 0.0, 1.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNormalContainsInfinity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Plane3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, double.PositiveInfinity, 1.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNormalIsZeroVector()
    {
        Assert.Throws<ArgumentException>(() =>
            new Plane3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 0.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNormalIsNotUnitVector()
    {
        Assert.Throws<ArgumentException>(() =>
            new Plane3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 2.0)));
    }

    [Fact]
    public void EqualPlanes_ShouldBeValueEqual()
    {
        var first = new Plane3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(0.0, 0.0, 1.0));

        var second = new Plane3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(0.0, 0.0, 1.0));

        Assert.Equal(first, second);
    }
}