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
        ArgumentNullException.ThrowIfNull(
            outerShell);

        List<Shell> shells =
            innerShells?.ToList() ??
            [];

        if (shells.Any(
                shell =>
                    shell is null))
        {
            throw new ArgumentException(
                "A solid cannot contain null inner shells.",
                nameof(innerShells));
        }

        OuterShell =
            outerShell;

        InnerShells =
            shells.AsReadOnly();

        Id =
            Guid.NewGuid();
    }
}