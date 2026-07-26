using MPC.Connectors.Step.Semantics;

namespace MPC.Mapping.Step;

public sealed class StepReferenceResolver : IReferenceResolver
{
    private const string ResolutionFailureCode =
        "STEP_REFERENCE_RESOLUTION_FAILED";

    private const string TypeMismatchCode =
        "STEP_REFERENCE_TYPE_MISMATCH";

    private readonly StepSemanticEntityResolver _semanticResolver;

    public StepReferenceResolver(
        StepSemanticEntityResolver semanticResolver)
    {
        ArgumentNullException.ThrowIfNull(
            semanticResolver);

        _semanticResolver =
            semanticResolver;
    }

    public ReferenceResolutionResult<T> Resolve<T>(
        string referenceId)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(
                referenceId))
        {
            throw new ArgumentException(
                "STEP reference identifier cannot be null, empty, or whitespace.",
                nameof(referenceId));
        }

        StepSemanticEntity semanticEntity;

        try
        {
            semanticEntity =
                _semanticResolver.Resolve(
                    referenceId);
        }
        catch (InvalidOperationException exception)
        {
            return CreateFailure<T>(
                referenceId,
                ResolutionFailureCode,
                exception.Message);
        }

        if (semanticEntity is not T typedEntity)
        {
            return CreateFailure<T>(
                referenceId,
                TypeMismatchCode,
                $"STEP semantic entity '{referenceId}' is '{semanticEntity.SourceEntityType}', but '{typeof(T).Name}' was required.");
        }

        return ReferenceResolutionResult<T>.Resolved(
            referenceId,
            typedEntity);
    }

    private static ReferenceResolutionResult<T> CreateFailure<T>(
        string referenceId,
        string code,
        string message)
        where T : class
    {
        var diagnostic =
            new MappingDiagnostic(
                code,
                message,
                MappingDiagnosticSeverity.Error);

        return ReferenceResolutionResult<T>.Failure(
            referenceId,
            [diagnostic]);
    }
}