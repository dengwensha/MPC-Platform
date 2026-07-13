using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public sealed class Arc3DTests
{
    [Fact]
    public void Constructor_ShouldStoreProvidedValues()
    {
        var circle = new Circle3D(
            new Point3D(1.0, 2.0, 3.0),
            new Vector3D(0.0, 0.0, 1.0),
            10.0);

        var arc = new Arc3D(
            circle,
            0.0,
            Math.PI,
            ArcDirection.CounterClockwise);

        Assert.Equal(circle, arc.Circle);
        Assert.Equal(0.0, arc.StartAngle);
        Assert.Equal(Math.PI, arc.EndAngle);
        Assert.Equal(ArcDirection.CounterClockwise, arc.Direction);
    }

    [Fact]
    public void Constructor_ShouldStoreClockwiseDirection()
    {
        var circle = CreateValidCircle();

        var arc = new Arc3D(
            circle,
            Math.PI,
            0.0,
            ArcDirection.Clockwise);

        Assert.Equal(ArcDirection.Clockwise, arc.Direction);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenStartAngleIsNaN()
    {
        var circle = CreateValidCircle();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Arc3D(
                circle,
                double.NaN,
                Math.PI,
                ArcDirection.CounterClockwise));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenStartAngleIsPositiveInfinity()
    {
        var circle = CreateValidCircle();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Arc3D(
                circle,
                double.PositiveInfinity,
                Math.PI,
                ArcDirection.CounterClockwise));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenStartAngleIsNegativeInfinity()
    {
        var circle = CreateValidCircle();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Arc3D(
                circle,
                double.NegativeInfinity,
                Math.PI,
                ArcDirection.CounterClockwise));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenEndAngleIsNaN()
    {
        var circle = CreateValidCircle();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Arc3D(
                circle,
                0.0,
                double.NaN,
                ArcDirection.CounterClockwise));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenEndAngleIsPositiveInfinity()
    {
        var circle = CreateValidCircle();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Arc3D(
                circle,
                0.0,
                double.PositiveInfinity,
                ArcDirection.CounterClockwise));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenEndAngleIsNegativeInfinity()
    {
        var circle = CreateValidCircle();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Arc3D(
                circle,
                0.0,
                double.NegativeInfinity,
                ArcDirection.CounterClockwise));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenStartAndEndAnglesAreEqual()
    {
        var circle = CreateValidCircle();

        Assert.Throws<ArgumentException>(() =>
            new Arc3D(
                circle,
                Math.PI,
                Math.PI,
                ArcDirection.CounterClockwise));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenDirectionIsInvalid()
    {
        var circle = CreateValidCircle();
        var invalidDirection = (ArcDirection)999;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Arc3D(
                circle,
                0.0,
                Math.PI,
                invalidDirection));
    }

    [Fact]
    public void EqualArcs_ShouldBeValueEqual()
    {
        var circle = CreateValidCircle();

        var first = new Arc3D(
            circle,
            0.0,
            Math.PI,
            ArcDirection.CounterClockwise);

        var second = new Arc3D(
            circle,
            0.0,
            Math.PI,
            ArcDirection.CounterClockwise);

        Assert.Equal(first, second);
    }

    private static Circle3D CreateValidCircle()
    {
        return new Circle3D(
            new Point3D(0.0, 0.0, 0.0),
            new Vector3D(0.0, 0.0, 1.0),
            5.0);
    }
}