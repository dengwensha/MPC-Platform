using MPC.Connectors.Step.Semantics;
using MPC.Topology.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepOrientedEdgeMapper :
    IEntityMapper<StepOrientedEdgeEntity, EdgeUse>
{
    public MappingResult<EdgeUse> Map(
        StepOrientedEdgeEntity source,
        MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);

        ReferenceResolutionResult<Edge> resolution =
            context.ReferenceResolver.Resolve<Edge>(
                source.EdgeElementReferenceId);

        if (!resolution.IsResolved)
        {
            context.AddDiagnostics(
                resolution.Diagnostics);

            return MappingResult<EdgeUse>.Failure(
                resolution.Diagnostics);
        }

        Edge edge =
            resolution.Value
            ?? throw new InvalidOperationException(
                "A resolved edge reference must contain an edge.");

        EdgeUseDirection direction =
            source.Orientation
                ? EdgeUseDirection.Forward
                : EdgeUseDirection.Reversed;

        var edgeUse =
            new EdgeUse(
                edge,
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
}