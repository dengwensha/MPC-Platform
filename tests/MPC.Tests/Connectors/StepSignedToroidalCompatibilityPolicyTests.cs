using MPC.Connectors;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;
using Xunit;

namespace MPC.Tests.Connectors;

public sealed class StepSignedToroidalCompatibilityPolicyTests
{
    [Fact]
    public void Read_WithSupportedSwStepHeader_MapsSignedMajorRadiusToInnerDegenerateSurface()
    {
        StepSemanticEntity result =
            Read(
                CreatePolicy(
                    preprocessorVersion: "SwSTEP 2.0",
                    originatingSystem: "SolidWorks 2019"),
                majorRadius: -178.5,
                minorRadius: 229.5);

        StepDegenerateToroidalSurfaceEntity surface =
            Assert.IsType<StepDegenerateToroidalSurfaceEntity>(
                result);

        Assert.Equal(
            "#170",
            surface.SourceEntityId);

        Assert.Equal(
            "TOROIDAL_SURFACE",
            surface.SourceEntityType);

        Assert.Equal(
            string.Empty,
            surface.Name);

        Assert.Equal(
            "#100",
            surface.PlacementReferenceId);

        Assert.Equal(
            178.5,
            surface.MajorRadius);

        Assert.Equal(
            229.5,
            surface.MinorRadius);

        Assert.False(
            surface.SelectOuter);
    }

    [Theory]
    [InlineData("SwSTEP 2.1", "SolidWorks 2019")]
    [InlineData("SwSTEP 2.0", "SolidWorks 2020")]
    [InlineData(null, "SolidWorks 2019")]
    [InlineData("SwSTEP 2.0", null)]
    public void Read_OutsideExactExporterFingerprint_PreservesSemanticRejection(
        string? preprocessorVersion,
        string? originatingSystem)
    {
        StepSemanticValueException exception =
            Assert.Throws<StepSemanticValueException>(
                () =>
                    Read(
                        CreatePolicy(
                            preprocessorVersion,
                            originatingSystem),
                        majorRadius: -178.5,
                        minorRadius: 229.5));

        Assert.Equal(
            "#170",
            exception.SourceEntityId);

        Assert.Equal(
            "TOROIDAL_SURFACE",
            exception.SourceEntityType);

        Assert.Equal(
            "majorRadius",
            exception.ParameterName);

        Assert.Equal(
            "-178.5",
            exception.RejectedValue);
    }

    [Theory]
    [InlineData(-229.5, 229.5)]
    [InlineData(-300.0, 229.5)]
    [InlineData(-178.5, 0.0)]
    [InlineData(-178.5, -229.5)]
    public void Read_WithUnsupportedSignedRadiusRelationship_PreservesSemanticRejection(
        double majorRadius,
        double minorRadius)
    {
        StepSemanticValueException exception =
            Assert.Throws<StepSemanticValueException>(
                () =>
                    Read(
                        CreatePolicy(
                            preprocessorVersion: "SwSTEP 2.0",
                            originatingSystem: "SolidWorks 2019"),
                        majorRadius,
                        minorRadius));

        Assert.Equal(
            "#170",
            exception.SourceEntityId);

        Assert.Equal(
            "TOROIDAL_SURFACE",
            exception.SourceEntityType);

        Assert.Contains(
            exception.ParameterName,
            new[]
            {
                "majorRadius",
                "minorRadius"
            });
    }

    [Fact]
    public void Read_WithSupportedPolicyAndPositiveRadii_PreservesStandardToroidalSurface()
    {
        StepSemanticEntity result =
            Read(
                CreatePolicy(
                    preprocessorVersion: "SwSTEP 2.0",
                    originatingSystem: "SolidWorks 2019"),
                majorRadius: 300.0,
                minorRadius: 100.0);

        StepToroidalSurfaceEntity surface =
            Assert.IsType<StepToroidalSurfaceEntity>(
                result);

        Assert.Equal(
            "TOROIDAL_SURFACE",
            surface.SourceEntityType);

        Assert.Equal(
            300.0,
            surface.MajorRadius);

        Assert.Equal(
            100.0,
            surface.MinorRadius);
    }

    private static SwStepSignedToroidalCompatibilityPolicy
        CreatePolicy(
            string? preprocessorVersion,
            string? originatingSystem)
    {
        var header =
            new StepHeaderInfo(
                descriptions:
                    new[]
                    {
                        "STEP AP203"
                    },
                implementationLevel: "1",
                fileName: "Part.step",
                timeStamp: null,
                author: null,
                organization: null,
                preprocessorVersion:
                    preprocessorVersion,
                originatingSystem:
                    originatingSystem,
                authorization: null,
                schemas:
                    new[]
                    {
                        "CONFIG_CONTROL_DESIGN"
                    });

        return new SwStepSignedToroidalCompatibilityPolicy(
            header);
    }

    private static StepSemanticEntity Read(
        SwStepSignedToroidalCompatibilityPolicy policy,
        double majorRadius,
        double minorRadius)
    {
        var reader =
            new StepSemanticEntityReader(
                new StepParameterParser(),
                new IStepSemanticEntityHandler[]
                {
                    new StepToroidalSurfaceEntityHandler(
                        policy)
                });

        var record =
            new StepDataEntityRecord(
                sourceEntityId: "#170",
                sourceEntityType: "TOROIDAL_SURFACE",
                rawStatement:
                    $"#170=TOROIDAL_SURFACE('',#100," +
                    $"{majorRadius.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                    $"{minorRadius.ToString(System.Globalization.CultureInfo.InvariantCulture)});",
                readStatus: CadReadStatus.Complete);

        return reader.Read(record)
            ?? throw new InvalidOperationException(
                "Expected a semantic entity.");
    }
}
