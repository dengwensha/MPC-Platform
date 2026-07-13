using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public sealed class Cone3DTests
{
    [Fact]
    public void Constructor_ShouldStoreProvidedValues()
    {
        var apex = new Point3D(1.0, 2.0, 3.0);
        var direction = new Vector3D(0.0, 0.0, 1.0);
        var semiAngle = Math.PI / 6.0;

        var cone = new Cone3D(
            apex,
            direction,
            semiAngle);

        Assert.Equal(apex, cone.Apex);
        Assert.Equal(direction, cone.AxisDirection);
        Assert.Equal(semiAngle, cone.SemiAngle);
    }

    [Fact]
    public void Constructor_ShouldAcceptUnitDirectionWithinTolerance()
    {
        var component = Math.Sqrt(0.5);

        var direction = new Vector3D(
            component,
            component,
            0.0);

        var cone = new Cone3D(
            new Point3D(),
            direction,
            Math.PI / 4.0);

        Assert.Equal(direction, cone.AxisDirection);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenApexContainsNaN()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Cone3D(
                new Point3D(double.NaN, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                Math.PI / 6.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDirectionContainsInfinity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Cone3D(
                new Point3D(),
                new Vector3D(
                    double.PositiveInfinity,
                    0.0,
                    0.0),
                Math.PI / 6.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDirectionIsNotUnitVector()
    {
        Assert.Throws<ArgumentException>(() =>
            new Cone3D(
                new Point3D(),
                new Vector3D(
                    0.0,
                    0.0,
                    2.0),
                Math.PI / 6.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSemiAngleIsZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Cone3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 1.0),
                0.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSemiAngleIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Cone3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 1.0),
                -0.1));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSemiAngleEqualsPiOverTwo()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Cone3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 1.0),
                Math.PI / 2.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSemiAngleIsNotFinite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Cone3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 1.0),
                double.PositiveInfinity));
    }

    [Fact]
    public void EqualCones_ShouldBeValueEqual()
    {
        var first = new Cone3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(0.0, 0.0, 1.0),
            Math.PI / 6.0);

        var second = new Cone3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(0.0, 0.0, 1.0),
            Math.PI / 6.0);

        Assert.Equal(first, second);
    }
}