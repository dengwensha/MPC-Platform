using System;
using System.IO;
using MPC.Contracts;
using MPC.Host.SolidWorks.Worker;

namespace MPC.Host.SolidWorks.Commands;

internal sealed class StepImportHostCommand
{
    private readonly IWorkerProcessLauncher
        _launcher;

    private readonly IWorkerProcessCompletionObserver
        _completionObserver;

    private readonly WorkerContractJsonSerializer
        _serializer;

    internal StepImportHostCommand()
        : this(
            new WorkerProcessLauncher(),
            new WorkerProcessCompletionObserver(),
            new WorkerContractJsonSerializer())
    {
    }

    internal StepImportHostCommand(
        IWorkerProcessLauncher launcher,
        IWorkerProcessCompletionObserver completionObserver,
        WorkerContractJsonSerializer serializer)
    {
        _launcher =
            launcher
            ?? throw new ArgumentNullException(
                nameof(launcher));

        _completionObserver =
            completionObserver
            ?? throw new ArgumentNullException(
                nameof(completionObserver));

        _serializer =
            serializer
            ?? throw new ArgumentNullException(
                nameof(serializer));
    }

    internal StepImportHostCommandResult Execute(
        StepImportHostCommandRequest? request)
    {
        if (request is null)
        {
            return InvalidRequest(
                "STEP import host command request is required.");
        }

        if (string.IsNullOrWhiteSpace(
                request.WorkerExecutablePath))
        {
            return InvalidRequest(
                "Worker executable path is required.");
        }

        if (string.IsNullOrWhiteSpace(
                request.StepFilePath))
        {
            return InvalidRequest(
                "STEP input file path is required.");
        }

        if (request.TimeoutMilliseconds <= 0)
        {
            return InvalidRequest(
                "Worker timeout must be positive.");
        }

        string transportDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "MPC",
                Guid.NewGuid().ToString("N"));

        string requestJsonPath =
            Path.Combine(
                transportDirectory,
                "request.json");

        string resultJsonPath =
            Path.Combine(
                transportDirectory,
                "result.json");

        string requestId =
            Guid.NewGuid().ToString("D");

        try
        {
            Directory.CreateDirectory(
                transportDirectory);

            var workerRequest =
                new WorkerOperationRequest(
                    ContractVersion.Current,
                    requestId,
                    WorkerOperationKind.AnalyzeModel,
                    request.StepFilePath);

            string requestJson =
                _serializer.SerializeRequest(
                    workerRequest);

            File.WriteAllText(
                requestJsonPath,
                requestJson);

            WorkerLaunchResult launchResult =
                _launcher.Launch(
                    new WorkerLaunchRequest(
                        request.WorkerExecutablePath,
                        requestJsonPath,
                        resultJsonPath));

            if (!launchResult.IsStarted ||
                launchResult.Handle is null)
            {
                return StepImportHostCommandResult.Failed(
                    StepImportHostCommandStatus.LaunchFailed,
                    launchResult.FailureMessage
                        ?? "Worker process could not be started.",
                    launchResult.FailureType);
            }

            WorkerCompletionResult completionResult =
                _completionObserver.Observe(
                    launchResult.Handle,
                    request.TimeoutMilliseconds);

            if (completionResult.Status ==
                WorkerCompletionStatus.TimedOut)
            {
                return StepImportHostCommandResult.Failed(
                    StepImportHostCommandStatus.TimedOut,
                    completionResult.FailureMessage
                        ?? "Worker process timed out.",
                    completionResult.FailureType);
            }

            if (completionResult.Status ==
                WorkerCompletionStatus.ObservationFailed)
            {
                return StepImportHostCommandResult.Failed(
                    StepImportHostCommandStatus.ObservationFailed,
                    completionResult.FailureMessage
                        ?? "Worker completion could not be observed.",
                    completionResult.FailureType);
            }

            WorkerOperationResult workerResult;

            try
            {
                string resultJson =
                    File.ReadAllText(
                        resultJsonPath);

                workerResult =
                    _serializer.DeserializeResult(
                        resultJson);
            }
            catch (Exception exception)
                when (exception is IOException
                      || exception is UnauthorizedAccessException
                      || exception is ArgumentException
                      || exception is NotSupportedException
                      || exception is System.Text.Json.JsonException)
            {
                return StepImportHostCommandResult.Failed(
                    StepImportHostCommandStatus.ResultReadFailed,
                    "Worker result could not be read.",
                    exception.GetType().Name);
            }

            if (!string.Equals(
                    workerResult.ContractVersion,
                    ContractVersion.Current,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    workerResult.RequestId,
                    requestId,
                    StringComparison.Ordinal))
            {
                return StepImportHostCommandResult.Failed(
                    StepImportHostCommandStatus.ResultReadFailed,
                    "Worker result identity did not match the request.",
                    workerResult: workerResult);
            }

            if (completionResult.ExitCode != 0 ||
                workerResult.Status !=
                WorkerOperationStatus.Succeeded)
            {
                return StepImportHostCommandResult.Failed(
                    StepImportHostCommandStatus.WorkerFailed,
                    "Worker STEP import operation failed.",
                    workerResult: workerResult);
            }

            return StepImportHostCommandResult.Succeeded(
                workerResult);
        }
        catch (Exception exception)
            when (exception is IOException
                  || exception is UnauthorizedAccessException
                  || exception is ArgumentException
                  || exception is NotSupportedException
                  || exception is PathTooLongException)
        {
            return StepImportHostCommandResult.Failed(
                StepImportHostCommandStatus.TransportFailed,
                "Worker transport could not be prepared.",
                exception.GetType().Name);
        }
        finally
        {
            TryDeleteTransportDirectory(
                transportDirectory);
        }
    }

    private static StepImportHostCommandResult InvalidRequest(
        string message)
    {
        return StepImportHostCommandResult.Failed(
            StepImportHostCommandStatus.InvalidRequest,
            message);
    }

    private static void TryDeleteTransportDirectory(
        string directoryPath)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(
                    directoryPath,
                    recursive: true);
            }
        }
        catch (Exception exception)
            when (exception is IOException
                  || exception is UnauthorizedAccessException)
        {
        }
    }
}
