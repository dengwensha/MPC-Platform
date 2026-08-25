using System;
using System.Collections.Generic;
using System.ComponentModel;
using MPC.Host.SolidWorks.Worker;
using Xunit;

namespace MPC.Host.SolidWorks.Tests.Worker;

public sealed class WorkerProcessCompletionObserverTests
{
    [Fact]
    public void Observe_RejectsNullHandle()
    {
        var observer =
            new WorkerProcessCompletionObserver();

        Assert.Throws<ArgumentNullException>(
            () => observer.Observe(
                null!,
                timeoutMilliseconds: 1000));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Observe_RejectsNonPositiveTimeout(
        int timeoutMilliseconds)
    {
        var execution =
            new RecordingProcessExecution();

        using var handle =
            new WorkerProcessHandle(execution);

        var observer =
            new WorkerProcessCompletionObserver();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => observer.Observe(
                handle,
                timeoutMilliseconds));

        Assert.Equal(0, execution.WaitCallCount);
        Assert.Equal(0, execution.DisposeCallCount);
    }

    [Fact]
    public void Observe_ReturnsSuccessfulCompletion_ForZeroExitCode()
    {
        var execution =
            new RecordingProcessExecution(
                exitCode: 0);

        var handle =
            new WorkerProcessHandle(execution);

        var observer =
            new WorkerProcessCompletionObserver();

        var result =
            observer.Observe(
                handle,
                timeoutMilliseconds: 2500);

        Assert.Equal(
            WorkerCompletionStatus.Completed,
            result.Status);

        Assert.True(result.IsSuccessful);
        Assert.Equal(0, result.ExitCode);
        Assert.Null(result.FailureType);
        Assert.Null(result.FailureMessage);
        Assert.Equal(
            new[] { 2500 },
            execution.WaitTimeouts);
        Assert.Equal(1, execution.ExitCodeReadCount);
        Assert.Equal(0, execution.TerminateCallCount);
        Assert.Equal(1, execution.DisposeCallCount);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(97)]
    public void Observe_PreservesNonZeroExitCode(
        int exitCode)
    {
        var execution =
            new RecordingProcessExecution(
                exitCode: exitCode);

        var observer =
            new WorkerProcessCompletionObserver();

        var result =
            observer.Observe(
                new WorkerProcessHandle(execution),
                timeoutMilliseconds: 1000);

        Assert.Equal(
            WorkerCompletionStatus.Completed,
            result.Status);

        Assert.False(result.IsSuccessful);
        Assert.Equal(exitCode, result.ExitCode);
        Assert.Equal(0, execution.TerminateCallCount);
        Assert.Equal(1, execution.DisposeCallCount);
    }

    [Fact]
    public void Observe_TerminatesAndWaits_WhenProcessTimesOut()
    {
        var execution =
            new RecordingProcessExecution(
                initialWaitResult: false,
                terminationWaitResult: true);

        var observer =
            new WorkerProcessCompletionObserver();

        var result =
            observer.Observe(
                new WorkerProcessHandle(execution),
                timeoutMilliseconds: 3000);

        Assert.Equal(
            WorkerCompletionStatus.TimedOut,
            result.Status);

        Assert.False(result.IsSuccessful);
        Assert.Null(result.ExitCode);
        Assert.Null(result.FailureType);
        Assert.False(
            string.IsNullOrWhiteSpace(
                result.FailureMessage));

        Assert.Equal(
            new[] { 3000, 5000 },
            execution.WaitTimeouts);
        Assert.Equal(0, execution.ExitCodeReadCount);
        Assert.Equal(1, execution.TerminateCallCount);
        Assert.Equal(1, execution.DisposeCallCount);
    }

    [Fact]
    public void Observe_PreservesTimeout_WhenTerminationFails()
    {
        var execution =
            new RecordingProcessExecution(
                initialWaitResult: false,
                terminateException:
                    new Win32Exception(
                        "Termination failed."));

        var observer =
            new WorkerProcessCompletionObserver();

        var result =
            observer.Observe(
                new WorkerProcessHandle(execution),
                timeoutMilliseconds: 3000);

        Assert.Equal(
            WorkerCompletionStatus.TimedOut,
            result.Status);

        Assert.False(result.IsSuccessful);
        Assert.Null(result.ExitCode);
        Assert.Null(result.FailureType);
        Assert.False(
            string.IsNullOrWhiteSpace(
                result.FailureMessage));

        Assert.Equal(
            new[] { 3000 },
            execution.WaitTimeouts);
        Assert.Equal(1, execution.TerminateCallCount);
        Assert.Equal(0, execution.ExitCodeReadCount);
        Assert.Equal(1, execution.DisposeCallCount);
    }

    [Fact]
    public void Observe_PreservesTimeout_WhenTerminationWaitExpires()
    {
        var execution =
            new RecordingProcessExecution(
                initialWaitResult: false,
                terminationWaitResult: false);

        var observer =
            new WorkerProcessCompletionObserver();

        var result =
            observer.Observe(
                new WorkerProcessHandle(execution),
                timeoutMilliseconds: 3000);

        Assert.Equal(
            WorkerCompletionStatus.TimedOut,
            result.Status);

        Assert.False(result.IsSuccessful);
        Assert.Null(result.ExitCode);
        Assert.Equal(
            new[] { 3000, 5000 },
            execution.WaitTimeouts);
        Assert.Equal(1, execution.TerminateCallCount);
        Assert.Equal(1, execution.DisposeCallCount);
    }

