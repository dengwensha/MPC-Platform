using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using MPC.Topology.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepVertexPointMapper :
    IEntityMapper<
        StepVertexPointEntity,
        Vertex>
{
    private const string InvalidVertexDiagnosticCode =
        "MAP-STEP-VERTEX-POINT-001";

    private const int RequiredDimension =
        3;

    public MappingResult<Vertex> Map(
        StepVertexPointEntity source,
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
                        source.VertexGeometryReferenceId);

        if (!pointResolution.IsResolved)
        {
            context.AddDiagnostics(
                pointResolution.Diagnostics);

            return MappingResult<Vertex>.Failure(
                pointResolution.Diagnostics);
        }

        StepCartesianPointEntity pointEntity =
            pointResolution.Value
            ?? throw new InvalidOperationException(
                "A resolved CARTESIAN_POINT reference must contain a semantic entity.");

        Vertex vertex;

        try
        {
            Point3D position =
                CreatePoint(
                    pointEntity);

            vertex =
                new Vertex(
                    position);
        }
        catch (ArgumentException exception)
        {
            MappingDiagnostic diagnostic =
                CreateInvalidVertexDiagnostic(
                    source,
                    exception.Message);

            context.AddDiagnostic(
                diagnostic);

            return MappingResult<Vertex>.Failure(
                [
                    diagnostic
                ]);
        }

        if (context.HasDiagnostics)
        {
            return MappingResult<Vertex>
                .SuccessWithDiagnostics(
                    vertex,
                    context.Diagnostics);
        }

        return MappingResult<Vertex>.Success(
            vertex);
    }

    private static Point3D CreatePoint(
        StepCartesianPointEntity source)
    {
        if (source.Coordinates.Count !=
            RequiredDimension)
        {
            string sourceIdentity =
                string.IsNullOrWhiteSpace(
                    source.SourceEntityId)
                    ? "CARTESIAN_POINT"
                    : source.SourceEntityId;

            throw new ArgumentException(
                $"STEP CARTESIAN_POINT '{sourceIdentity}' contains " +
                $"{source.Coordinates.Count} coordinate values. " +
                $"Three-dimensional canonical topology requires exactly " +
                $"{RequiredDimension} values.");
        }

        return new Point3D(
            source.Coordinates[0],
            source.Coordinates[1],
            source.Coordinates[2]);
    }

    private static MappingDiagnostic
        CreateInvalidVertexDiagnostic(
            StepVertexPointEntity source,
            string failureMessage)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "VERTEX_POINT"
                : source.SourceEntityId;

        return new MappingDiagnostic(
            InvalidVertexDiagnosticCode,
            $"STEP VERTEX_POINT '{sourceIdentity}' could not be mapped " +
            $"to a valid canonical vertex. {failureMessage}",
            MappingDiagnosticSeverity.Error);
    }
}