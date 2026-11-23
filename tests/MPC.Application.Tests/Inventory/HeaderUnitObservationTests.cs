using System.Text;
using MPC.Application.Inventory;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Validation;

namespace MPC.Application.Tests.Inventory;

public sealed class HeaderUnitObservationTests
{
    private const string Header = "ISO-10303-21;HEADER;FILE_DESCRIPTION(('test'),'1');FILE_NAME('a','',(''),(''),'','','');FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));ENDSEC;";
    private const string Units = "#1=(LENGTH_UNIT() NAMED_UNIT(*) SI_UNIT(.MILLI.,.METRE.));#2=(NAMED_UNIT(*) PLANE_ANGLE_UNIT() SI_UNIT($,.RADIAN.));";
    private static string Context(string id, string refs = "#1,#2") => id + "=(GEOMETRIC_REPRESENTATION_CONTEXT(3) GLOBAL_UNIT_ASSIGNED_CONTEXT((" + refs + ")) REPRESENTATION_CONTEXT('',''));";
    private static string File(string data, string? header = null) => (header ?? Header) + "DATA;" + data + "ENDSEC;END-ISO-10303-21;";
    private static HeaderUnitObservation Observe(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        using var inventoryStream = new MemoryStream(bytes);
        var inventory = new StepDataEntityReader(new StepDataInventoryValidator()).ReadCaptured(inventoryStream);
        using var headerStream = new MemoryStream(bytes);
        return HeaderUnitProducer.ReadCaptured(headerStream, inventory.Value);
    }
    [Theory]
    [InlineData("T001_PRISM")][InlineData("T002_SPHERE")][InlineData("T003_CYLINDER")]
    [InlineData("T101_BASIC_POCKET")][InlineData("T103_MULTIPLE_ISLANDS")]
    public void GoldenHeaderAndBothContextUnitsAreObserved(string name)
    {
        string path = Path.Combine(AppContext.BaseDirectory,"TestData","Step","Golden",name+".step");
        var capture = InventoryEvidence.ImportSession.CaptureFromFile(path);
        Assert.Null(capture.Failure);
        Assert.True(capture.Session!.Evidence.HeaderUnitProducerRevision.IsPresent);
        var o = capture.Session!.Evidence.HeaderUnits;
        var legacyHeader = new StepHeaderReader().Read(path).Value!;
        Assert.Equal(legacyHeader.Schemas.Single(),o.Schema);
        Assert.Equal(legacyHeader.ImplementationLevel,o.ImplementationLevel);
        Assert.Empty(o.HeaderReasons); Assert.Equal("AUTOMOTIVE_DESIGN",o.Schema);
        Assert.Equal(2,o.Contexts.Count); Assert.Empty(o.ContextSetReasons);
        Assert.All(o.Contexts,c => {Assert.Empty(c.Reasons); Assert.Contains(c.Units,u=>u.Dimension=="Length"&&u.Scale==new ExactUnitScale(1,1000));});
    }
    [Theory]
    [InlineData("2;1")][InlineData("3;1")]
    public void UnsupportedImplementationLevelIsDistinct(string level) =>
        Assert.Equal(new[]{"UnsupportedImplementationLevel"},Observe(File(Units+Context("#9"),Header.Replace("'1'","'"+level+"'"))).HeaderReasons);
    [Fact]
    public void RepositoryQuarterConeFixtureIsOutsideV1HeaderLevel()
    {
        string path=Path.Combine(AppContext.BaseDirectory,"TestData","Step","GE03B01-QuarterConePole.step");
        var o=InventoryEvidence.ImportSession.CaptureFromFile(path).Session!.Evidence.HeaderUnits;
        Assert.Equal("2;1",o.ImplementationLevel);Assert.Contains("UnsupportedImplementationLevel",o.HeaderReasons);
    }
    [Theory]
    [InlineData("FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));FILE_SCHEMA(('AUTOMOTIVE_DESIGN'))", "HeaderMalformed")]
    [InlineData("FILE_SCHEMA(($,'AUTOMOTIVE_DESIGN'))", "HeaderMalformed")]
    [InlineData("FILE_SCHEMA((''))", "HeaderMalformed")]
    [InlineData("FILE_SCHEMA(('AUTOMOTIVE_DESIGN','AUTOMOTIVE_DESIGN'))", "UnsupportedSchema")]
    [InlineData("FILE_SCHEMA(('AP242_MANAGED_MODEL_BASED_3D_ENGINEERING_MIM_LF'))", "UnsupportedSchema")]
    [InlineData("FILE_SCHEMA(('automotive_design'))", "UnsupportedSchemaForm")]
    [InlineData("FILE_SCHEMA(('AUTOMOTIVE_DESIGN{ 1 0 10303 214 }'))", "UnsupportedSchemaForm")]
    [InlineData("FILE_SCHEMA(('AUTOMOTIVE_DESIGN\\X2\\0041\\X0\\'))", "UnsupportedSchemaForm")]
    public void StrictHeaderRejectsRequiredCases(string replacement,string reason)
    {
        var o=Observe(File(Units+Context("#9"),Header.Replace("FILE_SCHEMA(('AUTOMOTIVE_DESIGN'))",replacement)));
        Assert.Contains(reason,o.HeaderReasons);
    }
    [Fact]
    public void NamesInStringsAndCommentsAreNotRecords()
    {
        string h=Header.Replace("'test'","'FILE_SCHEMA((''UNKNOWN'')); /* quoted */'")
            .Replace("HEADER;","HEADER;/* FILE_SCHEMA(('BAD')); */");
        Assert.Empty(Observe(File(Units+Context("#9"),h)).HeaderReasons);
        Assert.Contains("HeaderMalformed",Observe(File(Units+Context("#9"),h.Replace("FILE_SCHEMA(('AUTOMOTIVE_DESIGN'));",""))).HeaderReasons);
    }
    [Theory]
    [InlineData("FILE_DESCRIPTION(('test'),'1');")]
    [InlineData("FILE_NAME('a','',(''),(''),'','','');")]
    public void EveryRequiredRecordMustBeUnique(string record) =>
        Assert.Contains("HeaderMalformed",Observe(File(Units+Context("#9"),Header.Replace("HEADER;","HEADER;"+record))).HeaderReasons);
    [Theory]
    [InlineData("$",1L)][InlineData(".CENTI.",100L)][InlineData(".MILLI.",1000L)]
    public void ScaleDefinitionsAreExact(string prefix,long denominator)
    {
        var o=Observe(File(Units.Replace(".MILLI.",prefix)+Context("#9")));
        Assert.Equal(new ExactUnitScale(1,denominator),o.Contexts[0].Units.Single(u=>u.Dimension=="Length").Scale);
    }
    [Theory]
    [InlineData("LENGTH_UNIT", "INCH", "Length")]
    [InlineData("PLANE_ANGLE_UNIT", "DEGREE", "Angle")]
    public void ConversionUnitCannotBecomeItsReferencedSiBase(string dimension,string name,string expectedDimension)
    {
        string data=Units+"#3=("+dimension+"() NAMED_UNIT(*) CONVERSION_BASED_UNIT('"+name+"',#4));#4=MEASURE_WITH_UNIT(LENGTH_MEASURE(25.4),#1);"+Context("#9",expectedDimension=="Length"?"#3,#2":"#1,#3");
        var c=Observe(File(data)).Contexts[0];
        var conversion=Assert.Single(c.Units.Where(u=>u.Dimension==expectedDimension));
        Assert.Equal("BlockedConversionUnit",conversion.Code); Assert.Null(conversion.Scale);
        Assert.Contains("BlockedConversionUnit",c.Reasons);
    }
    [Fact]
    public void ContextsAreOrderedAndSameUnitsDoNotConflict()
    {
        var o=Observe(File(Units+Context("#9")+Context("#10")));
        Assert.Equal(new[]{"#10","#9"},o.Contexts.Select(c=>c.SourceId));
        Assert.All(o.Contexts,c=>Assert.Empty(c.Reasons));
    }
    [Fact]
    public void DistinctContextsRetainDistinctScales()
    {
        string data=Units+"#3=(LENGTH_UNIT() NAMED_UNIT(*) SI_UNIT($,.METRE.));"+Context("#9")+Context("#10","#3,#2");
        var o=Observe(File(data));Assert.Equal(new ExactUnitScale(1,1),o.Contexts[0].Units[0].Scale);
        Assert.Equal(new ExactUnitScale(1,1000),o.Contexts[1].Units[0].Scale);
    }
    [Theory]
    [InlineData("#1","MissingAngleUnit")]
    [InlineData("#2","MissingLengthUnit")]
    [InlineData("#1,#1,#2","AmbiguousLengthUnit")]
    [InlineData("#1,#2,#2","AmbiguousAngleUnit")]
    [InlineData("#1,#999","UnitReferenceInvalid")]
    public void MissingAndAmbiguousAssignmentsRemainVisible(string refs,string reason) =>
        Assert.Contains(reason,Observe(File(Units+Context("#9",refs))).Contexts[0].Reasons);
    [Fact]
    public void UnitlessAndMalformedContextsAreNotDropped()
    {
        var o=Observe(File(Units+"#9=(GEOMETRIC_REPRESENTATION_CONTEXT(3) REPRESENTATION_CONTEXT('',''));"));
        Assert.Single(o.Contexts);Assert.Contains("UnitAssignmentsMissing",o.Contexts[0].Reasons);
        Assert.Contains("MissingAngleUnit",o.Contexts[0].Reasons);
    }
    [Fact]
    public void UnsupportedPrefixDoesNotUseOtherGlobalSiRecord()
    {
        string data=Units.Replace(".MILLI.",".KILO.")+"#3=(LENGTH_UNIT() NAMED_UNIT(*) SI_UNIT(.MILLI.,.METRE.));"+Context("#9");
        Assert.Contains("UnsupportedUnit",Observe(File(data)).Contexts[0].Reasons);
    }
    [Theory]
    [InlineData(true)][InlineData(false)]
    public void ConeWithDegreeOrMissingAngleDoesNotGetRadianEvidence(bool degree)
    {
        string data=Units+"#3=(NAMED_UNIT(*) PLANE_ANGLE_UNIT() CONVERSION_BASED_UNIT('DEGREE',#4));#4=MEASURE_WITH_UNIT(PLANE_ANGLE_MEASURE(0.0174532925199433),#2);#5=CONICAL_SURFACE('',#6,1.,45.);#6=AXIS2_PLACEMENT_3D('',#7,$,$);#7=CARTESIAN_POINT('',(0.,0.,0.));"+Context("#9",degree?"#1,#3":"#1");
        Assert.Contains(degree?"BlockedConversionUnit":"MissingAngleUnit",Observe(File(data)).Contexts[0].Reasons);
    }
    [Theory]
    [InlineData("utf-8")][InlineData("utf-16")][InlineData("utf-16BE")][InlineData("utf-32")]
    public void HeaderAndInventoryUseSameBomDecoding(string encodingName)
    {
        var encoding=Encoding.GetEncoding(encodingName);byte[] bytes=encoding.GetPreamble().Concat(encoding.GetBytes(File(Units+Context("#9")))).ToArray();
        string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".step");
        try {
            System.IO.File.WriteAllBytes(path,bytes);
            var session=InventoryEvidence.ImportSession.CaptureFromFile(path).Session!;
            Assert.Empty(session.Evidence.HeaderUnits.HeaderReasons);
            Assert.Empty(session.Evidence.HeaderUnits.Contexts[0].Reasons);
            Assert.Equal(3,session.RetainedEntityCount);
        } finally {System.IO.File.Delete(path);}
    }
    [Fact]
    public void CapturedSourceReplacementCannotReplaceHeaderOrUnits()
    {
        string path=Path.Combine(Path.GetTempPath(),Guid.NewGuid()+".step");
        try {
            System.IO.File.WriteAllText(path,File(Units+Context("#9"),Header.Replace("AUTOMOTIVE_DESIGN","UNKNOWN")));
            var result=InventoryEvidence.ImportSession.CaptureFromFile(path,()=>System.IO.File.WriteAllText(path,File(Units+Context("#9"))));
            Assert.Contains("UnsupportedSchema",result.Session!.Evidence.HeaderUnits.HeaderReasons);
        } finally {System.IO.File.Delete(path);}
    }
    [Fact]
    public void InvalidUtf8FallbackAndMissingInventoryStayExplicit()
    {
        byte[] bytes=Encoding.UTF8.GetBytes(Header.Replace("test","t")).ToArray();
        int location=Array.IndexOf(bytes,(byte)'t');bytes[location]=255;
        using var stream=new MemoryStream(bytes);
        var o=HeaderUnitProducer.ReadCaptured(stream,null);
        Assert.Empty(o.HeaderReasons); Assert.Contains("InventoryUnavailable",o.ContextSetReasons);
        Assert.True(stream.CanRead);
    }
    [Fact]
    public void NoGeometricContextAndCyclicAssignedUnitDoNotLookComplete()
    {
        Assert.Contains("NoGeometricContext",Observe(File(Units)).ContextSetReasons);
        string data=Units+"#3=(LENGTH_UNIT() NAMED_UNIT(*) CONVERSION_BASED_UNIT('INCH',#3));"+Context("#9","#3,#2");
        Assert.Contains("BlockedConversionUnit",Observe(File(data)).Contexts[0].Reasons);
    }
}
