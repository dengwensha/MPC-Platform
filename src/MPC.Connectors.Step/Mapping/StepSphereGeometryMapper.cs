using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;

namespace MPC.Connectors.Step.Mapping;

public sealed class StepSphereGeometryMapper
{
    private readonly StepSemanticEntityResolver _resolver;

    public StepSphereGeometryMapper(
        StepSemanticEntityResolver resolver)
    {
        _resolver =
            resolver ??
            throw new ArgumentNullException(
                nameof(resolver));
    }

    public Sphere3D Map(
        StepSphericalSurfaceEntity source)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        var placement =
            _resolver.Resolve<StepAxis2Placement3DEntity>(
                source.PlacementReferenceId);

        var location =
            _resolver.Resolve<StepCartesianPointEntity>(
                placement.LocationReferenceId);

        if (location.Coordinates.Count != 3)
        {
            throw new InvalidOperationException(
                $"CARTESIAN_POINT '{location.SourceEntityId}' must contain exactly three coordinates.");
        }

        var center =
            new Point3D(
                location.Coordinates[0],
                location.Coordinates[1],
                location.Coordinates[2]);

        return new Sphere3D(
            center,
            source.Radius);
    }
}