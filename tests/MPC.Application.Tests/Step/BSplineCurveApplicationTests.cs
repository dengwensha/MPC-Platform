using MPC.Application.Step;

namespace MPC.Application.Tests.Step;

public sealed class BSplineCurveApplicationTests
{
    [Fact]
    public void ValidCurve_ReachesNextBoundaryReference()
    {
        var result = Execute(Curve);
        Assert.Equal(StepImportStatus.Failed, result.Status);
        Assert.Contains(result.Diagnostics, d => d.Code == "STEP_REFERENCE_RESOLUTION_FAILED" &&
            d.Message.Contains("#999", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Diagnostics, d => d.Message.Contains("'#20'", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Diagnostics, d => d.Code == StepImportApplicationService.UnexpectedFailureCode);
    }

    [Theory]
    [InlineData("('',2,", "('',0,")]
    [InlineData("(3,3)", "(2,2)")]
    public void InvalidCurve_IsAnEntityScopedSemanticFailure(string oldText, string newText)
    {
        var first = Execute(Curve.Replace(oldText, newText, StringComparison.Ordinal));
        var second = Execute(Curve.Replace(oldText, newText, StringComparison.Ordinal));
        var diagnostic = Assert.Single(first.Diagnostics);
        Assert.Equal(StepImportApplicationService.SemanticFailureCode, diagnostic.Code);
        Assert.Equal("#20", diagnostic.SourceEntityId);
        Assert.Equal(first.Diagnostics.Select(d => d.Message), second.Diagnostics.Select(d => d.Message));
        Assert.Null(first.Summary);
    }

    private const string Curve =
        "#20=B_SPLINE_CURVE_WITH_KNOTS('',2,(#1,#2,#3),.UNSPECIFIED.,.F.,.U.,(3,3),(-2.,5.),.UNSPECIFIED.);";

    private static StepImportResult Execute(string curve)
    {
        string path = Path.Combine(Path.GetTempPath(), $"MPC.B17.{Guid.NewGuid():N}.step");
        try
        {
            File.WriteAllText(path, """
                ISO-10303-21;
                HEADER;
                ENDSEC;
                DATA;
                #1=CARTESIAN_POINT('',(0.,0.,0.));
                #2=CARTESIAN_POINT('',(0.5,1.,0.));
                #3=CARTESIAN_POINT('',(1.,0.,0.));
                #10=VERTEX_POINT('',#1);
                #11=VERTEX_POINT('',#3);
                #21=EDGE_CURVE('',#10,#11,#20,.T.);
                #30=ORIENTED_EDGE('',*,*,#21,.T.);
                #40=EDGE_LOOP('',(#30,#999,#998));
                #41=FACE_OUTER_BOUND('',#40,.T.);
                #50=AXIS2_PLACEMENT_3D('',#1,#60,#61);
                #60=DIRECTION('',(0.,0.,1.));
                #61=DIRECTION('',(1.,0.,0.));
                #70=PLANE('',#50);
                #80=ADVANCED_FACE('',(#41),#70,.T.);
                #100=CLOSED_SHELL('',(#80));
                #110=MANIFOLD_SOLID_BREP('',#100);
                """ + "\n" + curve + "\nENDSEC;\nEND-ISO-10303-21;");
            return new StepImportApplicationService().Execute(new StepImportRequest(path));
        }
        finally { File.Delete(path); }
    }
}
