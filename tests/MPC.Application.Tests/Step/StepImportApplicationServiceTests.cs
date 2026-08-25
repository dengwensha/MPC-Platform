using MPC.Application.Step;

namespace MPC.Application.Tests.Step;

public sealed class StepImportApplicationServiceTests
{
    [Fact]
    public void Execute_NullRequest_ReturnsDeterministicFailure()
    {
        var service =
            new StepImportApplicationService();

        StepImportResult result =
            service.Execute(null);

        Assert.Equal(
            StepImportStatus.Failed,
            result.Status);

        Assert.Null(result.Summary);

        StepImportDiagnostic diagnostic =
            Assert.Single(result.Diagnostics);

        Assert.Equal(
            StepImportApplicationService.MissingRequestCode,
            diagnostic.Code);

        Assert.Equal(
            StepImportDiagnosticSeverity.Error,
            diagnostic.Severity);
    }

    [Fact]
    public void Execute_MissingFile_ReturnsRepeatableFailureDiagnostics()
    {
        string missingFilePath =
            Path.Combine(
                AppContext.BaseDirectory,
                "missing-input.step");

        var service =
            new StepImportApplicationService();

        var request =
            new StepImportRequest(
                missingFilePath);

        StepImportResult first =
            service.Execute(request);

        StepImportResult second =
            service.Execute(request);

        Assert.Equal(
            StepImportStatus.Failed,
            first.Status);

        Assert.Equal(
            StepImportStatus.Failed,
            second.Status);

        Assert.Null(first.Summary);
        Assert.Null(second.Summary);
        Assert.NotEmpty(first.Diagnostics);
        Assert.NotEmpty(second.Diagnostics);

        Assert.Equal(
            first.Diagnostics.Select(
                diagnostic => diagnostic.Code),
            second.Diagnostics.Select(
                diagnostic => diagnostic.Code));

        Assert.Equal(
            first.Diagnostics.Select(
                diagnostic => diagnostic.Message),
            second.Diagnostics.Select(
                diagnostic => diagnostic.Message));
    }

    [Theory]
    [InlineData("T001_PRISM.step")]
    [InlineData("T101_BASIC_POCKET.step")]
    [InlineData("T103_MULTIPLE_ISLANDS.step")]
    public void Execute_RepresentativeGoldenSolid_UsesCanonicalPipeline(
        string fileName)
    {
        string filePath =
            GetGoldenStepFilePath(fileName);

        var service =
            new StepImportApplicationService();

        StepImportResult result =
            service.Execute(
                new StepImportRequest(
                    filePath));

        Assert.True(
            result.Status is
                StepImportStatus.Succeeded or
                StepImportStatus.SucceededWithDiagnostics,
            CreateFailureMessage(result));

        StepImportSummary summary =
            Assert.IsType<StepImportSummary>(
                result.Summary);

        Assert.Equal(1, summary.SolidCount);
        Assert.Equal(1, summary.OuterShellCount);
        Assert.True(summary.FaceCount > 0);
        Assert.True(summary.LoopCount > 0);
        Assert.True(summary.EdgeCount > 0);
        Assert.True(summary.VertexCount > 0);
    }

    [Fact]
    public void Execute_T001Prism_ReportsCanonicalTopologySummary()
    {
        string filePath =
            GetGoldenStepFilePath(
                "T001_PRISM.step");

        var service =
            new StepImportApplicationService();

        StepImportResult result =
            service.Execute(
                new StepImportRequest(
                    filePath));

        Assert.Equal(
            StepImportStatus.Succeeded,
            result.Status);

        StepImportSummary summary =
            Assert.IsType<StepImportSummary>(
                result.Summary);

        Assert.Equal("#63", summary.SourceSolidEntityId);
        Assert.Equal(1, summary.SolidCount);
        Assert.Equal(1, summary.OuterShellCount);
        Assert.Equal(0, summary.InnerShellCount);
        Assert.Equal(6, summary.FaceCount);
        Assert.Equal(6, summary.LoopCount);
        Assert.Equal(12, summary.EdgeCount);
        Assert.Equal(8, summary.VertexCount);
        Assert.Empty(result.Diagnostics);
    }

    private static string GetGoldenStepFilePath(
        string fileName)
    {
        string filePath =
            Path.Combine(
                AppContext.BaseDirectory,
                "TestData",
                "Step",
                "Golden",
                fileName);

        Assert.True(
            File.Exists(filePath),
            $"Golden STEP file was not found: '{filePath}'.");

        return filePath;
    }

    private static string CreateFailureMessage(
        StepImportResult result)
    {
        if (result.Diagnostics.Count == 0)
        {
            return
                $"STEP import ended with status " +
                $"'{result.Status}' without diagnostics.";
        }

        return string.Join(
            Environment.NewLine,
            result.Diagnostics.Select(
                diagnostic =>
                    $"{diagnostic.Code}: " +
                    $"{diagnostic.Message}"));
    }
}
