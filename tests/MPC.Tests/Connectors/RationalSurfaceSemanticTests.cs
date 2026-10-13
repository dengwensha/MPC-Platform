using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class RationalSurfaceSemanticTests
{
    [Theory]
    [InlineData(3, 2, false, false)]
    [InlineData(2, 3, true, false)]
    [InlineData(1, 1, false, true)]
    public void Read_PreservesDegreesGridWeightsAndMetadata(int uDegree, int vDegree, bool reverse, bool commas)
    {
        string[] components = Components(uDegree, vDegree);
        if (reverse) Array.Reverse(components);
        var entity = Assert.IsType<StepRationalBSplineSurfaceEntity>(Reader().Read(Record(components, commas)));
        var basis = entity.Basis;
        Assert.Equal("#75", entity.SourceEntityId);
        Assert.Equal("COMPLEX_ENTITY", entity.SourceEntityType);
        Assert.Equal("A');, B", basis.Name);
        Assert.Equal(uDegree, basis.UDegree);
        Assert.Equal(vDegree, basis.VDegree);
        Assert.Equal("UNSPECIFIED", basis.SurfaceForm);
        Assert.Equal("UNSPECIFIED", basis.KnotSpecification);
        Assert.Equal(StepLogicalValue.True, basis.UClosed);
        Assert.Equal(StepLogicalValue.False, basis.VClosed);
        Assert.Equal(StepLogicalValue.Unknown, basis.SelfIntersect);
        Assert.Equal(new double[] { -2, 5 }, basis.UKnots);
        Assert.Equal(new double[] { -3, 7 }, basis.VKnots);
        Assert.Equal(new long[] { uDegree + 1, uDegree + 1 }, basis.UMultiplicities);
        Assert.Equal(new long[] { vDegree + 1, vDegree + 1 }, basis.VMultiplicities);
        for (int u = 0; u <= uDegree; u++)
            for (int v = 0; v <= vDegree; v++)
            {
                int index = u * (vDegree + 1) + v + 1;
                Assert.Equal($"#{index}", basis.ControlPointReferenceGrid[u][v]);
                Assert.Equal(index + 0.25, entity.Weights[u][v]);
            }
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("duplicate")]
    [InlineData("extra")]
    [InlineData("arity")]
    [InlineData("string")]
    [InlineData("weight-type")]
    [InlineData("weight-zero")]
    [InlineData("weight-negative")]
    [InlineData("weight-rows")]
    [InlineData("weight-columns")]
    [InlineData("degree")]
    [InlineData("knot-order")]
    [InlineData("logical")]
    public void InvalidRecord_FailsWithSourceIdentity(string mutation)
    {
        var components = Components(1, 1).ToList();
        switch (mutation)
        {
            case "missing": components.RemoveAt(0); break;
            case "duplicate": components.Add(components[0]); break;
            case "extra": components.Add("CURVE()"); break;
            case "arity": components[0] = "BOUNDED_SURFACE(1)"; break;
            case "string": components[5] = "REPRESENTATION_ITEM(1)"; break;
            case "weight-type": components[4] = "RATIONAL_B_SPLINE_SURFACE(((1.,$),(3.,4.)))"; break;
            case "weight-zero": components[4] = "RATIONAL_B_SPLINE_SURFACE(((1.,0.),(3.,4.)))"; break;
            case "weight-negative": components[4] = "RATIONAL_B_SPLINE_SURFACE(((1.,-2.),(3.,4.)))"; break;
            case "weight-rows": components[4] = "RATIONAL_B_SPLINE_SURFACE(((1.,2.)))"; break;
            case "weight-columns": components[4] = "RATIONAL_B_SPLINE_SURFACE(((1.),(3.,4.)))"; break;
            case "degree": components[1] = components[1].Replace("(1,1,", "(0,1,", StringComparison.Ordinal); break;
            case "knot-order": components[2] = components[2].Replace("(-2.,5.)", "(5.,-2.)", StringComparison.Ordinal); break;
            case "logical": components[1] = components[1].Replace(".U.", ".X.", StringComparison.Ordinal); break;
        }
        StepSemanticEntityReader reader = Reader();
        var failure = Assert.Throws<StepSemanticValueException>(() => reader.Read(Record(components)));
        Assert.Equal("#75", failure.SourceEntityId);
        Assert.Equal("COMPLEX_ENTITY", failure.SourceEntityType);
        Assert.False(string.IsNullOrWhiteSpace(failure.ParameterName));
        Assert.False(string.IsNullOrWhiteSpace(failure.RejectedValue));
        // Failed reads retain no mutable reader state.
        Assert.IsType<StepRationalBSplineSurfaceEntity>(reader.Read(Record(Components(1, 1))));
    }

    [Fact]
    public void MetadataMismatch_IsRejected()
    {
        var original = Record(Components(1, 1));
        var record = new StepDataEntityRecord("#75", "COMPLEX_ENTITY", original.RawStatement,
            CadReadStatus.Complete, original.ComponentTypes.Reverse());
        Assert.Throws<StepSemanticValueException>(() => Reader().Read(record));
    }

    [Theory]
    [InlineData("RATIONAL_B_SPLINE_CURVE((1.,2.))")]
    [InlineData("REPRESENTATION_ITEM('unrelated')")]
    public void UnsupportedComplexFamily_RemainsUninterpreted(string component)
    {
        Assert.Null(Reader().Read(Record([component])));
    }

    [Fact]
    public void Weights_AreDefensivelyCopiedAndReadOnly()
    {
        var original = Assert.IsType<StepRationalBSplineSurfaceEntity>(Reader().Read(Record(Components(1, 1))));
        double[][] weights = [[1, 2], [3, 4]];
        var entity = new StepRationalBSplineSurfaceEntity(original.Basis, weights);
        weights[0][0] = 99;
        weights[1] = [7, 8];
        Assert.Equal(1, entity.Weights[0][0]);
        Assert.Equal(3, entity.Weights[1][0]);
        Assert.Throws<NotSupportedException>(() => ((IList<double>)entity.Weights[0])[0] = 9);
        Assert.Throws<NotSupportedException>(() => ((IList<IReadOnlyList<double>>)entity.Weights)[0] = new double[] { 9, 9 });
    }

    private static StepSemanticEntityReader Reader() => new(new StepParameterParser(),
        [new StepRationalBSplineSurfaceEntityHandler()]);

    private static StepDataEntityRecord Record(IEnumerable<string> components, bool commas = false)
    {
        string[] values = components.ToArray();
        return new StepDataEntityRecord("#75", "COMPLEX_ENTITY",
            "#75=(" + string.Join(commas ? ",\n" : "\n", values) + ");", CadReadStatus.Complete,
            values.Select(value => value[..value.IndexOf('(')]));
    }

    private static string[] Components(int u, int v)
    {
        string grid = string.Join(",", Enumerable.Range(0, u + 1).Select(row =>
            "(" + string.Join(",", Enumerable.Range(0, v + 1).Select(col => $"#{row * (v + 1) + col + 1}")) + ")"));
        string weights = string.Join(",", Enumerable.Range(0, u + 1).Select(row =>
            "(" + string.Join(",", Enumerable.Range(0, v + 1).Select(col => $"{row * (v + 1) + col + 1}.25")) + ")"));
        return
        [
            "BOUNDED_SURFACE()",
            $"B_SPLINE_SURFACE({u},{v},({grid}),.UNSPECIFIED.,.T.,.F.,.U.)",
            $"B_SPLINE_SURFACE_WITH_KNOTS(({u + 1},{u + 1}),({v + 1},{v + 1}),(-2.,5.),(-3.,7.),.UNSPECIFIED.)",
            "GEOMETRIC_REPRESENTATION_ITEM()",
            $"RATIONAL_B_SPLINE_SURFACE(({weights}))",
            "REPRESENTATION_ITEM('A'');, B')",
            "SURFACE()"
        ];
    }
}
