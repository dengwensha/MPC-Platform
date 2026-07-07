using MPC.Geometry.Primitives;

namespace MPC.Tests.Geometry;

public class Line3DTests
{
    [Fact]
    public void Line3D_CanBeCreated()
    {
        // Arrange
        var start = new Point3D(0, 0, 0);
        var end = new Point3D(10, 20, 30);

        // Act
        var line = new Line3D(start, end);

        // Assert
        Assert.Equal(start, line.Start);
        Assert.Equal(end, line.End);
    }
}