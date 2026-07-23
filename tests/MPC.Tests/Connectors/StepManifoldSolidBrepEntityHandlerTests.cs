using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class StepManifoldSolidBrepEntityHandlerTests
{
    [Fact]
    public void Read_WithValidRecord_ReturnsSemanticManifoldSolidBrep()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#600",
                sourceEntityType:
                    "MANIFOLD_SOLID_BREP",
                rawStatement:
                    "#600=MANIFOLD_SOLID_BREP('Main Body',#500);",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepManifoldSolidBrepEntityHandler()
                    });

        var result =
            reader.Read(record);

        var solid =
            Assert.IsType<StepManifoldSolidBrepEntity>(
                result);

        Assert.Equal(
            "#600",
            solid.SourceEntityId);

        Assert.Equal(
            "MANIFOLD_SOLID_BREP",
            solid.SourceEntityType);

        Assert.Equal(
            "Main Body",
            solid.Name);

        Assert.Equal(
            "#500",
            solid.OuterShellReferenceId);
    }

    [Fact]
    public void Read_WithInvalidOuterShellParameter_ThrowsFormatException()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#601",
                sourceEntityType:
                    "MANIFOLD_SOLID_BREP",
                rawStatement:
                    "#601=MANIFOLD_SOLID_BREP('Main Body','Invalid');",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepManifoldSolidBrepEntityHandler()
                    });

        var exception =
            Assert.Throws<FormatException>(
                () => reader.Read(record));

        Assert.Equal(
            "MANIFOLD_SOLID_BREP second parameter must be a STEP entity reference.",
            exception.Message);
    }

    [Fact]
    public void Read_WithInvalidParameterCount_ThrowsFormatException()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#602",
                sourceEntityType:
                    "MANIFOLD_SOLID_BREP",
                rawStatement:
                    "#602=MANIFOLD_SOLID_BREP('Main Body');",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepManifoldSolidBrepEntityHandler()
                    });

        var exception =
            Assert.Throws<FormatException>(
                () => reader.Read(record));

        Assert.Equal(
            "MANIFOLD_SOLID_BREP must contain exactly two parameters: name and outer shell reference.",
            exception.Message);
    }
}