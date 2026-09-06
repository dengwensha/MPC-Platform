using MPC.Connectors.Diagnostics;
using MPC.Connectors.Results;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Semantics;
using MPC.Connectors.Step.Validation;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology.Primitives;

namespace MPC.Application.Step;

public sealed class StepImportApplicationService
{
    public const string MissingRequestCode =
        "APP-STEP-REQUEST-001";

    public const string MissingInventoryCode =
        "APP-STEP-INVENTORY-001";

    public const string InvalidSolidRootCode =
        "APP-STEP-ROOT-001";

    public const string MissingMappedSolidCode =
        "APP-STEP-MAPPING-001";

    public const string SemanticFailureCode =
        "APP-STEP-SEMANTIC-001";

    public const string UnexpectedFailureCode =
        "APP-STEP-UNEXPECTED-001";

    public StepImportResult Execute(
        StepImportRequest? request)
    {
        if (request is null)
        {
            return Failure(
                MissingRequestCode,
                "STEP import request is required.");
        }

        try
        {
            return ExecuteCore(request);
        }
        catch (StepSemanticValueException exception)
        {
            string sourceEntityId =
                string.IsNullOrWhiteSpace(
                    exception.SourceEntityId)
                    ? "<unknown>"
                    : exception.SourceEntityId;

            return StepImportResult.Failure(
                new[]
                {
                    new StepImportDiagnostic(
                        SemanticFailureCode,
                        StepImportDiagnosticSeverity.Error,
                        $"STEP semantic entity " +
                        $"'{sourceEntityId}' " +
                        $"of type " +
                        $"'{exception.SourceEntityType}' " +
                        $"rejected parameter " +
                        $"'{exception.ParameterName}' " +
                        $"with value " +
                        $"'{exception.RejectedValue}'.",
                        exception.SourceEntityId)
                });
        }
        catch (Exception exception)
        {
            return StepImportResult.Failure(
                new[]
                {
                    new StepImportDiagnostic(
                        UnexpectedFailureCode,
                        StepImportDiagnosticSeverity.Error,
                        $"STEP import failed unexpectedly: " +
                        $"{exception.GetType().Name}.")
                });
        }
    }

    private static StepImportResult ExecuteCore(
        StepImportRequest request)
    {
        var diagnostics =
            new List<StepImportDiagnostic>();

        var dataReader =
            new StepDataEntityReader(
                new StepDataInventoryValidator());

        CadConnectorResult<StepDataInventory> dataResult =
            dataReader.Read(
                request.InputFilePath);

        diagnostics.AddRange(
            dataResult.Diagnostics.Select(
                ConvertConnectorDiagnostic));

        if (dataResult.Status == CadConnectorResultStatus.Failed)
        {
            return StepImportResult.Failure(
                EnsureFailureDiagnostics(
                    diagnostics,
                    MissingInventoryCode,
                    "STEP data inventory could not be created."));
        }

        StepDataInventory? inventory =
            dataResult.Value;

        if (inventory is null)
        {
            diagnostics.Add(
                new StepImportDiagnostic(
                    MissingInventoryCode,
                    StepImportDiagnosticSeverity.Error,
                    "STEP data reader completed without an inventory."));

            return StepImportResult.Failure(
                diagnostics);
        }

        SwStepSignedToroidalCompatibilityPolicy?
            signedToroidalCompatibilityPolicy =
                TryCreateSignedToroidalCompatibilityPolicy(
                    request.InputFilePath);

        var semanticReader =
            new StepSemanticEntityReader(
                new StepParameterParser(),
                StepSemanticHandlerCatalog.Create(
                    signedToroidalCompatibilityPolicy));

        var semanticEntities =
            new List<StepSemanticEntity>();

        foreach (StepDataEntityRecord record in inventory.Entities)
        {
            StepSemanticEntity? semanticEntity =
                semanticReader.Read(record);

            if (semanticEntity is not null)
            {
                semanticEntities.Add(
                    semanticEntity);
            }
        }

        StepManifoldSolidBrepEntity[] solidRoots =
            semanticEntities
                .OfType<StepManifoldSolidBrepEntity>()
                .OrderBy(
                    entity => entity.SourceEntityId,
                    StringComparer.Ordinal)
                .ToArray();

        if (solidRoots.Length != 1)
        {
            diagnostics.Add(
                new StepImportDiagnostic(
                    InvalidSolidRootCode,
                    StepImportDiagnosticSeverity.Error,
                    $"STEP import requires exactly one " +
                    $"MANIFOLD_SOLID_BREP entity; found " +
                    $"{solidRoots.Length}."));

            return StepImportResult.Failure(
                diagnostics);
        }

        StepManifoldSolidBrepEntity solidRoot =
            solidRoots[0];

        var semanticResolver =
            new StepSemanticEntityResolver(
                semanticEntities);

        StepCanonicalMappingOperation operation =
            StepCanonicalMappingOperation.Create(
                semanticResolver);

        MappingResult<Solid> mappingResult =
            operation.Map(
                solidRoot);

        diagnostics.AddRange(
            mappingResult.Diagnostics.Select(
                ConvertMappingDiagnostic));

        if (!mappingResult.IsSuccessful)
        {
            return StepImportResult.Failure(
                EnsureFailureDiagnostics(
                    diagnostics,
                    MissingMappedSolidCode,
                    "Canonical STEP solid mapping failed."));
        }

        Solid? solid =
            mappingResult.Value;

        if (solid is null)
        {
            diagnostics.Add(
                new StepImportDiagnostic(
                    MissingMappedSolidCode,
                    StepImportDiagnosticSeverity.Error,
                    "Canonical STEP mapping completed without a solid."));

            return StepImportResult.Failure(
                diagnostics);
        }

        var assembler =
            new CanonicalTopologyAssembler();

        CanonicalTopologyAssemblyResult assemblyResult =
            assembler.Assemble(
                solid);

        string sourceSolidEntityId =
            string.IsNullOrWhiteSpace(
                solidRoot.SourceEntityId)
                ? "MANIFOLD_SOLID_BREP"
                : solidRoot.SourceEntityId;

        var summary =
            new StepImportSummary(
                sourceSolidEntityId,
                solidCount: 1,
                outerShellCount: 1,
                innerShellCount: solid.InnerShells.Count,
                faceCount: assemblyResult.Topology.Faces.Count,
                loopCount: assemblyResult.Topology.Loops.Count,
                edgeCount: assemblyResult.Topology.Edges.Count,
                vertexCount: assemblyResult.Topology.Vertices.Count);

        return diagnostics.Count == 0
            ? StepImportResult.Success(summary)
            : StepImportResult.SuccessWithDiagnostics(
                summary,
                diagnostics);
    }

