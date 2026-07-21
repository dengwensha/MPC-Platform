using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using System.Numerics;

namespace MPC.Connectors.Step.Mapping;

public sealed class StepPlaneGeometryMapper
{
    private readonly IReadOnlyDictionary<string, StepSemanticEntity> _entitiesById;

    public StepPlaneGeometryMapper(
        IEnumerable<StepSemanticEntity> semanticEntities)
    {
        if (semanticEntities is null)
        {
            throw new ArgumentNullException(
                nameof(semanticEntities));
        }

        var entitiesById =
            new Dictionary<string, StepSemanticEntity>(
                StringComparer.Ordinal);

        foreach (var entity in semanticEntities)
        {
            if (entity is null)
            {
                throw new ArgumentException(
                    "STEP semantic entity collection cannot contain null.",
                    nameof(semanticEntities));
            }

            if (string.IsNullOrWhiteSpace(
                    entity.SourceEntityId))
            {
                continue;
            }

            if (!entitiesById.TryAdd(
                    entity.SourceEntityId,
                    entity))
            {
                throw new InvalidOperationException(
                    $"Duplicate STEP semantic entity id '{entity.SourceEntityId}'.");
            }
        }

        _entitiesById =
            entitiesById;
    }

    public Plane3D Map(
        StepPlane source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(
                nameof(source));
        }

        var placement =
            Resolve<StepAxis2Placement3D>(
                source.PositionReferenceId);

        var location =
            Resolve<StepCartesianPoint>(
                placement.LocationReferenceId);

        var axis =
            Resolve<StepDirection>(
                placement.AxisReferenceId);

        if (location.Coordinates.Count != 3)
        {
            throw new InvalidOperationException(
                $"STEP CARTESIAN_POINT '{location.SourceEntityId}' must contain exactly three coordinates for Plane3D mapping.");
        }

        if (axis.DirectionRatios.Count != 3)
        {
            throw new InvalidOperationException(
                $"STEP DIRECTION '{axis.SourceEntityId}' must contain exactly three direction ratios for Plane3D mapping.");
        }

        var origin =
            new Point3D(
                location.Coordinates[0],
                location.Coordinates[1],
                location.Coordinates[2]);

        var normal =
            new Vector3D(
                axis.DirectionRatios[0],
                axis.DirectionRatios[1],
                axis.DirectionRatios[2]);

        return new Plane3D(
            origin,
            normal);
    }

    private TEntity Resolve<TEntity>(
        string referenceId)
        where TEntity : StepSemanticEntity
    {
        if (string.IsNullOrWhiteSpace(
                referenceId))
        {
            throw new InvalidOperationException(
                "STEP entity reference cannot be empty during geometry mapping.");
        }

        if (!_entitiesById.TryGetValue(
                referenceId,
                out var entity))
        {
            throw new InvalidOperationException(
                $"STEP semantic entity reference '{referenceId}' could not be resolved.");
        }

        if (entity is not TEntity typedEntity)
        {
            throw new InvalidOperationException(
                $"STEP semantic entity reference '{referenceId}' resolved to '{entity.SourceEntityType}', but '{typeof(TEntity).Name}' was required.");
        }

        return typedEntity;
    }
}