namespace MPC.Host.SolidWorks.Worker;

internal interface IWorkerProcessLauncher
{
    WorkerLaunchResult Launch(
        WorkerLaunchRequest? request);
}
