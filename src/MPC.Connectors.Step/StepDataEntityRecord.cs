using MPC.Connectors.Diagnostics;

namespace MPC.Connectors.Step.Models;

public sealed class StepDataEntityRecord
{
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

        var componentTypeList =
            componentTypes?.ToList() ??
            new List<string>();

        if (componentTypeList.Any(
                componentType =>
                    string.IsNullOrWhiteSpace(componentType)))
        {
            throw new ArgumentException(
                "Component types cannot contain null, empty, or whitespace entries.",
                nameof(componentTypes));
        }

        componentTypeList =
            componentTypeList
                .Select(
                    componentType =>
                        componentType
                            .Trim()
                            .ToUpperInvariant())
                .ToList();

        if (componentTypeList
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() != componentTypeList.Count)
        {
            throw new ArgumentException(
                "Component types cannot contain duplicate entries.",
                nameof(componentTypes));
        }

        var diagnosticList =
            diagnostics?.ToList() ??
            new List<CadDiagnostic>();

        if (diagnosticList.Any(
                diagnostic =>
                    diagnostic is null))
        {
            throw new ArgumentException(
                "Diagnostics cannot contain null entries.",
                nameof(diagnostics));
        }

        var referencedEntityIdList =
            referencedEntityIds?.ToList() ??
            new List<string>();

        if (referencedEntityIdList.Any(
                referencedEntityId =>
                    string.IsNullOrWhiteSpace(
                        referencedEntityId)))
        {
            throw new ArgumentException(
                "Referenced entity IDs cannot contain null, empty, or whitespace entries.",
                nameof(referencedEntityIds));
        }

        referencedEntityIdList =
            referencedEntityIdList
                .Select(
                    referencedEntityId =>
                        referencedEntityId.Trim())
                .ToList();

        EnsureStatusMatchesDiagnostics(
            readStatus,
            diagnosticList);

        EnsureComplexEntityState(
            sourceEntityType,
            componentTypeList);

        SourceEntityId =
            NormalizeOptional(
                sourceEntityId);

        SourceEntityType =
            sourceEntityType
                .Trim()
                .ToUpperInvariant();

        RawStatement =
            rawStatement.Trim();

        ReadStatus =
            readStatus;

        _componentTypes =
            componentTypeList.AsReadOnly();

        _diagnostics =
            diagnosticList.AsReadOnly();

        _referencedEntityIds =
            referencedEntityIdList.AsReadOnly();
    }

    private static void EnsureComplexEntityState(
        string sourceEntityType,
        IReadOnlyCollection<string> componentTypes)
    {
        var normalizedType =
            sourceEntityType
                .Trim()
                .ToUpperInvariant();

        if (normalizedType == "COMPLEX_ENTITY" &&
            componentTypes.Count == 0)
        {
            throw new InvalidOperationException(
                "Complex STEP entity must contain at least one component type.");
        }

        if (normalizedType != "COMPLEX_ENTITY" &&
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
            diagnostics.Any(
                diagnostic =>
                    diagnostic.Severity is
                        CadDiagnosticSeverity.Error or
                        CadDiagnosticSeverity.Fatal);

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