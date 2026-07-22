using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;

namespace MPC.Connectors.Step.Mapping;

public sealed class StepCircleGeometryMapper
{
    private readonly StepSemanticEntityResolver _resolver;

    public StepCircleGeometryMapper(
        IEnumerable<StepSemanticEntity> semanticEntities)
    {
        ArgumentNullException.ThrowIfNull(
            semanticEntities);

        _resolver =
            new StepSemanticEntityResolver(
                semanticEntities);
    }

    public Circle3D Map(
        StepCircleEntity source)
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
                $"STEP CARTESIAN_POINT '{location.SourceEntityId}' must contain exactly three coordinates for Circle3D mapping.");
        }

        var center =
            new Point3D(
                location.Coordinates[0],
                location.Coordinates[1],
                location.Coordinates[2]);

        Vector3D normal;

        if (placement.AxisReferenceId is null)
        {
            normal =
                new Vector3D(
                    0.0,
                    0.0,
                    1.0);
        }
        else
        {
            var axis =
                _resolver.Resolve<StepDirectionEntity>(
                    placement.AxisReferenceId);

            if (axis.Ratios.Count != 3)
            {
                throw new InvalidOperationException(
                    $"STEP DIRECTION '{axis.SourceEntityId}' must contain exactly three direction ratios for Circle3D mapping.");
            }

            normal =
                new Vector3D(
                    axis.Ratios[0],
                    axis.Ratios[1],
                    axis.Ratios[2]);
        }

        return new Circle3D(
            center,
            normal,
            source.Radius);
    }
}