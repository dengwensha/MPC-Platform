using MPC.Topology.Building;
using MPC.Topology.Primitives;

namespace MPC.Mapping;

public sealed class CanonicalTopologyAssemblyResult
{
    public Solid Solid { get; }

    public PublishedTopology Topology { get; }

    public CanonicalTopologyAssemblyResult(
        Solid solid,
        PublishedTopology topology)
    {
        Solid =
            solid
            ?? throw new ArgumentNullException(
                nameof(solid));

        Topology =
            topology
            ?? throw new ArgumentNullException(
                nameof(topology));
    }
}