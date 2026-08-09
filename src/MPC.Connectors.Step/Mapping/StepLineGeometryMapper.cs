using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;

namespace MPC.Connectors.Step.Mapping;

public sealed class StepLineGeometryMapper
{
    private readonly StepSemanticEntityResolver _resolver;

    public StepLineGeometryMapper(
        StepSemanticEntityResolver resolver)
    {
        _resolver =
            resolver ??
            throw new ArgumentNullException(
                nameof(resolver));
    }

    public Line3D Map(
        StepLineEntity source)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        var point =
            _resolver.Resolve<StepCartesianPointEntity>(
                source.PointReferenceId);

        var vector =
            _resolver.Resolve<StepVectorEntity>(
                source.VectorReferenceId);

        var direction =
            _resolver.Resolve<StepDirectionEntity>(
                vector.OrientationReferenceId);

        if (point.Coordinates.Count != 3)
        {
            throw new InvalidOperationException(
                $"CARTESIAN_POINT '{point.SourceEntityId}' must contain exactly three coordinates.");
        }

        if (direction.Ratios.Count != 3)
        {
            throw new InvalidOperationException(
                $"DIRECTION '{direction.SourceEntityId}' must contain exactly three direction ratios.");
        }

        var origin =
            new Point3D(
                point.Coordinates[0],
                point.Coordinates[1],
                point.Coordinates[2]);

        var lineDirection =
            new Vector3D(
                direction.Ratios[0],
                direction.Ratios[1],
                direction.Ratios[2]);

        return new Line3D(
            origin,
            lineDirection);
    }
}