using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public class Ellipse3DTests
{
    [Fact]
    public void Ellipse3D_CanBeCreated()
    {
        // Arrange
        var center = new Point3D(0, 0, 0);
        var normal = new Vector3D(0, 0, 1);
        var majorRadius = 20.0;
        var minorRadius = 10.0;

        // Act
        var ellipse = new Ellipse3D(center, normal, majorRadius, minorRadius);

        // Assert
        Assert.Equal(center, ellipse.Center);
        Assert.Equal(normal, ellipse.Normal);
        Assert.Equal(majorRadius, ellipse.MajorRadius);
        Assert.Equal(minorRadius, ellipse.MinorRadius);
    }
}