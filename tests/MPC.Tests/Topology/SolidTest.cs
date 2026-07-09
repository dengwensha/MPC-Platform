using MPC.Topology;
using MPC.Topology.Primitives;
using System.Runtime.CompilerServices;

namespace MPC.Tests.Topology;

public sealed class SolidTest
{
    [Fact]
    public void Constructor_ShouldCreateSolid_WithOuterShell()
    {
        var outerShell = CreateShell();

        var solid = new Solid(outerShell);

        Assert.NotEqual(Guid.Empty, solid.Id);
        Assert.Same(outerShell, solid.OuterShell);
        Assert.Empty(solid.InnerShells);
    }

    [Fact]
    public void Constructor_ShouldStoreInnerShells_WhenProvided()
    {
        var outerShell = CreateShell();
        var innerShell1 = CreateShell();
        var innerShell2 = CreateShell();

        var solid = new Solid(
            outerShell,
            new[] { innerShell1, innerShell2 });

        Assert.Same(outerShell, solid.OuterShell);
        Assert.Equal(2, solid.InnerShells.Count);
        Assert.Same(innerShell1, solid.InnerShells[0]);
        Assert.Same(innerShell2, solid.InnerShells[1]);
    }

    private static Shell CreateShell()
    {
        return (Shell)RuntimeHelpers.GetUninitializedObject(typeof(Shell));
    }
}