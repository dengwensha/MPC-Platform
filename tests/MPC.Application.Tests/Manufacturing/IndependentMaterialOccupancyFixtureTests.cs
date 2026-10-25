namespace MPC.Application.Tests.Manufacturing;

// GE03B-04B-03: synthetic mathematical occupancy oracle only.
// Does not read STEP, use canonical normals, or certify R01/R02 material sides.
public sealed class IndependentMaterialOccupancyFixtureTests
{
    private enum Occupancy { Material, Void, Boundary }
    private enum Side { Outward, Inward, Unverified }
    private const double Epsilon = 1e-9;
    private const double Offset = 0.1;

    // Exact independent reference: [-5,5] x [-5,5] x [0,10],
    // with the top-open rectangular pocket (-2,2) x (-2,2) x [6,10].
    // A boundary is never silently counted as material or void.
    private static Occupancy Occupies(double x, double y, double z)
    {
        if (Math.Abs(x) > 5 + Epsilon || Math.Abs(y) > 5 + Epsilon ||
            z < -Epsilon || z > 10 + Epsilon) return Occupancy.Void;
        if (Math.Abs(Math.Abs(x) - 5) <= Epsilon ||
            Math.Abs(Math.Abs(y) - 5) <= Epsilon ||
            Math.Abs(z) <= Epsilon) return Occupancy.Boundary;

        bool pocketFootprint = Math.Abs(x) < 2 - Epsilon &&
                               Math.Abs(y) < 2 - Epsilon;
        bool nearPocketWall = Math.Abs(x) <= 2 + Epsilon &&
                              Math.Abs(y) <= 2 + Epsilon &&
                              (Math.Abs(Math.Abs(x) - 2) <= Epsilon ||
                               Math.Abs(Math.Abs(y) - 2) <= Epsilon);
        if (z >= 6 - Epsilon && nearPocketWall) return Occupancy.Boundary;
        if (pocketFootprint && Math.Abs(z - 6) <= Epsilon) return Occupancy.Boundary;
        if (Math.Abs(z - 10) <= Epsilon && !pocketFootprint) return Occupancy.Boundary;
        if (pocketFootprint && z > 6 + Epsilon) return Occupancy.Void;
        return Occupancy.Material;
    }

    private static Side Evaluate(double x, double y, double z,
        double nx, double ny, double nz, double boundaryClearance)
    {
        // Fixture-supplied sample clearance. Reject seams, edges, and
        // unknown or non-unit normals; never extrapolate a whole face.
        double lengthSquared = nx * nx + ny * ny + nz * nz;
        if (boundaryClearance <= Offset || !double.IsFinite(lengthSquared) ||
            Math.Abs(lengthSquared - 1) > Epsilon)
            return Side.Unverified;
        var plus = Occupies(x + Offset * nx, y + Offset * ny, z + Offset * nz);
        var minus = Occupies(x - Offset * nx, y - Offset * ny, z - Offset * nz);
        if (plus == Occupancy.Void && minus == Occupancy.Material) return Side.Outward;
        if (plus == Occupancy.Material && minus == Occupancy.Void) return Side.Inward;
        return Side.Unverified;
    }

    [Theory]
    [InlineData(5, 0, 5, 1, 0, 0)]   // outside wall
    [InlineData(4, 0, 10, 0, 0, 1)]  // top, outside pocket
    [InlineData(0, 0, 6, 0, 0, 1)]   // pocket floor
    [InlineData(2, 0, 8, -1, 0, 0)]  // pocket inner wall: void toward -X
    public void KnownMaterialSide_AcceptsOutwardAndRejectsInverted(
        double x, double y, double z, double nx, double ny, double nz)
    {
        Assert.Equal(Side.Outward, Evaluate(x,y,z,nx,ny,nz,1));
        Assert.Equal(Side.Inward, Evaluate(x,y,z,-nx,-ny,-nz,1));
    }

    [Fact]
    public void PocketVoidAndSolid_AreDistinguishedWithoutFaceWinding()
    {
        Assert.Equal(Occupancy.Void, Occupies(0,0,8));
        Assert.Equal(Occupancy.Material, Occupies(0,0,4));
        Assert.Equal(Occupancy.Material, Occupies(3,0,8));
        Assert.Equal(Occupancy.Boundary, Occupies(2,0,8));
    }

    [Fact]
    public void EdgeAmbiguityAndUnsupportedInputs_FailClosed()
    {
        Assert.Equal(Side.Unverified, Evaluate(2,2,10,0,0,1,0));
        Assert.Equal(Side.Unverified, Evaluate(0,0,8,0,0,1,1));
        Assert.Equal(Side.Unverified, Evaluate(0,0,6,0,0,0,1));
        Assert.Equal(Side.Unverified, Evaluate(0,0,6,double.NaN,0,1,1));
    }
}
