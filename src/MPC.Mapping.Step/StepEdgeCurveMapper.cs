using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Topology.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepEdgeCurveMapper :
    IEntityMapper<
        StepEdgeCurveEntity,
        Edge>
{
    private const string MissingCanonicalResolverDiagnosticCode =
        "MAP-STEP-EDGE-CURVE-001";

    private const string UnsupportedSameSenseDiagnosticCode =
        "MAP-STEP-EDGE-CURVE-002";

    public MappingResult<Edge> Map(
        StepEdgeCurveEntity source,
        MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ArgumentNullException.ThrowIfNull(
            context);

        ICanonicalReferenceResolver? canonicalResolver =
            context.CanonicalReferenceResolver;

        if (canonicalResolver is null)
        {
            MappingDiagnostic diagnostic =
                CreateDiagnostic(
                    MissingCanonicalResolverDiagnosticCode,
                    source,
                    "Canonical reference resolution is required to map EDGE_CURVE.");

            context.AddDiagnostic(
                diagnostic);

            return MappingResult<Edge>.Failure(
                [
                    diagnostic
                ]);
        }

        if (!source.SameSense)
        {
            MappingDiagnostic diagnostic =
                CreateDiagnostic(
                    UnsupportedSameSenseDiagnosticCode,
                    source,
                    "EDGE_CURVE with SAME_SENSE = false is not supported by the current canonical curve-orientation contract.");

            context.AddDiagnostic(
                diagnostic);

            return MappingResult<Edge>.Failure(
                [
                    diagnostic
                ]);
        }

        MappingResult<Vertex> startVertexResult =
            canonicalResolver.Resolve<Vertex>(
                source.EdgeStartReferenceId,
                context);

        if (!startVertexResult.IsSuccessful ||
            startVertexResult.Value is null)
        {
            return MappingResult<Edge>.Failure(
                startVertexResult.Diagnostics);
        }

        MappingResult<Vertex> endVertexResult =
            canonicalResolver.Resolve<Vertex>(
                source.EdgeEndReferenceId,
                context);

        if (!endVertexResult.IsSuccessful ||
            endVertexResult.Value is null)
        {
            return MappingResult<Edge>.Failure(
                endVertexResult.Diagnostics);
        }

        MappingResult<ICurve3D> curveResult =
            canonicalResolver.Resolve<ICurve3D>(
                source.EdgeGeometryReferenceId,
                context);

        if (!curveResult.IsSuccessful ||
            curveResult.Value is null)
        {
            return MappingResult<Edge>.Failure(
                curveResult.Diagnostics);
        }

        var edge =
            new Edge(
                startVertexResult.Value,
                endVertexResult.Value,
                curveResult.Value);

        if (context.HasDiagnostics)
        {
            return MappingResult<Edge>
                .SuccessWithDiagnostics(
                    edge,
                    context.Diagnostics);
        }

        return MappingResult<Edge>.Success(
            edge);
    }

    private static MappingDiagnostic CreateDiagnostic(
        string code,
        StepEdgeCurveEntity source,
        string message)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "EDGE_CURVE"
                : source.SourceEntityId;

        return new MappingDiagnostic(
            code,
            $"STEP EDGE_CURVE '{sourceIdentity}' could not be mapped. {message}",
            MappingDiagnosticSeverity.Error);
    }
}