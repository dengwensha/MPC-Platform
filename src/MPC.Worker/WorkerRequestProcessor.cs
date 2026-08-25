using MPC.Application.Step;
using MPC.Contracts;
using MPC.Contracts.Validation;

namespace MPC.Worker;

public sealed class WorkerRequestProcessor
{
    private readonly StepImportApplicationService
        _stepImportService;

    public WorkerRequestProcessor()
        : this(
            new StepImportApplicationService())
    {
    }

    public WorkerRequestProcessor(
        StepImportApplicationService stepImportService)
    {
        _stepImportService =
            stepImportService
            ?? throw new ArgumentNullException(
                nameof(stepImportService));
    }

    public WorkerOperationResult Process(
        WorkerOperationRequest? request)
    {
        var validation =
            WorkerOperationRequestValidator.Validate(
                request);

        if (!validation.IsValid)
        {
            return new WorkerOperationResult(
                ContractVersion.Current,
                request?.RequestId,
                WorkerOperationStatus.Rejected,
                outputArtifactPath: null,
                validation.Diagnostics);
        }

        StepImportResult applicationResult =
            _stepImportService.Execute(
                new StepImportRequest(
                    request!.InputArtifactPath!));

        ContractDiagnostic[] diagnostics =
            applicationResult.Diagnostics
                .Select(ConvertDiagnostic)
                .ToArray();

        WorkerOperationSummary? summary =
            applicationResult.Summary is null
                ? null
                : ConvertSummary(
                    applicationResult.Summary);

        WorkerOperationStatus status =
            applicationResult.Status switch
            {
                StepImportStatus.Succeeded =>
                    WorkerOperationStatus.Succeeded,

                StepImportStatus.SucceededWithDiagnostics =>
                    WorkerOperationStatus.Succeeded,

                StepImportStatus.Failed =>
                    WorkerOperationStatus.Failed,

                _ =>
                    WorkerOperationStatus.Failed
            };

        return new WorkerOperationResult(
            ContractVersion.Current,
            request.RequestId,
            status,
            outputArtifactPath: null,
            diagnostics,
            summary);
    }

    private static ContractDiagnostic ConvertDiagnostic(
        StepImportDiagnostic diagnostic)
    {
        ContractDiagnosticSeverity severity =
            diagnostic.Severity switch
            {
                StepImportDiagnosticSeverity.Information =>
                    ContractDiagnosticSeverity.Information,

                StepImportDiagnosticSeverity.Warning =>
                    ContractDiagnosticSeverity.Warning,

                StepImportDiagnosticSeverity.Error =>
                    ContractDiagnosticSeverity.Error,

                _ =>
                    ContractDiagnosticSeverity.Error
            };

        return new ContractDiagnostic(
            diagnostic.Code,
            severity,
            diagnostic.Message);
    }

    private static WorkerOperationSummary ConvertSummary(
        StepImportSummary summary)
    {
        return new WorkerOperationSummary(
            summary.SourceSolidEntityId,
            summary.SolidCount,
            summary.OuterShellCount,
            summary.InnerShellCount,
            summary.FaceCount,
            summary.LoopCount,
            summary.EdgeCount,
            summary.VertexCount);
    }
}
