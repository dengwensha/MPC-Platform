using System;
using System.Diagnostics;

namespace MPC.Host.SolidWorks.Worker;

internal sealed class SystemProcessExecution : IProcessExecution
{
    private readonly Process _process;

    internal SystemProcessExecution(Process process)
    {
        _process = process
            ?? throw new ArgumentNullException(nameof(process));
    }

    public int Id => _process.Id;

    public int ExitCode => _process.ExitCode;

    public bool WaitForExit(int milliseconds)
    {
        return _process.WaitForExit(milliseconds);
    }

    public void Dispose()
    {
        _process.Dispose();
    }
}
