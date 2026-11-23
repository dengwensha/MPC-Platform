using System.Reflection;
using System.Security.Cryptography;
using MPC.Connectors.Diagnostics;
using MPC.Connectors.Results;
using MPC.Connectors.Step.Models;
using MPC.Connectors.Step.Readers;
using MPC.Connectors.Step.Validation;

namespace MPC.Application.Inventory;

internal enum InventoryReadStatus { Unknown, Succeeded, SucceededWithDiagnostics, Failed }
internal enum InventorySeverity { Unknown, Info, Warning, Error, Fatal }

internal sealed class InventoryDiagnostic
{
    internal string Code { get; }
    internal InventorySeverity Severity { get; }
    internal int RawSeverity { get; }
    internal string? SourceEntityId { get; }
    internal int OriginalIndex { get; }
    internal string Message { get; }
    internal string? ExceptionType { get; }
    internal string? SourceEntityType { get; }
    internal InventoryDiagnostic(string code, InventorySeverity severity, int rawSeverity,
        string? sourceEntityId, int originalIndex, string message, string? exceptionType, string? sourceEntityType)
    {
        Code = code; Severity = severity; RawSeverity = rawSeverity; SourceEntityId = sourceEntityId;
        OriginalIndex = originalIndex; Message = message; ExceptionType = exceptionType; SourceEntityType = sourceEntityType;
    }
}

internal sealed class ReaderRevision
{
    internal string Identity { get; }
    internal string Version { get; }
    internal string InformationalVersion { get; }
    internal string AssemblySha256 { get; }
    internal ReaderRevision(string identity, string version, string informationalVersion, string assemblySha256)
    {
        Identity = identity; Version = version; InformationalVersion = informationalVersion; AssemblySha256 = assemblySha256;
    }
    internal bool IsPresent => !string.IsNullOrWhiteSpace(Identity)
        && !string.IsNullOrWhiteSpace(Version) && !string.IsNullOrWhiteSpace(InformationalVersion)
        && InventoryPolicy.IsFingerprint(AssemblySha256);
}

// No metadata factory, serialization constructor or mutable connector payload.
internal sealed class InventoryEvidence
{
    internal const int CurrentSchemaRevision = 1;
    internal const int CurrentSeverityMappingRevision = 1;
    internal string DocumentSha256 { get; }
    internal InventoryReadStatus ReadStatus { get; }
    internal bool HasInventory { get; }
    internal IReadOnlyList<InventoryDiagnostic> Diagnostics { get; }
    internal ReaderRevision ReaderRevision { get; }
    internal int SchemaRevision => CurrentSchemaRevision;
    internal int SeverityMappingRevision => CurrentSeverityMappingRevision;
    private HeaderUnitData HeaderUnitData { get; }
    private EnvelopeData EnvelopeData { get; }
    // Same producer assembly identity; source/manifest pin for the envelope is independent.
    internal ReaderRevision EnvelopeProducerRevision => HeaderUnitProducerRevision;
    internal ReaderRevision HeaderUnitProducerRevision { get; }

    private InventoryEvidence(string hash, CadConnectorResult<StepDataInventory> result, ReaderRevision revision,
        byte[] capturedBytes, ReaderRevision headerUnitRevision)
    {
        HeaderUnitProducerRevision = headerUnitRevision;
        using var headerCapture = new MemoryStream(capturedBytes, writable: false);
        HeaderUnitData = HeaderUnitProducer.ReadCaptured(headerCapture, result.Value);
        using var envelopeCapture = new MemoryStream(capturedBytes, writable: false);
        EnvelopeData = EnvelopeProducer.ReadCaptured(envelopeCapture);
        DocumentSha256 = hash;
        ReadStatus = result.Status switch
        {
            CadConnectorResultStatus.Succeeded => InventoryReadStatus.Succeeded,
            CadConnectorResultStatus.SucceededWithDiagnostics => InventoryReadStatus.SucceededWithDiagnostics,
            CadConnectorResultStatus.Failed => InventoryReadStatus.Failed,
            _ => InventoryReadStatus.Unknown
        };
        HasInventory = result.Value is not null;
        Diagnostics = Array.AsReadOnly(result.Diagnostics.Select((d, i) => new InventoryDiagnostic(
            d.Code, InventoryPolicy.ProjectSeverity(d.Severity), (int)d.Severity,
            d.SourceEntityId, i, d.Message, d.ExceptionType, d.SourceEntityType)).ToArray());
        ReaderRevision = revision;
    }

