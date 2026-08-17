using System;

namespace MPC.Host.SolidWorks.Worker;

internal sealed class WorkerLaunchResult
{
    internal WorkerLaunchStatus Status { get; }
    internal WorkerProcessHandle? Handle { get; }
    internal string? FailureType { get; }
    internal string? FailureMessage { get; }
    internal bool IsStarted => Status == WorkerLaunchStatus.Started;

    private WorkerLaunchResult(
        WorkerLaunchStatus status,
        WorkerProcessHandle? handle,
        string? failureType,
        string? failureMessage)
    {
        if (!Enum.IsDefined(typeof(WorkerLaunchStatus), status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status), status, "Worker launch status is not defined.");
        }

        if (status == WorkerLaunchStatus.Started)
        {
            if (handle is null)
            {
                throw new ArgumentException(
                    "A started worker launch requires a process handle.",
                    nameof(handle));
            }

            if (failureType is not null || failureMessage is not null)
            {
                throw new ArgumentException(
                    "A started worker launch cannot contain failure details.");
            }
        }
        else
        {
            if (handle is not null)
            {
                throw new ArgumentException(
                    "A failed worker launch cannot contain a process handle.",
                    nameof(handle));
            }

            if (string.IsNullOrWhiteSpace(failureMessage))
            {
                throw new ArgumentException(
                    "A failed worker launch requires a failure message.",
                    nameof(failureMessage));
            }
        }

        Status = status;
        Handle = handle;
        FailureType = NormalizeOptional(failureType);
        FailureMessage = NormalizeOptional(failureMessage);
    }

    internal static WorkerLaunchResult Started(WorkerProcessHandle handle)
    {
        return new WorkerLaunchResult(
            WorkerLaunchStatus.Started,
            handle ?? throw new ArgumentNullException(nameof(handle)),
            null,
            null);
    }

    internal static WorkerLaunchResult Failed(
        WorkerLaunchStatus status,
        string failureMessage,
        string? failureType = null)
    {
        if (status == WorkerLaunchStatus.Started)
        {
            throw new ArgumentException(
                "Started status cannot represent a failed launch.",
                nameof(status));
        }

        return new WorkerLaunchResult(
            status, null, failureType, failureMessage);
    }

    private static string? NormalizeOptional(string? value)
    {
        if (value is null ||
            string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
