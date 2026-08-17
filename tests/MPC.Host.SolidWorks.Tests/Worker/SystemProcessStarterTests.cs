using System;
using MPC.Host.SolidWorks.Worker;
using Xunit;

namespace MPC.Host.SolidWorks.Tests.Worker;

public sealed class SystemProcessStarterTests
{
    [Fact]
    public void CreateStartInfo_RejectsNullRequest()
    {
        Assert.Throws<ArgumentNullException>(
            () => SystemProcessStarter.CreateStartInfo(null!));
    }

    [Fact]
    public void CreateStartInfo_AppliesRequiredHostProcessSettings()
    {
        var request =
            new ProcessStartRequest(
                @"C:\MPC Worker\MPC.Worker.exe",
                new[]
                {
                    @"C:\MPC Jobs\request.json",
                    @"C:\MPC Jobs\result.json"
                },
                @"C:\MPC Worker");

        var startInfo =
            SystemProcessStarter.CreateStartInfo(request);

        Assert.Equal(
            @"C:\MPC Worker\MPC.Worker.exe",
            startInfo.FileName);

        Assert.Equal(
            @"""C:\MPC Jobs\request.json"" ""C:\MPC Jobs\result.json""",
            startInfo.Arguments);

        Assert.Equal(
            @"C:\MPC Worker",
            startInfo.WorkingDirectory);

        Assert.False(startInfo.UseShellExecute);
        Assert.True(startInfo.CreateNoWindow);
        Assert.False(startInfo.RedirectStandardInput);
        Assert.False(startInfo.RedirectStandardOutput);
        Assert.False(startInfo.RedirectStandardError);
    }

    [Fact]
    public void BuildArguments_UsesCanonicalWorkerArgumentOrder()
    {
        var request =
            new ProcessStartRequest(
                "worker.exe",
                new[]
                {
                    "request.json",
                    "result.json"
                },
                "worker-directory");

        var arguments =
            SystemProcessStarter.BuildArguments(request);

        Assert.Equal(
            @"""request.json"" ""result.json""",
            arguments);
    }

    [Fact]
    public void BuildArguments_QuotesEmptyArguments()
    {
        var request =
            new ProcessStartRequest(
                "worker.exe",
                new[]
                {
                    string.Empty,
                    "result.json"
                },
                "worker-directory");

        var arguments =
            SystemProcessStarter.BuildArguments(request);

        Assert.Equal(
            @""""" ""result.json""",
            arguments);
    }

    [Fact]
    public void BuildArguments_EscapesEmbeddedQuotationMarks()
    {
        var request =
            new ProcessStartRequest(
                "worker.exe",
                new[]
                {
                    "request\"name.json",
                    "result.json"
                },
                "worker-directory");

        var arguments =
            SystemProcessStarter.BuildArguments(request);

        Assert.Equal(
            "\"request\\\"name.json\" \"result.json\"",
            arguments);
    }

    [Fact]
    public void BuildArguments_DoublesTrailingBackslashes()
    {
        var request =
            new ProcessStartRequest(
                "worker.exe",
                new[]
                {
                    "C:\\request\\",
                    "result.json"
                },
                "worker-directory");

        var arguments =
            SystemProcessStarter.BuildArguments(request);

        Assert.Equal(
            "\"C:\\request\\\\\" \"result.json\"",
            arguments);
    }
}
