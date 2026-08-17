namespace MPC.Host.SolidWorks.Worker;

internal interface IWorkerProcessCompletionObserver
{
    WorkerCompletionResult Observe(
        WorkerProcessHandle handle,
        int timeoutMilliseconds);
}
