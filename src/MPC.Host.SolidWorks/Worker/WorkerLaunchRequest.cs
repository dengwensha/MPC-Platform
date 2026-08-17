namespace MPC.Host.SolidWorks.Worker;

internal sealed class WorkerLaunchRequest
{
    internal string? WorkerExecutablePath { get; }

    internal string? RequestJsonPath { get; }

    internal string? ResultJsonPath { get; }

    internal WorkerLaunchRequest(
        string? workerExecutablePath,
        string? requestJsonPath,
        string? resultJsonPath)
    {
        WorkerExecutablePath = workerExecutablePath;
        RequestJsonPath = requestJsonPath;
        ResultJsonPath = resultJsonPath;
    }
}
