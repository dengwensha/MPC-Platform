using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public class CoordinateSystem3DTests
{
    [Fact]
    public void CoordinateSystem3D_CanBeCreated()
    {
        // Arrange
        var origin = new Point3D(0, 0, 0);
        var xAxis = new Vector3D(1, 0, 0);
        var yAxis = new Vector3D(0, 1, 0);
        var zAxis = new Vector3D(0, 0, 1);

        // Act
        var coordinateSystem = new CoordinateSystem3D(
            origin,
            xAxis,
            yAxis,
            zAxis);

        // Assert
        Assert.Equal(origin, coordinateSystem.Origin);
        Assert.Equal(xAxis, coordinateSystem.XAxis);
        Assert.Equal(yAxis, coordinateSystem.YAxis);
        Assert.Equal(zAxis, coordinateSystem.ZAxis);
    }
}