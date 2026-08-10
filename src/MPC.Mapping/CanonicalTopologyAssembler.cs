using MPC.Topology;
using MPC.Topology.Building;
using MPC.Topology.Primitives;

namespace MPC.Mapping;

public sealed class CanonicalTopologyAssembler
{
    public CanonicalTopologyAssemblyResult Assemble(
        Solid solid)
    {
        ArgumentNullException.ThrowIfNull(
            solid);

        var builder =
            new TopologyBuilder();

        RegisterShell(
            solid.OuterShell,
            builder);

        foreach (Shell innerShell
                 in solid.InnerShells)
        {
            RegisterShell(
                innerShell,
                builder);
        }

        builder.Verify();

        PublishedTopology publishedTopology =
            builder.Publish();

        return new CanonicalTopologyAssemblyResult(
            solid,
            publishedTopology);
    }

    private static void RegisterShell(
        Shell shell,
        TopologyBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(
            shell);

        ArgumentNullException.ThrowIfNull(
            builder);

        foreach (Face face
                 in shell.Faces)
        {
            builder.RegisterFace(
                face);
        }
    }
}
