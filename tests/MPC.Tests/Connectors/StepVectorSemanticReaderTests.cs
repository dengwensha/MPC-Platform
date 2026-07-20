using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepVectorSemanticReaderTests
{
    [Fact]
    public void Read_ShouldCreateVectorSemanticEntity()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#40=VECTOR('Axis Vector',#20,25.5);");

        var result =
            reader.Read(
                record);

        var vector =
            Assert.IsType<StepVectorEntity>(
                result);

        Assert.Equal(
            "#40",
            vector.SourceEntityId);

        Assert.Equal(
            "VECTOR",
            vector.SourceEntityType);

        Assert.Equal(
            "Axis Vector",
            vector.Name);

        Assert.Equal(
            "#20",
            vector.OrientationReferenceId);

        Assert.Equal(
            25.5,
            vector.Magnitude);
    }

    [Fact]
    public void Read_ShouldAcceptIntegerMagnitude()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#40=VECTOR('',#20,25);");

        var result =
            reader.Read(
                record);

        var vector =
            Assert.IsType<StepVectorEntity>(
                result);

        Assert.Equal(
            25.0,
            vector.Magnitude);
    }

    [Fact]
    public void Read_ShouldPreserveZeroMagnitude()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#40=VECTOR('',#20,0.0);");

        var result =
            reader.Read(
                record);

        var vector =
            Assert.IsType<StepVectorEntity>(
                result);

        Assert.Equal(
            0.0,
            vector.Magnitude);
    }

    [Fact]
    public void Read_ShouldPreserveNegativeMagnitude()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#40=VECTOR('',#20,-5.0);");

        var result =
            reader.Read(
                record);

        var vector =
            Assert.IsType<StepVectorEntity>(
                result);

        Assert.Equal(
            -5.0,
            vector.Magnitude);
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCount()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#40=VECTOR('',#20);");

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
                "#40=VECTOR(#10,#20,25.0);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNullOrientationReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#40=VECTOR('',$,25.0);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectOrientationThatIsNotEntityReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#40=VECTOR('',20,25.0);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectMagnitudeThatIsNotNumeric()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#40=VECTOR('',#20,'25.0');");

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
                    new StepVectorEntityHandler()
                });
    }

    private static StepDataEntityRecord
        CreateRecord(
            string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#40",
            sourceEntityType:
                "VECTOR",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}