using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public class BoundingBox3DTests
{
    [Fact]
    public void BoundingBox3D_CanBeCreated()
    {
        // Arrange
        var min = new Point3D(0, 0, 0);
        var max = new Point3D(100, 50, 25);

        // Act
        var box = new BoundingBox3D(min, max);

        // Assert
        Assert.Equal(min, box.Min);
        Assert.Equal(max, box.Max);
    }
}