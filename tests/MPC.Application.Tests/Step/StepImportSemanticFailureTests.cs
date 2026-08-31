using MPC.Application.Step;

namespace MPC.Application.Tests.Step;

public sealed class StepImportSemanticFailureTests
{
    [Fact]
    public void Execute_NegativeToroidalMajorRadius_ReturnsDeterministicSemanticDiagnostic()
    {
        string filePath =
            Path.Combine(
                Path.GetTempPath(),
                $"MPC.P01B03.{Guid.NewGuid():N}.step");

        try
        {
            File.WriteAllText(
                filePath,
                """
                ISO-10303-21;
                HEADER;
                ENDSEC;
                DATA;
                #170=TOROIDAL_SURFACE('',#100,-178.5,10.0);
                ENDSEC;
                END-ISO-10303-21;
                """);

            var service =
                new StepImportApplicationService();

            var request =
                new StepImportRequest(
                    filePath);

            StepImportResult first =
                service.Execute(request);

            StepImportResult second =
                service.Execute(request);

            AssertSemanticFailure(first);
            AssertSemanticFailure(second);

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
        finally
        {
            File.Delete(filePath);
        }
    }

    private static void AssertSemanticFailure(
        StepImportResult result)
    {
        Assert.Equal(
            StepImportStatus.Failed,
            result.Status);

        Assert.Null(result.Summary);

        StepImportDiagnostic diagnostic =
            Assert.Single(result.Diagnostics);

        Assert.Equal(
            StepImportApplicationService.SemanticFailureCode,
            diagnostic.Code);

        Assert.Equal(
            StepImportDiagnosticSeverity.Error,
            diagnostic.Severity);

        Assert.Equal(
            "#170",
            diagnostic.SourceEntityId);

        Assert.Equal(
            "STEP semantic entity '#170' " +
            "of type 'TOROIDAL_SURFACE' " +
            "rejected parameter 'majorRadius' " +
            "with value '-178.5'.",
            diagnostic.Message);

        Assert.DoesNotContain(
            result.Diagnostics,
            item =>
                item.Code ==
                StepImportApplicationService.UnexpectedFailureCode);
    }
}