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

    private InventoryEvidence(string hash, CadConnectorResult<StepDataInventory> result, ReaderRevision revision)
    {
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
        internal InventoryEvidence Evidence { get; }
        internal string DocumentSha256 { get; }
        private ImportSession(string hash, CadConnectorResult<StepDataInventory> result, ReaderRevision revision)
        {
            _inventory = result.Value;
            DocumentSha256 = hash;
            Evidence = new InventoryEvidence(hash, result, revision);
        }
        internal bool Owns(InventoryEvidence evidence) => ReferenceEquals(Evidence, evidence);
        internal int RetainedEntityCount => _inventory?.TotalEntityCount ?? 0;

        internal static InventoryCaptureResult CaptureFromFile(string path)
        {
            byte[] bytes;
            ReaderRevision revision;
            try
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(path);
                string extension = Path.GetExtension(path);
                if (!extension.Equals(".step", StringComparison.OrdinalIgnoreCase)
                    && !extension.Equals(".stp", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("A STEP/STP source is required.", nameof(path));
                bytes = File.ReadAllBytes(path);
                revision = ReadReaderRevision();
            }
            catch (Exception ex) when (IsExpectedFileFailure(ex) || ex is ArgumentException || ex is NotSupportedException)
            {
                return new InventoryCaptureResult(null, ex, null);
            }

            string hash = Convert.ToHexString(SHA256.HashData(bytes));
            string snapshot = Path.Combine(Path.GetTempPath(), $"mpc-inventory-{Guid.NewGuid():N}.step");
            ImportSession? session = null;
            Exception? failure = null;
            bool ownsSnapshot = false;
            var observation = SnapshotCleanup.Observe(() =>
            {
                // CreateNew never overwrites a pre-existing snapshot.
                try
                {
                    using var stream = new FileStream(snapshot, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                    ownsSnapshot = true;
                    stream.Write(bytes);
                }
                catch (Exception ex) when (IsExpectedFileFailure(ex))
                {
                    failure = ex;
                }
                if (failure is null)
                {
                    CadConnectorResult<StepDataInventory>? result = null;
                    try
                    {
                        result = new StepDataEntityReader(new StepDataInventoryValidator()).Read(snapshot);
                    }
                    catch (FormatException ex) { failure = ex; }
                    if (result is not null) session = new ImportSession(hash, result, revision);
                }
                return (session, failure);
            }, () => { if (ownsSnapshot) File.Delete(snapshot); });
            return new InventoryCaptureResult(observation.Value.session, observation.Value.failure, observation.CleanupFailure);
        }

        private static ReaderRevision ReadReaderRevision()
        {
            Assembly assembly = typeof(StepDataEntityReader).Assembly;
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

internal sealed class InventoryCaptureResult
{
    internal InventoryEvidence.ImportSession? Session { get; }
    internal Exception? Failure { get; }
    internal Exception? CleanupFailure { get; }
    internal InventoryCaptureResult(InventoryEvidence.ImportSession? session, Exception? failure, Exception? cleanupFailure)
    {
        Session = session; Failure = failure; CleanupFailure = cleanupFailure;
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

internal static class SnapshotCleanup
{
    // Two immediate attempts, no background service/unbounded retry. On persistent
    // failure the capture retains secondary evidence; OS temp housekeeping owns
    // the residual file. The private path is never a session/evidence API field.
    internal static (T Value, Exception? CleanupFailure) Observe<T>(Func<T> observe, Action delete)
    {
        Exception? cleanup = null;
        T value;
        try { value = observe(); }
        finally { cleanup = Attempt(delete); }
        return (value, cleanup);
    }

    internal static Exception? Attempt(Action delete)
    {
        for (int attempt = 0; attempt < 2; attempt++)
        {
            try { delete(); return null; }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
            {
                if (attempt == 1) return ex;
            }
        }
        return null;
    }
}
