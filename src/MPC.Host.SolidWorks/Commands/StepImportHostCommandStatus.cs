namespace MPC.Host.SolidWorks.Commands;

internal enum StepImportHostCommandStatus
{
    Succeeded = 0,
    InvalidRequest = 1,
    TransportFailed = 2,
    LaunchFailed = 3,
    TimedOut = 4,
    ObservationFailed = 5,
    WorkerFailed = 6,
    ResultReadFailed = 7
}
