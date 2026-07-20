using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;

namespace MPC.Tests.Connectors;

public sealed class StepParameterSpecialValueTests
{
    [Fact]
    public void Parse_WithAggregate_PreservesNestedParameterStructureAndOrder()
    {
        const string parameterText =
            """
            (
                1,
                2.5,
                #40,
                'A',
                LENGTH_MEASURE(8.75)
            )
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        var aggregate =
            Assert.IsType<StepAggregateParameter>(
                Assert.Single(
                    parameters));

        Assert.Equal(
            5,
            aggregate.Items.Count);

        var integerValue =
            Assert.IsType<StepIntegerParameter>(
                aggregate.Items[0]);

        Assert.Equal(
            1L,
            integerValue.Value);

        Assert.Equal(
            "1",
            integerValue.RawText);

        var realValue =
            Assert.IsType<StepRealParameter>(
                aggregate.Items[1]);

        Assert.Equal(
            2.5,
            realValue.Value,
            precision:
                10);

        var referenceValue =
            Assert.IsType<StepEntityReferenceParameter>(
                aggregate.Items[2]);

        Assert.Equal(
            "#40",
            referenceValue.EntityId);

        var stringValue =
            Assert.IsType<StepStringParameter>(
                aggregate.Items[3]);

        Assert.Equal(
            "A",
            stringValue.Value);

        var typedValue =
            Assert.IsType<StepTypedParameter>(
                aggregate.Items[4]);

        Assert.Equal(
            "LENGTH_MEASURE",
            typedValue.TypeName);

        var nestedReal =
            Assert.IsType<StepRealParameter>(
                Assert.Single(
                    typedValue.Parameters));

        Assert.Equal(
            8.75,
            nestedReal.Value,
            precision:
                10);
    }

    [Fact]
    public void Parse_WithNestedAggregate_PreservesRecursiveStructure()
    {
        const string parameterText =
            """
            (
                (1, 2),
                (#10, #11),
                ('A', ('B', 'C'))
            )
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        var root =
            Assert.IsType<StepAggregateParameter>(
                Assert.Single(
                    parameters));

        Assert.Equal(
            3,
            root.Items.Count);

        var integerAggregate =
            Assert.IsType<StepAggregateParameter>(
                root.Items[0]);

        Assert.Collection(
            integerAggregate.Items,
            item =>
            {
                var value =
                    Assert.IsType<StepIntegerParameter>(
                        item);

                Assert.Equal(
                    1L,
                    value.Value);
            },
            item =>
            {
                var value =
                    Assert.IsType<StepIntegerParameter>(
                        item);

                Assert.Equal(
                    2L,
                    value.Value);
            });

        var referenceAggregate =
            Assert.IsType<StepAggregateParameter>(
                root.Items[1]);

        Assert.Collection(
            referenceAggregate.Items,
            item =>
            {
                var value =
                    Assert.IsType<StepEntityReferenceParameter>(
                        item);

                Assert.Equal(
                    "#10",
                    value.EntityId);
            },
            item =>
            {
                var value =
                    Assert.IsType<StepEntityReferenceParameter>(
                        item);

                Assert.Equal(
                    "#11",
                    value.EntityId);
            });

        var stringAggregate =
            Assert.IsType<StepAggregateParameter>(
                root.Items[2]);

        Assert.Equal(
            "A",
            Assert.IsType<StepStringParameter>(
                    stringAggregate.Items[0])
                .Value);

        var nestedStringAggregate =
            Assert.IsType<StepAggregateParameter>(
                stringAggregate.Items[1]);

        Assert.Equal(
            "B",
            Assert.IsType<StepStringParameter>(
                    nestedStringAggregate.Items[0])
                .Value);

        Assert.Equal(
            "C",
            Assert.IsType<StepStringParameter>(
                    nestedStringAggregate.Items[1])
                .Value);
    }

    [Fact]
    public void Parse_WithNullAndOmittedValues_CreatesDistinctParameterTypes()
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

        var nullValue =
            Assert.IsType<StepNullParameter>(
                parameters[0]);

        Assert.Equal(
            "$",
            nullValue.RawText);

        var omittedValue =
            Assert.IsType<StepOmittedParameter>(
                parameters[1]);

        Assert.Equal(
            "*",
            omittedValue.RawText);
    }

    [Fact]
    public void Parse_WithEnumeration_PreservesNormalizedEnumerationName()
    {
        const string parameterText =
            """
            .T.,
            .F.,
            .UNSPECIFIED.,
            .USER_DEFINED_VALUE.
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        Assert.Equal(
            4,
            parameters.Count);

        Assert.Equal(
            "T",
            Assert.IsType<StepEnumerationParameter>(
                    parameters[0])
                .Name);

        Assert.Equal(
            "F",
            Assert.IsType<StepEnumerationParameter>(
                    parameters[1])
                .Name);

        Assert.Equal(
            "UNSPECIFIED",
            Assert.IsType<StepEnumerationParameter>(
                    parameters[2])
                .Name);

        Assert.Equal(
            "USER_DEFINED_VALUE",
            Assert.IsType<StepEnumerationParameter>(
                    parameters[3])
                .Name);

        Assert.Equal(
            ".USER_DEFINED_VALUE.",
            parameters[3].RawText);
    }

    [Fact]
    public void Parse_WithIntegerAndRealValues_CreatesDifferentParameterTypes()
    {
        const string parameterText =
            """
            42,
            -7,
            +15,
            42.0,
            -0.125,
            1.0E+3,
            -2.5E-2
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        Assert.Equal(
            7,
            parameters.Count);

        Assert.Equal(
            42L,
            Assert.IsType<StepIntegerParameter>(
                    parameters[0])
                .Value);

        Assert.Equal(
            -7L,
            Assert.IsType<StepIntegerParameter>(
                    parameters[1])
                .Value);

        Assert.Equal(
            15L,
            Assert.IsType<StepIntegerParameter>(
                    parameters[2])
                .Value);

        Assert.Equal(
            42.0,
            Assert.IsType<StepRealParameter>(
                    parameters[3])
                .Value,
            precision:
                10);

        Assert.Equal(
            -0.125,
            Assert.IsType<StepRealParameter>(
                    parameters[4])
                .Value,
            precision:
                10);

        Assert.Equal(
            1000.0,
            Assert.IsType<StepRealParameter>(
                    parameters[5])
                .Value,
            precision:
                10);

        Assert.Equal(
            -0.025,
            Assert.IsType<StepRealParameter>(
                    parameters[6])
                .Value,
            precision:
                10);
    }

    [Fact]
    public void Parse_WithSpecialValuesInsideTypedParameter_PreservesNestedTypes()
    {
        const string parameterText =
            """
            WRAPPED_VALUE(
                (
                    $,
                    *,
                    .UNSPECIFIED.,
                    10
                )
            )
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
            "WRAPPED_VALUE",
            typedParameter.TypeName);

        var aggregate =
            Assert.IsType<StepAggregateParameter>(
                Assert.Single(
                    typedParameter.Parameters));

        Assert.Collection(
            aggregate.Items,
            item =>
                Assert.IsType<StepNullParameter>(
                    item),
            item =>
                Assert.IsType<StepOmittedParameter>(
                    item),
            item =>
            {
                var value =
                    Assert.IsType<StepEnumerationParameter>(
                        item);

                Assert.Equal(
                    "UNSPECIFIED",
                    value.Name);
            },
            item =>
            {
                var value =
                    Assert.IsType<StepIntegerParameter>(
                        item);

                Assert.Equal(
                    10L,
                    value.Value);
            });
    }
}