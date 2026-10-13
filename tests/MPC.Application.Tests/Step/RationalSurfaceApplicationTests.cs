using MPC.Application.Step;

namespace MPC.Application.Tests.Step;

public sealed class RationalSurfaceApplicationTests
{
    [Fact]
    public void ValidSurface_ReachesBoundaryThroughProductionCatalog()
    {
        StepImportResult result = Execute(Surface);
        Assert.Equal(StepImportStatus.Failed, result.Status);
        Assert.Contains(result.Diagnostics, d =>
            d.Code == "STEP_REFERENCE_RESOLUTION_FAILED" &&
            d.Message.Contains("#90", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Diagnostics, d =>
            d.Message.Contains("#75", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Diagnostics, d =>
            d.Code == StepImportApplicationService.UnexpectedFailureCode);
    }

    [Theory]
    [InlineData("RATIONAL_B_SPLINE_SURFACE(((1.,2.),(3.,4.)))",
                "RATIONAL_B_SPLINE_SURFACE(((1.,0.),(3.,4.)))")]
    [InlineData("BOUNDED_SURFACE()", "")]
    public void InvalidSurface_IsAnEntityScopedSemanticFailure(string oldText, string newText)
    {
        StepImportResult first = Execute(Surface.Replace(oldText, newText, StringComparison.Ordinal));
        StepImportResult second = Execute(Surface.Replace(oldText, newText, StringComparison.Ordinal));
        var diagnostic = Assert.Single(first.Diagnostics);
        Assert.Equal(StepImportApplicationService.SemanticFailureCode, diagnostic.Code);
        Assert.Equal("#75", diagnostic.SourceEntityId);
        Assert.Contains("COMPLEX_ENTITY", diagnostic.Message);
        Assert.Equal(first.Diagnostics.Select(d => d.Message), second.Diagnostics.Select(d => d.Message));
        Assert.Null(first.Summary);
    }

    private const string Surface = """
        #75=(BOUNDED_SURFACE()
        B_SPLINE_SURFACE(1,1,((#1,#2),(#3,#4)),.UNSPECIFIED.,.F.,.F.,.U.)
        B_SPLINE_SURFACE_WITH_KNOTS((2,2),(2,2),(-2.,5.),(-3.,7.),.UNSPECIFIED.)
        GEOMETRIC_REPRESENTATION_ITEM()
        RATIONAL_B_SPLINE_SURFACE(((1.,2.),(3.,4.)))
        REPRESENTATION_ITEM('rational')
        SURFACE());
        """;

    private static StepImportResult Execute(string surface)
    {
        string path = Path.Combine(Path.GetTempPath(), $"MPC.B16.{Guid.NewGuid():N}.step");
        try
        {
            File.WriteAllText(path, """
                ISO-10303-21;
                HEADER;
                ENDSEC;
                DATA;
                #1=CARTESIAN_POINT('',(0.,0.,0.));
                #2=CARTESIAN_POINT('',(0.,1.,0.));
                #3=CARTESIAN_POINT('',(1.,0.,0.));
                #4=CARTESIAN_POINT('',(1.,1.,0.));
                """ + "\n" + surface + """
                
                #80=ADVANCED_FACE('',(#90),#75,.T.);
                #100=CLOSED_SHELL('',(#80));
                #110=MANIFOLD_SOLID_BREP('',#100);
                ENDSEC;
                END-ISO-10303-21;
                """);
            return new StepImportApplicationService().Execute(new StepImportRequest(path));
        }
        finally { File.Delete(path); }
    }
}
