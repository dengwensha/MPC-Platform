using MPC.Connectors.Diagnostics;

namespace MPC.Application.Inventory;

internal enum InventoryDecisionStatus { NoEvidence, Blocked, InventoryClear }
internal enum InventoryReasonCode
{
    MissingSnapshotContext, MissingInventoryEvidence, SnapshotOwnerMismatch,
    InvalidFingerprint, FingerprintMismatch, UnsupportedEvidenceRevision,
    InconsistentInventoryEvidence, InventoryReadFailed, UnknownDiagnosticSeverity, InventoryDiagnostic
}
internal sealed record InventoryReason(InventoryReasonCode Reason, string? DiagnosticCode = null,
    string? SourceEntityId = null, int RawSeverity = 0, int OriginalIndex = -1);

// Defensive test projection is data only. It can neither mint evidence nor issue
// the restricted InventoryDecision; there is no consumer of this neutral result.
internal sealed record InventoryPolicyInput(bool HasSession, bool HasEvidence, bool Owned,
    string? SessionHash, string? EvidenceHash, ReaderRevision? Revision, int SchemaRevision,
    int SeverityMappingRevision, InventoryReadStatus ReadStatus, bool HasInventory,
    IReadOnlyList<InventoryDiagnostic> Diagnostics);
internal sealed record InventoryPolicyResult(InventoryDecisionStatus Status, IReadOnlyList<InventoryReason> Reasons);

internal static class InventoryPolicy
{
    internal static InventorySeverity ProjectSeverity(CadDiagnosticSeverity severity) => severity switch
    {
        CadDiagnosticSeverity.Info => InventorySeverity.Info,
        CadDiagnosticSeverity.Warning => InventorySeverity.Warning,
        CadDiagnosticSeverity.Error => InventorySeverity.Error,
        CadDiagnosticSeverity.Fatal => InventorySeverity.Fatal,
        _ => InventorySeverity.Unknown
    };

    internal static bool IsFingerprint(string? hash) => hash is { Length: 64 }
        && hash.All(c => c is >= '0' and <= '9' or >= 'A' and <= 'F');

    internal static InventoryPolicyResult Evaluate(InventoryPolicyInput input)
    {
        static InventoryPolicyResult One(InventoryDecisionStatus status, InventoryReasonCode reason) =>
            new(status, Array.AsReadOnly(new[] { new InventoryReason(reason) }));
        if (!input.HasSession) return One(InventoryDecisionStatus.NoEvidence, InventoryReasonCode.MissingSnapshotContext);
        if (!input.HasEvidence) return One(InventoryDecisionStatus.NoEvidence, InventoryReasonCode.MissingInventoryEvidence);
        if (!input.Owned) return One(InventoryDecisionStatus.Blocked, InventoryReasonCode.SnapshotOwnerMismatch);
        if (!IsFingerprint(input.SessionHash) || !IsFingerprint(input.EvidenceHash))
            return One(InventoryDecisionStatus.Blocked, InventoryReasonCode.InvalidFingerprint);
        if (!string.Equals(input.SessionHash, input.EvidenceHash, StringComparison.Ordinal))
            return One(InventoryDecisionStatus.Blocked, InventoryReasonCode.FingerprintMismatch);
        if (input.Revision is not { IsPresent: true }
            || input.SchemaRevision != InventoryEvidence.CurrentSchemaRevision
            || input.SeverityMappingRevision != InventoryEvidence.CurrentSeverityMappingRevision)
            return One(InventoryDecisionStatus.Blocked, InventoryReasonCode.UnsupportedEvidenceRevision);
        bool fatal = input.Diagnostics.Any(d => d.Severity == InventorySeverity.Fatal);
        bool consistent = input.ReadStatus switch
        {
            InventoryReadStatus.Succeeded => input.HasInventory && input.Diagnostics.Count == 0,
            InventoryReadStatus.SucceededWithDiagnostics => input.HasInventory && input.Diagnostics.Count > 0 && !fatal,
            InventoryReadStatus.Failed => !input.HasInventory && fatal,
            _ => false
        };
        if (!consistent) return One(InventoryDecisionStatus.Blocked, InventoryReasonCode.InconsistentInventoryEvidence);
        var reasons = new List<InventoryReason>();
        if (input.ReadStatus == InventoryReadStatus.Failed)
            reasons.Add(new InventoryReason(InventoryReasonCode.InventoryReadFailed));
        foreach (InventoryDiagnostic diagnostic in input.Diagnostics)
        {
            if (diagnostic.Severity == InventorySeverity.Unknown || !Enum.IsDefined(diagnostic.Severity))
                reasons.Add(new InventoryReason(InventoryReasonCode.UnknownDiagnosticSeverity,
                    diagnostic.Code, diagnostic.SourceEntityId, diagnostic.RawSeverity, diagnostic.OriginalIndex));
            else if (diagnostic.Severity is InventorySeverity.Error or InventorySeverity.Fatal)
                reasons.Add(new InventoryReason(InventoryReasonCode.InventoryDiagnostic,
                    diagnostic.Code, diagnostic.SourceEntityId, diagnostic.RawSeverity, diagnostic.OriginalIndex));
        }
        var ordered = reasons.OrderBy(r => r.Reason)
            .ThenBy(r => r.DiagnosticCode, StringComparer.Ordinal)
            .ThenBy(r => r.SourceEntityId, StringComparer.Ordinal)
            .ThenBy(r => r.RawSeverity).ThenBy(r => r.OriginalIndex).ToArray();
        return new InventoryPolicyResult(ordered.Length == 0 ? InventoryDecisionStatus.InventoryClear : InventoryDecisionStatus.Blocked,
            Array.AsReadOnly(ordered));
    }
}

// The gate is nested in the decision so it alone can invoke the private issuer.
// The neutral policy core has no access to that constructor.
internal sealed class InventoryDecision
{
    internal InventoryDecisionStatus Status { get; }
    internal IReadOnlyList<InventoryReason> Reasons { get; }
    internal int PolicyRevision => 1;
    private InventoryDecision(InventoryPolicyResult result)
    {
        Status = result.Status;
        Reasons = Array.AsReadOnly(result.Reasons.ToArray());
    }

    internal static class InventoryEvidenceGate
    {
        internal static InventoryDecision Evaluate(InventoryEvidence? evidence, InventoryEvidence.ImportSession? expectedSession)
        {
            var input = new InventoryPolicyInput(expectedSession is not null, evidence is not null,
                evidence is not null && expectedSession is not null && expectedSession.Owns(evidence),
                expectedSession?.DocumentSha256, evidence?.DocumentSha256, evidence?.ReaderRevision,
                evidence?.SchemaRevision ?? 0, evidence?.SeverityMappingRevision ?? 0,
                evidence?.ReadStatus ?? InventoryReadStatus.Unknown, evidence?.HasInventory ?? false,
                evidence?.Diagnostics ?? Array.Empty<InventoryDiagnostic>());
            return new InventoryDecision(InventoryPolicy.Evaluate(input));
        }
    }
}
