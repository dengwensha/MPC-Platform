using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public class Arc3DTests
{
    [Fact]
    public void Arc3D_CanBeCreated()
    {
        // Arrange
        var center = new Point3D(0, 0, 0);
        var normal = new Vector3D(0, 0, 1);
        var radius = 25.0;
        var startAngle = 0.0;
        var endAngle = 90.0;

        // Act
        var arc = new Arc3D(center, normal, radius, startAngle, endAngle);

        // Assert
        Assert.Equal(center, arc.Center);
        Assert.Equal(normal, arc.Normal);
        Assert.Equal(radius, arc.Radius);
        Assert.Equal(startAngle, arc.StartAngle);
        Assert.Equal(endAngle, arc.EndAngle);
    }
}