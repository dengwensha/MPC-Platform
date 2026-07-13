using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public sealed class Sphere3DTests
{
    [Fact]
    public void Constructor_ShouldStoreProvidedValues()
    {
        var center = new Point3D(1.0, 2.0, 3.0);

        var sphere = new Sphere3D(center, 25.0);

        Assert.Equal(center, sphere.Center);
        Assert.Equal(25.0, sphere.Radius);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCenterXIsNotFinite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Sphere3D(
                new Point3D(double.NaN, 0.0, 0.0),
                10.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCenterYIsNotFinite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Sphere3D(
                new Point3D(0.0, double.PositiveInfinity, 0.0),
                10.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCenterZIsNotFinite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Sphere3D(
                new Point3D(0.0, 0.0, double.NegativeInfinity),
                10.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRadiusIsZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Sphere3D(
                new Point3D(0.0, 0.0, 0.0),
                0.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRadiusIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Sphere3D(
                new Point3D(0.0, 0.0, 0.0),
                -1.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRadiusIsNaN()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Sphere3D(
                new Point3D(0.0, 0.0, 0.0),
                double.NaN));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRadiusIsInfinity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Sphere3D(
                new Point3D(0.0, 0.0, 0.0),
                double.PositiveInfinity));
    }

    [Fact]
    public void EqualSpheres_ShouldBeValueEqual()
    {
        var first = new Sphere3D(
            new Point3D(1.0, 2.0, 3.0),
            25.0);

        var second = new Sphere3D(
            new Point3D(1.0, 2.0, 3.0),
            25.0);

        Assert.Equal(first, second);
    }
}