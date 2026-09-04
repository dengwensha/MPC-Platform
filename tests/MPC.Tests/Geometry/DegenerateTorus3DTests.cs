using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public sealed class DegenerateTorus3DTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_ShouldStoreProvidedValues(
        bool selectOuter)
    {
        var center = new Point3D(1.0, 2.0, 3.0);
        var axis = new Vector3D(0.0, 0.0, 1.0);

        var torus = new DegenerateTorus3D(
            center,
            axis,
            5.0,
            20.0,
            selectOuter);

        Assert.Equal(center, torus.Center);
        Assert.Equal(axis, torus.AxisDirection);
        Assert.Equal(5.0, torus.MajorRadius);
        Assert.Equal(20.0, torus.MinorRadius);
        Assert.Equal(selectOuter, torus.SelectOuter);
        Assert.IsAssignableFrom<ISurface3D>(torus);
    }

    [Fact]
    public void Constructor_ShouldAcceptUnitAxisWithinTolerance()
    {
        var component = Math.Sqrt(0.5);

        var axis = new Vector3D(
            component,
            component,
            0.0);

        var torus = new DegenerateTorus3D(
            new Point3D(),
            axis,
            5.0,
            20.0,
            true);

        Assert.Equal(axis, torus.AxisDirection);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenCenterContainsNaN()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DegenerateTorus3D(
                new Point3D(double.NaN, 0.0, 0.0),
                new Vector3D(0.0, 0.0, 1.0),
                5.0,
                20.0,
                true));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAxisContainsInfinity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DegenerateTorus3D(
                new Point3D(),
                new Vector3D(
                    double.PositiveInfinity,
                    0.0,
                    0.0),
                5.0,
                20.0,
                true));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAxisIsNotUnitVector()
    {
        Assert.Throws<ArgumentException>(() =>
            new DegenerateTorus3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 2.0),
                5.0,
                20.0,
                true));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMajorRadiusIsZero()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DegenerateTorus3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 1.0),
                0.0,
                20.0,
                true));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMinorRadiusIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DegenerateTorus3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 1.0),
                5.0,
                -1.0,
                true));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMajorRadiusIsNotFinite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DegenerateTorus3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 1.0),
                double.NaN,
                20.0,
                true));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMinorRadiusIsNotFinite()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DegenerateTorus3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 1.0),
                5.0,
                double.PositiveInfinity,
                true));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMajorRadiusEqualsMinorRadius()
    {
        Assert.Throws<ArgumentException>(() =>
            new DegenerateTorus3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 1.0),
                10.0,
                10.0,
                true));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenMajorRadiusIsGreaterThanMinorRadius()
    {
        Assert.Throws<ArgumentException>(() =>
            new DegenerateTorus3D(
                new Point3D(),
                new Vector3D(0.0, 0.0, 1.0),
                20.0,
                5.0,
                true));
    }

    [Fact]
    public void EqualDegenerateTori_ShouldBeValueEqual()
    {
        var first = new DegenerateTorus3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(0.0, 0.0, 1.0),
            5.0,
            20.0,
            true);

        var second = new DegenerateTorus3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(0.0, 0.0, 1.0),
            5.0,
            20.0,
            true);

        Assert.Equal(first, second);
    }

    [Fact]
    public void DegenerateTori_WithDifferentSelectOuter_ShouldNotBeValueEqual()
    {
        var selectOuter = new DegenerateTorus3D(
            new Point3D(),
            new Vector3D(0.0, 0.0, 1.0),
            5.0,
            20.0,
            true);

        var selectInner = new DegenerateTorus3D(
            new Point3D(),
            new Vector3D(0.0, 0.0, 1.0),
            5.0,
            20.0,
            false);

        Assert.NotEqual(selectOuter, selectInner);
    }
}
