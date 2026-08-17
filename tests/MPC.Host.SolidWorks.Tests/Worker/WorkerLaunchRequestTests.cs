using Xunit;
using MPC.Host.SolidWorks.Worker;

namespace MPC.Host.SolidWorks.Tests.Worker;

public sealed class WorkerLaunchRequestTests
{
    [Fact]
    public void Constructor_PreservesProvidedPaths()
    {
        var request =
            new WorkerLaunchRequest(
                @"C:\worker\MPC.Worker.exe",
                @"C:\jobs\request.json",
                @"C:\jobs\result.json");

        Assert.Equal(
            @"C:\worker\MPC.Worker.exe",
            request.WorkerExecutablePath);

        Assert.Equal(
            @"C:\jobs\request.json",
            request.RequestJsonPath);

        Assert.Equal(
            @"C:\jobs\result.json",
            request.ResultJsonPath);
    }

    [Fact]
    public void Constructor_AllowsValidationToRemainWithLauncher()
    {
        var request =
            new WorkerLaunchRequest(
                workerExecutablePath: null,
                requestJsonPath: null,
                resultJsonPath: null);

        Assert.Null(request.WorkerExecutablePath);
        Assert.Null(request.RequestJsonPath);
        Assert.Null(request.ResultJsonPath);
    }
}
