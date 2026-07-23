using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class StepAdvancedFaceEntityHandlerTests
{
    [Fact]
    public void Read_WithValidAdvancedFaceRecord_ReturnsSemanticAdvancedFace()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#400",
                sourceEntityType:
                    "ADVANCED_FACE",
                rawStatement:
                    "#400=ADVANCED_FACE('Top Face',(#300,#301),#250,.T.);",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepAdvancedFaceEntityHandler()
                    });

        var result =
            reader.Read(
                record);

        var advancedFace =
            Assert.IsType<StepAdvancedFaceEntity>(
                result);

        Assert.Equal(
            "#400",
            advancedFace.SourceEntityId);

        Assert.Equal(
            "ADVANCED_FACE",
            advancedFace.SourceEntityType);

        Assert.Equal(
            "Top Face",
            advancedFace.Name);

        Assert.Equal(
            new[]
            {
                "#300",
                "#301"
            },
            advancedFace.BoundReferenceIds);

        Assert.Equal(
            "#250",
            advancedFace.SurfaceReferenceId);

        Assert.True(
            advancedFace.SameSense);
    }

    [Fact]
    public void Read_WithFalseSameSense_ReturnsFalse()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#401",
                sourceEntityType:
                    "ADVANCED_FACE",
                rawStatement:
                    "#401=ADVANCED_FACE('',(#300),#250,.F.);",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepAdvancedFaceEntityHandler()
                    });

        var result =
            reader.Read(
                record);

        var advancedFace =
            Assert.IsType<StepAdvancedFaceEntity>(
                result);

        Assert.False(
            advancedFace.SameSense);
    }

    [Fact]
    public void Read_WithEmptyBoundAggregate_ThrowsFormatException()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#402",
                sourceEntityType:
                    "ADVANCED_FACE",
                rawStatement:
                    "#402=ADVANCED_FACE('',(),#250,.T.);",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepAdvancedFaceEntityHandler()
                    });

        var exception =
            Assert.Throws<FormatException>(
                () =>
                    reader.Read(
                        record));

        Assert.Equal(
            "ADVANCED_FACE face bound aggregate must contain at least one entity reference.",
            exception.Message);
    }

    [Fact]
    public void Read_WithNonReferenceBoundAggregateItem_ThrowsFormatException()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#403",
                sourceEntityType:
                    "ADVANCED_FACE",
                rawStatement:
                    "#403=ADVANCED_FACE('',(#300,'Invalid'),#250,.T.);",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepAdvancedFaceEntityHandler()
                    });

        var exception =
            Assert.Throws<FormatException>(
                () =>
                    reader.Read(
                        record));

        Assert.Equal(
            "ADVANCED_FACE aggregate item at index 1 must be a STEP entity reference.",
            exception.Message);
    }

    [Fact]
    public void Read_WithInvalidSameSense_ThrowsFormatException()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#404",
                sourceEntityType:
                    "ADVANCED_FACE",
                rawStatement:
                    "#404=ADVANCED_FACE('',(#300),#250,.UNKNOWN.);",
                readStatus:
                    CadReadStatus.Complete);

        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepAdvancedFaceEntityHandler()
                    });

        var exception =
            Assert.Throws<FormatException>(
                () =>
                    reader.Read(
                        record));

        Assert.Equal(
            "ADVANCED_FACE same sense must be .T. or .F., but '.UNKNOWN.' was provided.",
            exception.Message);
    }
}