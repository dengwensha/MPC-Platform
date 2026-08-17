using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepCircleGeometryMapper :
    IEntityMapper<StepCircleEntity, Circle3D>
{
    private const string InvalidLocationDiagnosticCode =
        "MAP-STEP-CIRCLE-001";

    private const string InvalidAxisDiagnosticCode =
        "MAP-STEP-CIRCLE-002";

    private const string InvalidCircleDiagnosticCode =
        "MAP-STEP-CIRCLE-003";

    private static readonly Vector3D DefaultAxis =
        new(
            0.0,
            0.0,
            1.0);

    public MappingResult<Circle3D> Map(
        StepCircleEntity source,
        MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ArgumentNullException.ThrowIfNull(
            context);

        int diagnosticCheckpoint =
            context.CaptureDiagnosticCheckpoint();

        ReferenceResolutionResult<StepAxis2Placement3DEntity>
            placementResolution =
                context.ReferenceResolver
                    .Resolve<StepAxis2Placement3DEntity>(
                        source.PlacementReferenceId);

        if (!placementResolution.IsResolved)
        {
            return CreateResolutionFailure(
                context,
                diagnosticCheckpoint,
                placementResolution.Diagnostics);
        }

        StepAxis2Placement3DEntity placement =
            placementResolution.Value
            ?? throw new InvalidOperationException(
                "A resolved AXIS2_PLACEMENT_3D reference must contain an entity.");

        ReferenceResolutionResult<StepCartesianPointEntity>
            locationResolution =
                context.ReferenceResolver
                    .Resolve<StepCartesianPointEntity>(
                        placement.LocationReferenceId);

        if (!locationResolution.IsResolved)
        {
            return CreateResolutionFailure(
                context,
                diagnosticCheckpoint,
                locationResolution.Diagnostics);
        }

        StepCartesianPointEntity location =
            locationResolution.Value
            ?? throw new InvalidOperationException(
                "A resolved CARTESIAN_POINT reference must contain an entity.");

        if (location.Coordinates.Count != 3)
        {
            return CreateFailure(
                context,
                diagnosticCheckpoint,
                InvalidLocationDiagnosticCode,
                $"STEP CARTESIAN_POINT '{location.SourceEntityId}' " +
                "must contain exactly three coordinates for Circle3D mapping.");
        }

        var center =
            new Point3D(
                location.Coordinates[0],
                location.Coordinates[1],
                location.Coordinates[2]);

        Vector3D axis =
            DefaultAxis;

        if (!string.IsNullOrWhiteSpace(
                placement.AxisReferenceId))
        {
            ReferenceResolutionResult<StepDirectionEntity>
                axisResolution =
                    context.ReferenceResolver
                        .Resolve<StepDirectionEntity>(
                            placement.AxisReferenceId);

            if (!axisResolution.IsResolved)
            {
                return CreateResolutionFailure(
                    context,
                    diagnosticCheckpoint,
                    axisResolution.Diagnostics);
            }

            StepDirectionEntity axisEntity =
                axisResolution.Value
                ?? throw new InvalidOperationException(
                    "A resolved DIRECTION reference must contain an entity.");

            if (axisEntity.Ratios.Count != 3)
            {
                return CreateFailure(
                    context,
                    diagnosticCheckpoint,
                    InvalidAxisDiagnosticCode,
                    $"STEP DIRECTION '{axisEntity.SourceEntityId}' " +
                    "must contain exactly three direction ratios " +
                    "for Circle3D normal mapping.");
            }

            axis =
                new Vector3D(
                    axisEntity.Ratios[0],
                    axisEntity.Ratios[1],
                    axisEntity.Ratios[2]);
        }

        Circle3D circle;

        try
        {
            CoordinateSystem3D coordinateSystem =
                CoordinateSystem3DFactory.Create(
                    center,
                    axis,
                    referenceDirection:
                        null);

            circle =
                new Circle3D(
                    coordinateSystem.Origin,
                    coordinateSystem.ZAxis,
                    source.Radius);
        }
        catch (ArgumentException exception)
        {
            return CreateFailure(
                context,
                diagnosticCheckpoint,
                InvalidCircleDiagnosticCode,
                CreateInvalidCircleMessage(
                    source,
                    exception));
        }

        IReadOnlyList<MappingDiagnostic> operationDiagnostics =
            context.GetDiagnosticsSince(
                diagnosticCheckpoint);

        if (operationDiagnostics.Count > 0)
        {
            return MappingResult<Circle3D>
                .SuccessWithDiagnostics(
                    circle,
                    operationDiagnostics);
        }

        return MappingResult<Circle3D>.Success(
            circle);
    }

    private static MappingResult<Circle3D>
        CreateResolutionFailure(
            MappingContext context,
            int diagnosticCheckpoint,
            IReadOnlyList<MappingDiagnostic> diagnostics)
    {
        context.AddDiagnostics(
            diagnostics);

        return MappingResult<Circle3D>.Failure(
            context.GetDiagnosticsSince(
                diagnosticCheckpoint));
    }

    private static MappingResult<Circle3D> CreateFailure(
        MappingContext context,
        int diagnosticCheckpoint,
        string code,
        string message)
    {
        var diagnostic =
            new MappingDiagnostic(
                code,
                message,
                MappingDiagnosticSeverity.Error);

        context.AddDiagnostic(
            diagnostic);

        return MappingResult<Circle3D>.Failure(
            context.GetDiagnosticsSince(
                diagnosticCheckpoint));
    }

    private static string CreateInvalidCircleMessage(
        StepCircleEntity source,
        ArgumentException exception)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "CIRCLE"
                : source.SourceEntityId;

        return
            $"STEP CIRCLE '{sourceIdentity}' could not be mapped " +
            $"to a valid canonical circle. {exception.Message}";
    }
}