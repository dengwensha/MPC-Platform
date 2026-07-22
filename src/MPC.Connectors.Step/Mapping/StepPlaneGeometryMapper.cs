using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using System.Numerics;

namespace MPC.Connectors.Step.Mapping;

public sealed class StepPlaneGeometryMapper
{
    private readonly StepSemanticEntityResolver _resolver;

    public StepPlaneGeometryMapper(
        IEnumerable<StepSemanticEntity> semanticEntities)
    {
        _resolver =
            new StepSemanticEntityResolver(
                semanticEntities);
    }

    public Plane3D Map(
        StepPlaneEntity source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(
                nameof(source));
        }

        var placement =
            _resolver.Resolve<StepAxis2Placement3DEntity>(
                source.PlacementReferenceId);

        var location =
            _resolver.Resolve<StepCartesianPointEntity>(
                placement.LocationReferenceId);

        if (location.Coordinates.Count != 3)
        {
            throw new InvalidOperationException(
                $"STEP CARTESIAN_POINT '{location.SourceEntityId}' must contain exactly three coordinates for Plane3D mapping.");
        }

        var origin =
            new Point3D(
                location.Coordinates[0],
                location.Coordinates[1],
                location.Coordinates[2]);

        Vector3D normal;

        if (string.IsNullOrWhiteSpace(
                placement.AxisReferenceId))
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
                    $"STEP DIRECTION '{axis.SourceEntityId}' must contain exactly three direction ratios for Plane3D mapping.");
            }

            normal =
                new Vector3D(
                    axis.Ratios[0],
                    axis.Ratios[1],
                    axis.Ratios[2]);
        }

        return new Plane3D(
            origin,
            normal);
    }
}

