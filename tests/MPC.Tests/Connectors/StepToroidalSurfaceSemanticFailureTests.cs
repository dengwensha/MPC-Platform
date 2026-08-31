using MPC.Connectors;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepToroidalSurfaceSemanticFailureTests
{
    [Fact]
    public void Read_NegativeMajorRadius_ReportsEntityScopedSemanticFailure()
    {
        var reader =
            new StepSemanticEntityReader(
                new StepParameterParser(),
                new IStepSemanticEntityHandler[]
                {
                    new StepToroidalSurfaceEntityHandler()
                });

        var record =
            new StepDataEntityRecord(
                sourceEntityId: "#170",
                sourceEntityType: "TOROIDAL_SURFACE",
                rawStatement:
                    "#170=TOROIDAL_SURFACE('',#100,-178.5,10.0);",
                readStatus: CadReadStatus.Complete);

        Exception exception =
            Assert.ThrowsAny<Exception>(
                () =>
                    reader.Read(record));

        Assert.Equal(
            "StepSemanticValueException",
            exception.GetType().Name);

        Assert.Equal(
            "#170",
            ReadStringProperty(
                exception,
                "SourceEntityId"));

        Assert.Equal(
            "TOROIDAL_SURFACE",
            ReadStringProperty(
                exception,
                "SourceEntityType"));

        Assert.Equal(
            "majorRadius",
            ReadStringProperty(
                exception,
                "ParameterName"));

        Assert.Equal(
            "-178.5",
            ReadStringProperty(
                exception,
                "RejectedValue"));

        Assert.IsType<ArgumentOutOfRangeException>(
            exception.InnerException);
    }

    private static string? ReadStringProperty(
        Exception exception,
        string propertyName)
    {
        return exception
            .GetType()
            .GetProperty(propertyName)?
            .GetValue(exception) as string;
    }
}