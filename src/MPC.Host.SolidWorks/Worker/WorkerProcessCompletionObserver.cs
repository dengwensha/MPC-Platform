using System;
using System.ComponentModel;

namespace MPC.Host.SolidWorks.Worker;

internal sealed class WorkerProcessCompletionObserver :
    IWorkerProcessCompletionObserver
{
    private const int TerminationTimeoutMilliseconds = 5000;

    public WorkerCompletionResult Observe(
        WorkerProcessHandle handle,
        int timeoutMilliseconds)
    {
        if (handle is null)
        {
            throw new ArgumentNullException(nameof(handle));
        }

        if (timeoutMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeoutMilliseconds),
                timeoutMilliseconds,
                "Worker observation timeout must be positive.");
        }

        try
        {
            handle.BeginObservation();

            if (!handle.WaitForExit(timeoutMilliseconds))
            {
                TryTerminateAndWait(handle);

                return WorkerCompletionResult.TimedOut(
                    "Worker process did not complete within the observation timeout.");
            }

            return WorkerCompletionResult.Completed(handle.GetExitCode());
        }
        catch (Exception exception)
            when (exception is InvalidOperationException
                  or Win32Exception
                  or NotSupportedException)
        {
            return WorkerCompletionResult.ObservationFailed(exception);
        }
        finally
        {
            handle.Dispose();
        }
    }

    private static void TryTerminateAndWait(
        WorkerProcessHandle handle)
    {
        try
        {
            handle.Terminate();
            handle.WaitForExit(
                TerminationTimeoutMilliseconds);
        }
        catch (Exception exception)
            when (exception is InvalidOperationException
                  or Win32Exception
                  or NotSupportedException)
        {
            // Timeout remains the primary result. Termination is best-effort
            // cleanup and must not replace the observed timeout status.
        }
    }
}
