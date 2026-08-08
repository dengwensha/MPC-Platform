using MPC.Connectors.Step.Semantics;
using MPC.Topology.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepOrientedEdgeMapper :
    IEntityMapper<
        StepOrientedEdgeEntity,
        EdgeUse>
{
    private const string MissingCanonicalResolverDiagnosticCode =
        "MAP-STEP-ORIENTED-EDGE-001";

    public MappingResult<EdgeUse> Map(
        StepOrientedEdgeEntity source,
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
                    source,
                    "Canonical reference resolution is required to map ORIENTED_EDGE.");

            context.AddDiagnostic(
                diagnostic);

            return MappingResult<EdgeUse>.Failure(
                [
                    diagnostic
                ]);
        }

        MappingResult<Edge> edgeResult =
            canonicalResolver.Resolve<Edge>(
                source.EdgeElementReferenceId,
                context);

        if (!edgeResult.IsSuccessful ||
            edgeResult.Value is null)
        {
            return MappingResult<EdgeUse>.Failure(
                edgeResult.Diagnostics);
        }

        EdgeUseDirection direction =
            source.Orientation
                ? EdgeUseDirection.Forward
                : EdgeUseDirection.Reversed;

        var edgeUse =
            new EdgeUse(
                edgeResult.Value,
                direction);

        if (context.HasDiagnostics)
        {
            return MappingResult<EdgeUse>
                .SuccessWithDiagnostics(
                    edgeUse,
                    context.Diagnostics);
        }

        return MappingResult<EdgeUse>.Success(
            edgeUse);
    }

    private static MappingDiagnostic CreateDiagnostic(
        StepOrientedEdgeEntity source,
        string message)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "ORIENTED_EDGE"
                : source.SourceEntityId;

        return new MappingDiagnostic(
            MissingCanonicalResolverDiagnosticCode,
            $"STEP ORIENTED_EDGE '{sourceIdentity}' could not be mapped. {message}",
            MappingDiagnosticSeverity.Error);
    }
}