    // Operation-scoped owner. Evidence deliberately has no back-reference to this
    // session or its potentially large inventory. No global cache or finalizer.
    internal sealed class ImportSession
    {
        private readonly StepDataInventory? _inventory;
        private HeaderUnitObservation HeaderUnits { get; }
        private EnvelopeObservation Envelope { get; }
        internal InventoryEvidence Evidence { get; }
        internal string DocumentSha256 { get; }
        private ImportSession(string hash, CadConnectorResult<StepDataInventory> result, ReaderRevision revision,
            byte[] capturedBytes, ReaderRevision headerUnitRevision)
        {
            _inventory = result.Value;
            DocumentSha256 = hash;
            Evidence = new InventoryEvidence(hash, result, revision, capturedBytes, headerUnitRevision);
            HeaderUnits = HeaderUnitObservation.EvidenceBinding.Bind(Evidence, this)!;
            Envelope = EnvelopeObservation.EvidenceBinding.Bind(Evidence, this)!;
        }
        // Bound header/unit evidence is exposed only through this owner check.
        // Equal hashes do not make another operation's evidence interchangeable.
        internal HeaderUnitObservation? ReadHeaderUnits(InventoryEvidence? evidence) =>
            evidence is not null && Owns(evidence) ? HeaderUnits : null;

        internal HeaderUnitData? ReadHeaderUnitData(InventoryEvidence? evidence) =>
            evidence is not null && Owns(evidence) ? evidence.HeaderUnitData : null;

        internal EnvelopeObservation? ReadEnvelope(InventoryEvidence? evidence) =>
            evidence is not null && Owns(evidence) ? Envelope : null;

        internal EnvelopeData? ReadEnvelopeData(InventoryEvidence? evidence) =>
            evidence is not null && Owns(evidence) ? evidence.EnvelopeData : null;

        internal bool Owns(InventoryEvidence evidence) => ReferenceEquals(Evidence, evidence);
        internal int RetainedEntityCount => _inventory?.TotalEntityCount ?? 0;

        internal static InventoryCaptureResult CaptureFromFile(string path, Action? afterSourceCaptured = null)
        {
            byte[] bytes;
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(path);
                string extension = Path.GetExtension(path);
                if (!extension.Equals(".step", StringComparison.OrdinalIgnoreCase)
                    && !extension.Equals(".stp", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("A STEP/STP source is required.", nameof(path));
                bytes = File.ReadAllBytes(path);
            }
            catch (Exception ex) when (IsExpectedFileFailure(ex) || ex is ArgumentException || ex is NotSupportedException)
            {
                return new InventoryCaptureResult(null, ex, InventoryCaptureFailureStage.SourceCapture);
            }

            // Payload-free observation/fault checkpoint. No bytes, stream, hash,
            // reader or result can be supplied by this callback. It runs outside
            // expected-IO catches so injected unexpected failures propagate through
            // the actual entry point, never becoming input/provenance failures.
            afterSourceCaptured?.Invoke();

            ReaderRevision revision;
            ReaderRevision headerUnitRevision;
            try
            {
                revision = ReadRevision(typeof(StepDataEntityReader).Assembly);
                headerUnitRevision = ReadRevision(typeof(HeaderUnitProducer).Assembly);
            }
            catch (Exception ex) when (IsExpectedFileFailure(ex) || ex is NotSupportedException)
            {
                return new InventoryCaptureResult(null, ex, InventoryCaptureFailureStage.ReaderProvenance);
            }

            string hash = Convert.ToHexString(SHA256.HashData(bytes));
            // Private bytes are shared with no caller; a non-writable stream over
            // exactly this array is read by the concrete producer. No temp STEP,
            // reopen window, cleanup, source-path diagnostic or success-reader seam.
            using var capture = new MemoryStream(bytes, writable: false);
            CadConnectorResult<StepDataInventory> result;
            try
            {
                result = new StepDataEntityReader(new StepDataInventoryValidator()).ReadCaptured(capture);
            }
            catch (FormatException ex)
            {
                return new InventoryCaptureResult(null, ex, InventoryCaptureFailureStage.InventoryParse);
            }
            return new InventoryCaptureResult(new ImportSession(hash, result, revision, bytes, headerUnitRevision), null, null);
        }

        private static ReaderRevision ReadRevision(Assembly assembly)
        {
            string location = assembly.Location;
            ReaderProvenance.RequireAssemblyLocation(location);
            return new ReaderRevision(assembly.FullName ?? "", assembly.GetName().Version?.ToString() ?? "",
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "",
                Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(location))));
        }
        private static bool IsExpectedFileFailure(Exception ex) => ex is IOException or UnauthorizedAccessException
            or System.Security.SecurityException;
    }
}

internal enum InventoryCaptureFailureStage { SourceCapture, ReaderProvenance, InventoryParse }

internal sealed class InventoryCaptureResult
{
    internal InventoryEvidence.ImportSession? Session { get; }
    internal Exception? Failure { get; }
    internal InventoryCaptureFailureStage? FailureStage { get; }
    internal InventoryCaptureResult(InventoryEvidence.ImportSession? session, Exception? failure,
        InventoryCaptureFailureStage? failureStage)
    {
        Session = session; Failure = failure; FailureStage = failureStage;
    }
}

internal static class ReaderProvenance
{
    // First slice supports file-backed assemblies only. Single-file deployment is
    // explicitly unavailable, never a guessed identity or omitted assembly hash.
    internal static void RequireAssemblyLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new NotSupportedException("Inventory evidence requires a file-backed reader assembly; single-file packaging is unsupported.");
    }
}
