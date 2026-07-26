using MPC.Geometry.Primitives;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class CoordinateSystem3DFactoryTests
{
    private const double Tolerance =
        1e-12;

    [Fact]
    public void Create_WithDefaultZAxisAndNoReferenceDirection_ReturnsValidCoordinateSystem()
    {
        var origin =
            new Point3D(
                1.0,
                2.0,
                3.0);

        var zAxis =
            new Vector3D(
                0.0,
                0.0,
                1.0);

        CoordinateSystem3D result =
            CoordinateSystem3DFactory.Create(
                origin,
                zAxis,
                null);

        Assert.Equal(
            origin,
            result.Origin);

        AssertCoordinateSystem(
            result);

        AssertVector(
            result.ZAxis,
            0.0,
            0.0,
            1.0);
    }

    [Fact]
    public void Create_WithReferenceDirection_UsesProjectedDirectionAsXAxis()
    {
        var origin =
            new Point3D(
                0.0,
                0.0,
                0.0);

        var zAxis =
            new Vector3D(
                0.0,
                0.0,
                1.0);

        var referenceDirection =
            new Vector3D(
                2.0,
                0.0,
                5.0);

        CoordinateSystem3D result =
            CoordinateSystem3DFactory.Create(
                origin,
                zAxis,
                referenceDirection);

        AssertVector(
            result.XAxis,
            1.0,
            0.0,
            0.0);

        AssertVector(
            result.YAxis,
            0.0,
            1.0,
            0.0);

        AssertVector(
            result.ZAxis,
            0.0,
            0.0,
            1.0);

        AssertCoordinateSystem(
            result);
    }

    [Fact]
    public void Create_WithNonUnitZAxis_NormalizesZAxis()
    {
        var origin =
            new Point3D(
                0.0,
                0.0,
                0.0);

        var zAxis =
            new Vector3D(
                0.0,
                0.0,
                10.0);

        CoordinateSystem3D result =
            CoordinateSystem3DFactory.Create(
                origin,
                zAxis,
                null);

        AssertVector(
            result.ZAxis,
            0.0,
            0.0,
            1.0);

        AssertCoordinateSystem(
            result);
    }

    [Fact]
    public void Create_WithTiltedZAxis_ReturnsOrthogonalRightHandedCoordinateSystem()
    {
        var origin =
            new Point3D(
                0.0,
                0.0,
                0.0);

        var zAxis =
            new Vector3D(
                1.0,
                1.0,
                1.0);

        var referenceDirection =
            new Vector3D(
                1.0,
                0.0,
                0.0);

        CoordinateSystem3D result =
            CoordinateSystem3DFactory.Create(
                origin,
                zAxis,
                referenceDirection);

        AssertCoordinateSystem(
            result);

        Vector3D cross =
            StepGeometryMath.Cross(
                result.XAxis,
                result.YAxis);

        AssertVectorEqual(
            cross,
            result.ZAxis);
    }

    [Fact]
    public void Create_WithoutReferenceDirection_IsDeterministic()
    {
        var origin =
            new Point3D(
                5.0,
                6.0,
                7.0);

        var zAxis =
            new Vector3D(
                1.0,
                2.0,
                3.0);

        CoordinateSystem3D first =
            CoordinateSystem3DFactory.Create(
                origin,
                zAxis,
                null);

        CoordinateSystem3D second =
            CoordinateSystem3DFactory.Create(
                origin,
                zAxis,
                null);

        AssertVectorEqual(
            first.XAxis,
            second.XAxis);

        AssertVectorEqual(
            first.YAxis,
            second.YAxis);

        AssertVectorEqual(
            first.ZAxis,
            second.ZAxis);
    }

    [Fact]
    public void Create_WithXAxisAsZAxis_SelectsValidDeterministicPerpendicularXAxis()
    {
        var origin =
            new Point3D(
                0.0,
                0.0,
                0.0);

        var zAxis =
            new Vector3D(
                1.0,
                0.0,
                0.0);

        CoordinateSystem3D result =
            CoordinateSystem3DFactory.Create(
                origin,
                zAxis,
                null);

        AssertCoordinateSystem(
            result);

        AssertVector(
            result.ZAxis,
            1.0,
            0.0,
            0.0);

        Assert.Equal(
            0.0,
            StepGeometryMath.Dot(
                result.XAxis,
                result.ZAxis),
            Tolerance);
    }

    [Fact]
    public void Create_WithYAxisAsZAxis_SelectsValidDeterministicPerpendicularXAxis()
    {
        var origin =
            new Point3D(
                0.0,
                0.0,
                0.0);

        var zAxis =
            new Vector3D(
                0.0,
                1.0,
                0.0);

        CoordinateSystem3D result =
            CoordinateSystem3DFactory.Create(
                origin,
                zAxis,
                null);

        AssertCoordinateSystem(
            result);

        AssertVector(
            result.ZAxis,
            0.0,
            1.0,
            0.0);
    }

    [Fact]
    public void Create_WithNegativeZAxis_PreservesDirection()
    {
        var origin =
            new Point3D(
                0.0,
                0.0,
                0.0);

        var zAxis =
            new Vector3D(
                0.0,
                0.0,
                -4.0);

        CoordinateSystem3D result =
            CoordinateSystem3DFactory.Create(
                origin,
                zAxis,
                null);

        AssertVector(
            result.ZAxis,
            0.0,
            0.0,
            -1.0);

        AssertCoordinateSystem(
            result);
    }

    [Fact]
    public void Create_WithZeroZAxis_ThrowsArgumentException()
    {
        var origin =
            new Point3D(
                0.0,
                0.0,
                0.0);

        var zAxis =
            new Vector3D(
                0.0,
                0.0,
                0.0);

        Assert.Throws<ArgumentException>(
            () =>
                CoordinateSystem3DFactory.Create(
                    origin,
                    zAxis,
                    null));
    }

    [Fact]
    public void Create_WithReferenceDirectionParallelToZAxis_ThrowsArgumentException()
    {
        var origin =
            new Point3D(
                0.0,
                0.0,
                0.0);

        var zAxis =
            new Vector3D(
                0.0,
                0.0,
                1.0);

        var referenceDirection =
            new Vector3D(
                0.0,
                0.0,
                5.0);

        Assert.Throws<ArgumentException>(
            () =>
                CoordinateSystem3DFactory.Create(
                    origin,
                    zAxis,
                    referenceDirection));
    }

    [Fact]
    public void Create_WithZeroReferenceDirection_ThrowsArgumentException()
    {
        var origin =
            new Point3D(
                0.0,
                0.0,
                0.0);

        var zAxis =
            new Vector3D(
                0.0,
                0.0,
                1.0);

        var referenceDirection =
            new Vector3D(
                0.0,
                0.0,
                0.0);

        Assert.Throws<ArgumentException>(
            () =>
                CoordinateSystem3DFactory.Create(
                    origin,
                    zAxis,
                    referenceDirection));
    }

    private static void AssertCoordinateSystem(
        CoordinateSystem3D coordinateSystem)
    {
        Assert.Equal(
            1.0,
            Length(
                coordinateSystem.XAxis),
            Tolerance);

        Assert.Equal(
            1.0,
            Length(
                coordinateSystem.YAxis),
            Tolerance);

        Assert.Equal(
            1.0,
            Length(
                coordinateSystem.ZAxis),
            Tolerance);

        Assert.Equal(
            0.0,
            StepGeometryMath.Dot(
                coordinateSystem.XAxis,
                coordinateSystem.YAxis),
            Tolerance);

        Assert.Equal(
            0.0,
            StepGeometryMath.Dot(
                coordinateSystem.XAxis,
                coordinateSystem.ZAxis),
            Tolerance);

        Assert.Equal(
            0.0,
            StepGeometryMath.Dot(
                coordinateSystem.YAxis,
                coordinateSystem.ZAxis),
            Tolerance);

        Vector3D expectedZAxis =
            StepGeometryMath.Normalize(
                StepGeometryMath.Cross(
                    coordinateSystem.XAxis,
                    coordinateSystem.YAxis));

        AssertVectorEqual(
            expectedZAxis,
            coordinateSystem.ZAxis);
    }

    private static double Length(
        Vector3D vector)
    {
        return Math.Sqrt(
            vector.X * vector.X +
            vector.Y * vector.Y +
            vector.Z * vector.Z);
    }

    private static void AssertVectorEqual(
        Vector3D actual,
        Vector3D expected)
    {
        AssertVector(
            actual,
            expected.X,
            expected.Y,
            expected.Z);
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