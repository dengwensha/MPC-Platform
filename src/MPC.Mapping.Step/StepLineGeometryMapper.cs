using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepLineGeometryMapper :
    IEntityMapper<
        StepLineEntity,
        Line3D>
{
    private const string InvalidLineDiagnosticCode =
        "MAP-STEP-LINE-001";

    private const int RequiredDimension =
        3;

    private const double ZeroLengthTolerance =
        1e-15;

    public MappingResult<Line3D> Map(
        StepLineEntity source,
        MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ArgumentNullException.ThrowIfNull(
            context);

        ReferenceResolutionResult<
            StepCartesianPointEntity> pointResolution =
                context.ReferenceResolver
                    .Resolve<StepCartesianPointEntity>(
                        source.PointReferenceId);

        if (!pointResolution.IsResolved)
        {
            context.AddDiagnostics(
                pointResolution.Diagnostics);

            return MappingResult<Line3D>.Failure(
                pointResolution.Diagnostics);
        }

        StepCartesianPointEntity point =
            pointResolution.Value
            ?? throw new InvalidOperationException(
                "A resolved CARTESIAN_POINT reference must contain a semantic entity.");

        ReferenceResolutionResult<
            StepVectorEntity> vectorResolution =
                context.ReferenceResolver
                    .Resolve<StepVectorEntity>(
                        source.VectorReferenceId);

        if (!vectorResolution.IsResolved)
        {
            context.AddDiagnostics(
                vectorResolution.Diagnostics);

            return MappingResult<Line3D>.Failure(
                vectorResolution.Diagnostics);
        }

        StepVectorEntity vector =
            vectorResolution.Value
            ?? throw new InvalidOperationException(
                "A resolved VECTOR reference must contain a semantic entity.");

        ReferenceResolutionResult<
            StepDirectionEntity> directionResolution =
                context.ReferenceResolver
                    .Resolve<StepDirectionEntity>(
                        vector.OrientationReferenceId);

        if (!directionResolution.IsResolved)
        {
            context.AddDiagnostics(
                directionResolution.Diagnostics);

            return MappingResult<Line3D>.Failure(
                directionResolution.Diagnostics);
        }

        StepDirectionEntity direction =
            directionResolution.Value
            ?? throw new InvalidOperationException(
                "A resolved DIRECTION reference must contain a semantic entity.");

        Line3D line;

        try
        {
            Point3D origin =
                CreatePoint(
                    point);

            Vector3D unitDirection =
                CreateUnitDirection(
                    direction);

            line =
                new Line3D(
                    origin,
                    unitDirection);
        }
        catch (ArgumentException exception)
        {
            MappingDiagnostic diagnostic =
                CreateInvalidLineDiagnostic(
                    source,
                    exception.Message);

            context.AddDiagnostic(
                diagnostic);

            return MappingResult<Line3D>.Failure(
                [
                    diagnostic
                ]);
        }

        if (context.HasDiagnostics)
        {
            return MappingResult<Line3D>
                .SuccessWithDiagnostics(
                    line,
                    context.Diagnostics);
        }

        return MappingResult<Line3D>.Success(
            line);
    }

    private static Point3D CreatePoint(
        StepCartesianPointEntity source)
    {
        if (source.Coordinates.Count !=
            RequiredDimension)
        {
            throw new ArgumentException(
                CreateDimensionMessage(
                    source.SourceEntityId,
                    "CARTESIAN_POINT",
                    source.Coordinates.Count,
                    "coordinate"));
        }

        return new Point3D(
            source.Coordinates[0],
            source.Coordinates[1],
            source.Coordinates[2]);
    }

    private static Vector3D CreateUnitDirection(
        StepDirectionEntity source)
    {
        if (source.Ratios.Count !=
            RequiredDimension)
        {
            throw new ArgumentException(
                CreateDimensionMessage(
                    source.SourceEntityId,
                    "DIRECTION",
                    source.Ratios.Count,
                    "direction ratio"));
        }

        double x =
            source.Ratios[0];

        double y =
            source.Ratios[1];

        double z =
            source.Ratios[2];

        double lengthSquared =
            x * x +
            y * y +
            z * z;

        if (!double.IsFinite(
                lengthSquared) ||
            lengthSquared <=
            ZeroLengthTolerance *
            ZeroLengthTolerance)
        {
            string sourceIdentity =
                string.IsNullOrWhiteSpace(
                    source.SourceEntityId)
                    ? "DIRECTION"
                    : source.SourceEntityId;

            throw new ArgumentException(
                $"STEP DIRECTION '{sourceIdentity}' cannot be converted " +
                "to a canonical unit vector because its magnitude is zero " +
                "or numerically invalid.");
        }

        double length =
            Math.Sqrt(
                lengthSquared);

        return new Vector3D(
            x / length,
            y / length,
            z / length);
    }

    private static string CreateDimensionMessage(
        string? sourceEntityId,
        string entityType,
        int actualCount,
        string componentName)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                sourceEntityId)
                ? entityType
                : sourceEntityId;

        return
            $"STEP {entityType} '{sourceIdentity}' contains " +
            $"{actualCount} {componentName} values. " +
            $"Three-dimensional canonical geometry requires exactly " +
            $"{RequiredDimension} values.";
    }

    private static MappingDiagnostic
        CreateInvalidLineDiagnostic(
            StepLineEntity source,
            string failureMessage)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "LINE"
                : source.SourceEntityId;

        return new MappingDiagnostic(
            InvalidLineDiagnosticCode,
            $"STEP LINE '{sourceIdentity}' could not be mapped " +
            $"to a valid canonical line. {failureMessage}",
            MappingDiagnosticSeverity.Error);
    }
}