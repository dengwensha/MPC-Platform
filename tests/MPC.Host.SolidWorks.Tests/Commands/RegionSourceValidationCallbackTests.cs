using System;
using System.IO;
using System.Text.Json;
using MPC.Host.SolidWorks.Commands;
using Xunit;

namespace MPC.Host.SolidWorks.Tests.Commands;

public sealed class RegionSourceValidationCallbackTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "MpcCallbackLog", Guid.NewGuid().ToString("N"));
    private string LogPath => Path.Combine(_folder, "host-last-failure.json");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnexpectedFailure_ContainsAndLogsWithOnlyAvailableCorrelation(bool afterRequest)
    {
        string id = Guid.NewGuid().ToString("D");
        var callback = new RegionSourceValidationCallback(new HostFailureLog(_folder));
        int errors = 0;
        var error = new InvalidOperationException("unexpected operation", new FormatException("inner evidence"));
        callback.Run(() =>
        {
            if (afterRequest) callback.RequestId = id;
            throw error;
        }, () => errors++);
        Assert.Equal(1, errors);
        using var json = JsonDocument.Parse(File.ReadAllText(LogPath));
        var root = json.RootElement;
        Assert.Equal(afterRequest ? id : null, root.GetProperty("RequestId").GetString());
        Assert.Equal("SourceValidationCallback", root.GetProperty("Boundary").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("HostStatus").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("WorkerExitCode").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("ReceivedOutcome").ValueKind);
        Assert.Equal(typeof(InvalidOperationException).FullName, root.GetProperty("FailureType").GetString());
        Assert.Contains("inner evidence", root.GetProperty("ExceptionDetails").GetString());
    }

    [Fact]
    public void FailedLoggingAndFailedErrorDisplay_DoNotEscapeCallback()
    {
        Directory.CreateDirectory(_folder);
        string blocked = Path.Combine(_folder, "blocked");
        File.WriteAllText(blocked, "not a directory");
        var callback = new RegionSourceValidationCallback(new HostFailureLog(blocked));
        int errors = 0;
        callback.Run(() => throw new InvalidOperationException("original"),
            () => { errors++; throw new Exception("display failed"); });
        Assert.Equal(1, errors);
        Assert.Equal("not a directory", File.ReadAllText(blocked));
    }

    [Fact]
    public void LoggerInitializationFailure_StillDisplaysOriginalGenericFailure()
    {
        var callback = new RegionSourceValidationCallback(
            () => throw new InvalidOperationException("logger unavailable"));
        int errors = 0;
        callback.Run(() => throw new InvalidOperationException("original"), () => errors++);
        Assert.Equal(1, errors);
        Assert.False(Directory.Exists(_folder));
    }

    [Fact]
    public void ErrorDisplayFailure_DoesNotReplaceOriginalEvidence()
    {
        var callback = new RegionSourceValidationCallback(new HostFailureLog(_folder));
        callback.Run(() => throw new InvalidOperationException("original evidence"),
            () => throw new Exception("display failed"));
        using var json = JsonDocument.Parse(File.ReadAllText(LogPath));
        string details = json.RootElement.GetProperty("ExceptionDetails").GetString()!;
        Assert.Contains("original evidence", details);
        Assert.DoesNotContain("display failed", details);
    }

    [Fact]
    public void NormalReturn_DoesNotCreateLogOrDisplayError()
    {
        var callback = new RegionSourceValidationCallback(new HostFailureLog(_folder));
        int calls = 0;
        callback.Run(() => calls++, () => throw new Exception("must not display"));
        Assert.Equal(1, calls);
        Assert.False(Directory.Exists(_folder));
    }

    [Fact]
    public void ReusedCallback_DoesNotCarryCorrelationIntoEarlyFailure()
    {
        var callback = new RegionSourceValidationCallback(new HostFailureLog(_folder));
        callback.Run(() => callback.RequestId = Guid.NewGuid().ToString("D"), () => { });
        callback.Run(() => throw new InvalidOperationException("before next request"), () => { });
        using var json = JsonDocument.Parse(File.ReadAllText(LogPath));
        Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty("RequestId").ValueKind);
    }

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, true);
    }
}
