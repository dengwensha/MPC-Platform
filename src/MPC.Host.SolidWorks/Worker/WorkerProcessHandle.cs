using System;

namespace MPC.Host.SolidWorks.Worker;

internal sealed class WorkerProcessHandle : IDisposable
{
    private readonly IProcessExecution _execution;
    private bool _isConsumed;
    private bool _isDisposed;

    internal int ProcessId { get; }

    internal WorkerProcessHandle(IProcessExecution execution)
    {
        _execution = execution
            ?? throw new ArgumentNullException(nameof(execution));

        ProcessId = execution.Id;

        if (ProcessId <= 0)
        {
            execution.Dispose();
            throw new ArgumentOutOfRangeException(
                nameof(execution),
                ProcessId,
                "Worker process identifier must be positive.");
        }
    }

    internal void BeginObservation()
    {
        ThrowIfDisposed();

        if (_isConsumed)
        {
            throw new InvalidOperationException(
                "Worker process handle has already been observed.");
        }

        _isConsumed = true;
    }

    internal bool WaitForExit(int milliseconds)
    {
        ThrowIfDisposed();
        return _execution.WaitForExit(milliseconds);
    }

    internal int GetExitCode()
    {
        ThrowIfDisposed();
        return _execution.ExitCode;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _execution.Dispose();
        _isDisposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(WorkerProcessHandle));
        }
    }
}
