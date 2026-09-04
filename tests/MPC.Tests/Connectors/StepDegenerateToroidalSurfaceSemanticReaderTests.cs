using System.Globalization;
using MPC.Connectors;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepDegenerateToroidalSurfaceSemanticReaderTests
{
    [Theory]
    [InlineData(".T.", true)]
    [InlineData(".F.", false)]
    public void Read_ShouldCreateDegenerateToroidalSurfaceSemanticEntity(
        string selectOuterToken,
        bool expectedSelectOuter)
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                $"#180=DEGENERATE_TOROIDAL_SURFACE('Degenerate Torus',#100,5.0,10.0,{selectOuterToken});");

        var result =
            Assert.IsType<StepDegenerateToroidalSurfaceEntity>(
                reader.Read(record));

        Assert.IsAssignableFrom<StepToroidalSurfaceEntity>(
            result);

        Assert.Equal(
            "#180",
            result.SourceEntityId);

        Assert.Equal(
            "DEGENERATE_TOROIDAL_SURFACE",
            result.SourceEntityType);

        Assert.Equal(
            "Degenerate Torus",
            result.Name);

        Assert.Equal(
            "#100",
            result.PlacementReferenceId);

        Assert.Equal(
            5.0,
            result.MajorRadius);

        Assert.Equal(
            10.0,
            result.MinorRadius);

        Assert.Equal(
            expectedSelectOuter,
            result.SelectOuter);
    }

    [Fact]
    public void Read_ShouldAcceptIntegerRadiiWithoutNormalization()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#180=DEGENERATE_TOROIDAL_SURFACE('',#100,5,10,.T.);");

        var result =
            Assert.IsType<StepDegenerateToroidalSurfaceEntity>(
                reader.Read(record));

        Assert.Equal(
            5.0,
            result.MajorRadius);

        Assert.Equal(
            10.0,
            result.MinorRadius);
    }

    [Theory]
    [InlineData(0.0, 10.0, "majorRadius", "0")]
    [InlineData(-5.0, 10.0, "majorRadius", "-5")]
    [InlineData(5.0, 0.0, "minorRadius", "0")]
    [InlineData(5.0, -10.0, "minorRadius", "-10")]
    [InlineData(10.0, 10.0, "minorRadius", "10")]
    [InlineData(11.0, 10.0, "minorRadius", "10")]
    public void Read_InvalidRadius_ReportsEntityScopedSemanticFailure(
        double majorRadius,
        double minorRadius,
        string expectedParameterName,
        string expectedRejectedValue)
    {
        var reader =
            CreateReader();

        string rawStatement =
            FormattableString.Invariant(
                $"#180=DEGENERATE_TOROIDAL_SURFACE('',#100,{majorRadius:R},{minorRadius:R},.T.);");

        var record =
            CreateRecord(
                rawStatement);

        StepSemanticValueException exception =
            Assert.Throws<StepSemanticValueException>(
                () =>
                    reader.Read(record));

        Assert.Equal(
            "#180",
            exception.SourceEntityId);

        Assert.Equal(
            "DEGENERATE_TOROIDAL_SURFACE",
            exception.SourceEntityType);

        Assert.Equal(
            expectedParameterName,
            exception.ParameterName);

        Assert.Equal(
            expectedRejectedValue,
            exception.RejectedValue);

        ArgumentOutOfRangeException innerException =
            Assert.IsType<ArgumentOutOfRangeException>(
                exception.InnerException);

        Assert.Equal(
            expectedParameterName,
            innerException.ParamName);
    }

    [Theory]
    [InlineData(
        "#180=DEGENERATE_TOROIDAL_SURFACE('',#100,5.0,10.0);")]
    [InlineData(
        "#180=DEGENERATE_TOROIDAL_SURFACE('',#100,5.0,10.0,.T.,#200);")]
    public void Read_ShouldRejectIncorrectParameterCount(
        string rawStatement)
    {
        var reader =
            CreateReader();

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    CreateRecord(
                        rawStatement)));
    }

    [Theory]
    [InlineData(
        "#180=DEGENERATE_TOROIDAL_SURFACE(#1,#100,5.0,10.0,.T.);")]
    [InlineData(
        "#180=DEGENERATE_TOROIDAL_SURFACE('',100,5.0,10.0,.T.);")]
    [InlineData(
        "#180=DEGENERATE_TOROIDAL_SURFACE('',#100,'5.0',10.0,.T.);")]
    [InlineData(
        "#180=DEGENERATE_TOROIDAL_SURFACE('',#100,5.0,'10.0',.T.);")]
    [InlineData(
        "#180=DEGENERATE_TOROIDAL_SURFACE('',#100,5.0,10.0,'T');")]
    public void Read_ShouldRejectInvalidParameterType(
        string rawStatement)
    {
        var reader =
            CreateReader();

        Assert.Throws<FormatException>(
            () =>
                reader.Read(
                    CreateRecord(
                        rawStatement)));
    }

    [Fact]
    public void Read_ShouldRejectUnsupportedSelectOuterEnumeration()
    {
        var reader =
            CreateReader();

        var record =
            CreateRecord(
                "#180=DEGENERATE_TOROIDAL_SURFACE('',#100,5.0,10.0,.UNKNOWN.);");

        Assert.Throws<FormatException>(
            () =>
                reader.Read(record));
    }

    [Fact]
    public void Handler_ShouldRejectDifferentEntityType()
    {
        var handler =
            new StepDegenerateToroidalSurfaceEntityHandler();

        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#180",
                sourceEntityType:
                    "TOROIDAL_SURFACE",
                rawStatement:
                    "#180=TOROIDAL_SURFACE('',#100,5.0,10.0);",
                readStatus:
                    CadReadStatus.Complete);

        Assert.Throws<InvalidOperationException>(
            () =>
                handler.Read(
                    record,
                    Array.Empty<StepParameter>()));
    }

    [Fact]
    public void Handler_ShouldThrowWhenRecordIsNull()
    {
        var handler =
            new StepDegenerateToroidalSurfaceEntityHandler();

        Assert.Throws<ArgumentNullException>(
            () =>
                handler.Read(
                    null!,
                    Array.Empty<StepParameter>()));
    }

    [Fact]
    public void Handler_ShouldThrowWhenParametersAreNull()
    {
        var handler =
            new StepDegenerateToroidalSurfaceEntityHandler();

        Assert.Throws<ArgumentNullException>(
            () =>
                handler.Read(
                    CreateRecord(
                        "#180=DEGENERATE_TOROIDAL_SURFACE('',#100,5.0,10.0,.T.);"),
                    null!));
    }

    private static StepSemanticEntityReader CreateReader()
    {
        return new StepSemanticEntityReader(
            parameterParser:
                new StepParameterParser(),
            handlers:
                new IStepSemanticEntityHandler[]
                {
                    new StepDegenerateToroidalSurfaceEntityHandler()
                });
    }

    private static StepDataEntityRecord CreateRecord(
        string rawStatement)
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#180",
            sourceEntityType:
                "DEGENERATE_TOROIDAL_SURFACE",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }
}
