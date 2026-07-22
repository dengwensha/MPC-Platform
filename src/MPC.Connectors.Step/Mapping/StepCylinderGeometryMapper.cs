using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;

namespace MPC.Connectors.Step.Mapping;

public sealed class StepCylinderGeometryMapper
{
    private readonly StepSemanticEntityResolver _resolver;

    public StepCylinderGeometryMapper(
        IEnumerable<StepSemanticEntity> semanticEntities)
    {
        ArgumentNullException.ThrowIfNull(
            semanticEntities);

        _resolver =
            new StepSemanticEntityResolver(
                semanticEntities);
    }

    public Cylinder3D Map(
        StepCylindricalSurfaceEntity source)
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
                $"STEP CARTESIAN_POINT '{location.SourceEntityId}' must contain exactly three coordinates for Cylinder3D mapping.");
        }

        var axisOrigin =
            new Point3D(
                location.Coordinates[0],
                location.Coordinates[1],
                location.Coordinates[2]);

        Vector3D axisDirection;

        if (string.IsNullOrWhiteSpace(
                placement.AxisReferenceId))
        {
            axisDirection =
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
                    $"STEP DIRECTION '{axis.SourceEntityId}' must contain exactly three direction ratios for Cylinder3D mapping.");
            }

            axisDirection =
                new Vector3D(
                    axis.Ratios[0],
                    axis.Ratios[1],
                    axis.Ratios[2]);
        }

        return new Cylinder3D(
            axisOrigin,
            axisDirection,
            source.Radius);
    }
}