using MPC.Connectors.Step.Semantics;
using MPC.Topology;
using MPC.Topology.Building;
using MPC.Topology.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepAdvancedFaceMapper :
    IEntityMapper<
        StepAdvancedFaceEntity,
        Face>
{
    private const string MissingOuterBoundDiagnosticCode =
        "MAP-STEP-ADVANCED-FACE-001";

    private const string MultipleOuterBoundsDiagnosticCode =
        "MAP-STEP-ADVANCED-FACE-002";

    private const string InvalidFaceDiagnosticCode =
        "MAP-STEP-ADVANCED-FACE-003";

    private readonly IEntityMapper<
        StepAdvancedFaceEntity,
        FaceBuilder> _faceBuilderMapper;

    private readonly IEntityMapper<
        StepFaceBoundEntity,
        FaceLoopMapping> _faceBoundMapper;

    public StepAdvancedFaceMapper()
        : this(
            new StepAdvancedFaceBuilderMapper(),
            new StepFaceBoundMapper(
                new StepEdgeLoopMapper(
                    new StepOrientedEdgeMapper())))
    {
    }

    public StepAdvancedFaceMapper(
        IEntityMapper<
            StepAdvancedFaceEntity,
            FaceBuilder> faceBuilderMapper,
        IEntityMapper<
            StepFaceBoundEntity,
            FaceLoopMapping> faceBoundMapper)
    {
        ArgumentNullException.ThrowIfNull(
            faceBuilderMapper);

        ArgumentNullException.ThrowIfNull(
            faceBoundMapper);

        _faceBuilderMapper =
            faceBuilderMapper;

        _faceBoundMapper =
            faceBoundMapper;
    }

    public MappingResult<Face> Map(
        StepAdvancedFaceEntity source,
        MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ArgumentNullException.ThrowIfNull(
            context);

        int diagnosticCheckpoint =
            context.CaptureDiagnosticCheckpoint();

        MappingResult<FaceBuilder> builderResult =
            _faceBuilderMapper.Map(
                source,
                context);

        if (!builderResult.IsSuccessful)
        {
            return MappingResult<Face>.Failure(
                builderResult.Diagnostics);
        }

        FaceBuilder faceBuilder =
            builderResult.Value
            ?? throw new InvalidOperationException(
                "A successful ADVANCED_FACE builder mapping must contain a face builder.");

        var boundaryMappings =
            new List<FaceLoopMapping>(
                source.BoundReferenceIds.Count);

        var operationDiagnostics =
            new List<MappingDiagnostic>();

        foreach (string boundReferenceId
                 in source.BoundReferenceIds)
        {
            ReferenceResolutionResult<
                StepFaceBoundEntity> resolution =
                    context.ReferenceResolver
                        .Resolve<StepFaceBoundEntity>(
                            boundReferenceId);

            if (!resolution.IsResolved)
            {
                context.AddDiagnostics(
                    resolution.Diagnostics);

                operationDiagnostics.AddRange(
                    resolution.Diagnostics);

                continue;
            }

            StepFaceBoundEntity faceBound =
                resolution.Value
                ?? throw new InvalidOperationException(
                    "A resolved FACE_BOUND reference must contain an entity.");

            MappingResult<FaceLoopMapping> boundResult =
                _faceBoundMapper.Map(
                    faceBound,
                    context);

            if (!boundResult.IsSuccessful)
            {
                operationDiagnostics.AddRange(
                    boundResult.Diagnostics);

                continue;
            }

            FaceLoopMapping boundaryMapping =
                boundResult.Value
                ?? throw new InvalidOperationException(
                    "A successful FACE_BOUND mapping must contain a face loop mapping.");

            boundaryMappings.Add(
                boundaryMapping);
        }

        if (operationDiagnostics.Count > 0)
        {
            return MappingResult<Face>.Failure(
                operationDiagnostics);
        }

        List<FaceLoopMapping> outerMappings =
            boundaryMappings
                .Where(
                    mapping =>
                        mapping.IsOuter)
                .ToList();

        if (outerMappings.Count == 0)
        {
            MappingDiagnostic diagnostic =
                CreateDiagnostic(
                    MissingOuterBoundDiagnosticCode,
                    source,
                    "A canonical face requires exactly one outer boundary, but no FACE_OUTER_BOUND was found.");

            context.AddDiagnostic(
                diagnostic);

            return MappingResult<Face>.Failure(
                [
                    diagnostic
                ]);
        }

        if (outerMappings.Count > 1)
        {
            MappingDiagnostic diagnostic =
                CreateDiagnostic(
                    MultipleOuterBoundsDiagnosticCode,
                    source,
                    $"A canonical face requires exactly one outer boundary, but {outerMappings.Count} FACE_OUTER_BOUND entities were found.");

            context.AddDiagnostic(
                diagnostic);

            return MappingResult<Face>.Failure(
                [
                    diagnostic
                ]);
        }

        try
        {
            FaceOrientation orientation =
                source.SameSense
                    ? FaceOrientation.Forward
                    : FaceOrientation.Reversed;

            faceBuilder.SetOrientation(
                orientation);

            foreach (FaceLoopMapping boundaryMapping
                     in boundaryMappings)
            {
                if (boundaryMapping.IsOuter)
                {
                    faceBuilder.SetOuterLoop(
                        boundaryMapping.Loop);
                }
                else
                {
                    faceBuilder.AddInnerLoop(
                        boundaryMapping.Loop);
                }
            }

            Face face =
                faceBuilder.Build();

            IReadOnlyList<MappingDiagnostic> operationResultDiagnostics =
                context.GetDiagnosticsSince(
                    diagnosticCheckpoint);

            if (operationResultDiagnostics.Count > 0)
            {
                return MappingResult<Face>
                    .SuccessWithDiagnostics(
                        face,
                        operationResultDiagnostics);
            }

            return MappingResult<Face>.Success(
                face);
        }
        catch (InvalidOperationException exception)
        {
            MappingDiagnostic diagnostic =
                CreateDiagnostic(
                    InvalidFaceDiagnosticCode,
                    source,
                    $"The canonical face could not be constructed. {exception.Message}");

            context.AddDiagnostic(
                diagnostic);

            return MappingResult<Face>.Failure(
                [
                    diagnostic
                ]);
        }
    }

    private static MappingDiagnostic CreateDiagnostic(
        string code,
        StepAdvancedFaceEntity source,
        string message)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "ADVANCED_FACE"
                : source.SourceEntityId;

        return new MappingDiagnostic(
            code,
            $"STEP ADVANCED_FACE '{sourceIdentity}' could not be mapped. {message}",
            MappingDiagnosticSeverity.Error);
    }
}