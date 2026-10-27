using System.Security.Cryptography;
using System.Text.Json;
using MPC.Contracts;
using MPC.Contracts.Validation;

namespace MPC.Worker.Tests;

public sealed class RegionSourceValidationWorkerTests
{
    private const string Id = "edbdf4d2-b324-4bf3-900e-8c24c1458088";
    private static string Golden => Path.Combine(AppContext.BaseDirectory,
        "TestData", "Step", "Golden", "T001_PRISM.step");
    private static string Hash(string path) =>
        Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    private static RegionSourceValidationRequest Request(string path, string? hash = null,
        RegionSourceValidationRequirement requirement = RegionSourceValidationRequirement.SourceLinkOnly,
        string version = "1.1", WorkerOperationKind operation = WorkerOperationKind.ValidateRegionSource,
        string face = "#8") =>
        new(version, Id, operation, path, face, hash ?? Hash(path), requirement);

    private static async Task<(int Exit, RegionSourceValidationResult Result)> Run(
        RegionSourceValidationRequest request)
    {
        string folder = Path.Combine(Path.GetTempPath(), "MpcRegionWorker", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string input = Path.Combine(folder, "request.json");
            string output = Path.Combine(folder, "result.json");
            await File.WriteAllTextAsync(input, JsonSerializer.Serialize(request));
            int exit = await new WorkerProcessRunner().RunAsync([input, output], TextWriter.Null);
            string json = await File.ReadAllTextAsync(output);
            Assert.DoesNotContain("CanonicalFaceId", json);
            Assert.DoesNotContain("SurfaceNormal", json);
            var result = Assert.IsType<RegionSourceValidationResult>(
                JsonSerializer.Deserialize<RegionSourceValidationResult>(json));
            Assert.True(RegionSourceValidationResultValidator.Validate(result, Id).IsValid);
            return (exit, result);
        }
        finally { Directory.Delete(folder, true); }
    }

    [Fact]
    public async Task GoldenImport_RoutesToV11AndReturnsOnlySourceMatch()
    {
        var (exit, result) = await Run(Request(Golden));
        Assert.Equal(WorkerExitCodes.Succeeded, exit);
        Assert.Equal("1.1", result.ContractVersion);
        Assert.Equal(WorkerOperationKind.ValidateRegionSource, result.Operation);
        Assert.Equal(RegionSourceValidationOutcome.SourceLinkMatched, result.Outcome);
    }

    [Fact]
    public async Task MaterialSideRequest_CannotReturnSuccessfulExit()
    {
        var (exit, result) = await Run(Request(Golden,
            requirement: RegionSourceValidationRequirement.MaterialSideDependent));
        Assert.Equal(WorkerExitCodes.RequestRejected, exit);
        Assert.Equal(RegionSourceValidationOutcome.MaterialSideUnverified, result.Outcome);
    }

