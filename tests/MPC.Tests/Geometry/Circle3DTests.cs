using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public class Circle3DTests
{
    [Fact]
    public void Circle3D_CanBeCreated()
    {
        // Arrange
        var center = new Point3D(0, 0, 0);
        var normal = new Vector3D(0, 0, 1);
        var radius = 25.0;

        // Act
        var circle = new Circle3D(center, normal, radius);

        // Assert
        Assert.Equal(center, circle.Center);
        Assert.Equal(normal, circle.Normal);
        Assert.Equal(radius, circle.Radius);
    }
}