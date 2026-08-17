using System;

namespace MPC.Host.SolidWorks.Worker;

internal sealed class WorkerCompletionResult
{
    internal WorkerCompletionStatus Status { get; }
    internal int? ExitCode { get; }
    internal string? FailureType { get; }
    internal string? FailureMessage { get; }

    internal bool IsSuccessful =>
        Status == WorkerCompletionStatus.Completed && ExitCode == 0;

    private WorkerCompletionResult(
        WorkerCompletionStatus status,
        int? exitCode,
        string? failureType,
        string? failureMessage)
    {
        if (!Enum.IsDefined(typeof(WorkerCompletionStatus), status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status), status, "Worker completion status is not defined.");
        }

        if (status == WorkerCompletionStatus.Completed)
        {
            if (!exitCode.HasValue)
            {
                throw new ArgumentException(
                    "A completed worker requires an exit code.",
                    nameof(exitCode));
            }

            if (failureType is not null || failureMessage is not null)
            {
                throw new ArgumentException(
                    "A completed worker cannot contain observation failure details.");
            }
        }
        else
        {
            if (exitCode.HasValue)
            {
                throw new ArgumentException(
                    "An incomplete observation cannot contain an exit code.",
                    nameof(exitCode));
            }

            if (string.IsNullOrWhiteSpace(failureMessage))
            {
                throw new ArgumentException(
                    "An incomplete observation requires a message.",
                    nameof(failureMessage));
            }
        }

        Status = status;
        ExitCode = exitCode;
        FailureType = NormalizeOptional(failureType);
        FailureMessage = NormalizeOptional(failureMessage);
    }

    internal static WorkerCompletionResult Completed(int exitCode)
    {
        return new WorkerCompletionResult(
            WorkerCompletionStatus.Completed, exitCode, null, null);
    }

    internal static WorkerCompletionResult TimedOut(string message)
    {
        return new WorkerCompletionResult(
            WorkerCompletionStatus.TimedOut, null, null, message);
    }

    internal static WorkerCompletionResult ObservationFailed(Exception exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        var message = string.IsNullOrWhiteSpace(exception.Message)
            ? "Worker process completion could not be observed."
            : exception.Message;

        return new WorkerCompletionResult(
            WorkerCompletionStatus.ObservationFailed,
            null,
            exception.GetType().Name,
            message);
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
