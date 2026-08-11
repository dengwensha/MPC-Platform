using MPC.Topology;
using MPC.Topology.Primitives;
using System.Runtime.CompilerServices;

namespace MPC.Tests.Topology;

public sealed class SolidTest
{
    [Fact]
    public void Constructor_ShouldCreateSolid_WithOuterShell()
    {
        var outerShell =
            CreateShell();

        var solid =
            new Solid(
                outerShell);

        Assert.NotEqual(
            Guid.Empty,
            solid.Id);

        Assert.Same(
            outerShell,
            solid.OuterShell);

        Assert.Empty(
            solid.InnerShells);
    }

    [Fact]
    public void Constructor_ShouldStoreInnerShells_WhenProvided()
    {
        var outerShell =
            CreateShell();

        var innerShell1 =
            CreateShell();

        var innerShell2 =
            CreateShell();

        var solid =
            new Solid(
                outerShell,
                new[]
                {
                    innerShell1,
                    innerShell2
                });

        Assert.Same(
            outerShell,
            solid.OuterShell);

        Assert.Equal(
            2,
            solid.InnerShells.Count);

        Assert.Same(
            innerShell1,
            solid.InnerShells[0]);

        Assert.Same(
            innerShell2,
            solid.InnerShells[1]);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOuterShellIsNull()
    {
        var exception =
            Assert.Throws<ArgumentNullException>(
                () =>
                    new Solid(
                        null!));

        Assert.Equal(
            "outerShell",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenInnerShellsContainNull()
    {
        var outerShell =
            CreateShell();

        Shell?[] innerShells =
        [
            CreateShell(),
            null
        ];

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new Solid(
                        outerShell,
                        innerShells!));

        Assert.Equal(
            "innerShells",
            exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldSnapshotInnerShells()
    {
        var outerShell =
            CreateShell();

        var innerShell =
            CreateShell();

        var source =
            new List<Shell>
            {
                innerShell
            };

        var solid =
            new Solid(
                outerShell,
                source);

        source.Clear();

        Assert.Single(
            solid.InnerShells);

        Assert.Same(
            innerShell,
            solid.InnerShells[0]);
    }

    [Fact]
    public void InnerShells_ShouldRejectExternalMutation()
    {
        var solid =
            new Solid(
                CreateShell(),
                new[]
                {
                    CreateShell()
                });

        var exposedCollection =
            Assert.IsAssignableFrom<IList<Shell>>(
                solid.InnerShells);

        Assert.Throws<NotSupportedException>(
            () =>
                exposedCollection.Clear());

        Assert.Single(
            solid.InnerShells);
    }

    private static Shell CreateShell()
    {
        return
            (Shell)RuntimeHelpers.GetUninitializedObject(
                typeof(Shell));
    }
}