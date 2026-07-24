using MPC.Connectors.Step.Semantics;
using MPC.Topology.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepFaceBoundMapper :
    IEntityMapper<
        StepFaceBoundEntity,
        FaceLoopMapping>
{
    private readonly IEntityMapper<
        StepEdgeLoopEntity,
        Loop> _edgeLoopMapper;

    public StepFaceBoundMapper(
        IEntityMapper<
            StepEdgeLoopEntity,
            Loop> edgeLoopMapper)
    {
        ArgumentNullException.ThrowIfNull(
            edgeLoopMapper);

        _edgeLoopMapper =
            edgeLoopMapper;
    }

    public MappingResult<FaceLoopMapping> Map(
        StepFaceBoundEntity source,
        MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(context);

        ReferenceResolutionResult<
            StepEdgeLoopEntity> resolution =
                context.ReferenceResolver
                    .Resolve<StepEdgeLoopEntity>(
                        source.LoopReferenceId);

        if (!resolution.IsResolved)
        {
            context.AddDiagnostics(
                resolution.Diagnostics);

            return MappingResult<
                FaceLoopMapping>.Failure(
                    resolution.Diagnostics);
        }

        StepEdgeLoopEntity edgeLoop =
            resolution.Value
            ?? throw new InvalidOperationException(
                "A resolved EDGE_LOOP reference must contain an entity.");

        MappingResult<Loop> loopResult =
            _edgeLoopMapper.Map(
                edgeLoop,
                context);

        if (!loopResult.IsSuccessful)
        {
            return MappingResult<
                FaceLoopMapping>.Failure(
                    loopResult.Diagnostics);
        }

        Loop loop =
            loopResult.Value
            ?? throw new InvalidOperationException(
                "A successful EDGE_LOOP mapping must contain a canonical loop.");

        var mapping =
            new FaceLoopMapping(
                loop,
                source.Orientation,
                source is StepFaceOuterBoundEntity);

        if (context.HasDiagnostics)
        {
            return MappingResult<
                FaceLoopMapping>
                .SuccessWithDiagnostics(
                    mapping,
                    context.Diagnostics);
        }

        return MappingResult<
            FaceLoopMapping>.Success(
                mapping);
    }
}