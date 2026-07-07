using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public class Point3DTests
{
    [Fact]
    public void Point3D_CanBeCreated()
    {
        // Arrange
        var x = 10.0;
        var y = 20.0;
        var z = 30.0;

        // Act
        var point = new Point3D(x, y, z);

        // Assert
        Assert.Equal(x, point.X);
        Assert.Equal(y, point.Y);
        Assert.Equal(z, point.Z);
    }
}