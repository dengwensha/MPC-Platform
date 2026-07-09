using MPC.Geometry.Contracts;

namespace MPC.Topology.Primitives;

public sealed class Face
{
    public Guid Id { get; }

    public ISurface3D Surface { get; }

    public Loop OuterLoop { get; }

    public IReadOnlyList<Loop> InnerLoops { get; }

    public FaceOrientation Orientation { get; }

    public Face(
        ISurface3D surface,
        Loop outerLoop,
        IEnumerable<Loop>? innerLoops = null,
        FaceOrientation orientation = FaceOrientation.Forward)
    {
        Surface = surface ?? throw new ArgumentNullException(nameof(surface));
        OuterLoop = outerLoop ?? throw new ArgumentNullException(nameof(outerLoop));

        var loops = innerLoops?.ToList() ?? new List<Loop>();

        if (loops.Any(loop => loop is null))
        {
            throw new ArgumentNullException(nameof(innerLoops));
        }

        InnerLoops = loops.AsReadOnly();

        Id = Guid.NewGuid();
        Orientation = orientation;
    }
}