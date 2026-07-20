using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;

namespace MPC.Tests.Connectors;

public sealed class StepParameterParserTests
{
    [Fact]
    public void Parse_WithStringReferenceAndTypedReal_PreservesParameterStructureAndOrder()
    {
        const string parameterText =
            """
            'Main Surface',
            #18,
            POSITIVE_LENGTH_MEASURE(20.5)
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        Assert.Equal(
            3,
            parameters.Count);

        var stringParameter =
            Assert.IsType<StepStringParameter>(
                parameters[0]);

        Assert.Equal(
            "Main Surface",
            stringParameter.Value);

        Assert.Equal(
            "'Main Surface'",
            stringParameter.RawText);

        var referenceParameter =
            Assert.IsType<StepEntityReferenceParameter>(
                parameters[1]);

        Assert.Equal(
            "#18",
            referenceParameter.EntityId);

        Assert.Equal(
            "#18",
            referenceParameter.RawText);

        var typedParameter =
            Assert.IsType<StepTypedParameter>(
                parameters[2]);

        Assert.Equal(
            "POSITIVE_LENGTH_MEASURE",
            typedParameter.TypeName);

        Assert.Equal(
            "POSITIVE_LENGTH_MEASURE(20.5)",
            typedParameter.RawText);

        var realParameter =
            Assert.IsType<StepRealParameter>(
                Assert.Single(
                    typedParameter.Parameters));

        Assert.Equal(
            20.5,
            realParameter.Value,
            precision:
                10);

        Assert.Equal(
            "20.5",
            realParameter.RawText);
    }

    [Fact]
    public void Parse_WithWhitespaceAndLineBreaks_PreservesLogicalParameterOrder()
    {
        const string parameterText =
            """
                'Surface A'
                ,
                #200
                ,
                LENGTH_MEASURE
                (
                    15.25
                )
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        Assert.Collection(
            parameters,
            parameter =>
            {
                var value =
                    Assert.IsType<StepStringParameter>(
                        parameter);

                Assert.Equal(
                    "Surface A",
                    value.Value);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepEntityReferenceParameter>(
                        parameter);

                Assert.Equal(
                    "#200",
                    value.EntityId);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepTypedParameter>(
                        parameter);

                Assert.Equal(
                    "LENGTH_MEASURE",
                    value.TypeName);

                var nestedValue =
                    Assert.IsType<StepRealParameter>(
                        Assert.Single(
                            value.Parameters));

                Assert.Equal(
                    15.25,
                    nestedValue.Value,
                    precision:
                        10);
            });
    }

    [Fact]
    public void Parse_WithEscapedApostrophe_DecodesStepStringWithoutBreakingParameterBoundaries()
    {
        const string parameterText =
            """
            'Owner''s Surface',
            #25
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        Assert.Equal(
            2,
            parameters.Count);

        var stringParameter =
            Assert.IsType<StepStringParameter>(
                parameters[0]);

        Assert.Equal(
            "Owner's Surface",
            stringParameter.Value);

        Assert.Equal(
            "'Owner''s Surface'",
            stringParameter.RawText);

        var referenceParameter =
            Assert.IsType<StepEntityReferenceParameter>(
                parameters[1]);

        Assert.Equal(
            "#25",
            referenceParameter.EntityId);
    }
}