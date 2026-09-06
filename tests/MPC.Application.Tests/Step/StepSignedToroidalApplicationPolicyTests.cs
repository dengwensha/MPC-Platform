using MPC.Application.Step;

namespace MPC.Application.Tests.Step;

public sealed class StepSignedToroidalApplicationPolicyTests
{
    [Fact]
    public void Execute_WithExactSwStepFingerprint_AppliesSignedToroidalCompatibility()
    {
        string filePath =
            CreateStepFile(
                preprocessorVersion: "SwSTEP 2.0",
                originatingSystem: "SolidWorks 2019");

        try
        {
            var service =
                new StepImportApplicationService();

            StepImportResult result =
                service.Execute(
                    new StepImportRequest(
                        filePath));

            Assert.Equal(
                StepImportStatus.Failed,
                result.Status);

            Assert.Null(
                result.Summary);

            Assert.Contains(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Code ==
                    StepImportApplicationService.InvalidSolidRootCode);

            Assert.DoesNotContain(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Code ==
                    StepImportApplicationService.SemanticFailureCode);

            Assert.DoesNotContain(
                result.Diagnostics,
                diagnostic =>
                    diagnostic.Code ==
                    StepImportApplicationService.UnexpectedFailureCode);
        }
        finally
        {
            File.Delete(
                filePath);
        }
    }

    [Fact]
    public void Execute_WithDifferentSwStepVersion_PreservesSemanticRejection()
    {
        string filePath =
            CreateStepFile(
                preprocessorVersion: "SwSTEP 2.1",
                originatingSystem: "SolidWorks 2019");

        try
        {
            var service =
                new StepImportApplicationService();

            StepImportResult result =
                service.Execute(
                    new StepImportRequest(
                        filePath));

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
                "#170",
                diagnostic.SourceEntityId);

            Assert.Equal(
                "STEP semantic entity '#170' " +
                "of type 'TOROIDAL_SURFACE' " +
                "rejected parameter 'majorRadius' " +
                "with value '-178.5'.",
                diagnostic.Message);
        }
        finally
        {
            File.Delete(
                filePath);
        }
    }

    private static string CreateStepFile(
        string preprocessorVersion,
        string originatingSystem)
    {
        string filePath =
            Path.Combine(
                Path.GetTempPath(),
                $"MPC.P01B10.{Guid.NewGuid():N}.step");

        string content =
            $"""
            ISO-10303-21;
            HEADER;
            FILE_DESCRIPTION(('STEP AP203'),'1');
            FILE_NAME(
                'Part.step',
                '',
                (''),
                (''),
                '{preprocessorVersion}',
                '{originatingSystem}',
                '');
            FILE_SCHEMA(('CONFIG_CONTROL_DESIGN'));
            ENDSEC;
            DATA;
            #170=TOROIDAL_SURFACE('',#100,-178.5,229.5);
            ENDSEC;
            END-ISO-10303-21;
            """;

        File.WriteAllText(
            filePath,
            content);

        return filePath;
    }
}
