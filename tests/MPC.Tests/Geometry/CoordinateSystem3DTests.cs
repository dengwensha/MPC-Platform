using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public sealed class CoordinateSystem3DTests
{
    [Fact]
    public void Constructor_ShouldStoreOriginAndInputAxes()
    {
        var origin = new Point3D(1.0, 2.0, 3.0);
        var xAxis = new Vector3D(1.0, 0.0, 0.0);
        var yAxis = new Vector3D(0.0, 1.0, 0.0);

        var coordinateSystem = new CoordinateSystem3D(
            origin,
            xAxis,
            yAxis);

        Assert.Equal(origin, coordinateSystem.Origin);
        Assert.Equal(xAxis, coordinateSystem.XAxis);
        Assert.Equal(yAxis, coordinateSystem.YAxis);
    }

    [Fact]
    public void Constructor_ShouldDeriveZAxisUsingCrossProduct()
    {
        var coordinateSystem = new CoordinateSystem3D(
            new Point3D(0.0, 0.0, 0.0),
            new Vector3D(1.0, 0.0, 0.0),
            new Vector3D(0.0, 1.0, 0.0));

        Assert.Equal(
            new Vector3D(0.0, 0.0, 1.0),
            coordinateSystem.ZAxis);
    }

    [Fact]
    public void Constructor_ShouldCreateRightHandedCoordinateSystem()
    {
        var coordinateSystem = new CoordinateSystem3D(
            new Point3D(0.0, 0.0, 0.0),
            new Vector3D(0.0, 1.0, 0.0),
            new Vector3D(0.0, 0.0, 1.0));

        Assert.Equal(
            new Vector3D(1.0, 0.0, 0.0),
            coordinateSystem.ZAxis);
    }

    [Fact]
    public void Constructor_ShouldAcceptUnitAxesWithinTolerance()
    {
        var component = Math.Sqrt(0.5);

        var coordinateSystem = new CoordinateSystem3D(
            new Point3D(0.0, 0.0, 0.0),
            new Vector3D(component, component, 0.0),
            new Vector3D(-component, component, 0.0));

        Assert.Equal(
            new Vector3D(component, component, 0.0),
            coordinateSystem.XAxis);

        Assert.Equal(
            new Vector3D(-component, component, 0.0),
            coordinateSystem.YAxis);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOriginContainsNaN()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CoordinateSystem3D(
                new Point3D(double.NaN, 0.0, 0.0),
                new Vector3D(1.0, 0.0, 0.0),
                new Vector3D(0.0, 1.0, 0.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOriginContainsInfinity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CoordinateSystem3D(
                new Point3D(0.0, double.PositiveInfinity, 0.0),
                new Vector3D(1.0, 0.0, 0.0),
                new Vector3D(0.0, 1.0, 0.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenXAxisContainsNonFiniteValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CoordinateSystem3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(double.NaN, 0.0, 0.0),
                new Vector3D(0.0, 1.0, 0.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenYAxisContainsNonFiniteValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CoordinateSystem3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(1.0, 0.0, 0.0),
                new Vector3D(0.0, double.NegativeInfinity, 0.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenXAxisIsNotUnitVector()
    {
        Assert.Throws<ArgumentException>(() =>
            new CoordinateSystem3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(2.0, 0.0, 0.0),
                new Vector3D(0.0, 1.0, 0.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenYAxisIsNotUnitVector()
    {
        Assert.Throws<ArgumentException>(() =>
            new CoordinateSystem3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(1.0, 0.0, 0.0),
                new Vector3D(0.0, 0.5, 0.0)));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAxesAreNotOrthogonal()
    {
        Assert.Throws<ArgumentException>(() =>
            new CoordinateSystem3D(
                new Point3D(0.0, 0.0, 0.0),
                new Vector3D(1.0, 0.0, 0.0),
                new Vector3D(
                    Math.Sqrt(0.5),
                    Math.Sqrt(0.5),
                    0.0)));
    }

    [Fact]
    public void EqualCoordinateSystems_ShouldBeValueEqual()
    {
        var first = new CoordinateSystem3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(1.0, 0.0, 0.0),
            new Vector3D(0.0, 1.0, 0.0));

        var second = new CoordinateSystem3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(1.0, 0.0, 0.0),
            new Vector3D(0.0, 1.0, 0.0));

        Assert.Equal(first, second);
    }
}