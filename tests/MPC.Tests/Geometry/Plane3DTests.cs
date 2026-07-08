using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public class Plane3DTests
{
    [Fact]
    public void Plane3D_CanBeCreated()
    {
        // Arrange
        var origin = new Point3D(0, 0, 0);
        var normal = new Vector3D(0, 0, 1);

        // Act
        var plane = new Plane3D(origin, normal);

        // Assert
        Assert.Equal(origin, plane.Origin);
        Assert.Equal(normal, plane.Normal);
    }
}