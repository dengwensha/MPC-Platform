using MPC.Application.Manufacturing;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Semantics;
using MPC.Connectors.Step.Validation;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;

namespace MPC.Tests.Connectors;

public sealed class Ge03B01StepPoleFixtureTests
{
    [Fact]
    public void AnalyticQuarterCone_StepSurface_ParsesMapsAndRejectsPole()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "TestData", "Step",
            "GE03B01-QuarterConePole.step");
        // A STEP surface-only fixture; NOT a valid trimmed solid or an independently exported CAD part.
        Assert.True(File.Exists(path));
        var inventory = new StepDataEntityReader(new StepDataInventoryValidator()).Read(path).Value;
        Assert.NotNull(inventory);
        var reader = new StepSemanticEntityReader(new StepParameterParser(),
            [new StepCartesianPointEntityHandler(), new StepRationalBSplineSurfaceEntityHandler()]);
        var entities = inventory.Entities.Select(reader.Read).OfType<StepSemanticEntity>().ToArray();
        var source = Assert.Single(entities.OfType<StepRationalBSplineSurfaceEntity>());
        var context = new MappingContext(new StepReferenceResolver(
            new StepSemanticEntityResolver(entities)));
        var result = new StepRationalBSplineSurfaceGeometryMapper().Map(source, context);
        Assert.True(result.IsSuccessful);
        var surface = Assert.IsType<RationalBSplineSurface3D>(result.Value);
        Assert.False(RationalBSplineNormalQuery.TryEvaluate(surface, 0.17, 1.0, out _));
        Assert.False(RationalBSplineNormalQuery.TryEvaluate(surface, 0.5, 1.0, out _));
        Assert.False(RationalBSplineNormalQuery.TryEvaluate(surface, 0.83, 1.0, out _));
        Assert.True(RationalBSplineNormalQuery.TryEvaluate(surface, 0.5, 0.5, out var normal));
        Assert.Equal(0.5, normal.X, 7);
        Assert.Equal(0.5, normal.Y, 7);
        Assert.Equal(Math.Sqrt(0.5), normal.Z, 7);
    }
}
