using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepAxis2Placement3DSemanticReaderTests
{
    [Fact]
    public void Read_ShouldCreateAxis2Placement3DSemanticEntity()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#100=AXIS2_PLACEMENT_3D('Main Axis',#10,#20,#30);");

        var result =
            reader.Read(
                record);

        var placement =
            Assert.IsType<StepAxis2Placement3DEntity>(
                result);

        Assert.Equal(
            "#100",
            placement.SourceEntityId);

        Assert.Equal(
            "AXIS2_PLACEMENT_3D",
            placement.SourceEntityType);

        Assert.Equal(
            "Main Axis",
            placement.Name);

        Assert.Equal(
            "#10",
            placement.LocationReferenceId);

        Assert.Equal(
            "#20",
            placement.AxisReferenceId);

        Assert.Equal(
            "#30",
            placement.RefDirectionReferenceId);
    }

    [Fact]
    public void Read_ShouldPreserveNullOptionalReferences()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#100=AXIS2_PLACEMENT_3D('',#10,$,$);");

        var result =
            reader.Read(
                record);

        var placement =
            Assert.IsType<StepAxis2Placement3DEntity>(
                result);

        Assert.Equal(
            "#10",
            placement.LocationReferenceId);

        Assert.Null(
            placement.AxisReferenceId);

        Assert.Null(
            placement.RefDirectionReferenceId);
    }

    [Fact]
    public void Read_ShouldPreserveAxisWhenRefDirectionIsNull()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#100=AXIS2_PLACEMENT_3D('',#10,#20,$);");

        var result =
            reader.Read(
                record);

        var placement =
            Assert.IsType<StepAxis2Placement3DEntity>(
                result);

        Assert.Equal(
            "#20",
            placement.AxisReferenceId);

        Assert.Null(
            placement.RefDirectionReferenceId);
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCount()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#100=AXIS2_PLACEMENT_3D('',#10,#20);");

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
                "#100=AXIS2_PLACEMENT_3D(#1,#10,#20,#30);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNullLocationReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#100=AXIS2_PLACEMENT_3D('',$,#20,#30);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectLocationThatIsNotEntityReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#100=AXIS2_PLACEMENT_3D('',10,#20,#30);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectAxisThatIsNotEntityReferenceOrNull()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#100=AXIS2_PLACEMENT_3D('',#10,20,#30);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectRefDirectionThatIsNotEntityReferenceOrNull()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#100=AXIS2_PLACEMENT_3D('',#10,#20,30);");

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
                    new StepAxis2Placement3DEntityHandler()
                });
    }

    private static StepDataEntityRecord
        CreateRecord(
            string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#100",
            sourceEntityType:
                "AXIS2_PLACEMENT_3D",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}