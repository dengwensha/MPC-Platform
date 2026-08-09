using MPC.Connectors.Step.Semantics;
using MPC.Topology;
using MPC.Topology.Primitives;

namespace MPC.Mapping.Step;

public sealed class StepClosedShellMapper :
    IEntityMapper<
        StepClosedShellEntity,
        Shell>
{
    private const string DuplicateFaceReferenceDiagnosticCode =
        "MAP-STEP-CLOSED-SHELL-001";

    private const string DuplicateCanonicalFaceDiagnosticCode =
        "MAP-STEP-CLOSED-SHELL-002";

    private const string InvalidShellDiagnosticCode =
        "MAP-STEP-CLOSED-SHELL-003";

    private readonly IEntityMapper<
        StepAdvancedFaceEntity,
        Face> _faceMapper;

    public StepClosedShellMapper()
        : this(
            new StepAdvancedFaceMapper())
    {
    }

    public StepClosedShellMapper(
        IEntityMapper<
            StepAdvancedFaceEntity,
            Face> faceMapper)
    {
        ArgumentNullException.ThrowIfNull(
            faceMapper);

        _faceMapper =
            faceMapper;
    }

    public MappingResult<Shell> Map(
        StepClosedShellEntity source,
        MappingContext context)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ArgumentNullException.ThrowIfNull(
            context);

        int diagnosticCheckpoint =
            context.CaptureDiagnosticCheckpoint();

        var faces =
            new List<Face>(
                source.FaceReferenceIds.Count);

        var operationDiagnostics =
            new List<MappingDiagnostic>();

        var mappedReferenceIds =
            new HashSet<string>(
                StringComparer.Ordinal);

        var mappedFaces =
            new HashSet<Face>(
                ReferenceEqualityComparer.Instance);

        foreach (string faceReferenceId
                 in source.FaceReferenceIds)
        {
            if (!mappedReferenceIds.Add(
                    faceReferenceId))
            {
                MappingDiagnostic diagnostic =
                    CreateDiagnostic(
                        DuplicateFaceReferenceDiagnosticCode,
                        source,
                        $"Face reference '{faceReferenceId}' occurs more than once.");

                context.AddDiagnostic(
                    diagnostic);

                operationDiagnostics.Add(
                    diagnostic);

                continue;
            }

            ReferenceResolutionResult<
                StepAdvancedFaceEntity> resolution =
                    context.ReferenceResolver
                        .Resolve<StepAdvancedFaceEntity>(
                            faceReferenceId);

            if (!resolution.IsResolved)
            {
                context.AddDiagnostics(
                    resolution.Diagnostics);

                operationDiagnostics.AddRange(
                    resolution.Diagnostics);

                continue;
            }

            StepAdvancedFaceEntity advancedFace =
                resolution.Value
                ?? throw new InvalidOperationException(
                    "A resolved ADVANCED_FACE reference must contain an entity.");

            MappingResult<Face> faceResult =
                _faceMapper.Map(
                    advancedFace,
                    context);

            if (!faceResult.IsSuccessful)
            {
                operationDiagnostics.AddRange(
                    faceResult.Diagnostics);

                continue;
            }

            Face face =
                faceResult.Value
                ?? throw new InvalidOperationException(
                    "A successful ADVANCED_FACE mapping must contain a canonical face.");

            if (!mappedFaces.Add(
                    face))
            {
                MappingDiagnostic diagnostic =
                    CreateDiagnostic(
                        DuplicateCanonicalFaceDiagnosticCode,
                        source,
                        $"Face reference '{faceReferenceId}' mapped to a canonical face that was already added.");

                context.AddDiagnostic(
                    diagnostic);

                operationDiagnostics.Add(
                    diagnostic);

                continue;
            }

            faces.Add(
                face);
        }

        if (operationDiagnostics.Count > 0)
        {
            return MappingResult<Shell>.Failure(
                operationDiagnostics);
        }

        try
        {
            var shell =
                new Shell(
                    faces);

            IReadOnlyList<MappingDiagnostic> operationResultDiagnostics =
                context.GetDiagnosticsSince(
                    diagnosticCheckpoint);

            if (operationResultDiagnostics.Count > 0)
            {
                return MappingResult<Shell>
                    .SuccessWithDiagnostics(
                        shell,
                        operationResultDiagnostics);
            }

            return MappingResult<Shell>.Success(
                shell);
        }
        catch (ArgumentException exception)
        {
            MappingDiagnostic diagnostic =
                CreateDiagnostic(
                    InvalidShellDiagnosticCode,
                    source,
                    $"The canonical shell could not be constructed. {exception.Message}");

            context.AddDiagnostic(
                diagnostic);

            return MappingResult<Shell>.Failure(
                new[]
                {
                    diagnostic
                });
        }
    }

    private static MappingDiagnostic CreateDiagnostic(
        string code,
        StepClosedShellEntity source,
        string message)
    {
        string sourceIdentity =
            string.IsNullOrWhiteSpace(
                source.SourceEntityId)
                ? "CLOSED_SHELL"
                : source.SourceEntityId;

        return new MappingDiagnostic(
            code,
            $"STEP CLOSED_SHELL '{sourceIdentity}' could not be mapped. {message}",
            MappingDiagnosticSeverity.Error);
    }
}