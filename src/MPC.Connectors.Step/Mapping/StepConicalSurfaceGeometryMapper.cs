using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;

namespace MPC.Connectors.Step.Mapping;

public sealed class StepConicalSurfaceGeometryMapper
{
    private readonly StepSemanticEntityResolver _resolver;

    public StepConicalSurfaceGeometryMapper(
        StepSemanticEntityResolver resolver)
    {
        _resolver =
            resolver ??
            throw new ArgumentNullException(
                nameof(resolver));
    }

    public Cone3D Map(
        StepConicalSurfaceEntity source)
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

        var placementOrigin =
            new Point3D(
                location.Coordinates[0],
                location.Coordinates[1],
                location.Coordinates[2]);

        var axisDirection =
            ResolveAxisDirection(
                placement);

        var canonicalSemiAngle =
            source.SemiAngle;

        if (canonicalSemiAngle < 0.0)
        {
            canonicalSemiAngle =
                -canonicalSemiAngle;

            axisDirection =
                new Vector3D(
                    -axisDirection.X,
                    -axisDirection.Y,
                    -axisDirection.Z);
        }

        if (canonicalSemiAngle <= 0.0 ||
            canonicalSemiAngle >= Math.PI / 2.0)
        {
            throw new InvalidOperationException(
                $"CONICAL_SURFACE '{source.SourceEntityId}' semi-angle must be greater than zero and less than PI / 2.");
        }

        var axialDistance =
            source.Radius /
            Math.Tan(
                canonicalSemiAngle);

        var apex =
            new Point3D(
                placementOrigin.X -
                    axisDirection.X * axialDistance,
                placementOrigin.Y -
                    axisDirection.Y * axialDistance,
                placementOrigin.Z -
                    axisDirection.Z * axialDistance);

        return new Cone3D(
            apex,
            axisDirection,
            canonicalSemiAngle);
    }

    private Vector3D ResolveAxisDirection(
        StepAxis2Placement3DEntity placement)
    {
        if (placement.AxisReferenceId is null)
        {
            return new Vector3D(
                0.0,
                0.0,
                1.0);
        }

        var axis =
            _resolver.Resolve<StepDirectionEntity>(
                placement.AxisReferenceId);

        if (axis.Ratios.Count != 3)
        {
            throw new InvalidOperationException(
                $"DIRECTION '{axis.SourceEntityId}' must contain exactly three direction ratios.");
        }

        return new Vector3D(
            axis.Ratios[0],
            axis.Ratios[1],
            axis.Ratios[2]);
    }
}