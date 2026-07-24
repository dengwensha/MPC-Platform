using MPC.Topology.Primitives;

namespace MPC.Mapping.Step;

public sealed class FaceLoopMapping
{
    public Loop Loop { get; }

    public bool Orientation { get; }

    public bool IsOuter { get; }

    public FaceLoopMapping(
        Loop loop,
        bool orientation,
        bool isOuter)
    {
        ArgumentNullException.ThrowIfNull(
            loop);

        Loop =
            loop;

        Orientation =
            orientation;

        IsOuter =
            isOuter;
    }
}