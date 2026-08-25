using System;
using MPC.Contracts;

namespace MPC.Host.SolidWorks.Commands;

internal sealed class StepImportHostCommandResult
{
    internal StepImportHostCommandStatus Status { get; }

    internal WorkerOperationResult? WorkerResult { get; }

    internal string? FailureType { get; }

    internal string? FailureMessage { get; }

    internal bool IsSuccessful =>
        Status == StepImportHostCommandStatus.Succeeded;

    private StepImportHostCommandResult(
        StepImportHostCommandStatus status,
        WorkerOperationResult? workerResult,
        string? failureType,
        string? failureMessage)
    {
        if (!Enum.IsDefined(
                typeof(StepImportHostCommandStatus),
                status))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "STEP import host command status is not defined.");
        }

        if (status == StepImportHostCommandStatus.Succeeded)
        {
            if (workerResult is null)
            {
                throw new ArgumentException(
                    "A succeeded host command requires a worker result.",
                    nameof(workerResult));
            }

            if (workerResult.Status !=
                WorkerOperationStatus.Succeeded)
            {
                throw new ArgumentException(
                    "A succeeded host command requires a succeeded worker result.",
                    nameof(workerResult));
            }

            if (failureType is not null ||
                failureMessage is not null)
            {
                throw new ArgumentException(
                    "A succeeded host command cannot contain failure details.");
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(failureMessage))
            {
                throw new ArgumentException(
                    "A failed host command requires a failure message.",
                    nameof(failureMessage));
            }
        }

        Status = status;
        WorkerResult = workerResult;
        FailureType = NormalizeOptional(failureType);
        FailureMessage = NormalizeOptional(failureMessage);
    }

    internal static StepImportHostCommandResult Succeeded(
        WorkerOperationResult workerResult)
    {
        return new StepImportHostCommandResult(
            StepImportHostCommandStatus.Succeeded,
            workerResult
                ?? throw new ArgumentNullException(
                    nameof(workerResult)),
            failureType: null,
            failureMessage: null);
    }

    internal static StepImportHostCommandResult Failed(
        StepImportHostCommandStatus status,
        string failureMessage,
        string? failureType = null,
        WorkerOperationResult? workerResult = null)
    {
        if (status ==
            StepImportHostCommandStatus.Succeeded)
        {
            throw new ArgumentException(
                "Succeeded status cannot represent a failed host command.",
                nameof(status));
        }

        return new StepImportHostCommandResult(
            status,
            workerResult,
            failureType,
            failureMessage);
    }

    private static string? NormalizeOptional(
        string? value)
    {
        if (value is null ||
            string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
