using System.Text;
using System.Text.Json;
using MPC.Application.Manufacturing;
using MPC.Application.Step;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Results;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Validation;

namespace MPC.Application.Tests.Step;

public sealed class E1DataExtractionTests
{
    private const string Point1 = "#1=CARTESIAN_POINT('',(0.,0.,0.));";
    private const string Point2 = "#2=CARTESIAN_POINT('',(1.,0.,0.));";
    private const string Fake = "DATA;#99=CARTESIAN_POINT('',(9.,9.,9.));ENDSEC;";

    [Theory]
    [InlineData("comment")][InlineData("string")][InlineData("binary")]
    public void RealDataWinsOverDecoysBeforeTheActualSection(string kind)
    {
        string prefix = kind switch {
            "comment" => "HEADER;/* " + Fake + " */ENDSEC;",
            "string" => "HEADER;FILE_DESCRIPTION(('" + Fake.Replace("'", "''") + "'),'1');ENDSEC;",
            _ => "HEADER;OTHER(\"DATA;ENDSEC;\");ENDSEC;"
        };
        WithFile("ISO-10303-21;" + prefix + "DATA;" + Point1 + Point2 + "ENDSEC;END-ISO-10303-21;", path =>
        {
            var result = Read(path);
            Assert.Equal(CadConnectorResultStatus.Succeeded, result.Status);
            Assert.Equal(new[] { "#1", "#2" }, result.Value!.Entities.Select(e => e.SourceEntityId));
            Assert.Empty(result.Diagnostics);
        });
    }

    [Theory]
    [InlineData("#1=CARTESIAN_POINT('ENDSEC; DATA;', (0.,0.,0.));")]
    [InlineData("#1=A(\"0ENDSEC;DATA;\");")]
    [InlineData("#1=CARTESIAN_POINT('',(0.,0.,0.));/* ENDSEC; DATA; #99=A(); */")]
    [InlineData("#1=CARTESIAN_POINT('quote '' ENDSEC; DATA;', (0.,0.,0.));")]
    public void EndsecInsidePayloadDoesNotTruncateInventory(string first)
    {
        WithFile("DATA;" + first + Point2 + "ENDSEC;", path =>
        {
            var result = Read(path);
            Assert.Equal(CadConnectorResultStatus.Succeeded, result.Status);
            Assert.Equal(new[] { "#1", "#2" }, result.Value!.Entities.Select(e => e.SourceEntityId));
        });
    }

    [Theory]
    [InlineData("DATA;#1=A();ENDSEC;DATA;#2=A();ENDSEC;")]
    [InlineData("DATA();#1=A();ENDSEC;")]
    [InlineData("DATA /* gap */ ('named',('X'));#1=A();ENDSEC;")]
    [InlineData("DATA;#1=A();ENDSEC;SIGNATURE;ENDSEC;")]
    [InlineData("REFERENCE;ENDSEC;DATA;#1=A();ENDSEC;")]
    [InlineData("ANCHOR;ENDSEC;DATA;#1=A();ENDSEC;")]
    [InlineData("DATA;#1=A();ENDSEC;garbage;")]
    [InlineData("DATA;#1=A();ENDSEC;END-ISO-10303-21;;")]
    [InlineData("DATA;#1=A();ENDSEC;END-ISO-10303-21;#2=A();")]
    [InlineData("DATA;#1=A('unfinished;ENDSEC;")]
    [InlineData("DATA;#1=A();/* unfinished ENDSEC;")]
    [InlineData("DATA;#1=A();")]
    public void AmbiguousUnsupportedOrBrokenDataNeverLooksLikeFirstSectionSuccess(string text)
    {
        WithFile(text, path =>
        {
            var result = Read(path);
            Assert.Equal(CadConnectorResultStatus.Failed, result.Status);
            Assert.Null(result.Value);
            var reason = Assert.Single(result.Diagnostics);
            Assert.Equal("STEP_DATA_SECTION_NOT_FOUND", reason.Code);
            Assert.Equal(CadDiagnosticSeverity.Fatal, reason.Severity);
        });
    }

