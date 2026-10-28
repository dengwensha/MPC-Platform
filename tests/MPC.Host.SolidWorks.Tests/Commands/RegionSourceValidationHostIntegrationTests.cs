using System;
using System.IO;
using System.Security.Cryptography;
using MPC.Contracts;
using MPC.Host.SolidWorks.Commands;
using Xunit;

namespace MPC.Host.SolidWorks.Tests.Commands;

public sealed class RegionSourceValidationHostIntegrationTests
{
    private static string Golden => Path.Combine(AppContext.BaseDirectory,
        "TestData", "Step", "Golden", "T001_PRISM.step");

    private static string Hash(string file)
    {
        using (var sha = SHA256.Create())
            return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(file))).Replace("-", "");
    }

    private static string Worker()
    {
        DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "src", "MPC.sln")))
            directory = directory.Parent;
        if (directory is null) throw new DirectoryNotFoundException("MPC repository root");
#if DEBUG
        const string configuration = "Debug";
#else
        const string configuration = "Release";
#endif
        string path = Path.Combine(directory.FullName, "src", "MPC.Worker", "bin",
            "x64", configuration, "net8.0", "MPC.Worker.exe");
        Assert.True(File.Exists(path), path);
        return path;
    }

    private static RegionSourceValidationHostCommandResult Execute(string file, string hash,
        RegionSourceValidationRequirement requirement = RegionSourceValidationRequirement.SourceLinkOnly) =>
        new RegionSourceValidationHostCommand().Execute(Worker(),
            new RegionSourceValidationRequest("1.1", Guid.NewGuid().ToString("D"),
                WorkerOperationKind.ValidateRegionSource, file, "#8", hash, requirement), 30000);

    [Fact]
    public void Net48Host_ToRealNet8Worker_MatchesGoldenSource()
    {
        var result = Execute(Golden, Hash(Golden));
        Assert.Equal(RegionSourceHostStatus.SourceLinkMatched, result.Status);
        Assert.Equal(RegionSourceValidationOutcome.SourceLinkMatched, result.WorkerResult!.Outcome);
        Assert.Equal("1.1", result.WorkerResult.ContractVersion);
    }

    [Fact]
    public void RealWorker_CannotApproveMaterialSide()
    {
        var result = Execute(Golden, Hash(Golden), RegionSourceValidationRequirement.MaterialSideDependent);
        Assert.Equal(RegionSourceHostStatus.Blocked, result.Status);
        Assert.Equal(RegionSourceValidationOutcome.MaterialSideUnverified, result.WorkerResult!.Outcome);
    }

    [Fact]
    public void ChangedFile_WithOldFingerprint_IsBlockedAcrossProcessBoundary()
    {
        string copy = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".step");
        try
        {
            File.Copy(Golden, copy);
            string expected = Hash(copy);
            File.AppendAllText(copy, Environment.NewLine);
            var result = Execute(copy, expected);
            Assert.Equal(RegionSourceHostStatus.Blocked, result.Status);
            Assert.Equal(RegionSourceValidationOutcome.DocumentMismatch, result.WorkerResult!.Outcome);
        }
        finally { File.Delete(copy); }
    }

    [Fact]
    public void MissingSourceFile_IsWorkerFailureAcrossProcessBoundary()
    {
        string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".step");
        var result = Execute(missing, new string('A', 64));
        Assert.Equal(RegionSourceHostStatus.WorkerFailed, result.Status);
        Assert.Equal(RegionSourceValidationOutcome.ImportFailed, result.WorkerResult!.Outcome);
    }
}
