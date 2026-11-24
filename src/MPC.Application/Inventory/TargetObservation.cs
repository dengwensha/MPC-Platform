namespace MPC.Application.Inventory;

internal enum TargetStatus { NoEvidence, Blocked, TargetBoundOnly }
internal enum TargetLayerStatus { Observed, Blocked, Unavailable }

// Strings/collections are immutable projections, never mutable connector objects.
internal sealed class TargetReason
{
    internal string Layer { get; }
    internal string Code { get; }
    internal string SourceId { get; }
    internal string Component { get; }
    internal IReadOnlyList<int> Indices { get; }
    internal IReadOnlyList<string> RelatedIds { get; }
    internal string? OriginalMessage { get; }
    internal string? CauseType { get; }
    internal string? CauseMessage { get; }
    internal int Occurrence { get; }
    internal string Severity => "Error";
    internal TargetReason(string layer, string code, string source = "", string component = "",
        IEnumerable<int>? indices = null, IEnumerable<string>? related = null, int occurrence = 0,
        string? message = null, string? causeType = null, string? causeMessage = null)
    {
        Layer = layer; Code = code; SourceId = source; Component = component;
        Indices = Array.AsReadOnly((indices ?? []).ToArray());
        RelatedIds = Array.AsReadOnly((related ?? []).ToArray()); Occurrence = occurrence;
        OriginalMessage = message; CauseType = causeType; CauseMessage = causeMessage;
    }
}

internal sealed class TargetData
{
    internal string? RootId { get; }
    internal string? RepresentationId { get; }
    internal string? MainRepresentationId { get; }
    internal string? ContextId { get; }
    internal int RecordCount { get; }
    internal bool GuardCalled { get; }
    internal TargetLayerStatus Binding { get; }
    internal TargetLayerStatus Compatibility { get; }
    internal IReadOnlyList<TargetReason> Reasons { get; }
    internal TargetData(string? root, string? representation, string? main, string? context,
        int count, bool guardCalled, TargetLayerStatus binding, TargetLayerStatus compatibility,
        IEnumerable<TargetReason> reasons)
    {
        RootId = root; RepresentationId = representation; MainRepresentationId = main; ContextId = context;
        RecordCount = count; GuardCalled = guardCalled; Binding = binding; Compatibility = compatibility;
        Reasons = Array.AsReadOnly(TargetReasonOrder.Order(reasons));
    }
}

internal static class TargetReasonOrder
{
    // Numeric nested field indices, not lexicographic "10" before "2".
    internal static TargetReason[] Order(IEnumerable<TargetReason> reasons) => reasons
        .OrderBy(r => r, Comparer<TargetReason>.Create(Compare)).ToArray();
    private static int Compare(TargetReason a, TargetReason b)
    {
        int c = StringComparer.Ordinal.Compare(a.Layer, b.Layer);
        if (c == 0) c = StringComparer.Ordinal.Compare(a.Code, b.Code);
        if (c == 0) c = StringComparer.Ordinal.Compare(a.SourceId, b.SourceId);
        if (c == 0) c = StringComparer.Ordinal.Compare(a.Component, b.Component);
        if (c == 0) c = Sequence(a.Indices, b.Indices, Comparer<int>.Default);
        if (c == 0) c = Sequence(a.RelatedIds, b.RelatedIds, StringComparer.Ordinal);
        return c == 0 ? a.Occurrence.CompareTo(b.Occurrence) : c;
    }
    private static int Sequence<T>(IReadOnlyList<T> a, IReadOnlyList<T> b, IComparer<T> comparer)
    {
        for (int i = 0; i < Math.Min(a.Count, b.Count); i++)
        { int c = comparer.Compare(a[i], b[i]); if (c != 0) return c; }
        return a.Count.CompareTo(b.Count);
    }
}

internal sealed class TargetObservation
{
    internal int ManifestRevision => 1;
    internal int BindingRevision => 1;
    internal int CompatibilityRevision => 1;
    internal int UnitRevision => HeaderUnitObservation.UnitPolicyRevision;
    internal int HeaderRevision => HeaderUnitObservation.HeaderPolicyRevision;
    internal int EnvelopeRevision => EnvelopeObservation.CurrentPolicyRevision;
    internal string? RootId { get; }
    internal string? RepresentationId { get; }
    internal string? MainRepresentationId { get; }
    internal string? ContextId { get; }
    internal int RecordCount { get; }
    internal bool GuardCalled { get; }
    internal TargetLayerStatus Binding { get; }
    internal TargetLayerStatus Compatibility { get; }
    internal IReadOnlyList<TargetReason> Reasons { get; }
    private TargetObservation(TargetData data)
    {
        RootId = data.RootId; RepresentationId = data.RepresentationId;
        MainRepresentationId = data.MainRepresentationId; ContextId = data.ContextId;
        RecordCount = data.RecordCount; GuardCalled = data.GuardCalled;
        Binding = data.Binding; Compatibility = data.Compatibility; Reasons = data.Reasons;
    }
    internal static class EvidenceBinding
    {
        internal static TargetObservation? Bind(InventoryEvidence? evidence, InventoryEvidence.ImportSession? owner)
        {
            var data = owner?.ReadTargetData(evidence);
            return data is null ? null : new TargetObservation(data);
        }
    }
}

internal static class TargetRevisions
{
    internal static bool Approved(int manifest, int binding, int compatibility, int unit, int header, int envelope) =>
        manifest == 1 && binding == 1 && compatibility == 1 && unit == 1 && header == 2 && envelope == 1;
}

// Target binding only. This is not the future combined eligibility gate.
internal sealed class TargetDecision
{
    internal TargetStatus Status { get; }
    internal IReadOnlyList<TargetReason> Reasons { get; }
    private TargetDecision(TargetStatus status, IEnumerable<TargetReason> reasons)
    { Status = status; Reasons = Array.AsReadOnly(TargetReasonOrder.Order(reasons)); }
    internal static class TargetEvidenceGate
    {
        internal static TargetDecision Evaluate(InventoryEvidence? evidence, InventoryEvidence.ImportSession? owner)
        {
            if (evidence is null || owner is null)
                return new(TargetStatus.NoEvidence, [new("Ownership", "TargetEvidenceUnavailable")]);
            if (!owner.Owns(evidence))
                return new(TargetStatus.Blocked, [new("Ownership", "TargetOwnerMismatch")]);
            var observation = owner.ReadTarget(evidence);
            if (observation is null)
                return new(TargetStatus.Blocked, [new("Ownership", "TargetEvidenceUnavailable")]);
            var reasons = observation.Reasons.ToList();
            if (!TargetRevisions.Approved(observation.ManifestRevision, observation.BindingRevision,
                observation.CompatibilityRevision, observation.UnitRevision, observation.HeaderRevision, observation.EnvelopeRevision))
                reasons.Add(new("Revision", "TargetRevisionUnsupported"));
            if (InventoryDecision.InventoryEvidenceGate.Evaluate(evidence, owner).Status != InventoryDecisionStatus.InventoryClear)
                reasons.Add(new("Inventory", "InventoryNotClear"));
            if (observation.Binding != TargetLayerStatus.Observed || observation.Compatibility != TargetLayerStatus.Observed)
                reasons.Add(new("Target", "DependencyUnavailable"));
            return new(reasons.Count == 0 ? TargetStatus.TargetBoundOnly : TargetStatus.Blocked, reasons);
        }
    }
}
