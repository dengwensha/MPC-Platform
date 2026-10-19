using MPC.Application.Manufacturing;

namespace MPC.Application.Tests.Manufacturing;

public sealed class LocalConcavityConvexityQueryTests
{
    [Theory]
    [InlineData(90, LocalDihedralShape.Convex)]
    [InlineData(-90, LocalDihedralShape.Concave)]
    [InlineData(0, LocalDihedralShape.Smooth)]
    [InlineData(0.1, LocalDihedralShape.Smooth)]
    [InlineData(180, LocalDihedralShape.Indeterminate)]
    [InlineData(-180, LocalDihedralShape.Indeterminate)]
    [InlineData(double.NaN, LocalDihedralShape.Indeterminate)]
    public void VerifiedOutwardAngle_ClassifiesConservatively(
        double angle, LocalDihedralShape expected)
    {
        Assert.Equal(expected,
            LocalConcavityConvexityQuery.ClassifyVerifiedOutwardAngle(angle,0.5));
    }

    [Fact]
    public void InvalidTolerance_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            LocalConcavityConvexityQuery.ClassifyVerifiedOutwardAngle(90,90));
    }
}
