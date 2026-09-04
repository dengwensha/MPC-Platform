using MPC.Application.Step;

namespace MPC.Application.Tests.Step;

public sealed class StepDegenerateToroidalSurfaceCatalogTests
{
    [Fact]
    public void Execute_InvalidDegenerateToroidalSurface_UsesRegisteredSemanticHandler()
    {
        string filePath =
            Path.Combine(
                Path.GetTempPath(),
                $"MPC.P01B05.{Guid.NewGuid():N}.step");

        try
        {
            File.WriteAllText(
                filePath,
                """
                ISO-10303-21;
                HEADER;
                ENDSEC;
                DATA;
                #180=DEGENERATE_TOROIDAL_SURFACE('',#100,10.0,10.0,.T.);
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

            AssertSemanticFailure(
                first);

            AssertSemanticFailure(
                second);

            Assert.Equal(
                first.Diagnostics.Select(
                    diagnostic =>
                        diagnostic.Code),
                second.Diagnostics.Select(
                    diagnostic =>
                        diagnostic.Code));

            Assert.Equal(
                first.Diagnostics.Select(
                    diagnostic =>
                        diagnostic.Message),
                second.Diagnostics.Select(
                    diagnostic =>
                        diagnostic.Message));
        }
        finally
        {
            File.Delete(
                filePath);
        }
    }

    private static void AssertSemanticFailure(
        StepImportResult result)
    {
        Assert.Equal(
            StepImportStatus.Failed,
            result.Status);

        Assert.Null(
            result.Summary);

        StepImportDiagnostic diagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            StepImportApplicationService.SemanticFailureCode,
            diagnostic.Code);

        Assert.Equal(
            StepImportDiagnosticSeverity.Error,
            diagnostic.Severity);

        Assert.Equal(
            "#180",
            diagnostic.SourceEntityId);

        Assert.Equal(
            "STEP semantic entity '#180' " +
            "of type 'DEGENERATE_TOROIDAL_SURFACE' " +
            "rejected parameter 'minorRadius' " +
            "with value '10'.",
            diagnostic.Message);

        Assert.DoesNotContain(
            result.Diagnostics,
            item =>
                item.Code ==
                StepImportApplicationService.UnexpectedFailureCode);
    }
}