    [Theory]
    [InlineData("comment")][InlineData("string")][InlineData("body-comment")][InlineData("inline-comment")]
    public void CurrentLegacyAndRegionImportsUseTheActualGoldenGeometry(string kind)
    {
        string golden = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "Step", "Golden", "T001_PRISM.step"));
        string text = kind switch {
            "comment" => golden.Replace("HEADER;", "HEADER;/* " + Fake + " */"),
            "body-comment" => golden.Replace("DATA;", "DATA;/* ENDSEC; " + Fake + " */"),
            "inline-comment" => golden.Replace("#1 = VERTEX_POINT", "#1 = /* ENDSEC; DATA; */ VERTEX_POINT"),
            _ => golden.Replace("'STEP AP214'", "'STEP AP214 " + Fake.Replace("'", "''") + "'")
        };
        WithFile(text, path =>
        {
            var imported = new StepImportApplicationService().Execute(new StepImportRequest(path));
            Assert.Equal(StepImportStatus.Succeeded, imported.Status);
            Assert.Equal("#63", imported.Summary!.SourceSolidEntityId);
            Assert.True(ManufacturingRegionImportService.TryCreate(path, "#8", out var region));
            Assert.NotNull(region); Assert.Equal("#8", region.SourceFaceId);
        });
    }

    [Theory]
    [InlineData("data ;#1=A(); endsec ;")]
    [InlineData("DATA;#1=A();ENDSEC;")]
    public void ExistingDataFragmentAndKeywordCaseCompatibilityIsPreserved(string text)
    {
        WithFile(text, path => Assert.Equal(CadConnectorResultStatus.Succeeded, Read(path).Status));
    }

    [Fact]
    public void PartialEntityDiagnosticsRemainSeparateFromSectionBoundaryValidation()
    {
        WithFile("DATA;#1=(;ENDSEC;", path =>
        {
            var result = Read(path);
            Assert.Equal(CadConnectorResultStatus.SucceededWithDiagnostics, result.Status);
            Assert.Equal("STEP_COMPLEX_ENTITY_COMPONENTS_NOT_FOUND", Assert.Single(result.Diagnostics).Code);
        });
    }

    [Fact]
    public void CommentsCannotJoinIdentifierFragmentsIntoAnAcceptedKeyword()
    {
        WithFile("DA/* gap */TA;#1=A();ENDSEC;", path =>
            Assert.Equal(CadConnectorResultStatus.Failed, Read(path).Status));
    }

    [Fact]
    public void CommittedKnownFailureControlNowReadsBothRealRecords()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "E1Audit", "C01-CommentData.step");
        var result = Read(path);
        Assert.Equal(CadConnectorResultStatus.Succeeded, result.Status);
        Assert.Equal(new[] { "#1", "#2" }, result.Value!.Entities.Select(e => e.SourceEntityId));
        Assert.DoesNotContain(result.Value.Entities, e => e.SourceEntityId == "#99");
    }

    [Theory]
    [InlineData("multi")][InlineData("parameterized")]
    public void CurrentImportPathsRejectUnsupportedSectionLayouts(string kind)
    {
        string golden = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "Step", "Golden", "T001_PRISM.step"));
        string text = kind == "multi" ? golden.Replace("END-ISO-10303-21;", "DATA;#999=A();ENDSEC;END-ISO-10303-21;")
            : golden.Replace("DATA;", "DATA('named',('AUTOMOTIVE_DESIGN'));");
        WithFile(text, path =>
        {
            Assert.Equal(StepImportStatus.Failed, new StepImportApplicationService().Execute(new StepImportRequest(path)).Status);
            Assert.False(ManufacturingRegionImportService.TryCreate(path, "#8", out var region));
            Assert.Null(region);
        });
    }

    private static CadConnectorResult<MPC.Connectors.Step.Models.StepDataInventory> Read(string path)
    {
        var reader = new StepDataEntityReader(new StepDataInventoryValidator());
        var fromPath = reader.Read(path);
        using var stream = new MemoryStream(File.ReadAllBytes(path));
        var fromCapture = reader.ReadCaptured(stream);
        Assert.True(stream.CanRead);
        Assert.Equal(fromPath.Status, fromCapture.Status);
        Assert.Equal(JsonSerializer.Serialize(fromPath.Value), JsonSerializer.Serialize(fromCapture.Value));
        Assert.Equal(JsonSerializer.Serialize(fromPath.Diagnostics), JsonSerializer.Serialize(fromCapture.Diagnostics));
        return fromPath;
    }
    private static void WithFile(string text, Action<string> action)
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".step");
        try { File.WriteAllText(path, text, Encoding.UTF8); action(path); }
        finally { File.Delete(path); }
    }
}
