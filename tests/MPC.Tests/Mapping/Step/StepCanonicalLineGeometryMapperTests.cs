using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepCanonicalLineGeometryMapperTests
{
    [Fact]
    public void Map_WithValidSemanticChain_ReturnsCanonicalLine()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Line origin",
                [
                    1.0,
                    2.0,
                    3.0
                ]);

        var direction =
            new StepDirectionEntity(
                "#20",
                "Line direction",
                [
                    1.0,
                    0.0,
                    0.0
                ]);

        var vector =
            new StepVectorEntity(
                "#30",
                "Line vector",
                "#20",
                25.0);

        var source =
            new StepLineEntity(
                "#40",
                "Main line",
                "#10",
                "#30");

        MappingContext context =
            CreateContext(
                point,
                direction,
                vector,
                source);

        var mapper =
            new StepLineGeometryMapper();

        MappingResult<Line3D> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Line3D line =
            Assert.IsType<Line3D>(
                result.Value);

        Assert.Equal(
            new Point3D(
                1.0,
                2.0,
                3.0),
            line.Origin);

        Assert.Equal(
            new Vector3D(
                1.0,
                0.0,
                0.0),
            line.Direction);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WithNonUnitDirection_NormalizesDirection()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Line origin",
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        var direction =
            new StepDirectionEntity(
                "#20",
                "Non-unit direction",
                [
                    1.0,
                    1.0,
                    0.0
                ]);

        var vector =
            new StepVectorEntity(
                "#30",
                "Line vector",
                "#20",
                1.0);

        var source =
            new StepLineEntity(
                "#40",
                "Main line",
                "#10",
                "#30");

        MappingContext context =
            CreateContext(
                point,
                direction,
                vector,
                source);

        var mapper =
            new StepLineGeometryMapper();

        MappingResult<Line3D> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Line3D line =
            Assert.IsType<Line3D>(
                result.Value);

        double expectedComponent =
            1.0 / Math.Sqrt(2.0);

        Assert.Equal(
            expectedComponent,
            line.Direction.X,
            12);

        Assert.Equal(
            expectedComponent,
            line.Direction.Y,
            12);

        Assert.Equal(
            0.0,
            line.Direction.Z,
            12);

        double lengthSquared =
            line.Direction.X * line.Direction.X +
            line.Direction.Y * line.Direction.Y +
            line.Direction.Z * line.Direction.Z;

        Assert.Equal(
            1.0,
            lengthSquared,
            12);
    }

    [Fact]
    public void Map_VectorMagnitudeDoesNotAffectCanonicalDirection()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Line origin",
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        var direction =
            new StepDirectionEntity(
                "#20",
                "Line direction",
                [
                    0.0,
                    1.0,
                    0.0
                ]);

        var vector =
            new StepVectorEntity(
                "#30",
                "Line vector",
                "#20",
                1000.0);

        var source =
            new StepLineEntity(
                "#40",
                "Main line",
                "#10",
                "#30");

        MappingContext context =
            CreateContext(
                point,
                direction,
                vector,
                source);

        var mapper =
            new StepLineGeometryMapper();

        MappingResult<Line3D> result =
            mapper.Map(
                source,
                context);

        Line3D line =
            Assert.IsType<Line3D>(
                result.Value);

        Assert.Equal(
            new Vector3D(
                0.0,
                1.0,
                0.0),
            line.Direction);
    }

    [Fact]
    public void Map_WithPreExistingContextDiagnostic_DoesNotIncludeItInSuccessfulResult()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Line origin",
                [
                    1.0,
                    2.0,
                    3.0
                ]);

        var direction =
            new StepDirectionEntity(
                "#20",
                "Line direction",
                [
                    1.0,
                    0.0,
                    0.0
                ]);

        var vector =
            new StepVectorEntity(
                "#30",
                "Line vector",
                "#20",
                25.0);

        var source =
            new StepLineEntity(
                "#40",
                "Main line",
                "#10",
                "#30");

        MappingContext context =
            CreateContext(
                point,
                direction,
                vector,
                source);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-LINE-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepLineGeometryMapper();

        MappingResult<Line3D> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Assert.IsType<Line3D>(
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
    public void Map_WhenPointReferenceCannotBeResolved_ReturnsFailure()
    {
        var direction =
            new StepDirectionEntity(
                "#20",
                "Line direction",
                [
                    1.0,
                    0.0,
                    0.0
                ]);

        var vector =
            new StepVectorEntity(
                "#30",
                "Line vector",
                "#20",
                1.0);

        var source =
            new StepLineEntity(
                "#40",
                "Main line",
                "#10",
                "#30");

        MappingContext context =
            CreateContext(
                direction,
                vector,
                source);

        var mapper =
            new StepLineGeometryMapper();

        MappingResult<Line3D> result =
            mapper.Map(
                source,
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

        Assert.Single(
            context.Diagnostics);

        Assert.Same(
            diagnostic,
            context.Diagnostics[0]);
    }

    [Fact]
    public void Map_WhenVectorReferenceCannotBeResolved_ReturnsFailure()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Line origin",
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        var source =
            new StepLineEntity(
                "#40",
                "Main line",
                "#10",
                "#30");

        MappingContext context =
            CreateContext(
                point,
                source);

        var mapper =
            new StepLineGeometryMapper();

        MappingResult<Line3D> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

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
    public void Map_WhenDirectionReferenceCannotBeResolved_ReturnsFailure()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Line origin",
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        var vector =
            new StepVectorEntity(
                "#30",
                "Line vector",
                "#20",
                1.0);

        var source =
            new StepLineEntity(
                "#40",
                "Main line",
                "#10",
                "#30");

        MappingContext context =
            CreateContext(
                point,
                vector,
                source);

        var mapper =
            new StepLineGeometryMapper();

        MappingResult<Line3D> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

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
    public void Map_WhenPointIsNotThreeDimensional_ReturnsInvalidLineFailure()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Invalid point",
                [
                    1.0,
                    2.0
                ]);

        var direction =
            new StepDirectionEntity(
                "#20",
                "Line direction",
                [
                    1.0,
                    0.0,
                    0.0
                ]);

        var vector =
            new StepVectorEntity(
                "#30",
                "Line vector",
                "#20",
                1.0);

        var source =
            new StepLineEntity(
                "#40",
                "Main line",
                "#10",
                "#30");

        MappingContext context =
            CreateContext(
                point,
                direction,
                vector,
                source);

        var mapper =
            new StepLineGeometryMapper();

        MappingResult<Line3D> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.Null(
            result.Value);

        MappingDiagnostic diagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "MAP-STEP-LINE-001",
            diagnostic.Code);

        Assert.Contains(
            "CARTESIAN_POINT",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.Contains(
            "requires exactly 3 values",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WhenDirectionIsNotThreeDimensional_ReturnsInvalidLineFailure()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Line origin",
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        var direction =
            new StepDirectionEntity(
                "#20",
                "Invalid direction",
                [
                    1.0,
                    0.0
                ]);

        var vector =
            new StepVectorEntity(
                "#30",
                "Line vector",
                "#20",
                1.0);

        var source =
            new StepLineEntity(
                "#40",
                "Main line",
                "#10",
                "#30");

        MappingContext context =
            CreateContext(
                point,
                direction,
                vector,
                source);

        var mapper =
            new StepLineGeometryMapper();

        MappingResult<Line3D> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Failed,
            result.Status);

        Assert.Null(
            result.Value);

        MappingDiagnostic diagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Equal(
            "MAP-STEP-LINE-001",
            diagnostic.Code);

        Assert.Contains(
            "DIRECTION",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.Contains(
            "requires exactly 3 values",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WhenDirectionMagnitudeIsZero_ReturnsInvalidLineFailure()
    {
        var point =
            new StepCartesianPointEntity(
                "#10",
                "Line origin",
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        var direction =
            new StepDirectionEntity(
                "#20",
                "Zero direction",
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        var vector =
            new StepVectorEntity(
                "#30",
                "Line vector",
                "#20",
                1.0);

        var source =
            new StepLineEntity(
                "#40",
                "Main line",
                "#10",
                "#30");

        MappingContext context =
            CreateContext(
                point,
                direction,
                vector,
                source);

        var mapper =
            new StepLineGeometryMapper();

        MappingResult<Line3D> result =
            mapper.Map(
                source,
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
            "MAP-STEP-LINE-001",
            diagnostic.Code);

        Assert.Contains(
            "magnitude is zero",
            diagnostic.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        var mapper =
            new StepLineGeometryMapper();

        MappingContext context =
            CreateContext();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                null!,
                context));
    }

    [Fact]
    public void Map_WithNullContext_ThrowsArgumentNullException()
    {
        var mapper =
            new StepLineGeometryMapper();

        var source =
            new StepLineEntity(
                "#40",
                "Main line",
                "#10",
                "#30");

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                source,
                null!));
    }

    private static MappingContext CreateContext(
        params StepSemanticEntity[] entities)
    {
        var semanticResolver =
            new StepSemanticEntityResolver(
                entities);

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        return new MappingContext(
            referenceResolver);
    }
}