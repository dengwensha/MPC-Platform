using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;

namespace MPC.Tests.Connectors;

public sealed class StepParameterPreservationTests
{
    [Fact]
    public void Parse_WithBinaryParameter_PreservesEncodedValueAndRawText()
    {
        const string parameterText =
            """
            "23B"
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        var binaryParameter =
            Assert.IsType<StepBinaryParameter>(
                Assert.Single(
                    parameters));

        Assert.Equal(
            "\"23B\"",
            binaryParameter.RawText);

        Assert.Equal(
            "23B",
            binaryParameter.EncodedValue);
    }

    [Fact]
    public void Parse_WithBinaryParameters_PreservesPart21Representations()
    {
        const string parameterText =
            """
            "0",
            "30",
            "31",
            "23B",
            "092A"
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        Assert.Equal(
            5,
            parameters.Count);

        Assert.Equal(
            "0",
            Assert.IsType<StepBinaryParameter>(
                    parameters[0])
                .EncodedValue);

        Assert.Equal(
            "30",
            Assert.IsType<StepBinaryParameter>(
                    parameters[1])
                .EncodedValue);

        Assert.Equal(
            "31",
            Assert.IsType<StepBinaryParameter>(
                    parameters[2])
                .EncodedValue);

        Assert.Equal(
            "23B",
            Assert.IsType<StepBinaryParameter>(
                    parameters[3])
                .EncodedValue);

        Assert.Equal(
            "092A",
            Assert.IsType<StepBinaryParameter>(
                    parameters[4])
                .EncodedValue);
    }

    [Fact]
    public void Parse_WithBinaryInsideAggregate_PreservesRecursiveStructure()
    {
        const string parameterText =
            """
            (
                "23B",
                "092A",
                #42
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
            3,
            aggregate.Items.Count);

        Assert.Equal(
            "23B",
            Assert.IsType<StepBinaryParameter>(
                    aggregate.Items[0])
                .EncodedValue);

        Assert.Equal(
            "092A",
            Assert.IsType<StepBinaryParameter>(
                    aggregate.Items[1])
                .EncodedValue);

        Assert.Equal(
            "#42",
            Assert.IsType<StepEntityReferenceParameter>(
                    aggregate.Items[2])
                .EntityId);
    }

    [Fact]
    public void Parse_WithBinaryInsideTypedParameter_PreservesRecursiveStructure()
    {
        const string parameterText =
            """
            BINARY_WRAPPER(
                "23B"
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
            "BINARY_WRAPPER",
            typedParameter.TypeName);

        var binaryParameter =
            Assert.IsType<StepBinaryParameter>(
                Assert.Single(
                    typedParameter.Parameters));

        Assert.Equal(
            "23B",
            binaryParameter.EncodedValue);

        Assert.Equal(
            "\"23B\"",
            binaryParameter.RawText);
    }

    [Fact]
    public void Parse_WithUnknownParameter_PreservesRawTextInsteadOfThrowing()
    {
        const string parameterText =
            """
            VENDOR_DEFINED_TOKEN
            """;

        var parser =
            new StepParameterParser();

        var exception =
            Record.Exception(
                () =>
                    parser.Parse(
                        parameterText));

        Assert.Null(
            exception);

        var parameters =
            parser.Parse(
                parameterText);

        var unknownParameter =
            Assert.IsType<StepUnknownParameter>(
                Assert.Single(
                    parameters));

        Assert.Equal(
            "VENDOR_DEFINED_TOKEN",
            unknownParameter.RawText);
    }

    [Fact]
    public void Parse_WithUnknownParameterAmongKnownParameters_PreservesOrder()
    {
        const string parameterText =
            """
            #10,
            VENDOR_DEFINED_TOKEN,
            42,
            .T.
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
            "#10",
            Assert.IsType<StepEntityReferenceParameter>(
                    parameters[0])
                .EntityId);

        Assert.Equal(
            "VENDOR_DEFINED_TOKEN",
            Assert.IsType<StepUnknownParameter>(
                    parameters[1])
                .RawText);

        Assert.Equal(
            42L,
            Assert.IsType<StepIntegerParameter>(
                    parameters[2])
                .Value);

        Assert.Equal(
            "T",
            Assert.IsType<StepEnumerationParameter>(
                    parameters[3])
                .Name);
    }

    [Fact]
    public void Parse_WithUnknownParameterInsideAggregate_PreservesParentStructure()
    {
        const string parameterText =
            """
            (
                #1,
                VENDOR_DEFINED_TOKEN,
                'A'
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
            3,
            aggregate.Items.Count);

        Assert.IsType<StepEntityReferenceParameter>(
            aggregate.Items[0]);

        Assert.Equal(
            "VENDOR_DEFINED_TOKEN",
            Assert.IsType<StepUnknownParameter>(
                    aggregate.Items[1])
                .RawText);

        Assert.Equal(
            "A",
            Assert.IsType<StepStringParameter>(
                    aggregate.Items[2])
                .Value);
    }

    [Fact]
    public void Parse_WithUnknownParameterInsideTypedParameter_PreservesParentStructure()
    {
        const string parameterText =
            """
            WRAPPED_VALUE(
                VENDOR_DEFINED_TOKEN
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

        var unknownParameter =
            Assert.IsType<StepUnknownParameter>(
                Assert.Single(
                    typedParameter.Parameters));

        Assert.Equal(
            "VENDOR_DEFINED_TOKEN",
            unknownParameter.RawText);
    }

    [Fact]
    public void Parse_WithWhitespaceAroundUnknownParameter_PreservesNormalizedSegment()
    {
        const string parameterText =
            """
                VENDOR_DEFINED_TOKEN
            """;

        var parser =
            new StepParameterParser();

        var parameters =
            parser.Parse(
                parameterText);

        var unknownParameter =
            Assert.IsType<StepUnknownParameter>(
                Assert.Single(
                    parameters));

        Assert.Equal(
            "VENDOR_DEFINED_TOKEN",
            unknownParameter.RawText);
    }
}