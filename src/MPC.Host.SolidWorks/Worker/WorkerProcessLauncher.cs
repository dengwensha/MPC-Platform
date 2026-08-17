using System;
using System.ComponentModel;
using System.IO;

namespace MPC.Host.SolidWorks.Worker;

internal sealed class WorkerProcessLauncher :
    IWorkerProcessLauncher
{
    private readonly IProcessStarter _processStarter;

    internal WorkerProcessLauncher()
        : this(new SystemProcessStarter())
    {
    }

    internal WorkerProcessLauncher(
        IProcessStarter processStarter)
    {
        _processStarter = processStarter
            ?? throw new ArgumentNullException(
                nameof(processStarter));
    }

    public WorkerLaunchResult Launch(
        WorkerLaunchRequest? request)
    {
        if (request is null)
        {
            return InvalidRequest(
                "Worker launch request is required.");
        }

        if (!TryNormalizePath(
                request.WorkerExecutablePath,
                out var executablePath))
        {
            return InvalidRequest(
                "Worker executable path is invalid.");
        }

        if (!TryNormalizePath(
                request.RequestJsonPath,
                out var requestJsonPath))
        {
            return InvalidRequest(
                "Worker request JSON path is invalid.");
        }

        if (!TryNormalizePath(
                request.ResultJsonPath,
                out var resultJsonPath))
        {
            return InvalidRequest(
                "Worker result JSON path is invalid.");
        }

        if (!File.Exists(executablePath))
        {
            return WorkerLaunchResult.Failed(
                WorkerLaunchStatus.ExecutableNotFound,
                "Worker executable was not found.");
        }

        if (!File.Exists(requestJsonPath))
        {
            return WorkerLaunchResult.Failed(
                WorkerLaunchStatus.RequestFileNotFound,
                "Worker request JSON file was not found.");
        }

        var workingDirectory =
            Path.GetDirectoryName(executablePath);

        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            return InvalidRequest(
                "Worker executable directory is invalid.");
        }

        var processRequest =
            new ProcessStartRequest(
                executablePath,
                new[]
                {
                    requestJsonPath,
                    resultJsonPath
                },
                workingDirectory);

        try
        {
            var handle =
                _processStarter.Start(processRequest);

            if (handle is null)
            {
                return WorkerLaunchResult.Failed(
                    WorkerLaunchStatus.StartFailed,
                    "Process starter did not return a worker process handle.");
            }

            return WorkerLaunchResult.Started(
                handle);
        }
        catch (Exception exception)
            when (exception is Win32Exception
                  or InvalidOperationException
                  or ArgumentException
                  or NotSupportedException
                  or UnauthorizedAccessException)
        {
            return WorkerLaunchResult.Failed(
                WorkerLaunchStatus.StartFailed,
                CreateFailureMessage(exception),
                exception.GetType().Name);
        }
    }

    private static bool TryNormalizePath(
        string? path,
        out string normalizedPath)
    {
        normalizedPath =
            string.Empty;

        if (path is null ||
            string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            normalizedPath =
                Path.GetFullPath(path.Trim());

            return true;
        }
        catch (Exception exception)
            when (exception is ArgumentException
                  or NotSupportedException
                  or PathTooLongException)
        {
            return false;
        }
    }

    private static WorkerLaunchResult InvalidRequest(
        string message)
    {
        return WorkerLaunchResult.Failed(
            WorkerLaunchStatus.InvalidRequest,
            message);
    }

    private static string CreateFailureMessage(
        Exception exception)
    {
        if (string.IsNullOrWhiteSpace(exception.Message))
        {
            return "Worker process could not be started.";
        }

        return exception.Message;
    }
}
