using MPC.Contracts;
using MPC.Contracts.Validation;

namespace MPC.Worker;

public sealed class WorkerRequestProcessor
{
    public const string OperationNotImplementedCode =
        "WORKER-OPERATION-001";

    public WorkerOperationResult Process(
        WorkerOperationRequest? request)
    {
        var validation =
            WorkerOperationRequestValidator.Validate(request);

        if (!validation.IsValid)
        {
            return new WorkerOperationResult(
                ContractVersion.Current,
                request?.RequestId,
                WorkerOperationStatus.Rejected,
                outputArtifactPath: null,
                validation.Diagnostics);
        }

        return new WorkerOperationResult(
            ContractVersion.Current,
            request!.RequestId,
            WorkerOperationStatus.Failed,
            outputArtifactPath: null,
            new[]
            {
                new ContractDiagnostic(
                    OperationNotImplementedCode,
                    ContractDiagnosticSeverity.Error,
                    "AnalyzeModel operation is not implemented.")
            });
    }
}
