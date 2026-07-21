using System.Globalization;
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

    [Fact]
    public void Parse_WithIntegerParameter_ReturnsIntegerParameter()
    {
        const string parameterText =
            """
            42
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        var integerParameter =
            Assert.IsType<StepIntegerParameter>(
                Assert.Single(
                    parameters));

        Assert.Equal(
            42L,
            integerParameter.Value);

        Assert.Equal(
            "42",
            integerParameter.RawText);
    }

    [Fact]
    public void Parse_WithNegativeIntegerParameter_PreservesSignedValue()
    {
        const string parameterText =
            """
            -42
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        var integerParameter =
            Assert.IsType<StepIntegerParameter>(
                Assert.Single(
                    parameters));

        Assert.Equal(
            -42L,
            integerParameter.Value);

        Assert.Equal(
            "-42",
            integerParameter.RawText);
    }

    [Fact]
    public void Parse_WithEnumerationParameter_ReturnsEnumerationParameter()
    {
        const string parameterText =
            """
            .UNSPECIFIED.
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        var enumerationParameter =
            Assert.IsType<StepEnumerationParameter>(
                Assert.Single(
                    parameters));

        Assert.Equal(
            "UNSPECIFIED",
            enumerationParameter.Name);

        Assert.Equal(
            ".UNSPECIFIED.",
            enumerationParameter.RawText);
    }

    [Theory]
    [InlineData(".T.", "T")]
    [InlineData(".F.", "F")]
    [InlineData(".UNKNOWN.", "UNKNOWN")]
    public void Parse_WithEnumerationParameter_PreservesEnumerationToken(
        string parameterText,
        string expectedValue)
    {
        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        var enumerationParameter =
            Assert.IsType<StepEnumerationParameter>(
                Assert.Single(
                    parameters));

        Assert.Equal(
            expectedValue,
            enumerationParameter.Name);

        Assert.Equal(
            parameterText,
            enumerationParameter.RawText);
    }

    [Fact]
    public void Parse_WithNullParameter_ReturnsNullParameter()
    {
        const string parameterText =
            """
            $
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        var nullParameter =
            Assert.IsType<StepNullParameter>(
                Assert.Single(
                    parameters));

        Assert.Equal(
            "$",
            nullParameter.RawText);
    }

    [Fact]
    public void Parse_WithOmittedParameter_ReturnsOmittedParameter()
    {
        const string parameterText =
            """
            *
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        var omittedParameter =
            Assert.IsType<StepOmittedParameter>(
                Assert.Single(
                    parameters));

        Assert.Equal(
            "*",
            omittedParameter.RawText);
    }

    [Fact]
    public void Parse_WithNullAndOmittedParameters_PreservesSemanticDifference()
    {
        const string parameterText =
            """
            $,
            *
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        Assert.Equal(
            2,
            parameters.Count);

        Assert.IsType<StepNullParameter>(
            parameters[0]);

        Assert.IsType<StepOmittedParameter>(
            parameters[1]);
    }

    [Fact]
    public void Parse_WithListParameter_PreservesItemOrder()
    {
        const string parameterText =
            """
            (1.0, 2.0, 3.0)
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        var listParameter =
            Assert.IsType<StepAggregateParameter>(
                Assert.Single(
                    parameters));

        Assert.Equal(
            "(1.0, 2.0, 3.0)",
            listParameter.RawText);

        Assert.Collection(
            listParameter.Items,
            parameter =>
            {
                var value =
                    Assert.IsType<StepRealParameter>(
                        parameter);

                Assert.Equal(
                    1.0,
                    value.Value,
                    precision:
                        10);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepRealParameter>(
                        parameter);

                Assert.Equal(
                    2.0,
                    value.Value,
                    precision:
                        10);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepRealParameter>(
                        parameter);

                Assert.Equal(
                    3.0,
                    value.Value,
                    precision:
                        10);
            });
    }

    [Fact]
    public void Parse_WithMixedListParameter_PreservesValueTypesAndOrder()
    {
        const string parameterText =
            """
            ('Point', #12, 5, 7.5, .T., $, *)
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        var listParameter =
            Assert.IsType<StepAggregateParameter>(
                Assert.Single(
                    parameters));

        Assert.Collection(
            listParameter.Items,
            parameter =>
            {
                var value =
                    Assert.IsType<StepStringParameter>(
                        parameter);

                Assert.Equal(
                    "Point",
                    value.Value);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepEntityReferenceParameter>(
                        parameter);

                Assert.Equal(
                    "#12",
                    value.EntityId);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepIntegerParameter>(
                        parameter);

                Assert.Equal(
                    5L,
                    value.Value);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepRealParameter>(
                        parameter);

                Assert.Equal(
                    7.5,
                    value.Value,
                    precision:
                        10);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepEnumerationParameter>(
                        parameter);

                Assert.Equal(
                    "T",
                    value.Name);
            },
            parameter =>
            {
                Assert.IsType<StepNullParameter>(
                    parameter);
            },
            parameter =>
            {
                Assert.IsType<StepOmittedParameter>(
                    parameter);
            });
    }

    [Fact]
    public void Parse_WithNestedLists_PreservesHierarchy()
    {
        const string parameterText =
            """
            ((1.0, 2.0), (#10, #11), ('A', 'B'))
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        var outerList =
            Assert.IsType<StepAggregateParameter>(
                Assert.Single(
                    parameters));

        Assert.Equal(
            3,
            outerList.Items.Count);

        var coordinateList =
            Assert.IsType<StepAggregateParameter>(
                outerList.Items[0]);

        Assert.Collection(
            coordinateList.Items,
            parameter =>
            {
                var value =
                    Assert.IsType<StepRealParameter>(
                        parameter);

                Assert.Equal(
                    1.0,
                    value.Value,
                    precision:
                        10);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepRealParameter>(
                        parameter);

                Assert.Equal(
                    2.0,
                    value.Value,
                    precision:
                        10);
            });

        var referenceList =
            Assert.IsType<StepAggregateParameter>(
                outerList.Items[1]);

        Assert.Collection(
            referenceList.Items,
            parameter =>
            {
                var value =
                    Assert.IsType<StepEntityReferenceParameter>(
                        parameter);

                Assert.Equal(
                    "#10",
                    value.EntityId);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepEntityReferenceParameter>(
                        parameter);

                Assert.Equal(
                    "#11",
                    value.EntityId);
            });

        var stringList =
            Assert.IsType<StepAggregateParameter>(
                outerList.Items[2]);

        Assert.Collection(
            stringList.Items,
            parameter =>
            {
                var value =
                    Assert.IsType<StepStringParameter>(
                        parameter);

                Assert.Equal(
                    "A",
                    value.Value);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepStringParameter>(
                        parameter);

                Assert.Equal(
                    "B",
                    value.Value);
            });
    }

    [Fact]
    public void Parse_WithTypedParameterContainingMultipleValues_PreservesNestedOrder()
    {
        const string parameterText =
            """
            CUSTOM_VALUE('A', #25, 12.5)
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        var typedParameter =
            Assert.IsType<StepTypedParameter>(
                Assert.Single(
                    parameters));

        Assert.Equal(
            "CUSTOM_VALUE",
            typedParameter.TypeName);

        Assert.Collection(
            typedParameter.Parameters,
            parameter =>
            {
                var value =
                    Assert.IsType<StepStringParameter>(
                        parameter);

                Assert.Equal(
                    "A",
                    value.Value);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepEntityReferenceParameter>(
                        parameter);

                Assert.Equal(
                    "#25",
                    value.EntityId);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepRealParameter>(
                        parameter);

                Assert.Equal(
                    12.5,
                    value.Value,
                    precision:
                        10);
            });
    }

    [Theory]
    [InlineData("1.25E+03", 1250.0)]
    [InlineData("1.25E3", 1250.0)]
    [InlineData("-2.5E-02", -0.025)]
    [InlineData("0.0", 0.0)]
    public void Parse_WithExponentReal_ReturnsExpectedValue(
        string parameterText,
        double expectedValue)
    {
        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        var realParameter =
            Assert.IsType<StepRealParameter>(
                Assert.Single(
                    parameters));

        Assert.Equal(
            expectedValue,
            realParameter.Value,
            precision:
                10);

        Assert.Equal(
            parameterText,
            realParameter.RawText);
    }

    [Fact]
    public void Parse_WithRealParameter_IsIndependentFromCurrentCulture()
    {
        var originalCulture =
            CultureInfo.CurrentCulture;

        var originalUiCulture =
            CultureInfo.CurrentUICulture;

        try
        {
            var turkishCulture =
                CultureInfo.GetCultureInfo(
                    "tr-TR");

            CultureInfo.CurrentCulture =
                turkishCulture;

            CultureInfo.CurrentUICulture =
                turkishCulture;

            const string parameterText =
                """
                12.5,
                -3.0,
                1.25E+03
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
                        Assert.IsType<StepRealParameter>(
                            parameter);

                    Assert.Equal(
                        12.5,
                        value.Value,
                        precision:
                            10);
                },
                parameter =>
                {
                    var value =
                        Assert.IsType<StepRealParameter>(
                            parameter);

                    Assert.Equal(
                        -3.0,
                        value.Value,
                        precision:
                            10);
                },
                parameter =>
                {
                    var value =
                        Assert.IsType<StepRealParameter>(
                            parameter);

                    Assert.Equal(
                        1250.0,
                        value.Value,
                        precision:
                            10);
                });
        }
        finally
        {
            CultureInfo.CurrentCulture =
                originalCulture;

            CultureInfo.CurrentUICulture =
                originalUiCulture;
        }
    }

    [Fact]
    public void Parse_WithMultipleTopLevelParameters_PreservesExactLogicalOrder()
    {
        const string parameterText =
            """
            'A',
            10,
            20.5,
            #30,
            .F.,
            $,
            *,
            (1.0, 2.0),
            LENGTH_MEASURE(15.0)
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
                    "A",
                    value.Value);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepIntegerParameter>(
                        parameter);

                Assert.Equal(
                    10L,
                    value.Value);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepRealParameter>(
                        parameter);

                Assert.Equal(
                    20.5,
                    value.Value,
                    precision:
                        10);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepEntityReferenceParameter>(
                        parameter);

                Assert.Equal(
                    "#30",
                    value.EntityId);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepEnumerationParameter>(
                        parameter);

                Assert.Equal(
                    "F",
                    value.Name);
            },
            parameter =>
            {
                Assert.IsType<StepNullParameter>(
                    parameter);
            },
            parameter =>
            {
                Assert.IsType<StepOmittedParameter>(
                    parameter);
            },
            parameter =>
            {
                Assert.IsType<StepAggregateParameter>(
                    parameter);
            },
            parameter =>
            {
                var value =
                    Assert.IsType<StepTypedParameter>(
                        parameter);

                Assert.Equal(
                    "LENGTH_MEASURE",
                    value.TypeName);
            });
    }
}
