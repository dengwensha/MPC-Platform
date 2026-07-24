using MPC.Connectors.Step.Semantics;
using MPC.Topology.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepEdgeLoopMapper :
    IEntityMapper<StepEdgeLoopEntity, Loop>
{
    private const string InvalidLoopDiagnosticCode =
        "MAP-STEP-EDGE-LOOP-001";

    private readonly IEntityMapper<
        StepOrientedEdgeEntity,
        EdgeUse> _orientedEdgeMapper;

    public StepEdgeLoopMapper(
        IEntityMapper<
            StepOrientedEdgeEntity,
            EdgeUse> orientedEdgeMapper)
    {
        ArgumentNullException.ThrowIfNull(
            orientedEdgeMapper);

        _orientedEdgeMapper =
            orientedEdgeMapper;
    }

    public MappingResult<Loop> Map(
        StepEdgeLoopEntity source,
        MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);

        var edgeUses =
            new List<EdgeUse>(
                source.OrientedEdgeReferenceIds.Count);

        var operationDiagnostics =
            new List<MappingDiagnostic>();

        foreach (string referenceId
                 in source.OrientedEdgeReferenceIds)
        {
            ReferenceResolutionResult<
                StepOrientedEdgeEntity> resolution =
                    context.ReferenceResolver
                        .Resolve<StepOrientedEdgeEntity>(
                            referenceId);

            if (!resolution.IsResolved)
            {
                context.AddDiagnostics(
                    resolution.Diagnostics);

                operationDiagnostics.AddRange(
                    resolution.Diagnostics);

                continue;
            }

            StepOrientedEdgeEntity orientedEdge =
                resolution.Value
                ?? throw new InvalidOperationException(
                    "A resolved ORIENTED_EDGE reference must contain an entity.");

            MappingResult<EdgeUse> edgeUseResult =
                _orientedEdgeMapper.Map(
                    orientedEdge,
                    context);

            if (!edgeUseResult.IsSuccessful)
            {
                operationDiagnostics.AddRange(
                    edgeUseResult.Diagnostics);

                continue;
            }

            EdgeUse edgeUse =
                edgeUseResult.Value
                ?? throw new InvalidOperationException(
                    "A successful ORIENTED_EDGE mapping must contain an edge use.");

            edgeUses.Add(
                edgeUse);
        }

        if (operationDiagnostics.Count > 0)
        {
            return MappingResult<Loop>.Failure(
                operationDiagnostics);
        }

        Loop loop;

        try
        {
            loop =
                new Loop(
                    edgeUses);
        }
        catch (ArgumentException exception)
        {
            var diagnostic =
                new MappingDiagnostic(
                    InvalidLoopDiagnosticCode,
                    CreateInvalidLoopMessage(
                        source,
                        exception),
                    MappingDiagnosticSeverity.Error);

            context.AddDiagnostic(
                diagnostic);

            return MappingResult<Loop>.Failure(
                new[]
                {
                    diagnostic
                });
        }

        if (context.HasDiagnostics)
        {
            return MappingResult<Loop>
                .SuccessWithDiagnostics(
                    loop,
                    context.Diagnostics);
        }

        return MappingResult<Loop>.Success(
            loop);
    }

    private static string CreateInvalidLoopMessage(
        StepEdgeLoopEntity source,
        ArgumentException exception)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "EDGE_LOOP"
                : source.SourceEntityId;

        return
            $"STEP EDGE_LOOP '{sourceIdentity}' could not be mapped " +
            $"to a valid canonical loop. {exception.Message}";
    }
}