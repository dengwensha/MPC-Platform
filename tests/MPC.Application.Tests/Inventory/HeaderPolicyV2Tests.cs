using System.Text;
using System.Reflection;
using MPC.Application.Inventory;
using Gate = MPC.Application.Inventory.InventoryDecision.InventoryEvidenceGate;

namespace MPC.Application.Tests.Inventory;

public sealed class HeaderPolicyV2Tests
{
    internal static string Header(string schema = "AUTOMOTIVE_DESIGN", string level = "2;1") =>
        "ISO-10303-21;HEADER;FILE_DESCRIPTION(('test'),'" + level + "');FILE_NAME('a','',(''),(''),'','','');FILE_SCHEMA(('" + schema + "'));ENDSEC;";

    [Theory]
    [InlineData("AUTOMOTIVE_DESIGN", "1")]
    [InlineData("AUTOMOTIVE_DESIGN", "2;1")]
    [InlineData("AUTOMOTIVE_DESIGN {1 0 10303 214 3 1 1}", "1")]
    [InlineData("AUTOMOTIVE_DESIGN {1 0 10303 214 3 1 1}", "2;1")]
    public void RecognizedCombinationsHaveNoHeaderReasons(string schema, string level)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Header(schema, level)));
        Assert.Empty(HeaderUnitProducer.ReadCaptured(stream, null).HeaderReasons);
    }

    [Theory]
    [InlineData("DATA;#1=CARTESIAN_POINT('',(0.,0.,0.));ENDSEC;DATA;#2=CARTESIAN_POINT('',(1.,0.,0.));ENDSEC;END-ISO-10303-21;")]
    [InlineData("DATA('named',('AUTOMOTIVE_DESIGN'));#1=CARTESIAN_POINT('',(0.,0.,0.));ENDSEC;END-ISO-10303-21;")]
    public void UnsupportedSectionsFailInventoryAndHaveIndependentEnvelopeRejection(string tail)
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".step");
        try
        {
            File.WriteAllText(path, Header() + tail);
            var result = InventoryEvidence.ImportSession.CaptureFromFile(path);
            Assert.Null(result.Failure);
            var session = Assert.IsType<InventoryEvidence.ImportSession>(result.Session);
            Assert.Equal(0, session.RetainedEntityCount);
            Assert.Equal(InventoryDecisionStatus.Blocked, Gate.Evaluate(session.Evidence, session).Status);
            Assert.False(session.Evidence.HasInventory);
            Assert.Equal("STEP_DATA_SECTION_NOT_FOUND", Assert.Single(session.Evidence.Diagnostics).Code);
            var entry = session.GetType().GetMethod("ReadEnvelope", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(entry);
            var observation = entry.Invoke(session, [session.Evidence]);
            Assert.NotNull(observation);
            var reasons = (IReadOnlyList<string>)observation.GetType().GetProperty("Reasons", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(observation)!;
            Assert.NotEmpty(reasons);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("AUTOMOTIVE_DESIGN { 1 0 10303 214 3 1 1 }")]
    [InlineData("AUTOMOTIVE_DESIGN   {1  0 10303 214 3 1 1}")]
    public void DeclaredTextIsPreservedSeparatelyFromRecognizedIdentity(string schema)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Header(schema, " 2;1 ")));
        var data = HeaderUnitProducer.ReadCaptured(stream, null);
        Assert.Empty(data.HeaderReasons);
        Assert.Equal(schema, data.Schema);
        Assert.Equal("'" + schema + "'", data.RawSchema);
        Assert.Equal("' 2;1 '", data.RawImplementationLevel);
        Assert.Equal("2;1", data.ImplementationLevel);
        Assert.Equal("AUTOMOTIVE_DESIGN", data.RecognizedSchema);
        Assert.Equal(new[] { "1", "0", "10303", "214", "3", "1", "1" }, data.SchemaOid);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)data.SchemaOid).Clear());
    }

    [Theory]
    [InlineData("AUTOMOTIVE_DESIGN {1 0 10303 214 1 1 1}", "UnsupportedSchema")]
    [InlineData("AUTOMOTIVE_DESIGN {1 0 10303 214 2 1 1}", "UnsupportedSchema")]
    [InlineData("AUTOMOTIVE_DESIGN {1 0 10303 214 9 1 1}", "UnsupportedSchema")]
    [InlineData("CONFIG_CONTROL_DESIGN", "UnsupportedSchema")]
    [InlineData("AP242_MANAGED_MODEL_BASED_3D_ENGINEERING_MIM_LF", "UnsupportedSchema")]
    [InlineData("OTHER {1 0 10303 214 3 1 1}", "UnsupportedSchema")]
    [InlineData("AUTOMOTIVE_DESIGN{1 0 10303 214 3 1 1}", "UnsupportedSchemaForm")]
    [InlineData("AUTOMOTIVE_DESIGN {1 0 10303 214 3 1}", "UnsupportedSchemaForm")]
    [InlineData("AUTOMOTIVE_DESIGN {1 0 10303 214 3 1 1 1}", "UnsupportedSchemaForm")]
    [InlineData("AUTOMOTIVE_DESIGN {01 0 10303 214 3 1 1}", "UnsupportedSchemaForm")]
    [InlineData("AUTOMOTIVE_DESIGN {+1 0 10303 214 3 1 1}", "UnsupportedSchemaForm")]
    [InlineData("AUTOMOTIVE_DESIGN {1,0,10303,214,3,1,1}", "UnsupportedSchemaForm")]
    [InlineData("AUTOMOTIVE_DESIGN\t{1 0 10303 214 3 1 1}", "UnsupportedSchemaForm")]
    [InlineData("AUTOMOTIVE_DESIGN {1\u00a00 10303 214 3 1 1}", "UnsupportedSchemaForm")]
    [InlineData("AUTOMOTIVE_DESIGN {1 0 10303 214 3 1 1} suffix", "UnsupportedSchemaForm")]
    [InlineData(" AUTOMOTIVE_DESIGN", "UnsupportedSchemaForm")]
    [InlineData("AUTOMOTIVE_DESIGN ", "UnsupportedSchemaForm")]
    [InlineData("automotive_design {1 0 10303 214 3 1 1}", "UnsupportedSchemaForm")]
    [InlineData("AUTOMOTIVE_DESIGN\\X2\\0041\\X0\\", "UnsupportedSchemaForm")]
    public void UnsupportedNamesAndFormsNeverBecomeBareAliases(string schema, string reason)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Header(schema)));
        var data = HeaderUnitProducer.ReadCaptured(stream, null);
        Assert.Equal(new[] { reason }, data.HeaderReasons);
        Assert.Null(data.RecognizedSchema);
        Assert.Equal(schema, data.Schema);
    }

    [Fact]
    public void ArbitrarilyLargeCanonicalOidArcDoesNotOverflowOrBecomeAllowed()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Header(
            "AUTOMOTIVE_DESIGN {1 0 " + new string('9', 5000) + " 214 3 1 1}")));
        var data = HeaderUnitProducer.ReadCaptured(stream, null);
        Assert.Equal(new[] { "UnsupportedSchema" }, data.HeaderReasons);
        Assert.Equal(5000, data.SchemaOid[2].Length);
        Assert.Null(data.RecognizedSchema);
    }

    [Theory]
    [InlineData("3;1")][InlineData("3;2")][InlineData("4;1")]
    [InlineData("2;2")][InlineData("2")][InlineData("2 ;1")]
    public void OtherImplementationLevelsRemainBlocked(string level)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Header(level: level)));
        Assert.Equal(new[] { "UnsupportedImplementationLevel" }, HeaderUnitProducer.ReadCaptured(stream, null).HeaderReasons);
    }

    [Theory]
    [InlineData("FILE_POPULATION();")][InlineData("SECTION_LANGUAGE();")][InlineData("SECTION_CONTEXT();")]
    public void ExtraHeaderRecordsDeliberatelyRemainMalformed(string extra)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Header().Replace("ENDSEC;", extra + "ENDSEC;")));
        Assert.Equal(new[] { "HeaderMalformed" }, HeaderUnitProducer.ReadCaptured(stream, null).HeaderReasons);
    }
}
