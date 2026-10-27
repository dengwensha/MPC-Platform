using System.Text.Json;
using MPC.Contracts.Validation;

namespace MPC.Contracts.Tests;

public sealed class RegionSourceValidationContractTests
{
    private const string Id = "751057f8-b21e-4b3a-b31a-f1cad8cdc155";
    private const string Hash = "BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD";

    private static RegionSourceValidationRequest Request(
        string? version = ContractVersion.RegionSourceValidation,
        WorkerOperationKind operation = WorkerOperationKind.ValidateRegionSource,
        string? face = "#8", string? hash = Hash,
        RegionSourceValidationRequirement requirement = RegionSourceValidationRequirement.SourceLinkOnly,
        string? path = "part.step", string? requestId = Id) =>
        new(version, requestId, operation, path, face, hash, requirement);

    [Fact]
    public void LegacyVersionAndOperationNumbers_ArePreserved()
    {
        Assert.Equal("1.0", ContractVersion.Current);
        Assert.Equal("1.1", ContractVersion.RegionSourceValidation);
        Assert.Equal(0, (int)WorkerOperationKind.AnalyzeModel);
        Assert.Equal(1, (int)WorkerOperationKind.ValidateRegionSource);
        Assert.False(ContractVersion.IsSupported("1.1"));
    }

    [Fact]
    public void LegacyJsonWithoutNewFields_RemainsAccepted()
    {
        string json = "{\"ContractVersion\":\"1.0\",\"RequestId\":\"" + Id +
            "\",\"Operation\":0,\"InputArtifactPath\":\"part.step\"}";
        var request = JsonSerializer.Deserialize<WorkerOperationRequest>(json);
        Assert.True(WorkerOperationRequestValidator.Validate(request).IsValid);
    }

    [Theory]
    [InlineData("1.0")]
    [InlineData("1.1")]
    public void NewOperation_CannotEnterLegacyImportDispatch(string version)
    {
        // Exercise actual legacy DTO deserialization and its live validator.
        var legacy = JsonSerializer.Deserialize<WorkerOperationRequest>(
            JsonSerializer.Serialize(Request(version: version)));
        var validation = WorkerOperationRequestValidator.Validate(legacy);
        Assert.False(validation.IsValid);
        Assert.Contains(validation.Diagnostics,
            d => d.Code == WorkerOperationRequestValidator.UnsupportedOperationCode);
    }

