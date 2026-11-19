using System.Security.Cryptography;
using System.Text;
using MPC.Application.Inventory;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Validation;
using Gate = MPC.Application.Inventory.InventoryDecision.InventoryEvidenceGate;

namespace MPC.Application.Tests.Inventory;

public sealed class CapturedInventoryStreamTests
{
    [Theory]
    [InlineData("T001_PRISM")]
    [InlineData("T002_SPHERE")]
    [InlineData("T003_CYLINDER")]
    [InlineData("T101_BASIC_POCKET")]
    [InlineData("T103_MULTIPLE_ISLANDS")]
    public void AllGoldenFiles_PathAndCapturedBytesGiveIdenticalInventories(string name)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "TestData", "Step", "Golden", name + ".step");
        var reader = new StepDataEntityReader(new StepDataInventoryValidator());
        var fromPath = reader.Read(path);
        using var capture = new MemoryStream(File.ReadAllBytes(path), writable: false);
        var fromCapture = reader.ReadCaptured(capture);
        Assert.True(capture.CanRead); // Reader never owns/disposes the caller's stream.
        Assert.Equal(fromPath.Status, fromCapture.Status);
        Assert.Equal(fromPath.Value!.Entities.Select(e => (e.SourceEntityId, e.SourceEntityType, e.RawStatement, e.ReadStatus)),
            fromCapture.Value!.Entities.Select(e => (e.SourceEntityId, e.SourceEntityType, e.RawStatement, e.ReadStatus)));
        Assert.Equal(fromPath.Diagnostics.Select(d => (d.Code, d.Severity, d.SourceEntityId, d.Message)),
            fromCapture.Diagnostics.Select(d => (d.Code, d.Severity, d.SourceEntityId, d.Message)));
    }

    [Theory]
    [InlineData("utf-8")]
    [InlineData("utf-16")]
    [InlineData("utf-16BE")]
    [InlineData("utf-32")]
    public void BomDetectionMatchesExistingFileDecoding(string encodingName)
    {
        Encoding encoding = Encoding.GetEncoding(encodingName);
        byte[] bytes = encoding.GetPreamble().Concat(encoding.GetBytes("DATA; #1=A('İı é'); #2=(); ENDSEC;")).ToArray();
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".step");
        try
        {
            File.WriteAllBytes(path, bytes);
            var reader = new StepDataEntityReader(new StepDataInventoryValidator());
            using var capture = new MemoryStream(bytes, writable: false);
            var a = reader.Read(path);
            var b = reader.ReadCaptured(capture);
            Assert.Equal(a.Status, b.Status);
            Assert.Equal(a.Value!.Entities.Select(e => e.RawStatement), b.Value!.Entities.Select(e => e.RawStatement));
            Assert.Equal(a.Diagnostics.Select(d => (d.Code, d.Severity)), b.Diagnostics.Select(d => (d.Code, d.Severity)));
            Assert.Equal(CadDiagnosticSeverity.Error, Assert.Single(b.Diagnostics).Severity);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void InvalidUtf8FallbackMatchesExistingReader()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("DATA; #1=A('").Concat(new byte[] { 255 })
            .Concat(Encoding.UTF8.GetBytes("'); ENDSEC;")).ToArray();
        using var capture = new MemoryStream(bytes, writable: false);
        var result = new StepDataEntityReader(new StepDataInventoryValidator()).ReadCaptured(capture);
        Assert.Contains("\uFFFD", Assert.Single(result.Value!.Entities).RawStatement, StringComparison.Ordinal);
    }

    [Fact]
    public void StreamAccessFailureIsNotMisclassifiedAsSourceDiagnostic()
    {
        var original = new IOException("capture infrastructure failure");
        using var capture = new FailingReadStream(original);
        var reader = new StepDataEntityReader(new StepDataInventoryValidator());
        Assert.Same(original, Assert.Throws<IOException>(() => reader.ReadCaptured(capture)));
        Assert.True(capture.CanRead);
    }

    [Fact]
    public void ActualCaptureKeepsOriginalBytes_WhenSourceIsReplacedDeletedAndRecreated()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".step");
        byte[] original = Encoding.UTF8.GetBytes("DATA; #1=A(); ENDSEC;");
        try
        {
            File.WriteAllBytes(path, original);
            var result = InventoryEvidence.ImportSession.CaptureFromFile(path, () =>
            {
                File.Delete(path);
                File.WriteAllText(path, "DATA; #999=(); ENDSEC;");
            });
            var session = Assert.IsType<InventoryEvidence.ImportSession>(result.Session);
            Assert.Null(result.Failure);
            Assert.Equal(Convert.ToHexString(SHA256.HashData(original)), session.DocumentSha256);
            Assert.Equal(1, session.RetainedEntityCount);
            Assert.Empty(session.Evidence.Diagnostics);
            Assert.Equal(InventoryDecisionStatus.InventoryClear, Gate.Evaluate(session.Evidence, session).Status);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void CheckpointCannotSupplyCleanReaderResultOrBypassAnInputError()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".step");
        try
        {
            File.WriteAllText(path, "DATA; #999=(); ENDSEC;");
            var result = InventoryEvidence.ImportSession.CaptureFromFile(path, () => File.WriteAllText(path, "DATA; ENDSEC;"));
            var session = Assert.IsType<InventoryEvidence.ImportSession>(result.Session);
            var diagnostic = Assert.Single(session.Evidence.Diagnostics);
            Assert.Equal("#999", diagnostic.SourceEntityId);
            Assert.DoesNotContain(path, diagnostic.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("mpc-inventory-", diagnostic.Message, StringComparison.Ordinal);
            Assert.Equal(InventoryDecisionStatus.Blocked, Gate.Evaluate(session.Evidence, session).Status);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void SourceAndParseFailuresCarryDistinctStages()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".step");
        var missing = InventoryEvidence.ImportSession.CaptureFromFile(path);
        Assert.Equal(InventoryCaptureFailureStage.SourceCapture, missing.FailureStage);
        Assert.IsType<FileNotFoundException>(missing.Failure);
        try
        {
            File.WriteAllText(path, "DATA; #1=COMPLEX_ENTITY(); ENDSEC;");
            var parse = InventoryEvidence.ImportSession.CaptureFromFile(path);
            Assert.Equal(InventoryCaptureFailureStage.InventoryParse, parse.FailureStage);
            Assert.Null(parse.Session);
            Assert.IsType<FormatException>(parse.Failure);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void CheckpointIOExceptionPropagates_ItIsNotAnInputOrProvenanceFailure()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".step");
        var original = new IOException("unexpected observation failure");
        try
        {
            File.WriteAllText(path, "DATA; ENDSEC;");
            Assert.Same(original, Assert.Throws<IOException>(() =>
                InventoryEvidence.ImportSession.CaptureFromFile(path, () => throw original)));
        }
        finally { File.Delete(path); }
    }

    private sealed class FailingReadStream(Exception failure) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) => throw failure;
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
