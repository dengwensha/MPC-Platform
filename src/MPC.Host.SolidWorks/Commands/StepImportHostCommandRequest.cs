using System;

namespace MPC.Host.SolidWorks.Commands;

internal sealed class StepImportHostCommandRequest
{
    internal string? WorkerExecutablePath { get; }

    internal string? StepFilePath { get; }

    internal int TimeoutMilliseconds { get; }

    internal StepImportHostCommandRequest(
        string? workerExecutablePath,
        string? stepFilePath,
        int timeoutMilliseconds)
    {
        WorkerExecutablePath = workerExecutablePath;
        StepFilePath = stepFilePath;
        TimeoutMilliseconds = timeoutMilliseconds;
    }
}
