using System;
using System.IO;
using MPC.Host.SolidWorks.Worker;
using Xunit;

namespace MPC.Host.SolidWorks.Tests.Worker;

public sealed class SystemProcessCompletionIntegrationTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void StarterAndObserver_PreserveRealProcessExitCode(
        int expectedExitCode)
    {
        using var scope =
            new TemporaryCommandScope(
                expectedExitCode);

        var starter =
            new SystemProcessStarter();

        var handle =
            starter.Start(
                new ProcessStartRequest(
                    scope.CommandInterpreterPath,
                    new[]
                    {
                        "/d",
                        "/c",
                        scope.BatchFilePath
                    },
                    scope.DirectoryPath));

        Assert.True(handle.ProcessId > 0);

        var observer =
            new WorkerProcessCompletionObserver();

        var result =
            observer.Observe(
                handle,
                timeoutMilliseconds: 5000);

        Assert.Equal(
            WorkerCompletionStatus.Completed,
            result.Status);

        Assert.Equal(
            expectedExitCode,
            result.ExitCode);

        Assert.Equal(
            expectedExitCode == 0,
            result.IsSuccessful);

        Assert.Null(result.FailureType);
        Assert.Null(result.FailureMessage);
    }

    private sealed class TemporaryCommandScope :
        IDisposable
    {
        internal string DirectoryPath { get; }

        internal string BatchFilePath { get; }

        internal string CommandInterpreterPath { get; }

        internal TemporaryCommandScope(
            int exitCode)
        {
            DirectoryPath =
                Path.Combine(
                    Path.GetTempPath(),
                    "MPC.SystemProcess.Tests",
                    Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(
                DirectoryPath);

            BatchFilePath =
                Path.Combine(
                    DirectoryPath,
                    "exit.cmd");

            File.WriteAllText(
                BatchFilePath,
                "@exit /b " + exitCode);

            CommandInterpreterPath =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.System),
                    "cmd.exe");

            if (!File.Exists(CommandInterpreterPath))
            {
                throw new InvalidOperationException(
                    "Windows command interpreter was not found.");
            }
        }

        public void Dispose()
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(
                    DirectoryPath,
                    recursive: true);
            }
        }
    }
}
