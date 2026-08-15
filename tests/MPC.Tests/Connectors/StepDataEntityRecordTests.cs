using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;

namespace MPC.Tests.Connectors;

public sealed class StepDataEntityRecordTests
{
    [Fact]
    public void Constructor_WithSimpleEntity_NormalizesScalarValues()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "  #25  ",
                sourceEntityType:
                    "  cartesian_point  ",
                rawStatement:
                    "  #25=CARTESIAN_POINT('',(1.,2.,3.));  ",
                readStatus:
                    CadReadStatus.Complete);

        Assert.Equal(
            "#25",
            record.SourceEntityId);

        Assert.Equal(
            "CARTESIAN_POINT",
            record.SourceEntityType);

        Assert.Equal(
            "#25=CARTESIAN_POINT('',(1.,2.,3.));",
            record.RawStatement);

        Assert.Equal(
            CadReadStatus.Complete,
            record.ReadStatus);

        Assert.False(
            record.IsComplexEntity);
    }

    [Fact]
    public void Constructor_WithoutOptionalCollections_UsesEmptyCollections()
    {
        var record =
            CreateCompleteRecord();

        Assert.Empty(
            record.ComponentTypes);

        Assert.Empty(
            record.Diagnostics);

        Assert.Empty(
            record.ReferencedEntityIds);
    }

    [Fact]
    public void Constructor_WithComplexEntity_NormalizesComponentTypes()
    {
        var componentTypes =
            new List<string>
            {
                "  representation_item  ",
                "geometric_representation_item",
                "  surface  "
            };

        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#100",
                sourceEntityType:
                    " complex_entity ",
                rawStatement:
                    "#100=(REPRESENTATION_ITEM()SURFACE());",
                readStatus:
                    CadReadStatus.Complete,
                componentTypes:
                    componentTypes);

        Assert.True(
            record.IsComplexEntity);

        Assert.Equal(
            new[]
            {
                "REPRESENTATION_ITEM",
                "GEOMETRIC_REPRESENTATION_ITEM",
                "SURFACE"
            },
            record.ComponentTypes);
    }

    [Fact]
    public void Constructor_NormalizesReferencedEntityIds()
    {
        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#25",
                sourceEntityType:
                    "EDGE_CURVE",
                rawStatement:
                    "#25=EDGE_CURVE('',#1,#2,#3,.T.);",
                readStatus:
                    CadReadStatus.Complete,
                referencedEntityIds:
                    new[]
                    {
                        "  #1 ",
                        "#2",
                        " #3  "
                    });

        Assert.Equal(
            new[]
            {
                "#1",
                "#2",
                "#3"
            },
            record.ReferencedEntityIds);
    }

    [Fact]
    public void Constructor_CopiesComponentTypeInput()
    {
        var componentTypes =
            new List<string>
            {
                "SURFACE"
            };

        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#100",
                sourceEntityType:
                    "COMPLEX_ENTITY",
                rawStatement:
                    "#100=(SURFACE());",
                readStatus:
                    CadReadStatus.Complete,
                componentTypes:
                    componentTypes);

        componentTypes[0] =
            "CHANGED";

        componentTypes.Add(
            "ADDED");

        Assert.Equal(
            new[]
            {
                "SURFACE"
            },
            record.ComponentTypes);
    }

    [Fact]
    public void Constructor_CopiesDiagnosticInput()
    {
        var originalDiagnostic =
            CreateErrorDiagnostic();

        var diagnostics =
            new List<CadDiagnostic>
            {
                originalDiagnostic
            };

        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#25",
                sourceEntityType:
                    "UNKNOWN",
                rawStatement:
                    "#25=INVALID();",
                readStatus:
                    CadReadStatus.Partial,
                diagnostics:
                    diagnostics);

        diagnostics.Clear();

        Assert.Same(
            originalDiagnostic,
            Assert.Single(
                record.Diagnostics));
    }

    [Fact]
    public void Constructor_CopiesReferencedEntityIdInput()
    {
        var referencedEntityIds =
            new List<string>
            {
                "#1",
                "#2"
            };

        var record =
            new StepDataEntityRecord(
                sourceEntityId:
                    "#25",
                sourceEntityType:
                    "EDGE_CURVE",
                rawStatement:
                    "#25=EDGE_CURVE('',#1,#2,$,.T.);",
                readStatus:
                    CadReadStatus.Complete,
                referencedEntityIds:
                    referencedEntityIds);

        referencedEntityIds[0] =
            "#99";

        referencedEntityIds.Add(
            "#100");

        Assert.Equal(
            new[]
            {
                "#1",
                "#2"
            },
            record.ReferencedEntityIds);
    }

    [Fact]
    public void Constructor_WithWhitespaceComponentType_Throws()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new StepDataEntityRecord(
                    sourceEntityId:
                        "#100",
                    sourceEntityType:
                        "COMPLEX_ENTITY",
                    rawStatement:
                        "#100=(SURFACE());",
                    readStatus:
                        CadReadStatus.Complete,
                    componentTypes:
                        new[]
                        {
                            "SURFACE",
                            " "
                        }));
    }

    [Fact]
    public void Constructor_WithWhitespaceReferencedEntityId_Throws()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new StepDataEntityRecord(
                    sourceEntityId:
                        "#25",
                    sourceEntityType:
                        "EDGE_CURVE",
                    rawStatement:
                        "#25=EDGE_CURVE('',#1,#2,$,.T.);",
                    readStatus:
                        CadReadStatus.Complete,
                    referencedEntityIds:
                        new[]
                        {
                            "#1",
                            " "
                        }));
    }

    [Fact]
    public void Constructor_WithNullDiagnostic_Throws()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new StepDataEntityRecord(
                    sourceEntityId:
                        "#25",
                    sourceEntityType:
                        "UNKNOWN",
                    rawStatement:
                        "#25=INVALID();",
                    readStatus:
                        CadReadStatus.Partial,
                    diagnostics:
                        new CadDiagnostic[]
                        {
                            null!
                        }));
    }

    [Fact]
    public void Constructor_WithCompleteStatusAndErrorDiagnostic_Throws()
    {
        Assert.Throws<InvalidOperationException>(
            () =>
                new StepDataEntityRecord(
                    sourceEntityId:
                        "#25",
                    sourceEntityType:
                        "UNKNOWN",
                    rawStatement:
                        "#25=INVALID();",
                    readStatus:
                        CadReadStatus.Complete,
                    diagnostics:
                        new[]
                        {
                            CreateErrorDiagnostic()
                        }));
    }

    [Fact]
    public void Constructor_WithPartialStatusAndNoErrorDiagnostic_Throws()
    {
        Assert.Throws<InvalidOperationException>(
            () =>
                new StepDataEntityRecord(
                    sourceEntityId:
                        "#25",
                    sourceEntityType:
                        "UNKNOWN",
                    rawStatement:
                        "#25=INVALID();",
                    readStatus:
                        CadReadStatus.Partial));
    }

    [Fact]
    public void Constructor_WithComplexEntityAndNoComponents_Throws()
    {
        Assert.Throws<InvalidOperationException>(
            () =>
                new StepDataEntityRecord(
                    sourceEntityId:
                        "#100",
                    sourceEntityType:
                        "COMPLEX_ENTITY",
                    rawStatement:
                        "#100=(SURFACE());",
                    readStatus:
                        CadReadStatus.Complete));
    }

    [Fact]
    public void Constructor_WithSimpleEntityAndComponents_Throws()
    {
        Assert.Throws<InvalidOperationException>(
            () =>
                new StepDataEntityRecord(
                    sourceEntityId:
                        "#25",
                    sourceEntityType:
                        "CARTESIAN_POINT",
                    rawStatement:
                        "#25=CARTESIAN_POINT('',(1.,2.,3.));",
                    readStatus:
                        CadReadStatus.Complete,
                    componentTypes:
                        new[]
                        {
                            "SURFACE"
                        }));
    }

    [Fact]
    public void Constructor_WithUndefinedReadStatus_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new StepDataEntityRecord(
                    sourceEntityId:
                        "#25",
                    sourceEntityType:
                        "CARTESIAN_POINT",
                    rawStatement:
                        "#25=CARTESIAN_POINT('',(1.,2.,3.));",
                    readStatus:
                        (CadReadStatus)999));
    }

    private static StepDataEntityRecord
        CreateCompleteRecord()
    {
        return new StepDataEntityRecord(
            sourceEntityId:
                "#25",
            sourceEntityType:
                "CARTESIAN_POINT",
            rawStatement:
                "#25=CARTESIAN_POINT('',(1.,2.,3.));",
            readStatus:
                CadReadStatus.Complete);
    }

    private static CadDiagnostic
        CreateErrorDiagnostic()
    {
        return new CadDiagnostic(
            code:
                "STEP_TEST_ERROR",
            severity:
                CadDiagnosticSeverity.Error,
            message:
                "Test diagnostic.");
    }
}