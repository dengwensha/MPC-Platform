using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepPlaneGeometryMapperTests
{
    private static readonly double[] LocationCoordinates =
    [
        1.0,
        2.0,
        3.0
    ];

    private static readonly double[] AxisRatios =
    [
        0.0,
        0.0,
        1.0
    ];

    private static readonly double[] ReferenceDirectionRatios =
    [
        1.0,
        0.0,
        0.0
    ];

    [Fact]
    public void Map_WithResolvedPlacement_ReturnsSuccessfulPlane3DResult()
    {
        CreateValidSemanticGraph(
            out StepPlaneEntity plane,
            out MappingContext context);

        var mapper =
            new StepPlaneGeometryMapper();

        MappingResult<Plane3D> result =
            mapper.Map(
                plane,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Plane3D mappedPlane =
            Assert.IsType<Plane3D>(
                result.Value);

        Assert.Empty(
            result.Diagnostics);

        Assert.Equal(
            1.0,
            mappedPlane.Origin.X,
            precision:
                10);

        Assert.Equal(
            2.0,
            mappedPlane.Origin.Y,
            precision:
                10);

        Assert.Equal(
            3.0,
            mappedPlane.Origin.Z,
            precision:
                10);

        Assert.Equal(
            0.0,
            mappedPlane.Normal.X,
            precision:
                10);

        Assert.Equal(
            0.0,
            mappedPlane.Normal.Y,
            precision:
                10);

        Assert.Equal(
            1.0,
            mappedPlane.Normal.Z,
            precision:
                10);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WithPreExistingWarning_DoesNotIncludeItInSuccessfulResult()
    {
        CreateValidSemanticGraph(
            out StepPlaneEntity plane,
            out MappingContext context);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-PLANE-WARNING-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepPlaneGeometryMapper();

        MappingResult<Plane3D> result =
            mapper.Map(
                plane,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Assert.IsType<Plane3D>(
            result.Value);

        Assert.Empty(
            result.Diagnostics);

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            existingDiagnostic,
            contextDiagnostic);
    }

    [Fact]
    public void Map_WithPreExistingError_DoesNotFailSuccessfulOperation()
    {
        CreateValidSemanticGraph(
            out StepPlaneEntity plane,
            out MappingContext context);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-PLANE-ERROR-001",
                "Error from a previous mapping operation.",
                MappingDiagnosticSeverity.Error);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepPlaneGeometryMapper();

        MappingResult<Plane3D> result =
            mapper.Map(
                plane,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Assert.IsType<Plane3D>(
            result.Value);

        Assert.Empty(
            result.Diagnostics);

        Assert.True(
            context.HasErrors);

        MappingDiagnostic contextDiagnostic =
            Assert.Single(
                context.Diagnostics);

        Assert.Same(
            existingDiagnostic,
            contextDiagnostic);
    }

    [Fact]
    public void Map_WhenPlacementReferenceCannotBeResolved_ReturnsOnlyCurrentOperationFailure()
    {
        var plane =
            new StepPlaneEntity(
                sourceEntityId:
                    "#30",
                name:
                    string.Empty,
                placementReferenceId:
                    "#20");

        var semanticResolver =
            new StepSemanticEntityResolver(
                [
                    plane
                ]);

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        var context =
            new MappingContext(
                referenceResolver);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-PLANE-WARNING-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepPlaneGeometryMapper();

        MappingResult<Plane3D> result =
            mapper.Map(
                plane,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.False(
            result.IsSuccessful);

        Assert.Null(
            result.Value);

        MappingDiagnostic resultDiagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "STEP_REFERENCE_RESOLUTION_FAILED",
            resultDiagnostic.Code);

        Assert.Equal(
            2,
            context.Diagnostics.Count);

        Assert.Same(
            existingDiagnostic,
            context.Diagnostics[0]);

        Assert.Same(
            resultDiagnostic,
            context.Diagnostics[1]);
    }

    [Fact]
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        CreateValidSemanticGraph(
            out _,
            out MappingContext context);

        var mapper =
            new StepPlaneGeometryMapper();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                null!,
                context));
    }

    [Fact]
    public void Map_WithNullContext_ThrowsArgumentNullException()
    {
        var plane =
            new StepPlaneEntity(
                sourceEntityId:
                    "#30",
                name:
                    string.Empty,
                placementReferenceId:
                    "#20");

        var mapper =
            new StepPlaneGeometryMapper();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                plane,
                null!));
    }

    private static void CreateValidSemanticGraph(
        out StepPlaneEntity plane,
        out MappingContext context)
    {
        var location =
            new StepCartesianPointEntity(
                sourceEntityId:
                    "#10",
                name:
                    string.Empty,
                coordinates:
                    LocationCoordinates);

        var axis =
            new StepDirectionEntity(
                sourceEntityId:
                    "#11",
                name:
                    string.Empty,
                ratios:
                    AxisRatios);

        var referenceDirection =
            new StepDirectionEntity(
                sourceEntityId:
                    "#12",
                name:
                    string.Empty,
                ratios:
                    ReferenceDirectionRatios);

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

        plane =
            new StepPlaneEntity(
                sourceEntityId:
                    "#30",
                name:
                    string.Empty,
                placementReferenceId:
                    "#20");

        StepSemanticEntity[] semanticEntities =
        [
            location,
            axis,
            referenceDirection,
            placement,
            plane
        ];

        var semanticResolver =
            new StepSemanticEntityResolver(
                semanticEntities);

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        context =
            new MappingContext(
                referenceResolver);
    }
}