using MPC.Application.Step;

namespace MPC.Application.Tests.Step;

public sealed class StepBSplineSurfaceWithKnotsCatalogTests
{
    [Fact]
    public void Execute_InvalidBSplineSurface_UsesRegisteredSemanticHandler()
    {
        string filePath =
            Path.Combine(
                Path.GetTempPath(),
                $"MPC.P01B12.{Guid.NewGuid():N}.step");

        try
        {
            File.WriteAllText(
                filePath,
                """
                ISO-10303-21;
                HEADER;
                ENDSEC;
                DATA;
                #75=B_SPLINE_SURFACE_WITH_KNOTS(
                    'NONE',
                    0,
                    1,
                    (
                        (#1,#2),
                        (#3,#4),
                        (#5,#6),
                        (#7,#8)
                    ),
                    .UNSPECIFIED.,
                    .F.,
                    .F.,
                    .F.,
                    (4,4),
                    (2,2),
                    (0.0,1.0),
                    (0.0,1.0),
                    .UNSPECIFIED.
                );
                ENDSEC;
                END-ISO-10303-21;
                """);

            var service =
                new StepImportApplicationService();

            var request =
                new StepImportRequest(
                    filePath);

            StepImportResult first =
                service.Execute(
                    request);

            StepImportResult second =
                service.Execute(
                    request);

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
            "#75",
            diagnostic.SourceEntityId);

        Assert.Equal(
            "STEP semantic entity '#75' " +
            "of type 'B_SPLINE_SURFACE_WITH_KNOTS' " +
            "rejected parameter 'uDegree' " +
            "with value '0'.",
            diagnostic.Message);

        Assert.DoesNotContain(
            result.Diagnostics,
            item =>
                item.Code ==
                StepImportApplicationService
                    .UnexpectedFailureCode);
    }
}
