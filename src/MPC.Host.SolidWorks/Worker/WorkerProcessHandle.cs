using System;

namespace MPC.Host.SolidWorks.Worker;

internal sealed class WorkerProcessHandle
{
    internal int ProcessId { get; }

    internal WorkerProcessHandle(int processId)
    {
        if (processId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(processId),
                processId,
                "Worker process identifier must be positive.");
        }

        ProcessId = processId;
    }
}
