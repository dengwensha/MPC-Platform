using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public class Vector3DTests
{
    [Fact]
    public void Vector3D_CanBeCreated()
    {
        // Arrange
        var x = 1.0;
        var y = 2.0;
        var z = 3.0;

        // Act
        var vector = new Vector3D(x, y, z);

        // Assert
        Assert.Equal(x, vector.X);
        Assert.Equal(y, vector.Y);
        Assert.Equal(z, vector.Z);
    }
}