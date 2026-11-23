using System.Reflection;
using System.Text;
using MPC.Application.Inventory;

namespace MPC.Application.Tests.Inventory;

public sealed class EnvelopeObservationTests
{
    private static string Valid(string data = "#1=CARTESIAN_POINT('',(0.,0.,0.));") =>
        HeaderPolicyV2Tests.Header() + "DATA;" + data + "ENDSEC;END-ISO-10303-21;";
    private static EnvelopeData Read(string text)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        var data = EnvelopeProducer.ReadCaptured(stream);
        Assert.True(stream.CanRead);
        return data;
    }

    [Fact]
    public void SingleUnparameterizedDataHasStructuralProofOnly()
    {
        var data = Read(Valid());
        Assert.Empty(data.Reasons); Assert.Equal(1, data.DataSectionCount);
    }

    [Theory]
    [InlineData("DATA;#2=A();ENDSEC;END-ISO-10303-21;", "MultipleDataSections")]
    [InlineData("DATA('other',('X'));#2=A();ENDSEC;END-ISO-10303-21;", "ParameterizedDataSection")]
    [InlineData("ANCHOR;ENDSEC;END-ISO-10303-21;", "UnsupportedEnvelopeSection")]
    [InlineData("REFERENCE;ENDSEC;END-ISO-10303-21;", "UnsupportedEnvelopeSection")]
    [InlineData("END-ISO-10303-21;SIGNATURE;ENDSEC;", "UnsupportedEnvelopeSection")]
    [InlineData("END-ISO-10303-21;#2=A();", "TrailingEnvelopeContent")]
    [InlineData("END-ISO-10303-21;END-ISO-10303-21;", "TrailingEnvelopeContent")]
    [InlineData("", "EnvelopeMalformed")]
    [InlineData("END-ISO-10303-21", "EnvelopeMalformed")]
    [InlineData("END-ISO-10303-21;/* unfinished", "EnvelopeMalformed")]
    public void ExtraSectionsAndBrokenEndingsCannotProduceClearEnvelope(string tail, string reason)
    {
        string prefix = HeaderPolicyV2Tests.Header() + "DATA;#1=A();ENDSEC;";
        Assert.Contains(reason, Read(prefix + tail).Reasons);
    }

    [Theory]
    [InlineData("DATA();", "ParameterizedDataSection")]
    [InlineData("DATA /* separator */ ('named',('X'));", "ParameterizedDataSection")]
    [InlineData("ANCHOR;ENDSEC;DATA;", "UnsupportedEnvelopeSection")]
    [InlineData("REFERENCE;ENDSEC;DATA;", "UnsupportedEnvelopeSection")]
    public void ForbiddenSectionsBeforeDataAreNotHiddenByTheFirstInventory(string opening, string reason) =>
        Assert.Contains(reason, Read(HeaderPolicyV2Tests.Header() + opening + "#1=A();ENDSEC;END-ISO-10303-21;").Reasons);

    [Theory]
    [InlineData("ISO-10303-21;DATA;#1=A();ENDSEC;END-ISO-10303-21;")]
    [InlineData("ISO-10303-21;HEADER;FILE_SCHEMA(('X'));DATA;#1=A();ENDSEC;END-ISO-10303-21;")]
    [InlineData("ISO-10303-21;HEADER;ENDSEC;END-ISO-10303-21;")]
    [InlineData("HEADER;ENDSEC;DATA;ENDSEC;END-ISO-10303-21;")]
    [InlineData("ISO-10303-21;HEADER;ENDSEC;DATA;#1=A('unfinished")]
    public void InvalidStructuralOrderOrLexicalTailIsNotProof(string input) =>
        Assert.Contains("EnvelopeMalformed", Read(input).Reasons);

    [Fact]
    public void KeywordsInsideStringsCommentsAndBinaryAreNotSections()
    {
        string text = Valid("/* DATA; ENDSEC; */#1=A('DATA; ''ENDSEC;'' ANCHOR; REFERENCE; SIGNATURE;',\"0ABC\");")
            .Replace("FILE_DESCRIPTION(('test')", "FILE_DESCRIPTION(('DATA; ENDSEC; REFERENCE;')");
        var data = Read(text + " /* DATA; SIGNATURE; */ \n\t");
        Assert.Empty(data.Reasons); Assert.Equal(1, data.DataSectionCount);
    }

    [Fact]
    public void VeryLargeEntityDoesNotRequireAnEntitySizedStatementBuffer()
    {
        var data = Read(Valid("#1=A('" + new string('a', 250_000) + "DATA;ENDSEC;');"));
        Assert.Empty(data.Reasons); Assert.Equal(1, data.DataSectionCount);
    }

    [Fact]
    public void ReaderIoFailureIsNotInputSyntaxOrCleanProof() =>
        Assert.Throws<IOException>(() => EnvelopeProducer.Read(new ThrowingReader()));

    private sealed class ThrowingReader : TextReader
    {
        public override int Read() => throw new IOException("injected reader failure");
    }

    [Fact]
    public void OwnedCaptureAndForeignEqualHashSessionStaySeparate()
    {
        WithFile(Valid(), path =>
        {
            var first = Capture(path); var second = Capture(path);
            Assert.Equal(first.DocumentSha256, second.DocumentSha256);
            Assert.NotNull(first.ReadEnvelope(first.Evidence));
            Assert.Same(first.ReadEnvelope(first.Evidence), first.ReadEnvelope(first.Evidence));
            Assert.Null(first.ReadEnvelope(second.Evidence));
            Assert.Null(first.ReadEnvelopeData(second.Evidence));
            Assert.Null(EnvelopeObservation.EvidenceBinding.Bind(second.Evidence, first));
            Assert.Null(EnvelopeObservation.EvidenceBinding.Bind(null, first));
            Assert.Null(EnvelopeObservation.EvidenceBinding.Bind(first.Evidence, null));
            Assert.True(first.Evidence.EnvelopeProducerRevision.IsPresent);
            Assert.Equal(first.Evidence.HeaderUnitProducerRevision.AssemblySha256, first.Evidence.EnvelopeProducerRevision.AssemblySha256);
        });
    }

    [Fact]
    public void SourceReplacementDoesNotReplaceCapturedEnvelope()
    {
        string bad = Valid().Replace("END-ISO-10303-21;", "DATA;#2=A();ENDSEC;END-ISO-10303-21;");
        WithFile(bad, path =>
        {
            var result = InventoryEvidence.ImportSession.CaptureFromFile(path, () => File.WriteAllText(path, Valid()));
            Assert.Null(result.Failure);
            var session = Assert.IsType<InventoryEvidence.ImportSession>(result.Session);
            Assert.Contains("MultipleDataSections", session.ReadEnvelope(session.Evidence)!.Reasons);
            var clean = Capture(path);
            Assert.NotEqual(session.DocumentSha256, clean.DocumentSha256);
            Assert.Empty(clean.ReadEnvelope(clean.Evidence)!.Reasons);
            Assert.Null(session.ReadEnvelope(clean.Evidence));
            Assert.Null(session.ReadEnvelopeData(clean.Evidence));
            Assert.Null(EnvelopeObservation.EvidenceBinding.Bind(clean.Evidence, session));
        });
    }

    [Fact]
    public void BoundEnvelopeCannotBeMintedFromMetadataOrMutated()
    {
        const BindingFlags members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Assert.All(typeof(EnvelopeObservation).GetConstructors(members), c => Assert.True(c.IsPrivate));
        Assert.All(typeof(EnvelopeObservation).GetProperties(members), p => Assert.Null(p.SetMethod));
        Assert.True(typeof(InventoryEvidence).GetProperty("EnvelopeData", members)!.GetMethod!.IsPrivate);
        Assert.All(typeof(EnvelopeObservation).GetFields(members), field =>
        {
            Assert.True(field.IsInitOnly);
            Assert.NotEqual(typeof(InventoryEvidence), field.FieldType);
            Assert.NotEqual(typeof(InventoryEvidence.ImportSession), field.FieldType);
            Assert.NotEqual(typeof(byte[]), field.FieldType);
        });
        WithFile(Valid(), path =>
        {
            var session = Capture(path);
            Assert.Throws<NotSupportedException>(() => ((IList<string>)session.ReadEnvelope(session.Evidence)!.Reasons).Add("fake"));
            var fake = new EnvelopeData(1, []);
            Assert.Empty(fake.Reasons);
            Assert.Null(EnvelopeObservation.EvidenceBinding.Bind(null, session));
        });
    }

    [Theory]
    [InlineData("utf-8")][InlineData("utf-16")][InlineData("utf-16BE")][InlineData("utf-32")]
    public void EnvelopeUsesTheSameBomDecodingAsHeaderAndInventory(string encodingName)
    {
        var encoding = Encoding.GetEncoding(encodingName);
        WithFile(Valid(), path =>
        {
            File.WriteAllBytes(path, encoding.GetPreamble().Concat(encoding.GetBytes(Valid())).ToArray());
            var session = Capture(path);
            Assert.Empty(session.ReadEnvelope(session.Evidence)!.Reasons);
            Assert.Empty(session.ReadHeaderUnits(session.Evidence)!.HeaderReasons);
        });
    }

    [Fact]
    public void InvalidUtf8FallbackDoesNotChangeStructuralBoundaries()
    {
        byte[] bytes = Encoding.UTF8.GetBytes(Valid());
        int location = Encoding.UTF8.GetString(bytes).IndexOf("test", StringComparison.Ordinal);
        bytes[location] = 255;
        WithFile(Valid(), path =>
        {
            File.WriteAllBytes(path, bytes);
            var session = Capture(path);
            Assert.Empty(session.ReadEnvelope(session.Evidence)!.Reasons);
            Assert.Empty(session.ReadHeaderUnits(session.Evidence)!.HeaderReasons);
        });
    }

    [Fact]
    public void EnvelopeRevisionReasonsSeverityAndIssuerSignaturesArePinned()
    {
        Assert.Equal(1, EnvelopeObservation.CurrentPolicyRevision);
        Assert.Equal("Error", EnvelopeProducer.ReasonSeverity);
        Assert.Equal(new[] { "EnvelopeMalformed", "MultipleDataSections", "ParameterizedDataSection",
            "UnsupportedEnvelopeSection", "TrailingEnvelopeContent" }, EnvelopeProducer.ReasonCodes);
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        foreach (var type in typeof(InventoryEvidence).Assembly.GetTypes().Where(t => t.Namespace == "MPC.Application.Inventory"))
        foreach (var method in type.GetMethods(flags))
        {
            if (method.ReturnType != typeof(EnvelopeObservation)) continue;
            if (type == typeof(InventoryEvidence.ImportSession))
            {
                if (method.Name == "get_Envelope") Assert.True(method.IsPrivate);
                else
                {
                    Assert.Equal("ReadEnvelope", method.Name);
                    Assert.Equal(new[] { typeof(InventoryEvidence) }, method.GetParameters().Select(p => p.ParameterType));
                }
            }
            else
            {
                Assert.Equal(typeof(EnvelopeObservation.EvidenceBinding), type);
                Assert.Equal("Bind", method.Name);
                Assert.Equal(new[] { typeof(InventoryEvidence), typeof(InventoryEvidence.ImportSession) }, method.GetParameters().Select(p => p.ParameterType));
            }
        }
    }

    private static InventoryEvidence.ImportSession Capture(string path)
    {
        var result = InventoryEvidence.ImportSession.CaptureFromFile(path);
        Assert.Null(result.Failure);
        return Assert.IsType<InventoryEvidence.ImportSession>(result.Session);
    }
    private static void WithFile(string text, Action<string> action)
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".step");
        try { File.WriteAllText(path, text); action(path); }
        finally { File.Delete(path); }
    }
}
