using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public sealed class Circle3DTests
{
    [Fact]
    public void Constructor_ShouldStoreProvidedValues()
    {
        var center = new Point3D(1.0, 2.0, 3.0);
        var normal = new Vector3D(0.0, 0.0, 1.0);
        const double radius = 10.0;

        var circle = new Circle3D(center, normal, radius);

        Assert.Equal(center, circle.Center);
        Assert.Equal(normal, circle.Normal);
        Assert.Equal(radius, circle.Radius);
    }

    [Fact]
    public void Constructor_ShouldAcceptUnitVectorWithinTolerance()
    {
        var normal = new Vector3D(
            Math.Sqrt(0.5),
            Math.Sqrt(0.5),
            0.0);

        var circle = new Circle3D(
            new Point3D(0.0, 0.0, 0.0),
            normal,
            5.0);

        Assert.Equal(normal, circle.Normal);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCenterXIsNotFinite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Circle3D(
                new Point3D(double.NaN, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                5.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCenterYIsNotFinite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Circle3D(
                new Point3D(0.0, double.PositiveInfinity, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                5.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCenterZIsNotFinite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Circle3D(
                new Point3D(0.0, 0.0, double.NegativeInfinity),
                new Vector3D(0.0, 0.0, 1.0),
                5.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNormalContainsNaN()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Circle3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(double.NaN, 0.0, 1.0),
                5.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNormalContainsInfinity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Circle3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, double.PositiveInfinity, 1.0),
                5.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNormalIsZeroVector()
    {
        Assert.Throws<ArgumentException>(() =>
            new Circle3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 0.0),
                5.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNormalIsNotUnitVector()
    {
        Assert.Throws<ArgumentException>(() =>
            new Circle3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 2.0),
                5.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRadiusIsZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Circle3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                0.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRadiusIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Circle3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                -1.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRadiusIsNaN()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Circle3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                double.NaN));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRadiusIsInfinity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Circle3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                double.PositiveInfinity));
    }

    [Fact]
    public void EqualCircles_ShouldBeValueEqual()
    {
        var first = new Circle3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(0.0, 0.0, 1.0),
            10.0);

        var second = new Circle3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(0.0, 0.0, 1.0),
            10.0);

        Assert.Equal(first, second);
    }
}