    [Fact]
    public async Task ChangedBytes_RejectPinnedFingerprint()
    {
        string copy = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".step");
        try
        {
            File.Copy(Golden, copy);
            string expected = Hash(copy);
            File.AppendAllText(copy, Environment.NewLine);
            var (exit, result) = await Run(Request(copy, expected));
            Assert.Equal(WorkerExitCodes.RequestRejected, exit);
            Assert.Equal(RegionSourceValidationOutcome.DocumentMismatch, result.Outcome);
        }
        finally { File.Delete(copy); }
    }

    [Fact]
    public async Task AbsentFace_DoesNotFallBackToWholeModelImport()
    {
        var (exit, result) = await Run(Request(Golden, face: "#999999"));
        Assert.Equal(WorkerExitCodes.RequestRejected, exit);
        Assert.Equal(RegionSourceValidationOutcome.MissingRegion, result.Outcome);
    }

    [Theory]
    [InlineData("1.0", 1)]
    [InlineData("1.1", 0)]
    [InlineData("1.0", 0)]
    [InlineData("2.0", 1)]
    [InlineData("1.1", 99)]
    public async Task InvalidVersionOperation_WithRegionPayload_NeverFallsBack(string version, int operation)
    {
        var (exit, result) = await Run(Request(Golden, version: version,
            operation: (WorkerOperationKind)operation));
        Assert.Equal(WorkerExitCodes.RequestRejected, exit);
        Assert.Equal(RegionSourceValidationOutcome.RequestRejected, result.Outcome);
    }

    [Fact]
    public async Task InvalidHash_IsRejectedBeforeOpeningMissingFile()
    {
        string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".step");
        var (exit, result) = await Run(Request(missing, "bad-hash"));
        Assert.Equal(WorkerExitCodes.RequestRejected, exit);
        Assert.Equal(RegionSourceValidationOutcome.RequestRejected, result.Outcome);
    }

    [Fact]
    public async Task MissingSourceFile_ProducesExplicitImportFailure()
    {
        string missing = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".step");
        var (exit, result) = await Run(Request(missing, new string('A', 64)));
        Assert.Equal(WorkerExitCodes.OperationFailed, exit);
        Assert.Equal(RegionSourceValidationOutcome.ImportFailed, result.Outcome);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    public async Task UnknownRequirement_IsRejectedBeforeFileAccess(int requirement)
    {
        var request = Request("not-read.step", new string('A', 64),
            (RegionSourceValidationRequirement)requirement);
        var (exit, result) = await Run(request);
        Assert.Equal(WorkerExitCodes.RequestRejected, exit);
        Assert.Equal(RegionSourceValidationOutcome.RequestRejected, result.Outcome);
    }

    [Fact]
    public void NullRequest_CannotApprove() =>
        Assert.Equal(RegionSourceValidationOutcome.RequestRejected,
            new RegionSourceValidationProcessor().Process(null).Outcome);

    [Fact]
    public async Task LegacyV10GoldenImport_PreservesOriginalResultShapeAndVersion()
    {
        string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string input = Path.Combine(folder, "request.json");
            string output = Path.Combine(folder, "result.json");
            await File.WriteAllTextAsync(input, JsonSerializer.Serialize(new WorkerOperationRequest(
                "1.0", Id, WorkerOperationKind.AnalyzeModel, Golden)));
            int exit = await new WorkerProcessRunner().RunAsync([input, output], TextWriter.Null);
            var result = JsonSerializer.Deserialize<WorkerOperationResult>(await File.ReadAllTextAsync(output));
            Assert.NotNull(result);
            Assert.Equal(WorkerExitCodes.Succeeded, exit);
            Assert.Equal("1.0", result.ContractVersion);
            Assert.Equal(Id, result.RequestId);
            Assert.Equal(WorkerOperationStatus.Succeeded, result.Status);
            Assert.NotNull(result.Summary);
        }
        finally { Directory.Delete(folder, true); }
    }

    [Fact]
    public async Task V11ResultWriteFailure_IsNotSuccessfulExit()
    {
        string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string input = Path.Combine(folder, "request.json");
            await File.WriteAllTextAsync(input, JsonSerializer.Serialize(Request(Golden)));
            int exit = await new WorkerProcessRunner().RunAsync([input, folder], TextWriter.Null);
            Assert.Equal(WorkerExitCodes.ResultWriteFailed, exit);
        }
        finally { Directory.Delete(folder, true); }
    }

    [Fact]
    public async Task MissingOperationInV11Json_IsRejectedInsteadOfDefaultingToImport()
    {
        string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string input = Path.Combine(folder, "request.json");
            string output = Path.Combine(folder, "result.json");
            await File.WriteAllTextAsync(input,
                "{\"ContractVersion\":\"1.1\",\"RequestId\":\"" + Id + "\"}");
            int exit = await new WorkerProcessRunner().RunAsync([input, output], TextWriter.Null);
            var result = JsonSerializer.Deserialize<RegionSourceValidationResult>(
                await File.ReadAllTextAsync(output));
            Assert.Equal(WorkerExitCodes.RequestRejected, exit);
            Assert.Equal(RegionSourceValidationOutcome.RequestRejected, result!.Outcome);
        }
        finally { Directory.Delete(folder, true); }
    }
}
