using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class RationalCurveSemanticTests
{
    [Theory]
    [InlineData(1, false, false)]
    [InlineData(2, true, false)]
    [InlineData(3, false, true)]
    public void Read_PreservesBasisWeightsAndMetadata(int degree, bool reverse, bool commas)
    {
        string[] components = Components(degree);
        if (reverse) Array.Reverse(components);
        var entity = Assert.IsType<StepRationalBSplineCurveEntity>(Reader().Read(Record(components, commas)));
        Assert.Equal("#20", entity.SourceEntityId);
        Assert.Equal("COMPLEX_ENTITY", entity.SourceEntityType);
        Assert.Equal("A');, B", entity.Basis.Name);
        Assert.Equal(degree, entity.Basis.Degree);
        Assert.Equal(StepLogicalValue.Unknown, entity.Basis.ClosedCurve);
        Assert.Equal(StepLogicalValue.True, entity.Basis.SelfIntersect);
        Assert.Equal("UNSPECIFIED", entity.Basis.CurveForm);
        Assert.Equal("UNSPECIFIED", entity.Basis.KnotSpecification);
        Assert.Equal(new double[] { -2, 5 }, entity.Basis.Knots);
        Assert.Equal(new long[] { degree + 1, degree + 1 }, entity.Basis.Multiplicities);
        Assert.Equal(Enumerable.Range(1, degree + 1).Select(i => $"#{i}"), entity.Basis.ControlPointReferences);
        Assert.Equal(Enumerable.Range(1, degree + 1).Select(i => i + 0.25), entity.Weights);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("duplicate")]
    [InlineData("extra")]
    [InlineData("arity")]
    [InlineData("name")]
    [InlineData("weight-type")]
    [InlineData("weight-zero")]
    [InlineData("weight-negative")]
    [InlineData("weight-infinity")]
    [InlineData("weight-count")]
    [InlineData("weight-scalar")]
    [InlineData("weight-nested")]
    [InlineData("degree")]
    [InlineData("multiplicity")]
    [InlineData("knot-order")]
    [InlineData("logical")]
    [InlineData("ambiguous")]
    public void InvalidRecord_IsEntityScopedAndReaderCanContinue(string mutation)
    {
        var components = Components(2).ToList();
        switch (mutation)
        {
            case "missing": components.RemoveAt(0); break;
            case "duplicate": components.Add(components[0]); break;
            case "extra": components.Add("SURFACE()"); break;
            case "arity": components[0] = "BOUNDED_CURVE(1)"; break;
            case "name": components[6] = "REPRESENTATION_ITEM(1)"; break;
            case "weight-type": components[5] = "RATIONAL_B_SPLINE_CURVE((1.,$,3.))"; break;
            case "weight-zero": components[5] = "RATIONAL_B_SPLINE_CURVE((1.,0.,3.))"; break;
            case "weight-negative": components[5] = "RATIONAL_B_SPLINE_CURVE((1.,-2.,3.))"; break;
            case "weight-infinity": components[5] = "RATIONAL_B_SPLINE_CURVE((1.,1.E309,3.))"; break;
            case "weight-count": components[5] = "RATIONAL_B_SPLINE_CURVE((1.,2.))"; break;
            case "weight-scalar": components[5] = "RATIONAL_B_SPLINE_CURVE(1.)"; break;
            case "weight-nested": components[5] = "RATIONAL_B_SPLINE_CURVE(((1.,2.,3.)))"; break;
            case "degree": components[1] = components[1].Replace("(2,", "(0,", StringComparison.Ordinal); break;
            case "multiplicity": components[2] = components[2].Replace("(3,3)", "(2,3)", StringComparison.Ordinal); break;
            case "knot-order": components[2] = components[2].Replace("(-2.,5.)", "(5.,-2.)", StringComparison.Ordinal); break;
            case "logical": components[1] = components[1].Replace(".U.", ".X.", StringComparison.Ordinal); break;
            case "ambiguous": components.Add("RATIONAL_B_SPLINE_SURFACE(((1.)))"); break;
        }
        var reader = Reader();
        var exception = Assert.Throws<StepSemanticValueException>(() => reader.Read(Record(components)));
        Assert.Equal("#20", exception.SourceEntityId);
        Assert.Equal("COMPLEX_ENTITY", exception.SourceEntityType);
        Assert.NotEmpty(exception.ParameterName);
        Assert.NotEmpty(exception.RejectedValue);
        Assert.IsType<StepRationalBSplineCurveEntity>(reader.Read(Record(Components(2))));
    }

    [Fact]
    public void MetadataMismatch_IsEntityScoped()
    {
        var original = Record(Components(2));
        var record = new StepDataEntityRecord("#20", "COMPLEX_ENTITY", original.RawStatement,
            CadReadStatus.Complete, original.ComponentTypes.Reverse());
        Assert.Throws<StepSemanticValueException>(() => Reader().Read(record));
    }

    [Fact]
    public void CompositeReader_PreservesSurfaceSupportAndUnknownFamilyBehavior()
    {
        string[] surface =
        [
            "BOUNDED_SURFACE()",
            "B_SPLINE_SURFACE(1,1,((#1,#2),(#3,#4)),.UNSPECIFIED.,.F.,.F.,.U.)",
            "B_SPLINE_SURFACE_WITH_KNOTS((2,2),(2,2),(-2.,5.),(-3.,7.),.UNSPECIFIED.)",
            "GEOMETRIC_REPRESENTATION_ITEM()",
            "RATIONAL_B_SPLINE_SURFACE(((1.,2.),(3.,4.)))",
            "REPRESENTATION_ITEM('surface')", "SURFACE()"
        ];
        var reader = Reader();
        var result = Assert.IsType<StepRationalBSplineSurfaceEntity>(reader.Read(Record(surface)));
        Assert.Equal(new double[] { 1, 2 }, result.Weights[0]);
        Assert.Equal(new double[] { 3, 4 }, result.Weights[1]);
        Assert.Null(reader.Read(Record(["REPRESENTATION_ITEM('unrelated')"])));
        Assert.IsType<StepRationalBSplineCurveEntity>(reader.Read(Record(Components(2))));
    }

    [Fact]
    public void Weights_AreCopiedAndValidatedAtTheSemanticBoundary()
    {
        var basis = Assert.IsType<StepRationalBSplineCurveEntity>(Reader().Read(Record(Components(2)))).Basis;
        double[] weights = [1, 0.5, 2];
        var entity = new StepRationalBSplineCurveEntity(basis, weights);
        weights[0] = 99;
        Assert.Equal(new double[] { 1, 0.5, 2 }, entity.Weights);
        Assert.Throws<NotSupportedException>(() => ((IList<double>)entity.Weights)[0] = 9);
        Assert.Throws<ArgumentNullException>(() => new StepRationalBSplineCurveEntity(null!, weights));
        Assert.Throws<ArgumentNullException>(() => new StepRationalBSplineCurveEntity(basis, null!));
        Assert.Throws<ArgumentException>(() => new StepRationalBSplineCurveEntity(basis, [1, 2]));
        foreach (double invalid in new[] { 0, -1, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            Assert.Throws<ArgumentOutOfRangeException>(() => new StepRationalBSplineCurveEntity(basis, [1, invalid, 2]));
    }

    private static StepSemanticEntityReader Reader() => new(new StepParameterParser(),
        [new StepComplexBSplineEntityHandler()]);

    private static StepDataEntityRecord Record(IEnumerable<string> components, bool commas = false)
    {
        string[] values = components.ToArray();
        return new StepDataEntityRecord("#20", "COMPLEX_ENTITY",
            "#20=(" + string.Join(commas ? ",\n" : "\n", values) + ");", CadReadStatus.Complete,
            values.Select(value => value[..value.IndexOf('(')]));
    }

    private static string[] Components(int degree) =>
    [
        "BOUNDED_CURVE()",
        $"B_SPLINE_CURVE({degree},({string.Join(",", Enumerable.Range(1, degree + 1).Select(i => $"#{i}"))}),.UNSPECIFIED.,.U.,.T.)",
        $"B_SPLINE_CURVE_WITH_KNOTS(({degree + 1},{degree + 1}),(-2.,5.),.UNSPECIFIED.)",
        "CURVE()", "GEOMETRIC_REPRESENTATION_ITEM()",
        $"RATIONAL_B_SPLINE_CURVE(({string.Join(",", Enumerable.Range(1, degree + 1).Select(i => $"{i}.25"))}))",
        "REPRESENTATION_ITEM('A'');, B')"
    ];
}
