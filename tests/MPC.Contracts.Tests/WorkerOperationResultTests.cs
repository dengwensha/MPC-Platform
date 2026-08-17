using System;
using MPC.Contracts;

namespace MPC.Contracts.Tests;

public sealed class WorkerOperationResultTests
{
    [Fact]
    public void Constructor_NormalizesOptionalOutputPath()
    {
        var result = new WorkerOperationResult(
            ContractVersion.Current,
            Guid.NewGuid().ToString("D"),
            WorkerOperationStatus.Succeeded,
            " ",
            Array.Empty<ContractDiagnostic>());

        Assert.Null(result.OutputArtifactPath);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Constructor_CopiesDiagnostics()
    {
        var diagnostics = new[]
        {
            new ContractDiagnostic(
                "WORKER-001",
                ContractDiagnosticSeverity.Warning,
                "Warning")
        };

        var result = new WorkerOperationResult(
            ContractVersion.Current,
            Guid.NewGuid().ToString("D"),
            WorkerOperationStatus.Succeeded,
            null,
            diagnostics);

        diagnostics[0] = new ContractDiagnostic(
            "CHANGED",
            ContractDiagnosticSeverity.Error,
            "Changed");

        var diagnostic = Assert.Single(result.Diagnostics);

        Assert.Equal("WORKER-001", diagnostic.Code);
    }

    [Fact]
    public void Constructor_RejectsUndefinedStatus()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new WorkerOperationResult(
                ContractVersion.Current,
                Guid.NewGuid().ToString("D"),
                (WorkerOperationStatus)999,
                null,
                Array.Empty<ContractDiagnostic>()));
    }

    [Fact]
    public void Constructor_RejectsNullDiagnosticEntry()
    {
        Assert.Throws<ArgumentException>(
            () => new WorkerOperationResult(
                ContractVersion.Current,
                Guid.NewGuid().ToString("D"),
                WorkerOperationStatus.Failed,
                null,
                new ContractDiagnostic[] { null! }));
    }
}
