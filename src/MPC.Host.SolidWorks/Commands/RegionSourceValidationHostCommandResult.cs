using MPC.Contracts;

namespace MPC.Host.SolidWorks.Commands;

internal enum RegionSourceHostStatus
{
    InvalidRequest = 0,
    SourceLinkMatched = 1,
    Blocked = 2,
    LaunchFailed = 3,
    TimedOut = 4,
    ObservationFailed = 5,
    ResultReadFailed = 6,
    WorkerFailed = 7,
    TransportFailed = 8
}

internal sealed class RegionSourceValidationHostCommandResult
{
    internal RegionSourceHostStatus Status { get; }
    internal string? FailureMessage { get; }
    internal RegionSourceValidationResult? WorkerResult { get; }

    internal RegionSourceValidationHostCommandResult(RegionSourceHostStatus status,
        string? failureMessage = null, RegionSourceValidationResult? workerResult = null)
    {
        Status = status;
        FailureMessage = failureMessage;
        WorkerResult = workerResult;
    }
}
