using System;
using MPC.Contracts;
using MPC.Contracts.Validation;

namespace MPC.Contracts.Tests.Validation;

public sealed class WorkerOperationRequestValidatorTests
{
    [Fact]
    public void Validate_AcceptsSupportedRequest()
    {
        var request = new WorkerOperationRequest(
            ContractVersion.Current,
            Guid.NewGuid().ToString("D"),
            WorkerOperationKind.AnalyzeModel,
            @"C:\MPC\input.model");

        var result = WorkerOperationRequestValidator.Validate(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Validate_RejectsNullRequest()
    {
        var result = WorkerOperationRequestValidator.Validate(null);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.False(result.IsValid);
        Assert.Equal(
            WorkerOperationRequestValidator.MissingRequestCode,
            diagnostic.Code);
        Assert.Equal(
            ContractDiagnosticSeverity.Error,
            diagnostic.Severity);
    }

    [Fact]
    public void Validate_RejectsUnsupportedVersion()
    {
        var request = CreateValidRequest(
            contractVersion: "2.0");

        var result = WorkerOperationRequestValidator.Validate(request);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.False(result.IsValid);
        Assert.Equal(
            WorkerOperationRequestValidator.UnsupportedVersionCode,
            diagnostic.Code);
    }

    [Fact]
    public void Validate_RejectsInvalidRequestId()
    {
        var request = CreateValidRequest(
            requestId: "not-a-guid");

        var result = WorkerOperationRequestValidator.Validate(request);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(
            WorkerOperationRequestValidator.InvalidRequestIdCode,
            diagnostic.Code);
    }

    [Fact]
    public void Validate_RejectsUnsupportedOperation()
    {
        var request = CreateValidRequest(
            operation: (WorkerOperationKind)999);

        var result = WorkerOperationRequestValidator.Validate(request);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(
            WorkerOperationRequestValidator.UnsupportedOperationCode,
            diagnostic.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_RejectsMissingInputArtifactPath(
        string? inputArtifactPath)
    {
        var request = CreateValidRequest(
            inputArtifactPath: inputArtifactPath);

        var result = WorkerOperationRequestValidator.Validate(request);

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal(
            WorkerOperationRequestValidator.MissingInputArtifactPathCode,
            diagnostic.Code);
    }

    [Fact]
    public void Validate_ReportsAllIndependentRequestErrors()
    {
        var request = new WorkerOperationRequest(
            "2.0",
            "invalid",
            (WorkerOperationKind)999,
            " ");

        var result = WorkerOperationRequestValidator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Collection(
            result.Diagnostics,
            diagnostic => Assert.Equal(
                WorkerOperationRequestValidator.UnsupportedVersionCode,
                diagnostic.Code),
            diagnostic => Assert.Equal(
                WorkerOperationRequestValidator.InvalidRequestIdCode,
                diagnostic.Code),
            diagnostic => Assert.Equal(
                WorkerOperationRequestValidator.UnsupportedOperationCode,
                diagnostic.Code),
            diagnostic => Assert.Equal(
                WorkerOperationRequestValidator.MissingInputArtifactPathCode,
                diagnostic.Code));
    }

    private static WorkerOperationRequest CreateValidRequest(
        string? contractVersion = ContractVersion.Current,
        string? requestId = null,
        WorkerOperationKind operation =
            WorkerOperationKind.AnalyzeModel,
        string? inputArtifactPath = @"C:\MPC\input.model")
    {
        return new WorkerOperationRequest(
            contractVersion,
            requestId ?? Guid.NewGuid().ToString("D"),
            operation,
            inputArtifactPath);
    }
}
