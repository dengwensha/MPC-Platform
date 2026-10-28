using System;
using System.IO;
using System.Text.Json;
using MPC.Contracts;
using MPC.Contracts.Validation;
using MPC.Host.SolidWorks.Worker;

namespace MPC.Host.SolidWorks.Commands;

// Separate from the existing v1.0 import command. A match is provenance only.
internal sealed class RegionSourceValidationHostCommand
{
    private readonly IWorkerProcessLauncher _launcher;
    private readonly IWorkerProcessCompletionObserver _observer;
    private readonly WorkerContractJsonSerializer _serializer;

    internal RegionSourceValidationHostCommand()
        : this(new WorkerProcessLauncher(), new WorkerProcessCompletionObserver(),
            new WorkerContractJsonSerializer()) { }

    internal RegionSourceValidationHostCommand(IWorkerProcessLauncher launcher,
        IWorkerProcessCompletionObserver observer, WorkerContractJsonSerializer serializer)
    {
        _launcher = launcher ?? throw new ArgumentNullException(nameof(launcher));
        _observer = observer ?? throw new ArgumentNullException(nameof(observer));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    internal RegionSourceValidationHostCommandResult Execute(string? workerExecutablePath,
        RegionSourceValidationRequest? request, int timeoutMilliseconds)
    {
        if (string.IsNullOrWhiteSpace(workerExecutablePath) || timeoutMilliseconds <= 0 ||
            !RegionSourceValidationRequestValidator.Validate(request).IsValid)
            return new RegionSourceValidationHostCommandResult(
                RegionSourceHostStatus.InvalidRequest, "Invalid region source host request.");

        string directory = Path.Combine(Path.GetTempPath(), "MPC", Guid.NewGuid().ToString("N"));
        string requestPath = Path.Combine(directory, "request.json");
        string resultPath = Path.Combine(directory, "result.json");
        try
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(requestPath, _serializer.SerializeRegionSourceRequest(request!));
            var launch = _launcher.Launch(new WorkerLaunchRequest(
                workerExecutablePath, requestPath, resultPath));
            if (!launch.IsStarted || launch.Handle is null)
                return new RegionSourceValidationHostCommandResult(
                    RegionSourceHostStatus.LaunchFailed, launch.FailureMessage);

            var completion = _observer.Observe(launch.Handle, timeoutMilliseconds);
            if (completion.Status == WorkerCompletionStatus.TimedOut)
                return new RegionSourceValidationHostCommandResult(
                    RegionSourceHostStatus.TimedOut, completion.FailureMessage);
            if (completion.Status != WorkerCompletionStatus.Completed)
                return new RegionSourceValidationHostCommandResult(
                    RegionSourceHostStatus.ObservationFailed, completion.FailureMessage);

            RegionSourceValidationResult result;
            try
            {
                result = _serializer.DeserializeRegionSourceResult(File.ReadAllText(resultPath));
            }
            catch (Exception exception) when (exception is IOException ||
                exception is UnauthorizedAccessException || exception is ArgumentException ||
                exception is NotSupportedException || exception is JsonException)
            {
                return new RegionSourceValidationHostCommandResult(
                    RegionSourceHostStatus.ResultReadFailed, exception.GetType().Name);
            }

            if (!RegionSourceValidationResultValidator.Validate(result, request!.RequestId).IsValid ||
                (result.Outcome == RegionSourceValidationOutcome.SourceLinkMatched &&
                 request.Requirement != RegionSourceValidationRequirement.SourceLinkOnly))
                return new RegionSourceValidationHostCommandResult(
                    RegionSourceHostStatus.ResultReadFailed, "Result did not match the request.", result);

            if (result.Outcome == RegionSourceValidationOutcome.SourceLinkMatched)
                return new RegionSourceValidationHostCommandResult(
                    completion.ExitCode == 0 ? RegionSourceHostStatus.SourceLinkMatched :
                        RegionSourceHostStatus.ResultReadFailed,
                    workerResult: result);

            if (result.Outcome == RegionSourceValidationOutcome.ImportFailed)
                return new RegionSourceValidationHostCommandResult(
                    completion.ExitCode == 3 ? RegionSourceHostStatus.WorkerFailed :
                        RegionSourceHostStatus.ResultReadFailed,
                    workerResult: result);

            // Known non-success outcomes must agree with the Worker's rejection exit.
            return new RegionSourceValidationHostCommandResult(
                completion.ExitCode == 2 ? RegionSourceHostStatus.Blocked :
                    RegionSourceHostStatus.ResultReadFailed,
                workerResult: result);
        }
        catch (Exception exception) when (exception is IOException ||
            exception is UnauthorizedAccessException || exception is ArgumentException ||
            exception is NotSupportedException)
        {
            return new RegionSourceValidationHostCommandResult(
                RegionSourceHostStatus.TransportFailed, exception.GetType().Name);
        }
        finally
        {
            try
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
            catch (Exception exception) when (exception is IOException ||
                exception is UnauthorizedAccessException) { }
        }
    }
}
