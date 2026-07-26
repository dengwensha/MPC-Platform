using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using MPC.Mapping;

namespace MPC.Mapping.Step;

public sealed class StepAxis2Placement3DMapper :
    IEntityMapper<
        StepAxis2Placement3DEntity,
        CoordinateSystem3D>
{
    private const string InvalidPlacementDiagnosticCode =
        "MAP-STEP-AXIS2-PLACEMENT-3D-001";

    private static readonly Vector3D DefaultZAxis =
        new(
            0.0,
            0.0,
            1.0);

    public MappingResult<CoordinateSystem3D> Map(
        StepAxis2Placement3DEntity source,
        MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);

        ReferenceResolutionResult<StepCartesianPoint>
            locationResolution =
                context.ReferenceResolver
                    .Resolve<StepCartesianPoint>(
                        source.LocationReferenceId);

        if (!locationResolution.IsResolved)
        {
            context.AddDiagnostics(
                locationResolution.Diagnostics);

            return MappingResult<
                CoordinateSystem3D>.Failure(
                    locationResolution.Diagnostics);
        }

        try
        {
            Point3D origin =
                StepGeometryConversions.ToPoint3D(
                    GetResolvedValue(
                        locationResolution,
                        "CARTESIAN_POINT"));

            Vector3D zAxis =
                ResolveAxis(
                    source,
                    context);

            Vector3D? referenceDirection =
                ResolveReferenceDirection(
                    source,
                    context);

            if (context.HasErrors)
            {
                return MappingResult<
                    CoordinateSystem3D>.Failure(
                        context.Diagnostics);
            }

            CoordinateSystem3D coordinateSystem =
                CoordinateSystem3DFactory.Create(
                    origin,
                    zAxis,
                    referenceDirection);

            if (context.HasDiagnostics)
            {
                return MappingResult<
                    CoordinateSystem3D>
                    .SuccessWithDiagnostics(
                        coordinateSystem,
                        context.Diagnostics);
            }

            return MappingResult<
                CoordinateSystem3D>.Success(
                    coordinateSystem);
        }
        catch (ArgumentException exception)
        {
            MappingDiagnostic diagnostic =
                CreateInvalidPlacementDiagnostic(
                    source,
                    exception.Message);

            context.AddDiagnostic(
                diagnostic);

            return MappingResult<
                CoordinateSystem3D>.Failure(
                    new[]
                    {
                        diagnostic
                    });
        }
    }

    private static Vector3D ResolveAxis(
        StepAxis2Placement3DEntity source,
        MappingContext context)
    {
        if (source.AxisReferenceId is null)
        {
            return DefaultZAxis;
        }

        ReferenceResolutionResult<StepDirection>
            resolution =
                context.ReferenceResolver
                    .Resolve<StepDirection>(
                        source.AxisReferenceId);

        if (!resolution.IsResolved)
        {
            context.AddDiagnostics(
                resolution.Diagnostics);

            return DefaultZAxis;
        }

        return StepGeometryConversions
            .ToUnitVector3D(
                GetResolvedValue(
                    resolution,
                    "axis DIRECTION"));
    }

    private static Vector3D? ResolveReferenceDirection(
        StepAxis2Placement3DEntity source,
        MappingContext context)
    {
        if (source.RefDirectionReferenceId is null)
        {
            return null;
        }

        ReferenceResolutionResult<StepDirection>
            resolution =
                context.ReferenceResolver
                    .Resolve<StepDirection>(
                        source.RefDirectionReferenceId);

        if (!resolution.IsResolved)
        {
            context.AddDiagnostics(
                resolution.Diagnostics);

            return null;
        }

        return StepGeometryConversions
            .ToUnitVector3D(
                GetResolvedValue(
                    resolution,
                    "reference DIRECTION"));
    }

    private static T GetResolvedValue<T>(
        ReferenceResolutionResult<T> resolution,
        string entityDescription)
        where T : class
    {
        return resolution.Value
            ?? throw new InvalidOperationException(
                $"A resolved {entityDescription} reference must contain an entity.");
    }

    private static MappingDiagnostic
        CreateInvalidPlacementDiagnostic(
            StepAxis2Placement3DEntity source,
            string failureMessage)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "AXIS2_PLACEMENT_3D"
                : source.SourceEntityId;

        return new MappingDiagnostic(
            InvalidPlacementDiagnosticCode,
            $"STEP AXIS2_PLACEMENT_3D '{sourceIdentity}' " +
            "could not be mapped to a valid canonical " +
            $"coordinate system. {failureMessage}",
            MappingDiagnosticSeverity.Error);
    }
}