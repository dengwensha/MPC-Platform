using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public sealed class BSplineSurface3DTests
{
    [Fact]
    public void Constructor_ShouldStoreProvidedValues()
    {
        var controlPointGrid = CreateValidControlPointGrid();
        var uMultiplicities = new[] { 3, 1, 3 };
        var vMultiplicities = new[] { 2, 1, 2 };
        var uKnots = new[] { 0.0, 0.5, 1.0 };
        var vKnots = new[] { -1.0, 0.0, 1.0 };

        var surface = new BSplineSurface3D(
            2,
            1,
            controlPointGrid,
            uMultiplicities,
            vMultiplicities,
            uKnots,
            vKnots);

        Assert.Equal(2, surface.UDegree);
        Assert.Equal(1, surface.VDegree);
        Assert.Equal(4, surface.ControlPointGrid.Count);
        Assert.All(
            surface.ControlPointGrid,
            row => Assert.Equal(3, row.Count));

        Assert.Equal(
            uMultiplicities,
            surface.UMultiplicities);

        Assert.Equal(
            vMultiplicities,
            surface.VMultiplicities);

        Assert.Equal(
            uKnots,
            surface.UKnots);

        Assert.Equal(
            vKnots,
            surface.VKnots);

        Assert.IsAssignableFrom<ISurface3D>(surface);
    }

    [Fact]
    public void Constructor_ShouldPreserveControlPointOrder()
    {
        var grid = CreateValidControlPointGrid();

        var surface = CreateSurface(
            controlPointGrid: grid);

        Assert.Equal(
            new Point3D(0.0, 0.0, 0.0),
            surface.ControlPointGrid[0][0]);

        Assert.Equal(
            new Point3D(1.0, 2.0, 0.0),
            surface.ControlPointGrid[1][2]);

        Assert.Equal(
            new Point3D(3.0, 2.0, 1.0),
            surface.ControlPointGrid[3][2]);
    }

    [Fact]
    public void Constructor_ShouldDefensivelyCopyInputs()
    {
        var grid = CreateValidControlPointGrid();
        var uMultiplicities = new List<int> { 3, 1, 3 };
        var vMultiplicities = new List<int> { 2, 1, 2 };
        var uKnots = new List<double> { 0.0, 0.5, 1.0 };
        var vKnots = new List<double> { -1.0, 0.0, 1.0 };

        var surface = new BSplineSurface3D(
            2,
            1,
            grid,
            uMultiplicities,
            vMultiplicities,
            uKnots,
            vKnots);

        grid[0][0] = new Point3D(99.0, 99.0, 99.0);
        grid.Add(
            new List<Point3D>
            {
                new(10.0, 0.0, 0.0),
                new(10.0, 1.0, 0.0),
                new(10.0, 2.0, 0.0)
            });

        uMultiplicities[0] = 1;
        vMultiplicities[0] = 1;
        uKnots[0] = -100.0;
        vKnots[0] = -100.0;

        Assert.Equal(4, surface.ControlPointGrid.Count);

        Assert.Equal(
            new Point3D(0.0, 0.0, 0.0),
            surface.ControlPointGrid[0][0]);

        Assert.Equal(3, surface.UMultiplicities[0]);
        Assert.Equal(2, surface.VMultiplicities[0]);
        Assert.Equal(0.0, surface.UKnots[0]);
        Assert.Equal(-1.0, surface.VKnots[0]);
    }

    [Fact]
    public void PublishedCollections_ShouldBeReadOnly()
    {
        var surface = CreateSurface();

        var outerGrid =
            Assert.IsAssignableFrom<
                IList<IReadOnlyList<Point3D>>>(
                    surface.ControlPointGrid);

        var firstRow =
            Assert.IsAssignableFrom<IList<Point3D>>(
                surface.ControlPointGrid[0]);

        var uMultiplicities =
            Assert.IsAssignableFrom<IList<int>>(
                surface.UMultiplicities);

        var uKnots =
            Assert.IsAssignableFrom<IList<double>>(
                surface.UKnots);

        Assert.Throws<NotSupportedException>(() =>
            outerGrid.Add(
                Array.Empty<Point3D>()));

        Assert.Throws<NotSupportedException>(() =>
            firstRow[0] =
                new Point3D(99.0, 99.0, 99.0));

        Assert.Throws<NotSupportedException>(() =>
            uMultiplicities[0] = 1);

        Assert.Throws<NotSupportedException>(() =>
            uKnots[0] = -100.0);
    }

    [Fact]
    public void EqualSurfaces_ShouldBeValueEqual()
    {
        var first = CreateSurface();
        var second = CreateSurface();

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.False(first != second);
        Assert.Equal(
            first.GetHashCode(),
            second.GetHashCode());
    }

    [Fact]
    public void SurfacesWithDifferentControlPoints_ShouldNotBeValueEqual()
    {
        var first = CreateSurface();
        var secondGrid = CreateValidControlPointGrid();

        secondGrid[3][2] =
            new Point3D(30.0, 20.0, 10.0);

        var second = CreateSurface(
            controlPointGrid: secondGrid);

        Assert.NotEqual(first, second);
        Assert.True(first != second);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectInvalidUDegree(
        int degree)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateSurface(
                uDegree: degree));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectInvalidVDegree(
        int degree)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateSurface(
                vDegree: degree));
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyControlPointGrid()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateSurface(
                controlPointGrid:
                    Array.Empty<IReadOnlyList<Point3D>>()));
    }

    [Fact]
    public void Constructor_ShouldRejectEmptyControlPointRow()
    {
        IEnumerable<IEnumerable<Point3D>> grid =
            new IEnumerable<Point3D>[]
            {
                Array.Empty<Point3D>(),
                new[]
                {
                    new Point3D(),
                    new Point3D(0.0, 1.0, 0.0)
                }
            };

        Assert.Throws<ArgumentException>(() =>
            CreateSurface(
                controlPointGrid: grid));
    }

    [Fact]
    public void Constructor_ShouldRejectNonRectangularGrid()
    {
        var grid = CreateValidControlPointGrid();

        grid[1].RemoveAt(2);

        Assert.Throws<ArgumentException>(() =>
            CreateSurface(
                controlPointGrid: grid));
    }

    [Fact]
    public void Constructor_ShouldRejectInsufficientUControlPoints()
    {
        var grid =
            CreateValidControlPointGrid()
                .Take(2)
                .ToArray();

        Assert.Throws<ArgumentException>(() =>
            CreateSurface(
                uDegree: 2,
                controlPointGrid: grid));
    }

    [Fact]
    public void Constructor_ShouldRejectInsufficientVControlPoints()
    {
        var grid =
            CreateValidControlPointGrid()
                .Select(row =>
                    (IEnumerable<Point3D>)
                        row.Take(1).ToArray())
                .ToArray();

        Assert.Throws<ArgumentException>(() =>
            CreateSurface(
                vDegree: 1,
                controlPointGrid: grid));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_ShouldRejectNonFiniteControlPoint(
        double coordinate)
    {
        var grid = CreateValidControlPointGrid();

        grid[0][0] =
            new Point3D(
                coordinate,
                0.0,
                0.0);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateSurface(
                controlPointGrid: grid));
    }

    [Fact]
    public void Constructor_ShouldRejectMismatchedKnotAndMultiplicityCounts()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateSurface(
                uMultiplicities:
                    new[] { 3, 1, 3 },
                uKnots:
                    new[] { 0.0, 1.0 }));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectNonPositiveMultiplicity(
        int multiplicity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateSurface(
                uMultiplicities:
                    new[] { 3, multiplicity, 3 }));
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidMultiplicitySum()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateSurface(
                uMultiplicities:
                    new[] { 3, 1, 2 }));
    }

    [Fact]
    public void Constructor_ShouldRejectExcessiveEndpointMultiplicity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateSurface(
                uMultiplicities:
                    new[] { 4, 3 }));
    }

    [Fact]
    public void Constructor_ShouldRejectExcessiveInteriorMultiplicity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateSurface(
                uMultiplicities:
                    new[] { 2, 3, 2 }));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_ShouldRejectNonFiniteKnot(
        double knot)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateSurface(
                uKnots:
                    new[] { 0.0, knot, 1.0 }));
    }

    [Fact]
    public void Constructor_ShouldRejectDuplicateDistinctKnots()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateSurface(
                uKnots:
                    new[] { 0.0, 0.0, 1.0 }));
    }

    [Fact]
    public void Constructor_ShouldRejectDescendingKnots()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateSurface(
                uKnots:
                    new[] { 0.0, 1.0, 0.5 }));
    }

    private static BSplineSurface3D CreateSurface(
        int uDegree = 2,
        int vDegree = 1,
        IEnumerable<IEnumerable<Point3D>>?
            controlPointGrid = null,
        IEnumerable<int>?
            uMultiplicities = null,
        IEnumerable<int>?
            vMultiplicities = null,
        IEnumerable<double>?
            uKnots = null,
        IEnumerable<double>?
            vKnots = null)
    {
        return new BSplineSurface3D(
            uDegree,
            vDegree,
            controlPointGrid ??
                CreateValidControlPointGrid(),
            uMultiplicities ??
                new[] { 3, 1, 3 },
            vMultiplicities ??
                new[] { 2, 1, 2 },
            uKnots ??
                new[] { 0.0, 0.5, 1.0 },
            vKnots ??
                new[] { -1.0, 0.0, 1.0 });
    }

    private static List<List<Point3D>>
        CreateValidControlPointGrid()
    {
        return new List<List<Point3D>>
        {
            new()
            {
                new(0.0, 0.0, 0.0),
                new(0.0, 1.0, 0.0),
                new(0.0, 2.0, 0.0)
            },
            new()
            {
                new(1.0, 0.0, 0.0),
                new(1.0, 1.0, 1.0),
                new(1.0, 2.0, 0.0)
            },
            new()
            {
                new(2.0, 0.0, 0.0),
                new(2.0, 1.0, 1.0),
                new(2.0, 2.0, 0.0)
            },
            new()
            {
                new(3.0, 0.0, 1.0),
                new(3.0, 1.0, 1.0),
                new(3.0, 2.0, 1.0)
            }
        };
    }
}
