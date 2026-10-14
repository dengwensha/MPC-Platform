using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Parsing;
using MPC.Connectors.Step.Semantics;

namespace MPC.Tests.Connectors;

public sealed class BSplineCurveSemanticTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Read_PreservesOrderedDataAndLogicalMetadata(int degree)
    {
        string references = string.Join(",", Enumerable.Range(1, degree + 1).Select(x => $"#{x}"));
        var curve = Assert.IsType<StepBSplineCurveWithKnotsEntity>(Reader().Read(Record(
            $"'A''B',{degree},({references}),.UNSPECIFIED.,.U.,.T.,({degree + 1},{degree + 1}),(-2.,5.),.UNSPECIFIED.")));
        Assert.Equal("#20", curve.SourceEntityId);
        Assert.Equal("A'B", curve.Name);
        Assert.Equal(degree, curve.Degree);
        Assert.Equal(Enumerable.Range(1, degree + 1).Select(x => $"#{x}"), curve.ControlPointReferences);
        Assert.Equal(StepLogicalValue.Unknown, curve.ClosedCurve);
        Assert.Equal(StepLogicalValue.True, curve.SelfIntersect);
        Assert.Equal("UNSPECIFIED", curve.CurveForm);
        Assert.Equal("UNSPECIFIED", curve.KnotSpecification);
        Assert.Equal(new double[] { -2, 5 }, curve.Knots);
        Assert.Equal(new long[] { degree + 1, degree + 1 }, curve.Multiplicities);
    }

    [Theory]
    [InlineData("2,(#1", "0,(#1")]
    [InlineData("(#1,#2,#3)", "(#1,#2)")]
    [InlineData("(#1,#2,#3)", "(#1,2,#3)")]
    [InlineData(".UNSPECIFIED.", ".INVALID.")]
    [InlineData(".F.", ".X.")]
    [InlineData("(3,3)", "(0,6)")]
    [InlineData("(3,3)", "(2,3)")]
    [InlineData("(3,3)", "(6)")]
    [InlineData("(-2.,5.)", "(5.,-2.)")]
    [InlineData("(-2.,5.)", "(-2.,-2.)")]
    [InlineData("(-2.,5.)", "(0.,1.E309)")]
    [InlineData("(-2.,5.)", "(0.,$)")]
    public void InvalidParameters_FailWithSourceIdentityAndReaderCanContinue(string oldText, string newText)
    {
        var reader = Reader();
        var failure = Assert.Throws<StepSemanticValueException>(() =>
            reader.Read(Record(Valid.Replace(oldText, newText, StringComparison.Ordinal))));
        Assert.Equal("#20", failure.SourceEntityId);
        Assert.Equal("B_SPLINE_CURVE_WITH_KNOTS", failure.SourceEntityType);
        Assert.NotEmpty(failure.RejectedValue);
        Assert.IsType<StepBSplineCurveWithKnotsEntity>(reader.Read(Record(Valid)));
    }

    [Theory]
    [InlineData("''")]
    [InlineData("'unterminated")]
    [InlineData("'x',2,(#1,#2,#3")]
    public void MalformedParameterText_IsEntityScoped(string text)
    {
        Assert.Throws<StepSemanticValueException>(() => Reader().Read(Record(text)));
    }

    [Fact]
    public void Constructor_CopiesItsCollections()
    {
        string[] points = ["#1", "#2", "#3"];
        long[] multiplicities = [3, 3];
        double[] knots = [-2, 5];
        var curve = new StepBSplineCurveWithKnotsEntity("#20", "", 2, points, "UNSPECIFIED",
            StepLogicalValue.False, StepLogicalValue.Unknown, multiplicities, knots, "UNSPECIFIED");
        points[0] = "#999"; multiplicities[0] = 99; knots[0] = 99;
        Assert.Equal("#1", curve.ControlPointReferences[0]);
        Assert.Equal(3, curve.Multiplicities[0]);
        Assert.Equal(-2, curve.Knots[0]);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)curve.ControlPointReferences)[0] = "#7");
    }

    private const string Valid = "'',2,(#1,#2,#3),.UNSPECIFIED.,.F.,.U.,(3,3),(-2.,5.),.UNSPECIFIED.";
    private static StepSemanticEntityReader Reader() => new(new StepParameterParser(), [new StepBSplineCurveWithKnotsEntityHandler()]);
    private static StepDataEntityRecord Record(string parameters) =>
        new("#20", "B_SPLINE_CURVE_WITH_KNOTS", $"#20=B_SPLINE_CURVE_WITH_KNOTS({parameters});", CadReadStatus.Complete);
}
