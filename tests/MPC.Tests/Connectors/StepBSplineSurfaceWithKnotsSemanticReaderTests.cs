using MPC.Connectors;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepBSplineSurfaceWithKnotsSemanticReaderTests
{
    [Fact]
    public void Read_R03SevenByTwoRecord_PreservesSemanticParameters()
    {
        var result =
            Read(
                """
                #75=B_SPLINE_SURFACE_WITH_KNOTS(
                    'NONE',
                    3,
                    1,
                    (
                        (#93314,#102277),
                        (#102940,#10335),
                        (#8293,#18589),
                        (#52171,#83013),
                        (#101593,#50137),
                        (#39805,#123500),
                        (#92631,#72736)
                    ),
                    .UNSPECIFIED.,
                    .F.,
                    .F.,
                    .F.,
                    (4,1,1,1,4),
                    (2,2),
                    (
                        0.000000000000000000,
                        0.2628677986259980170,
                        0.5189596503196842736,
                        0.7655977332333875340,
                        1.000000000000000000
                    ),
                    (0.000000000000000000,1.000000000000000000),
                    .UNSPECIFIED.
                );
                """);

        Assert.Equal(
            "#75",
            result.SourceEntityId);

        Assert.Equal(
            "B_SPLINE_SURFACE_WITH_KNOTS",
            result.SourceEntityType);

        Assert.Equal(
            "NONE",
            result.Name);

        Assert.Equal(
            3,
            result.UDegree);

        Assert.Equal(
            1,
            result.VDegree);

        Assert.Equal(
            7,
            result.ControlPointReferenceGrid.Count);

        Assert.All(
            result.ControlPointReferenceGrid,
            row =>
                Assert.Equal(
                    2,
                    row.Count));

        Assert.Equal(
            "#93314",
            result.ControlPointReferenceGrid[0][0]);

        Assert.Equal(
            "#102277",
            result.ControlPointReferenceGrid[0][1]);

        Assert.Equal(
            "#92631",
            result.ControlPointReferenceGrid[6][0]);

        Assert.Equal(
            "#72736",
            result.ControlPointReferenceGrid[6][1]);

        Assert.Equal(
            "UNSPECIFIED",
            result.SurfaceForm);

        Assert.Equal(
            StepLogicalValue.False,
            result.UClosed);

        Assert.Equal(
            StepLogicalValue.False,
            result.VClosed);

        Assert.Equal(
            StepLogicalValue.False,
            result.SelfIntersect);

        Assert.Equal(
            new long[] { 4, 1, 1, 1, 4 },
            result.UMultiplicities);

        Assert.Equal(
            new long[] { 2, 2 },
            result.VMultiplicities);

        Assert.Equal(
            new[]
            {
                0.0,
                0.2628677986259980170,
                0.5189596503196842736,
                0.7655977332333875340,
                1.0
            },
            result.UKnots);

        Assert.Equal(
            new[] { 0.0, 1.0 },
            result.VKnots);

        Assert.Equal(
            "UNSPECIFIED",
            result.KnotSpecification);
    }

    [Fact]
    public void Read_R03ElevenByTwoRecord_PreservesGridShapeAndKnots()
    {
        var result =
            Read(
                """
                #371=B_SPLINE_SURFACE_WITH_KNOTS(
                    'NONE',
                    3,
                    1,
                    (
                        (#93530,#81871),
                        (#134695,#40711),
                        (#98277,#2278),
                        (#32493,#107881),
                        (#54423,#96260),
                        (#116157,#13266),
                        (#105866,#11925),
                        (#126457,#24891),
                        (#23549,#12603),
                        (#87286,#33833),
                        (#96912,#42766)
                    ),
                    .UNSPECIFIED.,
                    .F.,
                    .F.,
                    .F.,
                    (4,1,1,1,1,1,1,1,4),
                    (2,2),
                    (
                        0.000000000000000000,
                        0.1445407895053954506,
                        0.2783997076670937099,
                        0.4060697555077860121,
                        0.5315855621125593622,
                        0.6525499928237524783,
                        0.7671265052482588720,
                        0.8807674065352522064,
                        1.000000000000000000
                    ),
                    (0.000000000000000000,1.000000000000000000),
                    .UNSPECIFIED.
                );
                """,
                sourceEntityId:
                    "#371");

        Assert.Equal(
            "#371",
            result.SourceEntityId);

        Assert.Equal(
            11,
            result.ControlPointReferenceGrid.Count);

        Assert.All(
            result.ControlPointReferenceGrid,
            row =>
                Assert.Equal(
                    2,
                    row.Count));

        Assert.Equal(
            new long[]
            {
                4, 1, 1, 1, 1, 1, 1, 1, 4
            },
            result.UMultiplicities);

        Assert.Equal(
            9,
            result.UKnots.Count);

        Assert.Equal(
            0.1445407895053954506,
            result.UKnots[1]);

        Assert.Equal(
            0.8807674065352522064,
            result.UKnots[7]);
    }

    [Theory]
    [InlineData(".F.", StepLogicalValue.False)]
    [InlineData(".T.", StepLogicalValue.True)]
    [InlineData(".U.", StepLogicalValue.Unknown)]
    public void Read_LogicalToken_PreservesThreeStateValue(
        string token,
        StepLogicalValue expected)
    {
        var result =
            Read(
                CreateValidStatement(
                    uClosed:
                        token,
                    vClosed:
                        token,
                    selfIntersect:
                        token));

        Assert.Equal(
            expected,
            result.UClosed);

        Assert.Equal(
            expected,
            result.VClosed);

        Assert.Equal(
            expected,
            result.SelfIntersect);
    }

    [Fact]
    public void Constructor_CopiesAllMutableInputCollections()
    {
        string[][] controlPoints =
        [
            ["#1", "#2"],
            ["#3", "#4"],
            ["#5", "#6"],
            ["#7", "#8"]
        ];

        long[] uMultiplicities =
        [
            4,
            4
        ];

        long[] vMultiplicities =
        [
            2,
            2
        ];

        double[] uKnots =
        [
            0.0,
            1.0
        ];

        double[] vKnots =
        [
            0.0,
            1.0
        ];

        var entity =
            new StepBSplineSurfaceWithKnotsEntity(
                sourceEntityId:
                    "#75",
                name:
                    "NONE",
                uDegree:
                    3,
                vDegree:
                    1,
                controlPointReferenceGrid:
                    controlPoints,
                surfaceForm:
                    "UNSPECIFIED",
                uClosed:
                    StepLogicalValue.False,
                vClosed:
                    StepLogicalValue.False,
                selfIntersect:
                    StepLogicalValue.False,
                uMultiplicities:
                    uMultiplicities,
                vMultiplicities:
                    vMultiplicities,
                uKnots:
                    uKnots,
                vKnots:
                    vKnots,
                knotSpecification:
                    "UNSPECIFIED");

        controlPoints[0][0] =
            "#999";

        uMultiplicities[0] =
            1;

        vMultiplicities[0] =
            1;

        uKnots[0] =
            -1.0;

        vKnots[0] =
            -1.0;

        Assert.Equal(
            "#1",
            entity.ControlPointReferenceGrid[0][0]);

        Assert.Equal(
            4,
            entity.UMultiplicities[0]);

        Assert.Equal(
            2,
            entity.VMultiplicities[0]);

        Assert.Equal(
            0.0,
            entity.UKnots[0]);

        Assert.Equal(
            0.0,
            entity.VKnots[0]);
    }

    [Theory]
    [InlineData(
        "#75=B_SPLINE_SURFACE_WITH_KNOTS('NONE',3,1);")]
    [InlineData(
        "#75=B_SPLINE_SURFACE_WITH_KNOTS('NONE',3,1,((#1,#2),(#3,#4),(#5,#6),(#7,#8)),.UNSPECIFIED.,.F.,.F.,.F.,(4,4),(2,2),(0.,1.),(0.,1.),.UNSPECIFIED.,#99);")]
    public void Read_IncorrectParameterCount_ThrowsFormatException(
        string rawStatement)
    {
        Assert.Throws<FormatException>(
            () =>
                Read(rawStatement));
    }

    [Theory]
    [InlineData(
        "#75=B_SPLINE_SURFACE_WITH_KNOTS('NONE',3.0,1,((#1,#2),(#3,#4),(#5,#6),(#7,#8)),.UNSPECIFIED.,.F.,.F.,.F.,(4,4),(2,2),(0.,1.),(0.,1.),.UNSPECIFIED.);")]
    [InlineData(
        "#75=B_SPLINE_SURFACE_WITH_KNOTS('NONE',3,1,(#1,#2,#3,#4),.UNSPECIFIED.,.F.,.F.,.F.,(4,4),(2,2),(0.,1.),(0.,1.),.UNSPECIFIED.);")]
    [InlineData(
        "#75=B_SPLINE_SURFACE_WITH_KNOTS('NONE',3,1,((#1,2),(#3,#4),(#5,#6),(#7,#8)),.UNSPECIFIED.,.F.,.F.,.F.,(4,4),(2,2),(0.,1.),(0.,1.),.UNSPECIFIED.);")]
    [InlineData(
        "#75=B_SPLINE_SURFACE_WITH_KNOTS('NONE',3,1,((#1,#2),(#3,#4),(#5,#6),(#7,#8)),.UNSPECIFIED.,.MAYBE.,.F.,.F.,(4,4),(2,2),(0.,1.),(0.,1.),.UNSPECIFIED.);")]
    [InlineData(
        "#75=B_SPLINE_SURFACE_WITH_KNOTS('NONE',3,1,((#1,#2),(#3,#4),(#5,#6),(#7,#8)),.UNSPECIFIED.,.F.,.F.,.F.,(4.0,4),(2,2),(0.,1.),(0.,1.),.UNSPECIFIED.);")]
    [InlineData(
        "#75=B_SPLINE_SURFACE_WITH_KNOTS('NONE',3,1,((#1,#2),(#3,#4),(#5,#6),(#7,#8)),.UNSPECIFIED.,.F.,.F.,.F.,(4,4),(2,2),('0',1.),(0.,1.),.UNSPECIFIED.);")]
    public void Read_InvalidParameterShape_ThrowsFormatException(
        string rawStatement)
    {
        Assert.Throws<FormatException>(
            () =>
                Read(rawStatement));
    }

    [Theory]
    [MemberData(nameof(InvalidSemanticRecords))]
    public void Read_InvalidSemanticValue_ReportsEntityScopedFailure(
        string rawStatement,
        string expectedParameterName)
    {
        StepSemanticValueException exception =
            Assert.Throws<StepSemanticValueException>(
                () =>
                    Read(rawStatement));

        Assert.Equal(
            "#75",
            exception.SourceEntityId);

        Assert.Equal(
            "B_SPLINE_SURFACE_WITH_KNOTS",
            exception.SourceEntityType);

        Assert.Equal(
            expectedParameterName,
            exception.ParameterName);

        Assert.False(
            string.IsNullOrWhiteSpace(
                exception.RejectedValue));

        Assert.IsAssignableFrom<ArgumentException>(
            exception.InnerException);
    }

    [Fact]
    public void Handler_DifferentEntityType_ThrowsInvalidOperationException()
    {
        var handler =
            new StepBSplineSurfaceWithKnotsEntityHandler();

        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#75",
                sourceEntityType:
                    "PLANE",
                rawStatement:
                    "#75=PLANE('',#1);",
                readStatus:
                    CadReadStatus.Complete);

        Assert.Throws<InvalidOperationException>(
            () =>
                handler.Read(
                    record,
                    Array.Empty<StepParameter>()));
    }

    [Fact]
    public void Handler_NullRecord_ThrowsArgumentNullException()
    {
        var handler =
            new StepBSplineSurfaceWithKnotsEntityHandler();

        Assert.Throws<ArgumentNullException>(
            () =>
                handler.Read(
                    null!,
                    Array.Empty<StepParameter>()));
    }

    [Fact]
    public void Handler_NullParameters_ThrowsArgumentNullException()
    {
        var handler =
            new StepBSplineSurfaceWithKnotsEntityHandler();

        Assert.Throws<ArgumentNullException>(
            () =>
                handler.Read(
                    CreateRecord(
                        CreateValidStatement()),
                    null!));
    }

    public static IEnumerable<object[]>
        InvalidSemanticRecords()
    {
        yield return
        [
            CreateValidStatement(
                uDegree:
                    0),
            "uDegree"
        ];

        yield return
        [
            """
            #75=B_SPLINE_SURFACE_WITH_KNOTS(
                'NONE',3,1,
                ((#1,#2),(#3,#4),(#5),(#7,#8)),
                .UNSPECIFIED.,.F.,.F.,.F.,
                (4,4),(2,2),(0.,1.),(0.,1.),
                .UNSPECIFIED.);
            """,
            "controlPointReferenceGrid"
        ];

        yield return
        [
            CreateValidStatement(
                uMultiplicities:
                    "(4)"),
            "uMultiplicities"
        ];

        yield return
        [
            CreateValidStatement(
                uMultiplicities:
                    "(3,4)"),
            "uMultiplicities"
        ];

        yield return
        [
            CreateValidStatement(
                uKnots:
                    "(1.0,0.0)"),
            "uKnots"
        ];

        yield return
        [
            CreateValidStatement(
                surfaceForm:
                    ".INVALID_FORM."),
            "surfaceForm"
        ];

        yield return
        [
            CreateValidStatement(
                knotSpecification:
                    ".INVALID_KNOTS."),
            "knotSpecification"
        ];
    }

    private static StepBSplineSurfaceWithKnotsEntity
        Read(
            string rawStatement,
            string sourceEntityId = "#75")
    {
        var reader =
            new StepSemanticEntityReader(
                parameterParser:
                    new StepParameterParser(),
                handlers:
                    new IStepSemanticEntityHandler[]
                    {
                        new StepBSplineSurfaceWithKnotsEntityHandler()
                    });

        return Assert.IsType<
            StepBSplineSurfaceWithKnotsEntity>(
                reader.Read(
                    CreateRecord(
                        rawStatement,
                        sourceEntityId)));
    }

    private static StepDataEntityRecord CreateRecord(
        string rawStatement,
        string sourceEntityId = "#75")
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                sourceEntityId,
            sourceEntityType:
                "B_SPLINE_SURFACE_WITH_KNOTS",
            rawStatement:
                rawStatement,
            readStatus:
                CadReadStatus.Complete);
    }

    private static string CreateValidStatement(
        long uDegree = 3,
        string surfaceForm = ".UNSPECIFIED.",
        string uClosed = ".F.",
        string vClosed = ".F.",
        string selfIntersect = ".F.",
        string uMultiplicities = "(4,4)",
        string uKnots = "(0.0,1.0)",
        string knotSpecification = ".UNSPECIFIED.")
    {
        return
            $"#75=B_SPLINE_SURFACE_WITH_KNOTS(" +
            $"'NONE',{uDegree},1," +
            "((#1,#2),(#3,#4),(#5,#6),(#7,#8))," +
            $"{surfaceForm}," +
            $"{uClosed},{vClosed},{selfIntersect}," +
            $"{uMultiplicities},(2,2)," +
            $"{uKnots},(0.0,1.0)," +
            $"{knotSpecification});";
    }
}
