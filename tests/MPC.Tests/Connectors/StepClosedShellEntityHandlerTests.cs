using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class StepClosedShellEntityHandlerTests
{
    [Fact]
    public void Read_WithValidClosedShellRecord_ReturnsSemanticClosedShell()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#500",
                sourceEntityType:
                    "CLOSED_SHELL",
                rawStatement:
                    "#500=CLOSED_SHELL('Main Shell',(#400,#401,#402));",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepClosedShellEntityHandler()
                    });

        var result =
            reader.Read(
                record);

        var shell =
            Assert.IsType<StepClosedShellEntity>(
                result);

        Assert.Equal(
            "#500",
            shell.SourceEntityId);

        Assert.Equal(
            "CLOSED_SHELL",
            shell.SourceEntityType);

        Assert.Equal(
            "Main Shell",
            shell.Name);

        Assert.Equal(
            new[]
            {
                "#400",
                "#401",
                "#402"
            },
            shell.FaceReferenceIds);
    }

    [Fact]
    public void Read_WithEmptyAggregate_ThrowsFormatException()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#501",
                sourceEntityType:
                    "CLOSED_SHELL",
                rawStatement:
                    "#501=CLOSED_SHELL('',());",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepClosedShellEntityHandler()
                    });

        var exception =
            Assert.Throws<FormatException>(
                () => reader.Read(record));

        Assert.Equal(
            "CLOSED_SHELL face aggregate must contain at least one entity reference.",
            exception.Message);
    }

    [Fact]
    public void Read_WithInvalidAggregateItem_ThrowsFormatException()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#502",
                sourceEntityType:
                    "CLOSED_SHELL",
                rawStatement:
                    "#502=CLOSED_SHELL('',(#400,'ABC'));",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepClosedShellEntityHandler()
                    });

        var exception =
            Assert.Throws<FormatException>(
                () => reader.Read(record));

        Assert.Equal(
            "CLOSED_SHELL aggregate item at index 1 must be a STEP entity reference.",
            exception.Message);
    }
}