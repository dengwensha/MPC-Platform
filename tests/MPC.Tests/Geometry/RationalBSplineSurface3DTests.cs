using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public sealed class RationalBSplineSurface3DTests
{
    [Fact]
    public void Weights_PreserveValuesAndAreImmutable()
    {
        double[][] weights = [[1, 2], [3, 4]];
        var surface = new RationalBSplineSurface3D(Basis(), weights);
        weights[0][0] = 99;
        weights[1] = [7, 8];
        Assert.Equal(1, surface.Weights[0][0]);
        Assert.Equal(3, surface.Weights[1][0]);
        Assert.Throws<NotSupportedException>(() => ((IList<double>)surface.Weights[0])[0] = 5);
        Assert.Throws<NotSupportedException>(() => ((IList<IReadOnlyList<double>>)surface.Weights)[0] = new double[] { 5, 6 });
        Assert.Equal(new double[] { -2, 5 }, surface.Basis.UKnots);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidWeight_IsRejected(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RationalBSplineSurface3D(Basis(), new double[][] { [1, value], [3, 4] }));
    }

    [Fact]
    public void InvalidDimensionsAndNulls_AreRejected()
    {
        Assert.Throws<ArgumentNullException>(() => new RationalBSplineSurface3D(null!, new double[][] { [1, 2], [3, 4] }));
        Assert.Throws<ArgumentNullException>(() => new RationalBSplineSurface3D(Basis(), null!));
        Assert.Throws<ArgumentException>(() => new RationalBSplineSurface3D(Basis(), Array.Empty<double[]>()));
        Assert.Throws<ArgumentException>(() => new RationalBSplineSurface3D(Basis(), new double[][] { [1, 2] }));
        Assert.Throws<ArgumentException>(() => new RationalBSplineSurface3D(Basis(), new double[][] { [1, 2], [3] }));
        Assert.Throws<ArgumentException>(() => new RationalBSplineSurface3D(Basis(), new double[][] { [1, 2], null! }));
    }

    [Fact]
    public void Equality_IncludesBasisAndUnnormalisedWeights()
    {
        var first = new RationalBSplineSurface3D(Basis(), new double[][] { [1, 2], [3, 4] });
        var equal = new RationalBSplineSurface3D(Basis(), new double[][] { [1, 2], [3, 4] });
        var scaled = new RationalBSplineSurface3D(Basis(), new double[][] { [2, 4], [6, 8] });
        var changedBasis = new RationalBSplineSurface3D(Basis(2), new double[][] { [1, 2], [3, 4] });
        Assert.Equal(first, equal);
        Assert.True(first == equal);
        Assert.Equal(first.GetHashCode(), equal.GetHashCode());
        Assert.True(first != scaled);
        Assert.NotEqual(first, changedBasis);
        Assert.False(first.Equals(null));
    }

    private static BSplineSurface3D Basis(double z = 0) => new(1, 1,
        new Point3D[][] { [new(0, 0, z), new(0, 1, z)], [new(1, 0, z), new(1, 1, z)] },
        [2, 2], [2, 2], [-2, 5], [-3, 7]);
}
