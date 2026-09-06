using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepDegenerateToroidalSurfaceGeometryMapper :
    IEntityMapper<
        StepDegenerateToroidalSurfaceEntity,
        DegenerateTorus3D>
{
    private const string InvalidLocationDiagnosticCode =
        "MAP-STEP-DEGENERATE-TORUS-001";

    private const string InvalidAxisDiagnosticCode =
        "MAP-STEP-DEGENERATE-TORUS-002";

    private const string InvalidReferenceDirectionDiagnosticCode =
        "MAP-STEP-DEGENERATE-TORUS-003";

    private const string InvalidDegenerateTorusDiagnosticCode =
        "MAP-STEP-DEGENERATE-TORUS-004";

    private static readonly Vector3D DefaultAxis =
        new(
            0.0,
            0.0,
            1.0);

    public MappingResult<DegenerateTorus3D> Map(
        StepDegenerateToroidalSurfaceEntity source,
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
                "must contain exactly three coordinates for " +
                "DegenerateTorus3D mapping.");
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
                    "for DegenerateTorus3D axis mapping.");
            }

            axis =
                new Vector3D(
                    axisEntity.Ratios[0],
                    axisEntity.Ratios[1],
                    axisEntity.Ratios[2]);
        }

        Vector3D? referenceDirection =
            null;

        if (!string.IsNullOrWhiteSpace(
                placement.RefDirectionReferenceId))
        {
            ReferenceResolutionResult<StepDirectionEntity>
                referenceDirectionResolution =
                    context.ReferenceResolver
                        .Resolve<StepDirectionEntity>(
                            placement.RefDirectionReferenceId);

            if (!referenceDirectionResolution.IsResolved)
            {
                return CreateResolutionFailure(
                    context,
                    diagnosticCheckpoint,
                    referenceDirectionResolution.Diagnostics);
            }

            StepDirectionEntity referenceDirectionEntity =
                referenceDirectionResolution.Value
                ?? throw new InvalidOperationException(
                    "A resolved DIRECTION reference must contain an entity.");

            if (referenceDirectionEntity.Ratios.Count != 3)
            {
                return CreateFailure(
                    context,
                    diagnosticCheckpoint,
                    InvalidReferenceDirectionDiagnosticCode,
                    $"STEP DIRECTION '{referenceDirectionEntity.SourceEntityId}' " +
                    "must contain exactly three direction ratios " +
                    "for DegenerateTorus3D reference-direction mapping.");
            }

            referenceDirection =
                new Vector3D(
                    referenceDirectionEntity.Ratios[0],
                    referenceDirectionEntity.Ratios[1],
                    referenceDirectionEntity.Ratios[2]);
        }

        DegenerateTorus3D degenerateTorus;

        try
        {
            CoordinateSystem3D coordinateSystem =
                CoordinateSystem3DFactory.Create(
                    center,
                    axis,
                    referenceDirection);

            degenerateTorus =
                new DegenerateTorus3D(
                    coordinateSystem.Origin,
                    coordinateSystem.ZAxis,
                    source.MajorRadius,
                    source.MinorRadius,
                    source.SelectOuter);
        }
        catch (ArgumentException exception)
        {
            return CreateFailure(
                context,
                diagnosticCheckpoint,
                InvalidDegenerateTorusDiagnosticCode,
                CreateInvalidDegenerateTorusMessage(
                    source,
                    exception));
        }

        IReadOnlyList<MappingDiagnostic> operationDiagnostics =
            context.GetDiagnosticsSince(
                diagnosticCheckpoint);

        if (operationDiagnostics.Count > 0)
        {
            return MappingResult<DegenerateTorus3D>
                .SuccessWithDiagnostics(
                    degenerateTorus,
                    operationDiagnostics);
        }

        return MappingResult<DegenerateTorus3D>.Success(
            degenerateTorus);
    }

    private static MappingResult<DegenerateTorus3D>
        CreateResolutionFailure(
            MappingContext context,
            int diagnosticCheckpoint,
            IReadOnlyList<MappingDiagnostic> diagnostics)
    {
        context.AddDiagnostics(
            diagnostics);

        return MappingResult<DegenerateTorus3D>.Failure(
            context.GetDiagnosticsSince(
                diagnosticCheckpoint));
    }

    private static MappingResult<DegenerateTorus3D>
        CreateFailure(
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

        return MappingResult<DegenerateTorus3D>.Failure(
            context.GetDiagnosticsSince(
                diagnosticCheckpoint));
    }

    private static string CreateInvalidDegenerateTorusMessage(
        StepDegenerateToroidalSurfaceEntity source,
        ArgumentException exception)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "DEGENERATE_TOROIDAL_SURFACE"
                : source.SourceEntityId;

        return
            $"STEP DEGENERATE_TOROIDAL_SURFACE '{sourceIdentity}' could not " +
            "be mapped to a valid canonical degenerate torus. " +
            exception.Message;
    }
}
