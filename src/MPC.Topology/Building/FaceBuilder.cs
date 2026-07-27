using MPC.Geometry.Contracts;
using MPC.Topology.Primitives;

namespace MPC.Topology.Building;

public sealed class FaceBuilder
{
    private readonly ISurface3D _surface;
    private readonly List<Loop> _innerLoops;

    private Loop? _outerLoop;
    private FaceOrientation _orientation;
    private bool _isBuilt;

    public ISurface3D Surface =>
        _surface;

    public Loop? OuterLoop =>
        _outerLoop;

    public IReadOnlyList<Loop> InnerLoops =>
        _innerLoops.AsReadOnly();

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

        _innerLoops = [];

        _orientation =
            FaceOrientation.Forward;
    }

    public FaceBuilder SetOuterLoop(
        Loop outerLoop)
    {
        EnsureNotBuilt();

        ArgumentNullException.ThrowIfNull(
            outerLoop);

        if (_outerLoop is not null)
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

        _outerLoop =
            outerLoop;

        return this;
    }

    public FaceBuilder AddInnerLoop(
        Loop innerLoop)
    {
        EnsureNotBuilt();

        ArgumentNullException.ThrowIfNull(
            innerLoop);

        if (ReferenceEquals(
                _outerLoop,
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

        _innerLoops.Add(
            innerLoop);

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

        if (_outerLoop is null)
        {
            throw new InvalidOperationException(
                "A face cannot be built without an outer loop.");
        }

        Face face =
            new(
                _surface,
                _outerLoop,
                _innerLoops,
                _orientation);

        _isBuilt =
            true;

        return face;
    }

    private bool ContainsInnerLoop(
        Loop loop)
    {
        return _innerLoops.Any(
            existingLoop =>
                ReferenceEquals(
                    existingLoop,
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