    [Theory]
    [InlineData("1.0", 1, false)]
    [InlineData("1.1", 0, false)]
    [InlineData("1.1", 1, true)]
    [InlineData("2.0", 1, false)]
    [InlineData("1.1", 99, false)]
    public void NewValidator_EnforcesVersionOperationPair(string version, int operation, bool valid) =>
        Assert.Equal(valid, RegionSourceValidationRequestValidator.Validate(
            Request(version: version, operation: (WorkerOperationKind)operation)).IsValid);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" 1.1 ")]
    public void MissingOrNonExactVersion_IsRejected(string? version) =>
        Assert.Contains(RegionSourceValidationRequestValidator.Validate(
            Request(version: version)).Diagnostics,
            d => d.Code == RegionSourceValidationRequestValidator.UnsupportedVersionCode);

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void ValidNewRequest_RoundTripsWithoutLosingSourceOrRequirement(int requirement)
    {
        var request = Request(requirement: (RegionSourceValidationRequirement)requirement);
        var restored = JsonSerializer.Deserialize<RegionSourceValidationRequest>(
            JsonSerializer.Serialize(request));
        Assert.NotNull(restored);
        Assert.True(RegionSourceValidationRequestValidator.Validate(restored).IsValid);
        Assert.Equal(Id, restored.RequestId);
        Assert.Equal(Hash, restored.ExpectedDocumentSha256);
        Assert.Equal("#8", restored.SourceFaceId);
        Assert.Equal((RegionSourceValidationRequirement)requirement, restored.Requirement);
    }

    [Fact]
    public void LowercaseHash_IsAcceptedWithoutMutatingRawValue()
    {
        var request = Request(hash: Hash.ToLowerInvariant());
        Assert.True(RegionSourceValidationRequestValidator.Validate(request).IsValid);
        Assert.Equal(Hash.ToLowerInvariant(), request.ExpectedDocumentSha256);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("GA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD")]
    [InlineData(" BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD")]
    public void InvalidHash_IsRejected(string? hash) =>
        Assert.Contains(RegionSourceValidationRequestValidator.Validate(Request(hash: hash)).Diagnostics,
            d => d.Code == RegionSourceValidationRequestValidator.InvalidHashCode);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("#0")]
    [InlineData("#08")]
    [InlineData("#-1")]
    [InlineData("8")]
    [InlineData("#8 ")]
    [InlineData("#８")]
    public void NonCanonicalSourceFace_IsRejected(string? face) =>
        Assert.Contains(RegionSourceValidationRequestValidator.Validate(Request(face: face)).Diagnostics,
            d => d.Code == RegionSourceValidationRequestValidator.InvalidFaceIdCode);

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    public void UnknownRequirement_IsRejected(int requirement) =>
        Assert.Contains(RegionSourceValidationRequestValidator.Validate(
            Request(requirement: (RegionSourceValidationRequirement)requirement)).Diagnostics,
            d => d.Code == RegionSourceValidationRequestValidator.InvalidRequirementCode);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void MissingPath_IsRejected(string? path) =>
        Assert.Contains(RegionSourceValidationRequestValidator.Validate(Request(path: path)).Diagnostics,
            d => d.Code == RegionSourceValidationRequestValidator.MissingInputPathCode);

    [Fact]
    public void InvalidRequestIdentity_IsRejected() =>
        Assert.Contains(RegionSourceValidationRequestValidator.Validate(
            Request(requestId: "not-a-guid")).Diagnostics,
            d => d.Code == RegionSourceValidationRequestValidator.InvalidRequestIdCode);

    [Fact]
    public void MissingFields_NeverDeserializeToValidNewRequest()
    {
        var request = JsonSerializer.Deserialize<RegionSourceValidationRequest>("{}");
        var validation = RegionSourceValidationRequestValidator.Validate(request);
        Assert.False(validation.IsValid);
        Assert.Equal(7, validation.Diagnostics.Count);
        Assert.All(validation.Diagnostics, d => Assert.Equal(ContractDiagnosticSeverity.Error, d.Severity));
        Assert.Single(RegionSourceValidationRequestValidator.Validate(null).Diagnostics);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void DefinedResult_RoundTripsAndCorrelates(int outcome)
    {
        var result = new RegionSourceValidationResult("1.1", Id,
            WorkerOperationKind.ValidateRegionSource, (RegionSourceValidationOutcome)outcome);
        var restored = JsonSerializer.Deserialize<RegionSourceValidationResult>(
            JsonSerializer.Serialize(result));
        Assert.True(RegionSourceValidationResultValidator.Validate(restored, Id).IsValid);
        Assert.Equal((RegionSourceValidationOutcome)outcome, restored!.Outcome);
        Assert.False(RegionSourceValidationResultValidator.Validate(restored,
            "8bf686bc-8bf6-49ca-921a-64b142836e80").IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    public void UnknownResultOutcome_IsRejected(int outcome) =>
        Assert.Contains(RegionSourceValidationResultValidator.Validate(
            new RegionSourceValidationResult("1.1", Id, WorkerOperationKind.ValidateRegionSource,
                (RegionSourceValidationOutcome)outcome), Id).Diagnostics,
            d => d.Code == RegionSourceValidationResultValidator.InvalidOutcomeCode);

    [Fact]
    public void WrongResultVersionOrOperation_IsRejected()
    {
        var result = new RegionSourceValidationResult("1.0", Id,
            WorkerOperationKind.AnalyzeModel, RegionSourceValidationOutcome.SourceLinkMatched);
        var validation = RegionSourceValidationResultValidator.Validate(result, Id);
        Assert.Equal(2, validation.Diagnostics.Count);
        Assert.Contains(validation.Diagnostics, d => d.Code == RegionSourceValidationResultValidator.UnsupportedVersionCode);
        Assert.Contains(validation.Diagnostics, d => d.Code == RegionSourceValidationResultValidator.UnsupportedOperationCode);
    }

    [Fact]
    public void MissingResultFieldsOrExpectedIdentity_CannotApprove()
    {
        var result = JsonSerializer.Deserialize<RegionSourceValidationResult>("{}");
        Assert.False(RegionSourceValidationResultValidator.Validate(result, Id).IsValid);
        Assert.Single(RegionSourceValidationResultValidator.Validate(null, Id).Diagnostics);
        Assert.False(RegionSourceValidationResultValidator.Validate(
            new RegionSourceValidationResult("1.1", null, WorkerOperationKind.ValidateRegionSource,
                RegionSourceValidationOutcome.SourceLinkMatched), null).IsValid);
    }
}
