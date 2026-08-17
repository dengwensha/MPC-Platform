namespace MPC.Host.SolidWorks.Worker;

internal interface IProcessStarter
{
    WorkerProcessHandle Start(ProcessStartRequest request);
}
