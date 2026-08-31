using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepCanonicalCircleResolutionTests
{
    [Fact]
    public void Resolve_Circle_ReturnsCanonicalCircleAndPreservesIdentity()
    {
        StepSemanticEntity[] entities =
            CreateCircleEntities();

        var semanticResolver =
            new StepSemanticEntityResolver(
                entities);

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        var canonicalResolver =
            new StepCanonicalReferenceResolver(
                semanticResolver);

        var context =
            new MappingContext(
                referenceResolver,
                canonicalResolver);

        MappingResult<Circle3D> first =
            canonicalResolver.Resolve<Circle3D>(
                "#30",
                context);

        MappingResult<ICurve3D> second =
            canonicalResolver.Resolve<ICurve3D>(
                "#30",
                context);

        Assert.Equal(
            MappingStatus.Succeeded,
            first.Status);

        Assert.Equal(
            MappingStatus.Succeeded,
            second.Status);

        Circle3D circle =
            Assert.IsType<Circle3D>(
                first.Value);

        ICurve3D curve =
            Assert.IsAssignableFrom<ICurve3D>(
                second.Value);

        Assert.Same(
            circle,
            curve);

        Assert.Equal(
            new Point3D(
                10.0,
                20.0,
                30.0),
            circle.Center);

        Assert.Equal(
            new Vector3D(
                0.0,
                0.0,
                1.0),
            circle.Normal);

        Assert.Equal(
            12.5,
            circle.Radius);

        Assert.Empty(
            first.Diagnostics);

        Assert.Empty(
            second.Diagnostics);

        Assert.Empty(
            context.Diagnostics);
    }

    [Fact]
    public void Resolve_CircleAsWrongCanonicalType_ReturnsTypeMismatchFailure()
    {
        StepSemanticEntity[] entities =
            CreateCircleEntities();

        var semanticResolver =
            new StepSemanticEntityResolver(
                entities);

        var referenceResolver =
            new StepReferenceResolver(
                semanticResolver);

        var canonicalResolver =
            new StepCanonicalReferenceResolver(
                semanticResolver);

        var context =
            new MappingContext(
                referenceResolver,
                canonicalResolver);

        MappingResult<Plane3D> result =
            canonicalResolver.Resolve<Plane3D>(
                "#30",
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
            "MAP-STEP-CANONICAL-REFERENCE-002",
            diagnostic.Code);

        Assert.Contains(
            "Circle3D",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.Contains(
            "Plane3D",
            diagnostic.Message,
            StringComparison.Ordinal);

        Assert.Same(
            diagnostic,
            Assert.Single(
                context.Diagnostics));
    }

    private static StepSemanticEntity[] CreateCircleEntities()
    {
        var location =
            new StepCartesianPointEntity(
                "#10",
                "Circle center",
                [
                    10.0,
                    20.0,
                    30.0
                ]);

        var axis =
            new StepDirectionEntity(
                "#11",
                "Circle axis",
                [
                    0.0,
                    0.0,
                    1.0
                ]);

        var referenceDirection =
            new StepDirectionEntity(
                "#12",
                "Circle reference direction",
                [
                    1.0,
                    0.0,
                    0.0
                ]);

        var placement =
            new StepAxis2Placement3DEntity(
                "#20",
                "Circle placement",
                "#10",
                "#11",
                "#12");

        var circle =
            new StepCircleEntity(
                "#30",
                "Circle",
                "#20",
                12.5);

        return
        [
            location,
            axis,
            referenceDirection,
            placement,
            circle
        ];
    }
}