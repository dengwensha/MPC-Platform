using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepToroidalSurfaceCompositionTests
{
    [Fact]
    public void AdvancedFaceGeometryMapper_WhenSurfaceIsStandardRegularTorus_ReturnsCanonicalTorus()
    {
        (
            StepAdvancedFaceEntity advancedFace,
            MappingContext context) =
                CreateFixture(
                    majorRadius:
                        229.5,
                    minorRadius:
                        178.5);

        StepAdvancedFaceGeometryMapper mapper =
            CreateMapper();

        MappingResult<ISurface3D> result =
            mapper.Map(
                advancedFace,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Torus3D torus =
            Assert.IsType<Torus3D>(
                result.Value);

        Assert.Equal(
            1.0,
            torus.Center.X,
            precision:
                10);

        Assert.Equal(
            2.0,
            torus.Center.Y,
            precision:
                10);

        Assert.Equal(
            3.0,
            torus.Center.Z,
            precision:
                10);

        Assert.Equal(
            0.0,
            torus.AxisDirection.X,
            precision:
                10);

        Assert.Equal(
            0.0,
            torus.AxisDirection.Y,
            precision:
                10);

        Assert.Equal(
            1.0,
            torus.AxisDirection.Z,
            precision:
                10);

        Assert.Equal(
            229.5,
            torus.MajorRadius,
            precision:
                10);

        Assert.Equal(
            178.5,
            torus.MinorRadius,
            precision:
                10);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Theory]
    [InlineData(178.5, 229.5)]
    [InlineData(229.5, 229.5)]
    public void AdvancedFaceGeometryMapper_WhenRegularTorusViolatesCanonicalRadiusRelationship_ReturnsFailureWithoutNormalization(
        double majorRadius,
        double minorRadius)
    {
        (
            StepAdvancedFaceEntity advancedFace,
            MappingContext context) =
                CreateFixture(
                    majorRadius,
                    minorRadius);

        StepAdvancedFaceGeometryMapper mapper =
            CreateMapper();

        MappingResult<ISurface3D> result =
            mapper.Map(
                advancedFace,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.False(
            result.IsSuccessful);

        Assert.Null(
            result.Value);

        MappingDiagnostic diagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "MAP-STEP-TORUS-004",
            diagnostic.Code);

        Assert.Equal(
            MappingDiagnosticSeverity.Error,
            diagnostic.Severity);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    private static StepAdvancedFaceGeometryMapper CreateMapper()
    {
        return new StepAdvancedFaceGeometryMapper(
            new StepPlaneGeometryMapper(),
            new StepCylindricalSurfaceGeometryMapper(),
            new StepConicalSurfaceGeometryMapper(),
            new StepSphericalSurfaceGeometryMapper());
    }

    private static (
        StepAdvancedFaceEntity AdvancedFace,
        MappingContext Context)
        CreateFixture(
            double majorRadius,
            double minorRadius)
    {
        var location =
            new StepCartesianPointEntity(
                sourceEntityId:
                    "#10",
                name:
                    string.Empty,
                coordinates:
                [
                    1.0,
                    2.0,
                    3.0
                ]);

        var axis =
            new StepDirectionEntity(
                sourceEntityId:
                    "#11",
                name:
                    string.Empty,
                ratios:
                [
                    0.0,
                    0.0,
                    1.0
                ]);

        var referenceDirection =
            new StepDirectionEntity(
                sourceEntityId:
                    "#12",
                name:
                    string.Empty,
                ratios:
                [
                    1.0,
                    0.0,
                    0.0
                ]);

        var placement =
            new StepAxis2Placement3DEntity(
                sourceEntityId:
                    "#20",
                name:
                    string.Empty,
                locationReferenceId:
                    "#10",
                axisReferenceId:
                    "#11",
                refDirectionReferenceId:
                    "#12");

        var toroidalSurface =
            new StepToroidalSurfaceEntity(
                sourceEntityId:
                    "#30",
                name:
                    string.Empty,
                placementReferenceId:
                    "#20",
                majorRadius:
                    majorRadius,
                minorRadius:
                    minorRadius);

        var advancedFace =
            new StepAdvancedFaceEntity(
                sourceEntityId:
                    "#40",
                name:
                    string.Empty,
                boundReferenceIds:
                [
                    "#50"
                ],
                surfaceReferenceId:
                    "#30",
                sameSense:
                    true);

        StepSemanticEntity[] entities =
        [
            location,
            axis,
            referenceDirection,
            placement,
            toroidalSurface,
            advancedFace
        ];

        var semanticResolver =
            new StepSemanticEntityResolver(
                entities);

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        var context =
            new MappingContext(
                referenceResolver);

        return (
            advancedFace,
            context);
    }
}
