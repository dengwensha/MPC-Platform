using MPC.Connectors.Snapshots.Geometry;

namespace MPC.Tests.Connectors;

public sealed class CadGeometryValueTests
{
    [Fact]
    public void CadPoint3_StoresSourceValuesWithoutModification()
    {
        var point = new CadPoint3(
            X: 1.25,
            Y: -2.5,
            Z: 0.0);

        Assert.Equal(1.25, point.X);
        Assert.Equal(-2.5, point.Y);
        Assert.Equal(0.0, point.Z);
    }

    [Fact]
    public void CadVector3_AllowsNonNormalizedDirection()
    {
        var vector = new CadVector3(
            X: 10.0,
            Y: 0.0,
            Z: 0.0);

        Assert.Equal(10.0, vector.X);
        Assert.Equal(0.0, vector.Y);
        Assert.Equal(0.0, vector.Z);
    }

    [Fact]
    public void CadVector3_AllowsZeroVector()
    {
        var vector = new CadVector3(
            X: 0.0,
            Y: 0.0,
            Z: 0.0);

        Assert.Equal(
            new CadVector3(0.0, 0.0, 0.0),
            vector);
    }

    [Fact]
    public void CadAxis3_StoresOriginAndDirection()
    {
        var origin = new CadPoint3(
            X: 1.0,
            Y: 2.0,
            Z: 3.0);

        var direction = new CadVector3(
            X: 0.0,
            Y: 0.0,
            Z: 5.0);

        var axis = new CadAxis3(
            origin,
            direction);

        Assert.Equal(origin, axis.Origin);
        Assert.Equal(direction, axis.Direction);
    }
}
