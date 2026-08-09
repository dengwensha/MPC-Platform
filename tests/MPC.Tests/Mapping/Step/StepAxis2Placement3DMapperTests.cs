using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepAxis2Placement3DMapperTests
{
    [Fact]
    public void Map_WithValidReferences_ReturnsCanonicalCoordinateSystem()
    {
        var location =
            new StepCartesianPoint(
                "#10",
                "Origin",
                [
                    1.0,
                    2.0,
                    3.0
                ]);

        var axis =
            new StepDirection(
                "#20",
                "Axis",
                [
                    0.0,
                    0.0,
                    1.0
                ]);

        var referenceDirection =
            new StepDirection(
                "#30",
                "Reference direction",
                [
                    1.0,
                    0.0,
                    0.0
                ]);

        var source =
            new StepAxis2Placement3DEntity(
                "#40",
                "Placement",
                "#10",
                "#20",
                "#30");

        MappingContext context =
            CreateContext(
                location,
                axis,
                referenceDirection);

        var mapper =
            new StepAxis2Placement3DMapper();

        MappingResult<CoordinateSystem3D> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        CoordinateSystem3D coordinateSystem =
            Assert.IsType<CoordinateSystem3D>(
                result.Value);

        Assert.Equal(
            new Point3D(
                1.0,
                2.0,
                3.0),
            coordinateSystem.Origin);

        Assert.Equal(
            new Vector3D(
                0.0,
                0.0,
                1.0),
            coordinateSystem.ZAxis);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WithDefaultAxisAndReferenceDirection_ReturnsCanonicalCoordinateSystem()
    {
        var location =
            new StepCartesianPoint(
                "#10",
                "Origin",
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        var source =
            new StepAxis2Placement3DEntity(
                "#40",
                "Placement",
                "#10",
                null,
                null);

        MappingContext context =
            CreateContext(
                location);

        var mapper =
            new StepAxis2Placement3DMapper();

        MappingResult<CoordinateSystem3D> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        CoordinateSystem3D coordinateSystem =
            Assert.IsType<CoordinateSystem3D>(
                result.Value);

        Assert.Equal(
            new Point3D(
                0.0,
                0.0,
                0.0),
            coordinateSystem.Origin);

        Assert.Equal(
            new Vector3D(
                0.0,
                0.0,
                1.0),
            coordinateSystem.ZAxis);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Map_WithPreExistingWarning_DoesNotIncludeItInSuccessfulResult()
    {
        var location =
            new StepCartesianPoint(
                "#10",
                "Origin",
                [
                    1.0,
                    2.0,
                    3.0
                ]);

        var source =
            new StepAxis2Placement3DEntity(
                "#40",
                "Placement",
                "#10",
                null,
                null);

        MappingContext context =
            CreateContext(
                location);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-WARNING-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Warning);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepAxis2Placement3DMapper();

        MappingResult<CoordinateSystem3D> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Assert.NotNull(
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
        var location =
            new StepCartesianPoint(
                "#10",
                "Origin",
                [
                    1.0,
                    2.0,
                    3.0
                ]);

        var source =
            new StepAxis2Placement3DEntity(
                "#40",
                "Placement",
                "#10",
                null,
                null);

        MappingContext context =
            CreateContext(
                location);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-ERROR-001",
                "Error from a previous mapping operation.",
                MappingDiagnosticSeverity.Error);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepAxis2Placement3DMapper();

        MappingResult<CoordinateSystem3D> result =
            mapper.Map(
                source,
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Assert.NotNull(
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
    public void Map_WhenLocationCannotBeResolved_ReturnsOnlyCurrentOperationFailure()
    {
        var source =
            new StepAxis2Placement3DEntity(
                "#40",
                "Placement",
                "#10",
                null,
                null);

        var existingDiagnostic =
            new MappingDiagnostic(
                "TEST-PRE-EXISTING-WARNING-001",
                "Diagnostic from a previous mapping operation.",
                MappingDiagnosticSeverity.Warning);

        var resolutionDiagnostic =
            new MappingDiagnostic(
                "TEST-LOCATION-RESOLUTION-001",
                "STEP CARTESIAN_POINT '#10' could not be resolved.",
                MappingDiagnosticSeverity.Error);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<string, object>(),
                new Dictionary<string, MappingDiagnostic>
                {
                    ["#10"] =
                        resolutionDiagnostic
                });

        var context =
            new MappingContext(
                resolver);

        context.AddDiagnostic(
            existingDiagnostic);

        var mapper =
            new StepAxis2Placement3DMapper();

        MappingResult<CoordinateSystem3D> result =
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

        MappingDiagnostic resultDiagnostic =
            Assert.Single(
                result.Diagnostics);

        Assert.Same(
            resolutionDiagnostic,
            resultDiagnostic);

        Assert.Equal(
            2,
            context.Diagnostics.Count);

        Assert.Same(
            existingDiagnostic,
            context.Diagnostics[0]);

        Assert.Same(
            resolutionDiagnostic,
            context.Diagnostics[1]);
    }

    [Fact]
    public void Map_WhenAxisCannotBeResolved_ReturnsOnlyCurrentOperationFailure()
    {
        var location =
            new StepCartesianPoint(
                "#10",
                "Origin",
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        var source =
            new StepAxis2Placement3DEntity(
                "#40",
                "Placement",
                "#10",
                "#20",
                null);

        var resolutionDiagnostic =
            new MappingDiagnostic(
                "TEST-AXIS-RESOLUTION-001",
                "STEP DIRECTION '#20' could not be resolved.",
                MappingDiagnosticSeverity.Error);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<string, object>
                {
                    ["#10"] =
                        location
                },
                new Dictionary<string, MappingDiagnostic>
                {
                    ["#20"] =
                        resolutionDiagnostic
                });

        var context =
            new MappingContext(
                resolver);

        var mapper =
            new StepAxis2Placement3DMapper();

        MappingResult<CoordinateSystem3D> result =
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

        Assert.Same(
            resolutionDiagnostic,
            diagnostic);

        Assert.Same(
            resolutionDiagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WhenReferenceDirectionCannotBeResolved_ReturnsOnlyCurrentOperationFailure()
    {
        var location =
            new StepCartesianPoint(
                "#10",
                "Origin",
                [
                    0.0,
                    0.0,
                    0.0
                ]);

        var source =
            new StepAxis2Placement3DEntity(
                "#40",
                "Placement",
                "#10",
                null,
                "#30");

        var resolutionDiagnostic =
            new MappingDiagnostic(
                "TEST-REFERENCE-DIRECTION-RESOLUTION-001",
                "STEP DIRECTION '#30' could not be resolved.",
                MappingDiagnosticSeverity.Error);

        var resolver =
            new StubReferenceResolver(
                new Dictionary<string, object>
                {
                    ["#10"] =
                        location
                },
                new Dictionary<string, MappingDiagnostic>
                {
                    ["#30"] =
                        resolutionDiagnostic
                });

        var context =
            new MappingContext(
                resolver);

        var mapper =
            new StepAxis2Placement3DMapper();

        MappingResult<CoordinateSystem3D> result =
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

        Assert.Same(
            resolutionDiagnostic,
            diagnostic);

        Assert.Same(
            resolutionDiagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    [Fact]
    public void Map_WithNullSource_ThrowsArgumentNullException()
    {
        MappingContext context =
            CreateContext();

        var mapper =
            new StepAxis2Placement3DMapper();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                null!,
                context));
    }

    [Fact]
    public void Map_WithNullContext_ThrowsArgumentNullException()
    {
        var source =
            new StepAxis2Placement3DEntity(
                "#40",
                "Placement",
                "#10",
                null,
                null);

        var mapper =
            new StepAxis2Placement3DMapper();

        Assert.Throws<ArgumentNullException>(
            () => mapper.Map(
                source,
                null!));
    }

    private static MappingContext CreateContext(
        params StepSemanticEntity[] entities)
    {
        var values =
            new Dictionary<string, object>(
                StringComparer.Ordinal);

        foreach (StepSemanticEntity entity in entities)
        {
            if (!string.IsNullOrWhiteSpace(
                    entity.SourceEntityId))
            {
                values.Add(
                    entity.SourceEntityId,
                    entity);
            }
        }

        return new MappingContext(
            new StubReferenceResolver(
                values,
                new Dictionary<string, MappingDiagnostic>()));
    }

    private sealed class StubReferenceResolver :
        IReferenceResolver
    {
        private readonly IReadOnlyDictionary<string, object>
            _values;

        private readonly IReadOnlyDictionary<
            string,
            MappingDiagnostic> _failures;

        public StubReferenceResolver(
            IReadOnlyDictionary<string, object> values,
            IReadOnlyDictionary<string, MappingDiagnostic> failures)
        {
            ArgumentNullException.ThrowIfNull(
                values);

            ArgumentNullException.ThrowIfNull(
                failures);

            _values =
                values;

            _failures =
                failures;
        }

        public ReferenceResolutionResult<T> Resolve<T>(
            string referenceId)
            where T : class
        {
            if (_failures.TryGetValue(
                    referenceId,
                    out MappingDiagnostic? diagnostic))
            {
                return ReferenceResolutionResult<T>.Failure(
                    referenceId,
                    [
                        diagnostic
                    ]);
            }

            if (!_values.TryGetValue(
                    referenceId,
                    out object? value))
            {
                var missingDiagnostic =
                    new MappingDiagnostic(
                        "TEST-REFERENCE-RESOLUTION-FAILED",
                        $"Reference '{referenceId}' could not be resolved.",
                        MappingDiagnosticSeverity.Error);

                return ReferenceResolutionResult<T>.Failure(
                    referenceId,
                    [
                        missingDiagnostic
                    ]);
            }

            if (value is not T typedValue)
            {
                var typeDiagnostic =
                    new MappingDiagnostic(
                        "TEST-REFERENCE-TYPE-MISMATCH",
                        $"Reference '{referenceId}' did not resolve to '{typeof(T).Name}'.",
                        MappingDiagnosticSeverity.Error);

                return ReferenceResolutionResult<T>.Failure(
                    referenceId,
                    [
                        typeDiagnostic
                    ]);
            }

            return ReferenceResolutionResult<T>.Resolved(
                referenceId,
                typedValue);
        }
    }
}
