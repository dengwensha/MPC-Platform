using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepPlaneSemanticReaderTests
{
    [Fact]
    public void Read_ShouldCreatePlaneSemanticEntity()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#120=PLANE('Base Plane',#100);");

        var result =
            reader.Read(
                record);

        var plane =
            Assert.IsType<StepPlaneEntity>(
                result);

        Assert.Equal(
            "#120",
            plane.SourceEntityId);

        Assert.Equal(
            "PLANE",
            plane.SourceEntityType);

        Assert.Equal(
            "Base Plane",
            plane.Name);

        Assert.Equal(
            "#100",
            plane.PlacementReferenceId);
    }

    [Fact]
    public void Read_ShouldPreserveEmptyName()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#120=PLANE('',#100);");

        var result =
            reader.Read(
                record);

        var plane =
            Assert.IsType<StepPlaneEntity>(
                result);

        Assert.Equal(
            string.Empty,
            plane.Name);
    }

    [Fact]
    public void Read_ShouldPreservePlacementReferenceExactly()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#120=PLANE('Base Plane',#987);");

        var result =
            reader.Read(
                record);

        var plane =
            Assert.IsType<StepPlaneEntity>(
                result);

        Assert.Equal(
            "#987",
            plane.PlacementReferenceId);
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCountWhenPlacementIsMissing()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#120=PLANE('Base Plane');");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectIncorrectParameterCountWhenExtraParameterExists()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#120=PLANE('Base Plane',#100,#101);");

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
                "#120=PLANE(#1,#100);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectNullPlacementReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#120=PLANE('Base Plane',$);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    [Fact]
    public void Read_ShouldRejectPlacementThatIsNotEntityReference()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#120=PLANE('Base Plane',100);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    record));
    }

    private static StepSemanticEntityReader CreateReader()
    {
        return new StepSemanticEntityReader(
            parameterParser:
                new StepParameterParser(),
            handlers:
                new IStepSemanticEntityHandler[]
                {
                    new StepPlaneEntityHandler()
                });
    }

    private static StepDataEntityRecord CreateRecord(
        string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#120",
            sourceEntityType:
                "PLANE",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}