    private static SwStepSignedToroidalCompatibilityPolicy?
        TryCreateSignedToroidalCompatibilityPolicy(
            string inputFilePath)
    {
        var headerReader =
            new StepHeaderReader();

        CadConnectorResult<StepHeaderInfo> headerResult =
            headerReader.Read(
                inputFilePath);

        if (
            headerResult.Status ==
                CadConnectorResultStatus.Failed ||
            headerResult.Value is null)
        {
            return null;
        }

        return new SwStepSignedToroidalCompatibilityPolicy(
            headerResult.Value);
    }

    private static StepImportDiagnostic ConvertConnectorDiagnostic(
        CadDiagnostic diagnostic)
    {
        StepImportDiagnosticSeverity severity =
            diagnostic.Severity switch
            {
                CadDiagnosticSeverity.Info =>
                    StepImportDiagnosticSeverity.Information,

                CadDiagnosticSeverity.Warning =>
                    StepImportDiagnosticSeverity.Warning,

                CadDiagnosticSeverity.Error =>
                    StepImportDiagnosticSeverity.Error,

                CadDiagnosticSeverity.Fatal =>
                    StepImportDiagnosticSeverity.Error,

                _ =>
                    StepImportDiagnosticSeverity.Error
            };

        return new StepImportDiagnostic(
            diagnostic.Code,
            severity,
            diagnostic.Message,
            diagnostic.SourceEntityId);
    }

    private static StepImportDiagnostic ConvertMappingDiagnostic(
        MappingDiagnostic diagnostic)
    {
        StepImportDiagnosticSeverity severity =
            diagnostic.Severity switch
            {
                MappingDiagnosticSeverity.Information =>
                    StepImportDiagnosticSeverity.Information,

                MappingDiagnosticSeverity.Warning =>
                    StepImportDiagnosticSeverity.Warning,

                MappingDiagnosticSeverity.Error =>
                    StepImportDiagnosticSeverity.Error,

                _ =>
                    StepImportDiagnosticSeverity.Error
            };

        return new StepImportDiagnostic(
            diagnostic.Code,
            severity,
            diagnostic.Message);
    }

    private static IReadOnlyList<StepImportDiagnostic>
        EnsureFailureDiagnostics(
            IReadOnlyCollection<StepImportDiagnostic> diagnostics,
            string fallbackCode,
            string fallbackMessage)
    {
        if (diagnostics.Count > 0)
        {
            return diagnostics.ToArray();
        }

        return new[]
        {
            new StepImportDiagnostic(
                fallbackCode,
                StepImportDiagnosticSeverity.Error,
                fallbackMessage)
        };
    }

    private static StepImportResult Failure(
        string code,
        string message)
    {
        return StepImportResult.Failure(
            new[]
            {
                new StepImportDiagnostic(
                    code,
                    StepImportDiagnosticSeverity.Error,
                    message)
            });
    }
}
