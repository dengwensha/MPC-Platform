using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepLineSemanticReaderTests
{
    [Fact]
    public void Read_ShouldCreateLineSemanticEntity()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#50=LINE('Main Line',#10,#40);");

        var result =
            reader.Read(
                record);

        var line =
            Assert.IsType<StepLineEntity>(
                result);

        Assert.Equal(
            "#50",
            line.SourceEntityId);

        Assert.Equal(
            "LINE",
            line.SourceEntityType);

        Assert.Equal(
            "Main Line",
            line.Name);

        Assert.Equal(
            "#10",
            line.PointReferenceId);

        Assert.Equal(
            "#40",
            line.VectorReferenceId);
    }

    [Fact]
    public void Read_ShouldPreserveEmptyName()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#50=LINE('',#10,#40);");

        var result =
            reader.Read(
                record);

        var line =
            Assert.IsType<StepLineEntity>(
                result);

        Assert.Equal(
            string.Empty,
            line.Name);
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCount()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#50=LINE('',#10);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectExtraParameter()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#50=LINE('',#10,#40,#60);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNameThatIsNotString()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#50=LINE(#1,#10,#40);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNullPointReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#50=LINE('',$,#40);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectPointThatIsNotEntityReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#50=LINE('',10,#40);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNullVectorReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#50=LINE('',#10,$);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectVectorThatIsNotEntityReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#50=LINE('',#10,40);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    private static StepSemanticEntityReader
        CreateReader()
    {
        return new StepSemanticEntityReader(
            parameterParser:
                new StepParameterParser(),
            handlers:
                new IStepSemanticEntityHandler[]
                {
                    new StepLineEntityHandler()
                });
    }

    private static StepDataEntityRecord
        CreateRecord(
            string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#50",
            sourceEntityType:
                "LINE",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}