namespace MPC.Host.SolidWorks.Worker;

internal enum WorkerLaunchStatus
{
    Started = 0,
    InvalidRequest = 1,
    ExecutableNotFound = 2,
    RequestFileNotFound = 3,
    StartFailed = 4
}
