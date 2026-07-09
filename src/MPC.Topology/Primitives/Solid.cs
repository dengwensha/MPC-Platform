namespace MPC.Topology.Primitives;

public sealed class Solid
{
    public Guid Id { get; }

    public Shell OuterShell { get; }

    public IReadOnlyList<Shell> InnerShells { get; }

    public Solid(
        Shell outerShell,
        IEnumerable<Shell>? innerShells = null)
    {
        OuterShell = outerShell;
        InnerShells = innerShells?.ToList() ?? [];
        Id = Guid.NewGuid();
    }
}