    [Fact]
    public void Observe_ReturnsObservationFailed_WhenWaitThrows()
    {
        var execution =
            new RecordingProcessExecution(
                waitException:
                    new InvalidOperationException(
                        "Wait failed."));

        var observer =
            new WorkerProcessCompletionObserver();

        var result =
            observer.Observe(
                new WorkerProcessHandle(execution),
                timeoutMilliseconds: 1000);

        Assert.Equal(
            WorkerCompletionStatus.ObservationFailed,
            result.Status);

        Assert.Equal(
            nameof(InvalidOperationException),
            result.FailureType);

        Assert.Equal(
            "Wait failed.",
            result.FailureMessage);

        Assert.Null(result.ExitCode);
        Assert.Equal(0, execution.ExitCodeReadCount);
        Assert.Equal(0, execution.TerminateCallCount);
        Assert.Equal(1, execution.DisposeCallCount);
    }

    [Fact]
    public void Observe_ReturnsObservationFailed_WhenExitCodeReadThrows()
    {
        var execution =
            new RecordingProcessExecution(
                exitCodeException:
                    new InvalidOperationException(
                        "Exit code unavailable."));

        var observer =
            new WorkerProcessCompletionObserver();

        var result =
            observer.Observe(
                new WorkerProcessHandle(execution),
                timeoutMilliseconds: 1000);

        Assert.Equal(
            WorkerCompletionStatus.ObservationFailed,
            result.Status);

        Assert.Equal(
            nameof(InvalidOperationException),
            result.FailureType);

        Assert.Equal(
            "Exit code unavailable.",
            result.FailureMessage);

        Assert.Equal(1, execution.ExitCodeReadCount);
        Assert.Equal(0, execution.TerminateCallCount);
        Assert.Equal(1, execution.DisposeCallCount);
    }

    [Fact]
    public void Observe_DoesNotAllowSecondObservation()
    {
        var execution =
            new RecordingProcessExecution();

        var handle =
            new WorkerProcessHandle(execution);

        var observer =
            new WorkerProcessCompletionObserver();

        var first =
            observer.Observe(
                handle,
                timeoutMilliseconds: 1000);

        var second =
            observer.Observe(
                handle,
                timeoutMilliseconds: 1000);

        Assert.Equal(
            WorkerCompletionStatus.Completed,
            first.Status);

        Assert.Equal(
            WorkerCompletionStatus.ObservationFailed,
            second.Status);

        Assert.Equal(
            nameof(ObjectDisposedException),
            second.FailureType);

        Assert.Equal(1, execution.WaitCallCount);
        Assert.Equal(1, execution.DisposeCallCount);
    }

    [Fact]
    public void Handle_RejectsInvalidProcessIdentifier_AndDisposesExecution()
    {
        var execution =
            new RecordingProcessExecution(
                processId: 0);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new WorkerProcessHandle(execution));

        Assert.Equal(1, execution.DisposeCallCount);
    }

    private sealed class RecordingProcessExecution :
        IProcessExecution
    {
        private readonly int _processId;
        private readonly int _exitCode;
        private readonly bool _initialWaitResult;
        private readonly bool _terminationWaitResult;
        private readonly Exception? _waitException;
        private readonly Exception? _exitCodeException;
        private readonly Exception? _terminateException;
        private readonly List<int> _waitTimeouts =
            new List<int>();

        internal int WaitCallCount =>
            _waitTimeouts.Count;

        internal IReadOnlyList<int> WaitTimeouts =>
            _waitTimeouts;

        internal int ExitCodeReadCount { get; private set; }

        internal int TerminateCallCount { get; private set; }

        internal int DisposeCallCount { get; private set; }

        internal RecordingProcessExecution(
            int processId = 4172,
            int exitCode = 0,
            bool initialWaitResult = true,
            bool terminationWaitResult = true,
            Exception? waitException = null,
            Exception? exitCodeException = null,
            Exception? terminateException = null)
        {
            _processId = processId;
            _exitCode = exitCode;
            _initialWaitResult = initialWaitResult;
            _terminationWaitResult = terminationWaitResult;
            _waitException = waitException;
            _exitCodeException = exitCodeException;
            _terminateException = terminateException;
        }

        public int Id => _processId;

        public int ExitCode
        {
            get
            {
                ExitCodeReadCount++;

                if (_exitCodeException is not null)
                {
                    throw _exitCodeException;
                }

                return _exitCode;
            }
        }

        public bool WaitForExit(int milliseconds)
        {
            _waitTimeouts.Add(milliseconds);

            if (_waitException is not null)
            {
                throw _waitException;
            }

            return _waitTimeouts.Count == 1
                ? _initialWaitResult
                : _terminationWaitResult;
        }

        public void Terminate()
        {
            TerminateCallCount++;

            if (_terminateException is not null)
            {
                throw _terminateException;
            }
        }

        public void Dispose()
        {
            DisposeCallCount++;
        }
    }
}
