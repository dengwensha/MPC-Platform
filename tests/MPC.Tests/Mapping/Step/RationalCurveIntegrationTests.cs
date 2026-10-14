using MPC.Connectors.Step.Semantics;
using MPC.Geometry.Contracts;
using MPC.Geometry.Primitives;
using MPC.Mapping;
using MPC.Mapping.Step;
using MPC.Topology.Primitives;

namespace MPC.Tests.Mapping.Step;

public sealed class RationalCurveIntegrationTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Resolver_ReturnsSameCurveAcrossTypedAndInterfaceRequests(bool interfaceFirst)
    {
        var operation = Operation(Entities());
        object first = interfaceFirst ? operation.Resolve<ICurve3D>("#20").Value! : operation.Resolve<RationalBSplineCurve3D>("#20").Value!;
        var curve = Assert.IsType<RationalBSplineCurve3D>(first);
        Assert.Same(curve, operation.Resolve<ICurve3D>("#20").Value);
        Assert.Same(curve, operation.Resolve<RationalBSplineCurve3D>("#20").Value);
        Assert.Equal(new double[] { 1, 0.5, 2 }, curve.Weights);
        Assert.Equal(2, curve.Basis.Degree);
        Assert.Equal(new double[] { -2, 5 }, curve.Basis.Knots);
        Assert.Equal(new[] { 3, 3 }, curve.Basis.Multiplicities);
        Assert.Equal(new Point3D[] { new(0,0,0), new(1,2,0), new(3,0,0) }, curve.Basis.ControlPoints);
        Assert.Empty(operation.Context.Diagnostics);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TypeMismatch_DoesNotPoisonCurveCacheOrSuccessfulDiagnostics(bool cached)
    {
        var operation = Operation(Entities());
        var priorCurve = cached ? operation.Resolve<RationalBSplineCurve3D>("#20").Value : null;
        var mismatch = operation.Resolve<Vertex>("#20");
        Assert.Equal("MAP-STEP-CANONICAL-REFERENCE-002", Assert.Single(mismatch.Diagnostics).Code);
        var valid = operation.Resolve<ICurve3D>("#20");
        Assert.Equal(MappingStatus.Succeeded, valid.Status);
        Assert.Empty(valid.Diagnostics);
        if (cached) Assert.Same(priorCurve, valid.Value);
        Assert.Same(valid.Value, operation.Resolve<RationalBSplineCurve3D>("#20").Value);
    }

    [Theory]
    [InlineData("missing", "STEP_REFERENCE_RESOLUTION_FAILED")]
    [InlineData("wrong-type", "STEP_REFERENCE_TYPE_MISMATCH")]
    [InlineData("two-coordinates", "MAP-STEP-BSPLINE-CURVE-001")]
    public void BadControlPoint_ReturnsOnlyCurrentFailure(string mutation, string expectedCode)
    {
        var entities = Entities().Where(e => e.SourceEntityId != "#2").ToList();
        if (mutation == "wrong-type") entities.Add(new StepDirectionEntity("#2", "", [1,0,0]));
        if (mutation == "two-coordinates") entities.Add(new StepCartesianPointEntity("#2", "", [1,2]));
        var operation = Operation(entities);
        operation.Context.AddDiagnostic(new MappingDiagnostic("PRIOR", "earlier", MappingDiagnosticSeverity.Error));
        var failed = operation.Resolve<ICurve3D>("#20");
        Assert.Equal(MappingStatus.Failed, failed.Status);
        Assert.Null(failed.Value);
        Assert.Equal(expectedCode, Assert.Single(failed.Diagnostics).Code);
        var repeated = operation.Resolve<ICurve3D>("#20");
        Assert.Equal(expectedCode, Assert.Single(repeated.Diagnostics).Code);
        Assert.Null(repeated.Value);
    }

    [Fact]
    public void DirectMapping_RequiresOnlySemanticPointResolution()
    {
        var context = new MappingContext(new StepReferenceResolver(new StepSemanticEntityResolver(Entities())));
        context.AddDiagnostic(new MappingDiagnostic("PRIOR", "earlier", MappingDiagnosticSeverity.Error));
        var result = new StepRationalBSplineCurveGeometryMapper().Map(Curve(), context);
        Assert.Equal(MappingStatus.Succeeded, result.Status);
        Assert.IsType<RationalBSplineCurve3D>(result.Value);
        Assert.Empty(result.Diagnostics);
        Assert.Null(context.CanonicalReferenceResolver);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EdgeAndOrientedUse_PreserveSharedCurveAndVertexIdentity(bool forward)
    {
        var operation = Operation(Entities());
        var edgeResult = operation.Resolve<Edge>("#50");
        Assert.Equal(MappingStatus.Succeeded, edgeResult.Status);
        var edge = Assert.IsType<Edge>(edgeResult.Value);
        Assert.Same(operation.Resolve<RationalBSplineCurve3D>("#20").Value, edge.Curve);
        Assert.Same(operation.Resolve<Vertex>("#10").Value, edge.StartVertex);
        Assert.Same(operation.Resolve<Vertex>("#11").Value, edge.EndVertex);
        Assert.Same(edge, operation.Resolve<Edge>("#50").Value);
        var use = new StepOrientedEdgeMapper().Map(new StepOrientedEdgeEntity("#60", "", "#50", forward), operation.Context);
        Assert.Equal(MappingStatus.Succeeded, use.Status);
        var value = Assert.IsType<EdgeUse>(use.Value);
        Assert.Same(edge, value.Edge);
        Assert.Same(forward ? edge.StartVertex : edge.EndVertex, value.StartVertex);
        Assert.Same(forward ? edge.EndVertex : edge.StartVertex, value.EndVertex);
        Assert.Empty(operation.Context.Diagnostics);
    }

    [Fact]
    public void SameSenseFalse_RemainsExplicitlyUnsupported()
    {
        var entities = Entities().Where(e => e.SourceEntityId != "#50")
            .Append(new StepEdgeCurveEntity("#50", "", "#10", "#11", "#20", false));
        var result = Operation(entities).Resolve<Edge>("#50");
        Assert.Equal("MAP-STEP-EDGE-CURVE-002", Assert.Single(result.Diagnostics).Code);
        Assert.Null(result.Value);
    }

    private static StepCanonicalMappingOperation Operation(IEnumerable<StepSemanticEntity> entities) =>
        StepCanonicalMappingOperation.Create(new StepSemanticEntityResolver(entities));

    private static StepRationalBSplineCurveEntity Curve() =>
        new(new StepBSplineCurveWithKnotsEntity("#20", "", 2, ["#1","#2","#3"], "UNSPECIFIED", StepLogicalValue.False,
            StepLogicalValue.Unknown, [3,3], [-2,5], "UNSPECIFIED"), [1,0.5,2]);

    private static IEnumerable<StepSemanticEntity> Entities() =>
    [
        new StepCartesianPointEntity("#1", "", [0,0,0]),
        new StepCartesianPointEntity("#2", "", [1,2,0]),
        new StepCartesianPointEntity("#3", "", [3,0,0]),
        new StepVertexPointEntity("#10", "", "#1"), new StepVertexPointEntity("#11", "", "#3"),
        Curve(), new StepEdgeCurveEntity("#50", "", "#10", "#11", "#20", true)
    ];
}
