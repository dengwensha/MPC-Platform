using System;

namespace MPC.Host.SolidWorks.Commands;

// The existing add-in callback containment boundary, testable without COM.
internal sealed class RegionSourceValidationCallback
{
    private readonly HostFailureLog _failureLog;

    internal RegionSourceValidationCallback(HostFailureLog failureLog)
    {
        _failureLog = failureLog ?? throw new ArgumentNullException(nameof(failureLog));
    }

    internal string? RequestId { get; set; }

    internal void Run(Action action, Action showFailure)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));
        if (showFailure is null) throw new ArgumentNullException(nameof(showFailure));
        RequestId = null;
        try { action(); }
        catch (Exception exception)
        {
            _failureLog.TryWriteUnexpected(RequestId, exception);
            try { showFailure(); }
            catch { /* Preserve the existing COM callback containment behavior. */ }
        }
    }
}
