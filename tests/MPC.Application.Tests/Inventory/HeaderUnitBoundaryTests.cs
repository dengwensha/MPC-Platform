using System.Text;
using MPC.Application.Inventory;

namespace MPC.Application.Tests.Inventory;

public sealed class HeaderUnitBoundaryTests
{
    private const string Header="ISO-10303-21;HEADER;FILE_DESCRIPTION(('ENDSEC; FILE_SCHEMA'),'1');FILE_NAME('a','',(''),(''),'','','');FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));/* ENDSEC; */ENDSEC;";

    [Theory]
    [InlineData("DATA;#1=(;ENDSEC;END-ISO-10303-21;")]
    [InlineData("DATA;#1=A('unterminated")]
    [InlineData("DATA;/*unterminated")]
    public void DataSyntaxCannotBeAttributedToHeader(string tail)
    {
        using var stream=new MemoryStream(Encoding.UTF8.GetBytes(Header+tail));
        var result=HeaderUnitProducer.ReadCaptured(stream,null);
        Assert.Empty(result.HeaderReasons);
        Assert.Contains("InventoryUnavailable",result.ContextSetReasons);
    }

    [Theory]
    [InlineData("#1=(")]
    [InlineData("#1=(;")]
    [InlineData("#1=()garbage")]
    public void IncompleteComplexBodyHasAControlledFormatFailure(string statement) =>
        Assert.Throws<FormatException>(()=>StepEvidenceSyntax.Components(statement));

    [Fact]
    public void HeaderScannerDoesNotRequestCharactersAfterItsEndsec()
    {
        using var reader=new HeaderOnlyReader(Header);
        Assert.Equal(6,StepEvidenceSyntax.HeaderStatements(reader).Count);
    }

    [Theory]
    [InlineData("ISO-10303-21;HEADER;FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));")]
    [InlineData("ISO-10303-21;HEADER;FILE_DESCRIPTION(('unterminated")]
    public void BrokenHeaderStillFails(string input)
    {
        using var stream=new MemoryStream(Encoding.UTF8.GetBytes(input));
        Assert.Contains("HeaderMalformed",HeaderUnitProducer.ReadCaptured(stream,null).HeaderReasons);
    }

    private sealed class HeaderOnlyReader(string text) : TextReader
    {
        private int position;
        public override int Read() => position<text.Length?text[position++]:throw new IOException("Read past HEADER ENDSEC.");
        public override int Peek() => position<text.Length?text[position]:throw new IOException("Peek past HEADER ENDSEC.");
    }

    [Fact]
    public void ActualCaptureRetainsHeaderObservationForMalformedDataRecord()
    {
        string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".step");
        try
        {
            File.WriteAllText(path,Header+"DATA;#3=(;#9=(GEOMETRIC_REPRESENTATION_CONTEXT(3) GLOBAL_UNIT_ASSIGNED_CONTEXT((#3)) REPRESENTATION_CONTEXT('',''));ENDSEC;END-ISO-10303-21;");
            var result=InventoryEvidence.ImportSession.CaptureFromFile(path);
            Assert.Null(result.Failure);Assert.NotNull(result.Session);
            Assert.Empty(result.Session.ReadHeaderUnits(result.Session.Evidence)!.HeaderReasons);
            Assert.NotEmpty(result.Session.Evidence.Diagnostics);
        }
        finally {File.Delete(path);}
    }
}
