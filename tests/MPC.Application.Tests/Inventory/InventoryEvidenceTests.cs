using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using MPC.Application.Inventory;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Validation;
using Gate = MPC.Application.Inventory.InventoryDecision.InventoryEvidenceGate;

namespace MPC.Application.Tests.Inventory;

public sealed class InventoryEvidenceTests
{
    private static string Golden => Path.Combine(AppContext.BaseDirectory, "TestData", "Step", "Golden", "T001_PRISM.step");
    private static string Write(string text, string extension = ".step")
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
        File.WriteAllText(path, text);
        return path;
    }
    private static InventoryEvidence.ImportSession Capture(string text)
    {
        string path = Write(text);
        try
        {
            var result = InventoryEvidence.ImportSession.CaptureFromFile(path);
            Assert.Null(result.Failure);
            Assert.Null(result.FailureStage);
            return Assert.IsType<InventoryEvidence.ImportSession>(result.Session);
        }
        finally { File.Delete(path); }
    }
    private static InventoryPolicyInput Clean => new(true, true, true, new string('A', 64), new string('A', 64),
        new ReaderRevision("reader", "1", "1+source", new string('B', 64)), 1, 1,
        InventoryReadStatus.Succeeded, true, Array.Empty<InventoryDiagnostic>());
    private static InventoryDiagnostic Diagnostic(string code, InventorySeverity severity, int index = 0, string? id = null) =>
        new(code, severity, severity == InventorySeverity.Unknown ? 99 : (int)severity - 1, id, index, "trace", null, null);
    private static void Reason(InventoryPolicyInput input, InventoryDecisionStatus status, InventoryReasonCode reason)
    {
        var result = InventoryPolicy.Evaluate(input);
        Assert.Equal(status, result.Status);
        Assert.Equal(reason, result.Reasons[0].Reason);
    }

    [Fact]
    public void GoldenCapture_HasActualHashRevisionAndOwnedInventoryOnlyDecision()
    {
        var capture = InventoryEvidence.ImportSession.CaptureFromFile(Golden);
        Assert.Null(capture.Failure);
        var session = Assert.IsType<InventoryEvidence.ImportSession>(capture.Session);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Golden))), session.DocumentSha256);
        Assert.True(session.RetainedEntityCount > 0);
        Assert.True(session.Evidence.ReaderRevision.IsPresent);
        Assert.Equal(typeof(StepDataEntityReader).Assembly.FullName, session.Evidence.ReaderRevision.Identity);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(StepDataEntityReader).Assembly.Location))),
            session.Evidence.ReaderRevision.AssemblySha256);
        Assert.Equal(InventoryDecisionStatus.InventoryClear, Gate.Evaluate(session.Evidence, session).Status);
        Assert.Empty(session.Evidence.Diagnostics);
    }

    [Fact]
    public void SameBytesDifferentSession_AreForeignEvenWithIdenticalHashes()
    {
        var a = Capture("DATA; #1=UNKNOWN(); ENDSEC;");
        var b = Capture("DATA; #1=UNKNOWN(); ENDSEC;");
        Assert.Equal(a.DocumentSha256, b.DocumentSha256);
        Assert.False(a.Owns(b.Evidence));
        Assert.Equal(InventoryReasonCode.SnapshotOwnerMismatch, Assert.Single(Gate.Evaluate(b.Evidence, a).Reasons).Reason);
        Assert.Equal(InventoryDecisionStatus.InventoryClear, Gate.Evaluate(a.Evidence, a).Status);
    }

    [Fact]
    public void ErrorOnUnreferencedRecord_IsPreservedOnceAndBlocks()
    {
        var session = Capture("DATA; #1=UNKNOWN(); #999=(); ENDSEC;");
        var d = Assert.Single(session.Evidence.Diagnostics);
        Assert.Equal("STEP_COMPLEX_ENTITY_COMPONENTS_NOT_FOUND", d.Code);
        Assert.Equal(InventorySeverity.Error, d.Severity);
        Assert.Equal(2, d.RawSeverity);
        Assert.Equal("#999", d.SourceEntityId);
        Assert.Equal(InventoryReadStatus.SucceededWithDiagnostics, session.Evidence.ReadStatus);
        Assert.Equal(InventoryDecisionStatus.Blocked, Gate.Evaluate(session.Evidence, session).Status);
        Assert.Single(Gate.Evaluate(session.Evidence, session).Reasons);
        Assert.Throws<NotSupportedException>(() => ((IList)session.Evidence.Diagnostics).Clear());
    }

    [Fact]
    public void GoldenPlusUnreferencedError_BlocksWithoutLosingGoldenRecords()
    {
        string text = File.ReadAllText(Golden, Encoding.UTF8);
        int end = text.LastIndexOf("ENDSEC;", StringComparison.Ordinal);
        var original = InventoryEvidence.ImportSession.CaptureFromFile(Golden).Session!;
        var changed = Capture(text.Insert(end, "#999999=();\n"));
        Assert.Equal(original.RetainedEntityCount + 1, changed.RetainedEntityCount);
        Assert.NotEqual(original.DocumentSha256, changed.DocumentSha256);
        Assert.Equal("#999999", Assert.Single(changed.Evidence.Diagnostics).SourceEntityId);
        Assert.Equal(InventoryDecisionStatus.Blocked, Gate.Evaluate(changed.Evidence, changed).Status);
    }

    [Fact]
    public void ObservedFailedInventory_IsNotUnavailable()
    {
        var session = Capture("ISO-10303-21; END-ISO-10303-21;");
        Assert.False(session.Evidence.HasInventory);
        Assert.Equal(InventoryReadStatus.Failed, session.Evidence.ReadStatus);
        Assert.Equal(InventorySeverity.Fatal, Assert.Single(session.Evidence.Diagnostics).Severity);
        var decision = Gate.Evaluate(session.Evidence, session);
        Assert.Equal(new[] { InventoryReasonCode.InventoryReadFailed, InventoryReasonCode.InventoryDiagnostic },
            decision.Reasons.Select(r => r.Reason));
    }

    [Theory]
    [InlineData("DATA; #1=COMPLEX_ENTITY(); ENDSEC;", ".step", typeof(FormatException))]
    [InlineData("DATA; ENDSEC;", ".txt", typeof(ArgumentException))]
    public void UnavailableCapture_PreservesOriginalFailure(string text, string extension, Type expected)
    {
        string path = Write(text, extension);
        try
        {
            var capture = InventoryEvidence.ImportSession.CaptureFromFile(path);
            Assert.Null(capture.Session);
            Assert.IsType(expected, capture.Failure);
            Assert.Equal(InventoryDecisionStatus.NoEvidence, Gate.Evaluate(null, capture.Session).Status);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void MissingAndLockedSource_DoNotMintEvidence()
    {
        var missing = InventoryEvidence.ImportSession.CaptureFromFile(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".step"));
        Assert.Null(missing.Session);
        Assert.IsType<FileNotFoundException>(missing.Failure);
        string path = Write("DATA; ENDSEC;");
        try
        {
            using var locked = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            var capture = InventoryEvidence.ImportSession.CaptureFromFile(path);
            Assert.Null(capture.Session);
            Assert.IsAssignableFrom<IOException>(capture.Failure);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void OriginalReplacement_DoesNotChangeEvidenceOrDecision()
    {
        string path = Write("DATA; #1=UNKNOWN(); ENDSEC;");
        try
        {
            var session = InventoryEvidence.ImportSession.CaptureFromFile(path).Session!;
            string hash = session.DocumentSha256;
            File.WriteAllText(path, "DATA; #1=(); ENDSEC;");
            File.Delete(path);
            Assert.Equal(hash, session.Evidence.DocumentSha256);
            Assert.Empty(session.Evidence.Diagnostics);
            Assert.Equal(InventoryDecisionStatus.InventoryClear, Gate.Evaluate(session.Evidence, session).Status);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void NullContextAndEvidence_HaveDefinedPrecedence()
    {
        Assert.Equal(InventoryReasonCode.MissingSnapshotContext, Assert.Single(Gate.Evaluate(null, null).Reasons).Reason);
        var session = Capture("DATA; ENDSEC;");
        Assert.Equal(InventoryReasonCode.MissingInventoryEvidence, Assert.Single(Gate.Evaluate(null, session).Reasons).Reason);
    }

    [Fact]
    public void StructuralDefensiveStates_FailClosed()
    {
        Reason(Clean with { Owned = false, EvidenceHash = "invalid" }, InventoryDecisionStatus.Blocked, InventoryReasonCode.SnapshotOwnerMismatch);
        foreach (string hash in new[] { "", new string('a', 64), new string('G', 64), new string('A', 63) })
            Reason(Clean with { EvidenceHash = hash }, InventoryDecisionStatus.Blocked, InventoryReasonCode.InvalidFingerprint);
        Reason(Clean with { SessionHash = null }, InventoryDecisionStatus.Blocked, InventoryReasonCode.InvalidFingerprint);
        Reason(Clean with { EvidenceHash = new string('C', 64) }, InventoryDecisionStatus.Blocked, InventoryReasonCode.FingerprintMismatch);
        foreach (var input in new[] { Clean with { Revision = null }, Clean with { SchemaRevision = 2 },
                     Clean with { SeverityMappingRevision = 2 }, Clean with { Revision = new ReaderRevision("reader", "1", "", new string('B', 64)) } })
            Reason(input, InventoryDecisionStatus.Blocked, InventoryReasonCode.UnsupportedEvidenceRevision);
    }

    [Fact]
    public void ContradictoryStatuses_AreNotClean()
    {
        var error = Diagnostic("E", InventorySeverity.Error);
        var fatal = Diagnostic("F", InventorySeverity.Fatal);
        foreach (var input in new[] {
            Clean with { ReadStatus = InventoryReadStatus.Unknown }, Clean with { ReadStatus = (InventoryReadStatus)99 },
            Clean with { HasInventory = false }, Clean with { Diagnostics = new[] { error } },
            Clean with { ReadStatus = InventoryReadStatus.SucceededWithDiagnostics },
            Clean with { ReadStatus = InventoryReadStatus.SucceededWithDiagnostics, Diagnostics = new[] { fatal } },
            Clean with { ReadStatus = InventoryReadStatus.Failed, HasInventory = false },
            Clean with { ReadStatus = InventoryReadStatus.Failed, Diagnostics = new[] { fatal } } })
            Reason(input, InventoryDecisionStatus.Blocked, InventoryReasonCode.InconsistentInventoryEvidence);
    }

    [Theory]
    [InlineData(CadDiagnosticSeverity.Info, 0)]
    [InlineData(CadDiagnosticSeverity.Warning, 1)]
    [InlineData(CadDiagnosticSeverity.Error, 2)]
    [InlineData(CadDiagnosticSeverity.Fatal, 3)]
    [InlineData((CadDiagnosticSeverity)99, -1)]
    public void SeverityProjection_IsExplicit(CadDiagnosticSeverity raw, int expected) =>
        Assert.Equal(expected, InventoryPolicy.ProjectSeverity(raw) == InventorySeverity.Unknown ? -1 : (int)InventoryPolicy.ProjectSeverity(raw) - 1);

    [Fact]
    public void InfoWarningDoNotBlock_UnknownAndErrorsDo_WithStableDuplicateOrdering()
    {
        var input = Clean with { ReadStatus = InventoryReadStatus.SucceededWithDiagnostics,
            Diagnostics = new[] { Diagnostic("W", InventorySeverity.Warning), Diagnostic("I", InventorySeverity.Info, 1) } };
        Assert.Equal(InventoryDecisionStatus.InventoryClear, InventoryPolicy.Evaluate(input).Status);
        input = input with { Diagnostics = new[] { Diagnostic("I", InventorySeverity.Error, 4, "#2"),
            Diagnostic("I", InventorySeverity.Error, 3, "#2"), Diagnostic("Z", InventorySeverity.Unknown, 0),
            Diagnostic("I", InventorySeverity.Error, 2, null) } };
        var before = InventoryPolicy.Evaluate(input);
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.Equal(before.Reasons, InventoryPolicy.Evaluate(input).Reasons);
        }
        finally { CultureInfo.CurrentCulture = previous; }
        Assert.Equal(new[] { 0, 2, 3, 4 }, before.Reasons.Select(r => r.OriginalIndex));
        Assert.Equal(InventoryReasonCode.UnknownDiagnosticSeverity, before.Reasons[0].Reason);
        Assert.Equal(99, before.Reasons[0].RawSeverity);
    }

    [Fact]
    public void TypedApi_HasNoMetadataIssuerOrEvidenceSessionBackreference()
    {
        foreach (Type type in new[] { typeof(InventoryEvidence), typeof(InventoryEvidence.ImportSession), typeof(InventoryDecision) })
        {
            Assert.True(type.IsSealed);
            Assert.All(type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), c => Assert.True(c.IsPrivate));
            Assert.All(type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), p => Assert.Null(p.SetMethod));
        }
        Assert.All(typeof(InventoryPolicy).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic),
            m => Assert.NotEqual(typeof(InventoryDecision), m.ReturnType));
        Assert.All(typeof(InventoryEvidence).GetFields(BindingFlags.Instance | BindingFlags.NonPublic), f =>
        {
            Assert.True(f.IsInitOnly);
            Assert.NotEqual(typeof(InventoryEvidence.ImportSession), f.FieldType);
            Assert.False(f.FieldType.Namespace?.StartsWith("MPC.Connectors", StringComparison.Ordinal) ?? false);
        });
        foreach (Type type in typeof(InventoryEvidence).Assembly.GetTypes().Where(t => t.Namespace == "MPC.Application.Inventory"))
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                if (method.ReturnType == typeof(InventoryDecision))
                {
                    Assert.Equal(typeof(Gate), type);
                    Assert.Equal("Evaluate", method.Name);
                    Assert.Equal(new[] { typeof(InventoryEvidence), typeof(InventoryEvidence.ImportSession) }, method.GetParameters().Select(p => p.ParameterType));
                }
        }
        Assert.DoesNotContain(typeof(InventoryEvidence).Assembly.GetExportedTypes(), t => t.Namespace == "MPC.Application.Inventory");
    }

    [Fact]
    public void EmptyReaderLocation_IsExplicitlyUnsupported() =>
        Assert.Throws<NotSupportedException>(() => ReaderProvenance.RequireAssemblyLocation(""));

    [Fact]
    public void CaptureCheckpoint_PropagatesUnexpectedExceptionWithoutIssuingEvidence()
    {
        string path = Write("DATA; #1=UNKNOWN(); ENDSEC;");
        var original = new InvalidOperationException("unexpected checkpoint failure");
        try
        {
            Assert.Same(original, Assert.Throws<InvalidOperationException>(() =>
                InventoryEvidence.ImportSession.CaptureFromFile(path, () => throw original)));
        }
        finally { File.Delete(path); }
    }
}
