using MPC.Connectors.Diagnostics;

namespace MPC.Connectors.Step.Models;

public sealed class StepDataEntityRecord
{
    private static readonly IReadOnlyList<string>
        EmptyStrings =
            Array.Empty<string>();

    private static readonly IReadOnlyList<CadDiagnostic>
        EmptyDiagnostics =
            Array.Empty<CadDiagnostic>();

    private readonly IReadOnlyList<string> _componentTypes;
    private readonly IReadOnlyList<CadDiagnostic> _diagnostics;
    private readonly IReadOnlyList<string> _referencedEntityIds;

    public string? SourceEntityId { get; }

    public string SourceEntityType { get; }

    public string RawStatement { get; }

    public CadReadStatus ReadStatus { get; }

    public bool IsComplexEntity =>
        _componentTypes.Count > 0;

    public IReadOnlyList<string> ComponentTypes =>
        _componentTypes;

    public IReadOnlyList<CadDiagnostic> Diagnostics =>
        _diagnostics;

    public IReadOnlyList<string> ReferencedEntityIds =>
        _referencedEntityIds;

    public StepDataEntityRecord(
        string? sourceEntityId,
        string sourceEntityType,
        string rawStatement,
        CadReadStatus readStatus,
        IEnumerable<string>? componentTypes = null,
        IEnumerable<CadDiagnostic>? diagnostics = null,
        IEnumerable<string>? referencedEntityIds = null)
    {
        if (string.IsNullOrWhiteSpace(sourceEntityType))
        {
            throw new ArgumentException(
                "Source entity type cannot be null, empty, or whitespace.",
                nameof(sourceEntityType));
        }

        if (string.IsNullOrWhiteSpace(rawStatement))
        {
            throw new ArgumentException(
                "Raw STEP statement cannot be null, empty, or whitespace.",
                nameof(rawStatement));
        }

        if (!Enum.IsDefined(readStatus))
        {
            throw new ArgumentOutOfRangeException(
                nameof(readStatus),
                readStatus,
                "Read status is not defined.");
        }

        var normalizedSourceEntityType =
            sourceEntityType
                .Trim()
                .ToUpperInvariant();

        var normalizedComponentTypes =
            MaterializeStrings(
                componentTypes,
                normalizeToUpperInvariant:
                    true,
                parameterName:
                    nameof(componentTypes),
                invalidEntryMessage:
                    "Component types cannot contain null, empty, or whitespace entries.");

        var materializedDiagnostics =
            MaterializeDiagnostics(
                diagnostics);

        var normalizedReferencedEntityIds =
            MaterializeStrings(
                referencedEntityIds,
                normalizeToUpperInvariant:
                    false,
                parameterName:
                    nameof(referencedEntityIds),
                invalidEntryMessage:
                    "Referenced entity IDs cannot contain null, empty, or whitespace entries.");

        EnsureStatusMatchesDiagnostics(
            readStatus,
            materializedDiagnostics);

        EnsureComplexEntityState(
            normalizedSourceEntityType,
            normalizedComponentTypes);

        SourceEntityId =
            NormalizeOptional(
                sourceEntityId);

        SourceEntityType =
            normalizedSourceEntityType;

        RawStatement =
            rawStatement.Trim();

        ReadStatus =
            readStatus;

        _componentTypes =
            normalizedComponentTypes;

        _diagnostics =
            materializedDiagnostics;

        _referencedEntityIds =
            normalizedReferencedEntityIds;
    }

    private static IReadOnlyList<string>
        MaterializeStrings(
            IEnumerable<string>? values,
            bool normalizeToUpperInvariant,
            string parameterName,
            string invalidEntryMessage)
    {
        if (values is null)
        {
            return EmptyStrings;
        }

        var materializedValues =
            values is ICollection<string> collection
                ? new List<string>(
                    collection.Count)
                : new List<string>();

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    invalidEntryMessage,
                    parameterName);
            }

            var normalizedValue =
                value.Trim();

            if (normalizeToUpperInvariant)
            {
                normalizedValue =
                    normalizedValue
                        .ToUpperInvariant();
            }

            materializedValues.Add(
                normalizedValue);
        }

        return materializedValues.Count == 0
            ? EmptyStrings
            : materializedValues.AsReadOnly();
    }

    private static IReadOnlyList<CadDiagnostic>
        MaterializeDiagnostics(
            IEnumerable<CadDiagnostic>? diagnostics)
    {
        if (diagnostics is null)
        {
            return EmptyDiagnostics;
        }

        var materializedDiagnostics =
            diagnostics is
                ICollection<CadDiagnostic> collection
                ? new List<CadDiagnostic>(
                    collection.Count)
                : new List<CadDiagnostic>();

        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic is null)
            {
                throw new ArgumentException(
                    "Diagnostics cannot contain null entries.",
                    nameof(diagnostics));
            }

            materializedDiagnostics.Add(
                diagnostic);
        }

        return materializedDiagnostics.Count == 0
            ? EmptyDiagnostics
            : materializedDiagnostics.AsReadOnly();
    }

    private static void EnsureComplexEntityState(
        string normalizedSourceEntityType,
        IReadOnlyCollection<string> componentTypes)
    {
        if (normalizedSourceEntityType ==
                "COMPLEX_ENTITY" &&
            componentTypes.Count == 0)
        {
            throw new InvalidOperationException(
                "Complex STEP entity must contain at least one component type.");
        }

        if (normalizedSourceEntityType !=
                "COMPLEX_ENTITY" &&
            componentTypes.Count > 0)
        {
            throw new InvalidOperationException(
                "Only COMPLEX_ENTITY records may contain component types.");
        }
    }

    private static void EnsureStatusMatchesDiagnostics(
        CadReadStatus readStatus,
        IReadOnlyCollection<CadDiagnostic> diagnostics)
    {
        var hasErrorOrFatal =
            false;

        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Severity is
                CadDiagnosticSeverity.Error or
                CadDiagnosticSeverity.Fatal)
            {
                hasErrorOrFatal =
                    true;

                break;
            }
        }

        if (readStatus == CadReadStatus.Complete &&
            hasErrorOrFatal)
        {
            throw new InvalidOperationException(
                "Complete STEP entity cannot contain error or fatal diagnostics.");
        }

        if (readStatus is
                CadReadStatus.Partial or
                CadReadStatus.Failed &&
            !hasErrorOrFatal)
        {
            throw new InvalidOperationException(
                "Partial or failed STEP entity must contain at least one error or fatal diagnostic.");
        }
    }

    private static string? NormalizeOptional(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}