using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public sealed class Torus3DTests
{
    [Fact]
    public void Constructor_ShouldStoreProvidedValues()
    {
        var center = new Point3D(1.0, 2.0, 3.0);
        var axis = new Vector3D(0.0, 0.0, 1.0);

        var torus = new Torus3D(
            center,
            axis,
            20.0,
            5.0);

        Assert.Equal(center, torus.Center);
        Assert.Equal(axis, torus.AxisDirection);
        Assert.Equal(20.0, torus.MajorRadius);
        Assert.Equal(5.0, torus.MinorRadius);
    }

    [Fact]
    public void Constructor_ShouldAcceptUnitAxisWithinTolerance()
    {
        var component = Math.Sqrt(0.5);

        var axis = new Vector3D(
            component,
            component,
            0.0);

        var torus = new Torus3D(
            new Point3D(),
            axis,
            20.0,
            5.0);

        Assert.Equal(axis, torus.AxisDirection);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCenterContainsNaN()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Torus3D(
                new Point3D(double.NaN, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                20.0,
                5.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAxisContainsInfinity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Torus3D(
                new Point3D(),
                new Vector3D(
                    double.PositiveInfinity,
                    0.0,
                    0.0),
                20.0,
                5.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAxisIsNotUnitVector()
    {
        Assert.Throws<ArgumentException>(() =>
            new Torus3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 2.0),
                20.0,
                5.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMajorRadiusIsZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Torus3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 1.0),
                0.0,
                5.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMinorRadiusIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Torus3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 1.0),
                20.0,
                -1.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMajorRadiusEqualsMinorRadius()
    {
        Assert.Throws<ArgumentException>(() =>
            new Torus3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 1.0),
                10.0,
                10.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMajorRadiusIsLessThanMinorRadius()
    {
        Assert.Throws<ArgumentException>(() =>
            new Torus3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 1.0),
                5.0,
                10.0));
    }

    [Fact]
    public void EqualTori_ShouldBeValueEqual()
    {
        var first = new Torus3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(0.0, 0.0, 1.0),
            20.0,
            5.0);

        var second = new Torus3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(0.0, 0.0, 1.0),
            20.0,
            5.0);

        Assert.Equal(first, second);
    }
}