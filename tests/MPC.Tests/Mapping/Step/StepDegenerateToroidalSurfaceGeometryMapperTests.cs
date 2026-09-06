using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepDegenerateToroidalSurfaceGeometryMapperTests
{
    private const string LocationReferenceId =
        "#10";

    private const string AxisReferenceId =
        "#11";

    private const string ReferenceDirectionReferenceId =
        "#12";

    private const string PlacementReferenceId =
        "#20";

    private const string SurfaceReferenceId =
        "#30";

    private static readonly double[] ValidLocationCoordinates =
    [
        1.0,
        2.0,
        3.0
    ];

    private static readonly double[] ValidAxisRatios =
    [
        0.0,
        0.0,
        -2.0
    ];

    private static readonly double[] ValidReferenceDirectionRatios =
    [
        -1.0,
        0.0,
        0.0
    ];

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Map_WithResolvedPlacement_PreservesCanonicalDegenerateTorusContract(
        bool selectOuter)
    {
        MappingContext context =
            CreateValidContext(
                selectOuter,
                out StepDegenerateToroidalSurfaceEntity surface);

        var mapper =
            new StepDegenerateToroidalSurfaceGeometryMapper();

        MappingResult<DegenerateTorus3D> result =
            mapper.Map(
                surface,
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
            -1.0,
            torus.AxisDirection.Z,
            precision:
                10);

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

        Assert.Equal(
            selectOuter,
            torus.SelectOuter);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WithoutPlacementAxis_UsesDefaultPositiveZAxis()
    {
        var location =
            new StepCartesianPointEntity(
                sourceEntityId:
                    LocationReferenceId,
                name:
                    string.Empty,
                coordinates:
                    ValidLocationCoordinates);

        var placement =
            new StepAxis2Placement3DEntity(
                sourceEntityId:
                    PlacementReferenceId,
                name:
                    string.Empty,
                locationReferenceId:
                    LocationReferenceId,
                axisReferenceId:
                    null,
                refDirectionReferenceId:
                    null);

        StepDegenerateToroidalSurfaceEntity surface =
            CreateSurface(
                selectOuter:
                    true);

        MappingContext context =
            CreateContext(
                location,
                placement,
                surface);

        var mapper =
            new StepDegenerateToroidalSurfaceGeometryMapper();

        MappingResult<DegenerateTorus3D> result =
            mapper.Map(
                surface,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        DegenerateTorus3D torus =
            Assert.IsType<DegenerateTorus3D>(
                result.Value);

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

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WhenPlacementCannotBeResolved_ReturnsFailure()
    {
        StepDegenerateToroidalSurfaceEntity surface =
            CreateSurface(
                selectOuter:
                    true);

        MappingContext context =
            CreateContext(
                surface);

        var mapper =
            new StepDegenerateToroidalSurfaceGeometryMapper();

        MappingResult<DegenerateTorus3D> result =
            mapper.Map(
                surface,
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
            "STEP_REFERENCE_RESOLUTION_FAILED",
            diagnostic.Code);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WithPreExistingDiagnostic_DoesNotIncludeItInSuccessfulResult()
    {
        MappingContext context =
            CreateValidContext(
                selectOuter:
                    true,
                out StepDegenerateToroidalSurfaceEntity surface);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-DEGENERATE-TORUS-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepDegenerateToroidalSurfaceGeometryMapper();

        MappingResult<DegenerateTorus3D> result =
            mapper.Map(
                surface,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Assert.IsType<DegenerateTorus3D>(
            result.Value);

        Assert.Empty(
            result.Diagnostics);

        Assert.Same(
            existingDiagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        MappingContext context =
            CreateContext();

        var mapper =
            new StepDegenerateToroidalSurfaceGeometryMapper();

        Assert.Throws<ArgumentNullException>(
            () =>
                mapper.Map(
                    null!,
                    context));
    }

    [Fact]
    public void Map_WithNullContext_ThrowsArgumentNullException()
    {
        StepDegenerateToroidalSurfaceEntity surface =
            CreateSurface(
                selectOuter:
                    true);

        var mapper =
            new StepDegenerateToroidalSurfaceGeometryMapper();

        Assert.Throws<ArgumentNullException>(
            () =>
                mapper.Map(
                    surface,
                    null!));
    }

    private static MappingContext CreateValidContext(
        bool selectOuter,
        out StepDegenerateToroidalSurfaceEntity surface)
    {
        var location =
            new StepCartesianPointEntity(
                sourceEntityId:
                    LocationReferenceId,
                name:
                    string.Empty,
                coordinates:
                    ValidLocationCoordinates);

        var axis =
            new StepDirectionEntity(
                sourceEntityId:
                    AxisReferenceId,
                name:
                    string.Empty,
                ratios:
                    ValidAxisRatios);

        var referenceDirection =
            new StepDirectionEntity(
                sourceEntityId:
                    ReferenceDirectionReferenceId,
                name:
                    string.Empty,
                ratios:
                    ValidReferenceDirectionRatios);

        var placement =
            new StepAxis2Placement3DEntity(
                sourceEntityId:
                    PlacementReferenceId,
                name:
                    string.Empty,
                locationReferenceId:
                    LocationReferenceId,
                axisReferenceId:
                    AxisReferenceId,
                refDirectionReferenceId:
                    ReferenceDirectionReferenceId);

        surface =
            CreateSurface(
                selectOuter);

        return CreateContext(
            location,
            axis,
            referenceDirection,
            placement,
            surface);
    }

    private static StepDegenerateToroidalSurfaceEntity CreateSurface(
        bool selectOuter)
    {
        return new StepDegenerateToroidalSurfaceEntity(
            sourceEntityId:
                SurfaceReferenceId,
            name:
                string.Empty,
            placementReferenceId:
                PlacementReferenceId,
            majorRadius:
                178.5,
            minorRadius:
                229.5,
            selectOuter:
                selectOuter);
    }

    private static MappingContext CreateContext(
        params StepSemanticEntity[] semanticEntities)
    {
        var semanticResolver =
            new StepSemanticEntityResolver(
                semanticEntities);

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        return new MappingContext(
            referenceResolver);
    }
}
