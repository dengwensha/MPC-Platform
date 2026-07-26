using MPC.Geometry.Primitives;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepGeometryMathTests
{
    private const double Tolerance =
        1e-12;

    [Fact]
    public void Dot_WithOrthogonalVectors_ReturnsZero()
    {
        var first =
            new Vector3D(
                1.0,
                0.0,
                0.0);

        var second =
            new Vector3D(
                0.0,
                1.0,
                0.0);

        double result =
            StepGeometryMath.Dot(
                first,
                second);

        Assert.Equal(
            0.0,
            result,
            Tolerance);
    }

    [Fact]
    public void Dot_WithGeneralVectors_ReturnsExpectedValue()
    {
        var first =
            new Vector3D(
                1.0,
                2.0,
                3.0);

        var second =
            new Vector3D(
                4.0,
                -5.0,
                6.0);

        double result =
            StepGeometryMath.Dot(
                first,
                second);

        Assert.Equal(
            12.0,
            result,
            Tolerance);
    }

    [Fact]
    public void Cross_WithXAxisAndYAxis_ReturnsPositiveZAxis()
    {
        var xAxis =
            new Vector3D(
                1.0,
                0.0,
                0.0);

        var yAxis =
            new Vector3D(
                0.0,
                1.0,
                0.0);

        Vector3D result =
            StepGeometryMath.Cross(
                xAxis,
                yAxis);

        AssertVector(
            result,
            0.0,
            0.0,
            1.0);
    }

    [Fact]
    public void Cross_ReversingOperands_ReversesDirection()
    {
        var xAxis =
            new Vector3D(
                1.0,
                0.0,
                0.0);

        var yAxis =
            new Vector3D(
                0.0,
                1.0,
                0.0);

        Vector3D result =
            StepGeometryMath.Cross(
                yAxis,
                xAxis);

        AssertVector(
            result,
            0.0,
            0.0,
            -1.0);
    }

    [Fact]
    public void Normalize_WithNonUnitVector_ReturnsUnitVector()
    {
        var source =
            new Vector3D(
                3.0,
                4.0,
                0.0);

        Vector3D result =
            StepGeometryMath.Normalize(
                source);

        AssertVector(
            result,
            0.6,
            0.8,
            0.0);

        Assert.Equal(
            1.0,
            Length(
                result),
            Tolerance);
    }

    [Fact]
    public void Normalize_WithUnitVector_PreservesDirection()
    {
        var source =
            new Vector3D(
                0.0,
                0.0,
                -1.0);

        Vector3D result =
            StepGeometryMath.Normalize(
                source);

        AssertVector(
            result,
            0.0,
            0.0,
            -1.0);
    }

    [Fact]
    public void Normalize_WithZeroVector_ThrowsArgumentException()
    {
        var source =
            new Vector3D(
                0.0,
                0.0,
                0.0);

        ArgumentException exception =
            Assert.Throws<ArgumentException>(
                () =>
                    StepGeometryMath.Normalize(
                        source));

        Assert.Equal(
            "vector",
            exception.ParamName);

        Assert.Contains(
            "zero-length",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Normalize_WithNumericallyTooSmallVector_ThrowsArgumentException()
    {
        var source =
            new Vector3D(
                1e-20,
                0.0,
                0.0);

        Assert.Throws<ArgumentException>(
            () =>
                StepGeometryMath.Normalize(
                    source));
    }

    [Fact]
    public void Normalize_WithNonFiniteVector_ThrowsArgumentException()
    {
        var source =
            new Vector3D(
                double.PositiveInfinity,
                0.0,
                0.0);

        Assert.Throws<ArgumentException>(
            () =>
                StepGeometryMath.Normalize(
                    source));
    }

    [Fact]
    public void ProjectOntoPlane_RemovesNormalComponent()
    {
        var source =
            new Vector3D(
                2.0,
                3.0,
                4.0);

        var planeNormal =
            new Vector3D(
                0.0,
                0.0,
                1.0);

        Vector3D result =
            StepGeometryMath.ProjectOntoPlane(
                source,
                planeNormal);

        AssertVector(
            result,
            2.0,
            3.0,
            0.0);

        Assert.Equal(
            0.0,
            StepGeometryMath.Dot(
                result,
                planeNormal),
            Tolerance);
    }

    [Fact]
    public void ProjectOntoPlane_WithNonUnitNormal_ReturnsExpectedProjection()
    {
        var source =
            new Vector3D(
                2.0,
                3.0,
                4.0);

        var planeNormal =
            new Vector3D(
                0.0,
                0.0,
                5.0);

        Vector3D result =
            StepGeometryMath.ProjectOntoPlane(
                source,
                planeNormal);

        AssertVector(
            result,
            2.0,
            3.0,
            0.0);
    }

    [Fact]
    public void ProjectOntoPlane_WithZeroNormal_ThrowsArgumentException()
    {
        var source =
            new Vector3D(
                1.0,
                2.0,
                3.0);

        var planeNormal =
            new Vector3D(
                0.0,
                0.0,
                0.0);

        Assert.Throws<ArgumentException>(
            () =>
                StepGeometryMath.ProjectOntoPlane(
                    source,
                    planeNormal));
    }

    [Fact]
    public void ProjectOntoPlaneAndNormalize_ReturnsUnitVectorInPlane()
    {
        var source =
            new Vector3D(
                2.0,
                2.0,
                5.0);

        var planeNormal =
            new Vector3D(
                0.0,
                0.0,
                1.0);

        Vector3D result =
            StepGeometryMath
                .ProjectOntoPlaneAndNormalize(
                    source,
                    planeNormal);

        double expectedComponent =
            1.0 /
            Math.Sqrt(
                2.0);

        AssertVector(
            result,
            expectedComponent,
            expectedComponent,
            0.0);

        Assert.Equal(
            1.0,
            Length(
                result),
            Tolerance);

        Assert.Equal(
            0.0,
            StepGeometryMath.Dot(
                result,
                planeNormal),
            Tolerance);
    }

    [Fact]
    public void ProjectOntoPlaneAndNormalize_WithParallelVector_ThrowsArgumentException()
    {
        var source =
            new Vector3D(
                0.0,
                0.0,
                4.0);

        var planeNormal =
            new Vector3D(
                0.0,
                0.0,
                1.0);

        Assert.Throws<ArgumentException>(
            () =>
                StepGeometryMath
                    .ProjectOntoPlaneAndNormalize(
                        source,
                        planeNormal));
    }

    private static double Length(
        Vector3D vector)
    {
        return Math.Sqrt(
            vector.X * vector.X +
            vector.Y * vector.Y +
            vector.Z * vector.Z);
    }

    private static void AssertVector(
        Vector3D actual,
        double expectedX,
        double expectedY,
        double expectedZ)
    {
        Assert.Equal(
            expectedX,
            actual.X,
            Tolerance);

        Assert.Equal(
            expectedY,
            actual.Y,
            Tolerance);

        Assert.Equal(
            expectedZ,
            actual.Z,
            Tolerance);
    }
}