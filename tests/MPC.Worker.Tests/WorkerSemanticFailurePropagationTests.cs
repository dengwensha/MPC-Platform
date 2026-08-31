using MPC.Contracts;
using MPC.Worker;

namespace MPC.Worker.Tests;

public sealed class WorkerSemanticFailurePropagationTests
{
    [Fact]
    public void Process_NegativeToroidalMajorRadius_PreservesSemanticContext()
    {
        string filePath =
            Path.Combine(
                Path.GetTempPath(),
                $"MPC.P01B03.Worker.{Guid.NewGuid():N}.step");

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

            var processor =
                new WorkerRequestProcessor();

            var request =
                new WorkerOperationRequest(
                    ContractVersion.Current,
                    Guid.NewGuid().ToString(),
                    WorkerOperationKind.AnalyzeModel,
                    filePath);

            WorkerOperationResult result =
                processor.Process(request);

            Assert.Equal(
                WorkerOperationStatus.Failed,
                result.Status);

            Assert.Null(result.Summary);

            ContractDiagnostic diagnostic =
                Assert.Single(result.Diagnostics);

            Assert.Equal(
                "APP-STEP-SEMANTIC-001",
                diagnostic.Code);

            Assert.Equal(
                ContractDiagnosticSeverity.Error,
                diagnostic.Severity);

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
                    "APP-STEP-UNEXPECTED-001");
        }
        finally
        {
            File.Delete(filePath);
        }
    }
}