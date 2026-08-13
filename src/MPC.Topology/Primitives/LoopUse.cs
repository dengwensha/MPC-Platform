namespace MPC.Topology.Primitives;

public sealed class LoopUse
{
    public Loop Loop { get; }

    public LoopUseDirection Direction { get; }

    public LoopUse(
        Loop loop,
        LoopUseDirection direction =
            LoopUseDirection.Forward)
    {
        Loop =
            loop
            ?? throw new ArgumentNullException(
                nameof(loop));

        if (!Enum.IsDefined(
                direction))
        {
            throw new ArgumentOutOfRangeException(
                nameof(direction),
                direction,
                "Unsupported loop-use direction.");
        }

        Direction =
            direction;
    }
}