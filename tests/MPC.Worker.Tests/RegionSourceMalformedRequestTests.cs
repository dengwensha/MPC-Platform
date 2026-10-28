using System.Text.Json;
using MPC.Contracts;
using MPC.Contracts.Validation;

namespace MPC.Worker.Tests;

public sealed class RegionSourceMalformedRequestTests
{
    private const string Id = "edbdf4d2-b324-4bf3-900e-8c24c1458088";

    private static Dictionary<string, object?> Payload() => new()
    {
        ["ContractVersion"] = "1.1", ["RequestId"] = Id,
        ["Operation"] = 1, ["InputArtifactPath"] = "must-not-be-read.step",
        ["SourceFaceId"] = "#8", ["ExpectedDocumentSha256"] = new string('A', 64),
        ["Requirement"] = 1
    };

    [Theory]
    [InlineData("ContractVersion", "11")]
    [InlineData("Operation", "\"ValidateRegionSource\"")]
    [InlineData("InputArtifactPath", "42")]
    [InlineData("SourceFaceId", "42")]
    [InlineData("ExpectedDocumentSha256", "[]")]
    [InlineData("Requirement", "\"SourceLinkOnly\"")]
    [InlineData("Requirement", "{}")]
    public async Task WrongFieldType_ReturnsCorrelatedV11Rejection(string field, string json)
    {
        var payload = Payload();
        payload[field] = JsonSerializer.Deserialize<JsonElement>(json);
        var (exit, result) = await Run(JsonSerializer.Serialize(payload));
        Assert.Equal(WorkerExitCodes.RequestRejected, exit);
        Assert.Equal("1.1", result.GetProperty("ContractVersion").GetString());
        Assert.Equal(Id, result.GetProperty("RequestId").GetString());
        Assert.Equal(1, result.GetProperty("Operation").GetInt32());
        Assert.Equal(8, result.GetProperty("Outcome").GetInt32());
        Assert.False(result.TryGetProperty("Status", out _));
        Assert.False(result.TryGetProperty("Diagnostics", out _));
        var typed = result.Deserialize<RegionSourceValidationResult>();
        Assert.True(RegionSourceValidationResultValidator.Validate(typed, Id).IsValid);
    }

    [Theory]
    [InlineData("null", null)]
    [InlineData("42", null)]
    [InlineData("{}", null)]
    [InlineData("[]", null)]
    [InlineData("\"invalid-id\"", "invalid-id")]
    [InlineData("\"\"", "")]
    [InlineData("missing", null)]
    public async Task UnusableRequestId_IsNotReplacedWithInventedCorrelation(
        string json, string? expectedId)
    {
        var payload = Payload();
        // Force deserialization failure even when RequestId itself is a string.
        payload["Requirement"] = "SourceLinkOnly";
        if (json == "missing") payload.Remove("RequestId");
        else payload["RequestId"] = JsonSerializer.Deserialize<JsonElement>(json);
        var (exit, result) = await Run(JsonSerializer.Serialize(payload));
        Assert.Equal(WorkerExitCodes.RequestRejected, exit);
        Assert.Equal("1.1", result.GetProperty("ContractVersion").GetString());
        Assert.Equal(1, result.GetProperty("Operation").GetInt32());
        Assert.Equal(8, result.GetProperty("Outcome").GetInt32());
        Assert.Equal(expectedId, result.GetProperty("RequestId").GetString());
        Assert.False(RegionSourceValidationResultValidator.Validate(
            result.Deserialize<RegionSourceValidationResult>(), Id).IsValid);
    }

    [Fact]
    public async Task MalformedRegionResultWriteFailure_ReturnsWriteFailure()
    {
        var payload = Payload();
        payload["Requirement"] = "SourceLinkOnly";
        string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string input = Path.Combine(folder, "request.json");
            await File.WriteAllTextAsync(input, JsonSerializer.Serialize(payload));
            using var error = new StringWriter();
            int exit = await new WorkerProcessRunner().RunAsync([input, folder], error);
            Assert.Equal(WorkerExitCodes.ResultWriteFailed, exit);
            Assert.Contains("could not be written", error.ToString());
        }
        finally { Directory.Delete(folder, true); }
    }

    [Theory]
    [InlineData("{\"ContractVersion\":\"1.0\",\"Operation\":\"AnalyzeModel\"}")]
    [InlineData("{\"ContractVersion\":\"1.1\",")]
    [InlineData("{ malformed")]
    public async Task UnroutableOrLegacyReadFailure_PreservesLegacyEnvelope(string json)
    {
        var (exit, result) = await Run(json);
        Assert.Equal(WorkerExitCodes.RequestReadFailed, exit);
        Assert.Equal("1.0", result.GetProperty("ContractVersion").GetString());
        Assert.True(result.TryGetProperty("Status", out _));
        Assert.False(result.TryGetProperty("Outcome", out _));
        Assert.Equal(WorkerProcessRunner.RequestReadFailedCode,
            result.GetProperty("Diagnostics")[0].GetProperty("Code").GetString());
    }

    private static async Task<(int Exit, JsonElement Result)> Run(string json)
    {
        string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string input = Path.Combine(folder, "request.json");
            string output = Path.Combine(folder, "result.json");
            await File.WriteAllTextAsync(input, json);
            int exit = await new WorkerProcessRunner().RunAsync([input, output], TextWriter.Null);
            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(output));
            return (exit, document.RootElement.Clone());
        }
        finally { Directory.Delete(folder, true); }
    }
}
