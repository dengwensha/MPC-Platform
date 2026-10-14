using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public sealed class BSplineCurve3DTests
{
    [Fact]
    public void Curve_CopiesCollectionsAndUsesValueEquality()
    {
        Point3D[] points = [new(0, 0, 0), new(1, 2, 3), new(3, 2, 1)];
        int[] counts = [3, 3]; double[] knots = [-2, 5];
        var first = new BSplineCurve3D(2, points, counts, knots);
        var equal = new BSplineCurve3D(2, points, counts, knots);
        points[0] = new(99, 99, 99); counts[0] = 99; knots[0] = 99;
        Assert.Equal(new Point3D(0, 0, 0), first.ControlPoints[0]);
        Assert.Equal(3, first.Multiplicities[0]); Assert.Equal(-2, first.Knots[0]);
        Assert.True(first == equal); Assert.Equal(first.GetHashCode(), equal.GetHashCode());
        Assert.True(first != new BSplineCurve3D(2, points, [3, 3], [-2, 5]));
        Assert.False(first.Equals(null));
        Assert.Throws<NotSupportedException>(() => ((IList<Point3D>)first.ControlPoints)[0] = new(8, 8, 8));
        Assert.Throws<NotSupportedException>(() => ((IList<int>)first.Multiplicities)[0] = 9);
        Assert.Throws<NotSupportedException>(() => ((IList<double>)first.Knots)[0] = 9);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(int.MaxValue)]
    public void InvalidDegreeOrPointCount_IsRejected(int degree)
    {
        Assert.ThrowsAny<ArgumentException>(() => new BSplineCurve3D(degree, Points(), [3, 3], [0, 1]));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonFiniteValues_AreRejected(double value)
    {
        Assert.ThrowsAny<ArgumentException>(() => new BSplineCurve3D(2,
            new Point3D[] { new(value, 0, 0), new(1, 0, 0), new(2, 0, 0) }, [3, 3], [0, 1]));
        Assert.ThrowsAny<ArgumentException>(() => new BSplineCurve3D(2, Points(), [3, 3], [0, value]));
    }

    [Fact]
    public void InvalidKnotAndMultiplicityCombinations_AreRejected()
    {
        Assert.Throws<ArgumentException>(() => new BSplineCurve3D(2, Points(), [3, 3], [1, 0]));
        Assert.Throws<ArgumentException>(() => new BSplineCurve3D(2, Points(), [3, 3], [0, 0]));
        Assert.Throws<ArgumentException>(() => new BSplineCurve3D(2, Points(), [2, 3], [0, 1]));
        Assert.Throws<ArgumentException>(() => new BSplineCurve3D(2, Points(), [0, 6], [0, 1]));
        Assert.Throws<ArgumentException>(() => new BSplineCurve3D(2, Points(), [6], [0, 1]));
        Assert.Throws<ArgumentException>(() => new BSplineCurve3D(2, Points(), [1, 3, 2], [0, 0.5, 1]));
        Assert.Throws<ArgumentNullException>(() => new BSplineCurve3D(2, null!, [3, 3], [0, 1]));
        Assert.Throws<ArgumentNullException>(() => new BSplineCurve3D(2, Points(), null!, [0, 1]));
        Assert.Throws<ArgumentNullException>(() => new BSplineCurve3D(2, Points(), [3, 3], null!));
    }

    [Fact]
    public void InteriorKnotsRemainInSourceOrderWithoutNormalization()
    {
        var curve = new BSplineCurve3D(2, new Point3D[] { new(0,0,0), new(1,0,0), new(2,0,0), new(3,0,0) },
            [3,1,3], [-5,2,7]);
        Assert.Equal(new[] { -5.0,2.0,7.0 }, curve.Knots);
        Assert.Equal(new[] { 3,1,3 }, curve.Multiplicities);
    }

    private static Point3D[] Points() => [new(0,0,0), new(1,2,0), new(3,0,0)];
}
