using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public sealed class RationalBSplineCurve3DTests
{
    [Fact]
    public void Weights_AreCopiedReadOnlyAndNotNormalized()
    {
        double[] weights = [1, 0.5, 2];
        var basis = Basis();
        var curve = new RationalBSplineCurve3D(basis, weights);
        weights[0] = 99;
        Assert.Same(basis, curve.Basis);
        Assert.Equal(new double[] { 1, 0.5, 2 }, curve.Weights);
        Assert.Throws<NotSupportedException>(() => ((IList<double>)curve.Weights)[0] = 9);
    }

    [Fact]
    public void Equality_IncludesBasisAndOrderedWeights()
    {
        var first = new RationalBSplineCurve3D(Basis(), [1, 0.5, 2]);
        var equal = new RationalBSplineCurve3D(Basis(), [1, 0.5, 2]);
        Assert.True(first == equal);
        Assert.True(first.Equals((object)equal));
        Assert.Equal(first.GetHashCode(), equal.GetHashCode());
        Assert.True(first != new RationalBSplineCurve3D(Basis(), [2, 0.5, 1]));
        Assert.True(first != new RationalBSplineCurve3D(Basis(), [2, 1, 4]));
        Assert.True(first != new RationalBSplineCurve3D(
            new BSplineCurve3D(2, [new(0,0,0), new(1,3,0), new(3,0,0)], [3,3], [-2,5]), [1,0.5,2]));
        Assert.False(first.Equals(null));
        Assert.False(first.Equals("curve"));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidWeights_AreRejected(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RationalBSplineCurve3D(Basis(), [1,value,2]));
    }

    [Fact]
    public void MissingOrMismatchedInputs_AreRejected()
    {
        Assert.Throws<ArgumentNullException>(() => new RationalBSplineCurve3D(null!, [1,1,1]));
        Assert.Throws<ArgumentNullException>(() => new RationalBSplineCurve3D(Basis(), null!));
        Assert.Throws<ArgumentException>(() => new RationalBSplineCurve3D(Basis(), []));
        Assert.Throws<ArgumentException>(() => new RationalBSplineCurve3D(Basis(), [1,1]));
        Assert.Throws<ArgumentException>(() => new RationalBSplineCurve3D(Basis(), [1,1,1,1]));
    }

    private static BSplineCurve3D Basis() => new(2, [new(0,0,0), new(1,2,0), new(3,0,0)], [3,3], [-2,5]);
}
