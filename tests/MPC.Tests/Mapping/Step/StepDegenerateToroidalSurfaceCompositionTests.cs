using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepDegenerateToroidalSurfaceCompositionTests
{
    [Fact]
    public void AdvancedFaceGeometryMapper_WhenSurfaceIsStandardDegenerateTorus_ReturnsCanonicalDegenerateTorus()
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

        var degenerateToroidalSurface =
            new StepDegenerateToroidalSurfaceEntity(
                sourceEntityId:
                    "#30",
                name:
                    string.Empty,
                placementReferenceId:
                    "#20",
                majorRadius:
                    178.5,
                minorRadius:
                    229.5,
                selectOuter:
                    false);

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
            degenerateToroidalSurface,
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

        var mapper =
            new StepAdvancedFaceGeometryMapper(
                new StepPlaneGeometryMapper(),
                new StepCylindricalSurfaceGeometryMapper(),
                new StepConicalSurfaceGeometryMapper(),
                new StepSphericalSurfaceGeometryMapper());

        MappingResult<ISurface3D> result =
            mapper.Map(
                advancedFace,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        DegenerateTorus3D torus =
            Assert.IsType<DegenerateTorus3D>(
                result.Value);

        Assert.Equal(
            178.5,
            torus.MajorRadius,
            precision:
                10);

        Assert.Equal(
            229.5,
            torus.MinorRadius,
            precision:
                10);

        Assert.False(
            torus.SelectOuter);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }
}
