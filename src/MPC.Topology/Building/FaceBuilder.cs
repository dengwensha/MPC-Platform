using MPC.Geometry.Contracts;
using MPC.Topology.Primitives;

namespace MPC.Topology.Building;

public sealed class FaceBuilder
{
    private readonly ISurface3D _surface;
    private readonly List<LoopUse> _innerLoopUses;

    private LoopUse? _outerLoopUse;
    private FaceOrientation _orientation;
    private bool _isBuilt;

    public ISurface3D Surface =>
        _surface;

    public Loop? OuterLoop =>
        _outerLoopUse?.Loop;

    public LoopUse? OuterLoopUse =>
        _outerLoopUse;

    public IReadOnlyList<Loop> InnerLoops =>
        _innerLoopUses
            .Select(
                loopUse =>
                    loopUse.Loop)
            .ToList()
            .AsReadOnly();

    public IReadOnlyList<LoopUse> InnerLoopUses =>
        _innerLoopUses.AsReadOnly();

    public FaceOrientation Orientation =>
        _orientation;

    public bool IsBuilt =>
        _isBuilt;

    public FaceBuilder(
        ISurface3D surface)
    {
        _surface =
            surface
            ?? throw new ArgumentNullException(
                nameof(surface));

        _innerLoopUses = [];

        _orientation =
            FaceOrientation.Forward;
    }

    public FaceBuilder SetOuterLoop(
        Loop outerLoop)
    {
        return SetOuterLoop(
            outerLoop,
            LoopUseDirection.Forward);
    }

    public FaceBuilder SetOuterLoop(
        Loop outerLoop,
        LoopUseDirection direction)
    {
        EnsureNotBuilt();

        ArgumentNullException.ThrowIfNull(
            outerLoop);

        if (_outerLoopUse is not null)
        {
            throw new InvalidOperationException(
                "A face outer loop has already been assigned.");
        }

        if (ContainsInnerLoop(
                outerLoop))
        {
            throw new InvalidOperationException(
                "A face inner loop cannot also be assigned as its outer loop.");
        }

        _outerLoopUse =
            new LoopUse(
                outerLoop,
                direction);

        return this;
    }

    public FaceBuilder AddInnerLoop(
        Loop innerLoop)
    {
        return AddInnerLoop(
            innerLoop,
            LoopUseDirection.Forward);
    }

    public FaceBuilder AddInnerLoop(
        Loop innerLoop,
        LoopUseDirection direction)
    {
        EnsureNotBuilt();

        ArgumentNullException.ThrowIfNull(
            innerLoop);

        if (_outerLoopUse is not null &&
            ReferenceEquals(
                _outerLoopUse.Loop,
                innerLoop))
        {
            throw new InvalidOperationException(
                "A face outer loop cannot also be added as an inner loop.");
        }

        if (ContainsInnerLoop(
                innerLoop))
        {
            throw new InvalidOperationException(
                "The same inner loop cannot be added more than once.");
        }

        _innerLoopUses.Add(
            new LoopUse(
                innerLoop,
                direction));

        return this;
    }

    public FaceBuilder SetOrientation(
        FaceOrientation orientation)
    {
        EnsureNotBuilt();

        _orientation =
            orientation;

        return this;
    }

    public Face Build()
    {
        EnsureNotBuilt();

        if (_outerLoopUse is null)
        {
            throw new InvalidOperationException(
                "A face cannot be built without an outer loop.");
        }

        Face face =
            Face.CreateWithLoopUses(
                _surface,
                _outerLoopUse,
                _innerLoopUses,
                _orientation);

        _isBuilt =
            true;

        return face;
    }

    private bool ContainsInnerLoop(
        Loop loop)
    {
        return _innerLoopUses.Any(
            existingLoopUse =>
                ReferenceEquals(
                    existingLoopUse.Loop,
                    loop));
    }

    private void EnsureNotBuilt()
    {
        if (_isBuilt)
        {
            throw new InvalidOperationException(
                "FaceBuilder cannot be modified after the face has been built.");
        }
    }
}