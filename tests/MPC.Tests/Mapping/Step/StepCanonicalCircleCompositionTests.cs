using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Mapping.Step;

public sealed class StepCanonicalCircleCompositionTests
{
    [Fact]
    public void Create_ResolveCircle_UsesProductionCircleComposition()
    {
        var location =
            new StepCartesianPointEntity(
                "#10",
                "Circle center",
                [
                    5.0,
                    6.0,
                    7.0
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

        var sourceCircle =
            new StepCircleEntity(
                "#30",
                "Circle",
                "#20",
                25.0);

        StepSemanticEntity[] entities =
        [
            location,
            axis,
            referenceDirection,
            placement,
            sourceCircle
        ];

        var semanticResolver =
            new StepSemanticEntityResolver(
                entities);

        StepCanonicalMappingOperation operation =
            StepCanonicalMappingOperation.Create(
                semanticResolver);

        MappingResult<ICurve3D> result =
            operation.Resolve<ICurve3D>(
                "#30");

        Assert.Equal(
            MappingStatus.Succeeded,
            result.Status);

        Assert.True(
            result.IsSuccessful);

        Circle3D circle =
            Assert.IsType<Circle3D>(
                result.Value);

        Assert.Equal(
            new Point3D(
                5.0,
                6.0,
                7.0),
            circle.Center);

        Assert.Equal(
            new Vector3D(
                0.0,
                0.0,
                1.0),
            circle.Normal);

        Assert.Equal(
            25.0,
            circle.Radius);

        Assert.Empty(
            result.Diagnostics);

        Assert.Empty(
            operation.Context.Diagnostics);
    }
}