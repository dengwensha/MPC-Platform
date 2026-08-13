using MPC.Geometry.Contracts;

namespace MPC.Topology.Primitives;

public sealed class Face
{
    public Guid Id { get; }

    public ISurface3D Surface { get; }

    public LoopUse OuterLoopUse { get; }

    public IReadOnlyList<LoopUse> InnerLoopUses { get; }

    public Loop OuterLoop =>
        OuterLoopUse.Loop;

    public IReadOnlyList<Loop> InnerLoops { get; }

    public FaceOrientation Orientation { get; }

    public Face(
        ISurface3D surface,
        Loop outerLoop,
        IEnumerable<Loop>? innerLoops = null,
        FaceOrientation orientation =
            FaceOrientation.Forward)
    {
        ArgumentNullException.ThrowIfNull(
            surface);

        ArgumentNullException.ThrowIfNull(
            outerLoop);

        List<Loop> loops =
            innerLoops?.ToList()
            ?? [];

        if (loops.Any(
                loop => loop is null))
        {
            throw new ArgumentNullException(
                nameof(innerLoops));
        }

        Surface =
            surface;

        OuterLoopUse =
            new LoopUse(
                outerLoop,
                LoopUseDirection.Forward);

        InnerLoopUses =
            loops
                .Select(
                    loop =>
                        new LoopUse(
                            loop,
                            LoopUseDirection.Forward))
                .ToList()
                .AsReadOnly();

        InnerLoops =
            loops.AsReadOnly();

        Id =
            Guid.NewGuid();

        Orientation =
            orientation;
    }

    private Face(
        ISurface3D surface,
        LoopUse outerLoopUse,
        IEnumerable<LoopUse> innerLoopUses,
        FaceOrientation orientation)
    {
        ArgumentNullException.ThrowIfNull(
            surface);

        ArgumentNullException.ThrowIfNull(
            outerLoopUse);

        ArgumentNullException.ThrowIfNull(
            innerLoopUses);

        List<LoopUse> loopUses =
            innerLoopUses.ToList();

        if (loopUses.Any(
                loopUse => loopUse is null))
        {
            throw new ArgumentNullException(
                nameof(innerLoopUses));
        }

        Surface =
            surface;

        OuterLoopUse =
            outerLoopUse;

        InnerLoopUses =
            loopUses.AsReadOnly();

        InnerLoops =
            loopUses
                .Select(
                    loopUse =>
                        loopUse.Loop)
                .ToList()
                .AsReadOnly();

        Id =
            Guid.NewGuid();

        Orientation =
            orientation;
    }

    internal static Face CreateWithLoopUses(
        ISurface3D surface,
        LoopUse outerLoopUse,
        IEnumerable<LoopUse> innerLoopUses,
        FaceOrientation orientation)
    {
        return new Face(
            surface,
            outerLoopUse,
            innerLoopUses,
            orientation);
    }
}