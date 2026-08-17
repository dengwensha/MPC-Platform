using System;

namespace MPC.Host.SolidWorks.Worker;

internal interface IProcessExecution : IDisposable
{
    int Id { get; }

    int ExitCode { get; }

    bool WaitForExit(int milliseconds);
}
