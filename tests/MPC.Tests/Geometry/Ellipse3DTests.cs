using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public sealed class Ellipse3DTests
{
    [Fact]
    public void Constructor_ShouldStoreProvidedValues()
    {
        var center = new Point3D(1.0, 2.0, 3.0);
        var normal = new Vector3D(0.0, 0.0, 1.0);
        var majorAxisDirection = new Vector3D(1.0, 0.0, 0.0);

        var ellipse = new Ellipse3D(
            center,
            normal,
            majorAxisDirection,
            20.0,
            10.0);

        Assert.Equal(center, ellipse.Center);
        Assert.Equal(normal, ellipse.Normal);
        Assert.Equal(majorAxisDirection, ellipse.MajorAxisDirection);
        Assert.Equal(20.0, ellipse.MajorRadius);
        Assert.Equal(10.0, ellipse.MinorRadius);
    }

    [Fact]
    public void Constructor_ShouldAcceptUnitDirectionsWithinTolerance()
    {
        var component = Math.Sqrt(0.5);

        var ellipse = new Ellipse3D(
            new Point3D(0.0, 0.0, 0.0),
            new Vector3D(0.0, 0.0, 1.0),
            new Vector3D(component, component, 0.0),
            20.0,
            10.0);

        Assert.Equal(
            new Vector3D(component, component, 0.0),
            ellipse.MajorAxisDirection);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCenterContainsNonFiniteValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Ellipse3D(
                new Point3D(double.NaN, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                new Vector3D(1.0, 0.0, 0.0),
                20.0,
                10.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNormalContainsNonFiniteValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Ellipse3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(double.PositiveInfinity, 0.0, 1.0),
                new Vector3D(1.0, 0.0, 0.0),
                20.0,
                10.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMajorAxisDirectionContainsNonFiniteValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Ellipse3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                new Vector3D(1.0, double.NaN, 0.0),
                20.0,
                10.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNormalIsNotUnitVector()
    {
        Assert.Throws<ArgumentException>(() =>
            new Ellipse3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 2.0),
                new Vector3D(1.0, 0.0, 0.0),
                20.0,
                10.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMajorAxisDirectionIsNotUnitVector()
    {
        Assert.Throws<ArgumentException>(() =>
            new Ellipse3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                new Vector3D(2.0, 0.0, 0.0),
                20.0,
                10.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenNormalAndMajorAxisDirectionAreNotOrthogonal()
    {
        Assert.Throws<ArgumentException>(() =>
            new Ellipse3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                new Vector3D(
                    Math.Sqrt(0.5),
                    0.0,
                    Math.Sqrt(0.5)),
                20.0,
                10.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMajorRadiusIsZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Ellipse3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                new Vector3D(1.0, 0.0, 0.0),
                0.0,
                10.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMinorRadiusIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Ellipse3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                new Vector3D(1.0, 0.0, 0.0),
                20.0,
                -1.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenRadiusIsNotFinite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Ellipse3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                new Vector3D(1.0, 0.0, 0.0),
                double.PositiveInfinity,
                10.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMajorRadiusEqualsMinorRadius()
    {
        Assert.Throws<ArgumentException>(() =>
            new Ellipse3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                new Vector3D(1.0, 0.0, 0.0),
                10.0,
                10.0));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMajorRadiusIsLessThanMinorRadius()
    {
        Assert.Throws<ArgumentException>(() =>
            new Ellipse3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                new Vector3D(1.0, 0.0, 0.0),
                5.0,
                10.0));
    }

    [Fact]
    public void EqualEllipses_ShouldBeValueEqual()
    {
        var first = new Ellipse3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(0.0, 0.0, 1.0),
            new Vector3D(1.0, 0.0, 0.0),
            20.0,
            10.0);

        var second = new Ellipse3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(0.0, 0.0, 1.0),
            new Vector3D(1.0, 0.0, 0.0),
            20.0,
            10.0);

        Assert.Equal(first, second);
    }
}