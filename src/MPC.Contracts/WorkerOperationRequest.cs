namespace MPC.Contracts;

public sealed class WorkerOperationRequest
{
    public string? ContractVersion { get; }

    public string? RequestId { get; }

    public WorkerOperationKind Operation { get; }

    public string? InputArtifactPath { get; }

    public WorkerOperationRequest(
        string? contractVersion,
        string? requestId,
        WorkerOperationKind operation,
        string? inputArtifactPath)
    {
        ContractVersion = contractVersion;
        RequestId = requestId;
        Operation = operation;
        InputArtifactPath = inputArtifactPath;
    }
}
