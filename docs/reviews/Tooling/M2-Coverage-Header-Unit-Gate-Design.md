# M2 evaluation scope, header and unit gates — proposal

Date: 2026-11-22 Europe/Istanbul. Class: VERIFY / architecture discovery.
Start: `17a112446078b9349eac39962561b2adb9d63a8c`.
State: PROPOSED, NOT RATIFIED OR IMPLEMENTED. M2 OPEN; N1 acceptance pending.
Owner: Mehmet İlkay Kınacı. No product consumer or wire change in this slice.

## One goal and evidence for the missing capability

Define what must be proven before the first geometry-dependent evaluator can
produce feature, time or cost results. InventoryClear alone cannot supply it:
the measured corrected BACK PLATE has zero inventory diagnostics but four BREP
records; the owner subsequently showed SOLIDWORKS Katı Gövdeler(4), confirming
four imported solid bodies. Owner says this was unintended, possibly overlooked
during editing. This observation does not identify which edit caused the split.
Original BACK PLATE is the baseline; preserve corrected version as comparison.

Verified current boundaries at Start:

| Component | Current behavior | Missing proof |
| --- | --- | --- |
| InventoryEvidence.ImportSession | Privately captures bytes, retains inventory, issues owned inventory evidence; no consumer | No header, context-unit, target-selection or geometry-coverage evidence |
| StepHeaderReader.Read(path) | Reopens a path; parses header metadata and schemas; IO/parse failure diagnostics | Cannot be used by a new same-capture gate by reopening the original path |
| StepUnitResolver | Collects recognized SI length/angle units across inventory; one distinct recognized candidate or Unknown | Not tied to the evaluated representation context; unsupported candidates filtered out; no conversion-factor/completeness attestation |
| CadUnitInfo | Declares internal metre/radian units | Metadata declaration does not prove mapped coordinates were converted |
| ManufacturingRegionImportService | Separate snapshot-based face import; requires exactly one semantic MANIFOLD_SOLID_BREP root, then maps it | Existing root restriction is not a general multi-body policy or full-file coverage proof |
| RegionSourceGate | SourceLinkMatched establishes document/face provenance only | No dimensional, material-side or evaluator authorization |

Source references: src/MPC.Application/Inventory/InventoryEvidence.cs;
src/MPC.Connectors.Step/Readers/StepHeaderReader.cs;
src/MPC.Connectors.Step/Resolution/StepUnitResolver.cs;
src/MPC.Connectors/Snapshots/Units/CadUnitInfo.cs;
src/MPC.Application/Manufacturing/ManufacturingRegionImportService.cs;
src/MPC.Application/Manufacturing/RegionSourceGate.cs.

## Proposed decision boundary for review

Application owns an operation-scoped composite decision. Inventory policy A is
unchanged: whole-file Error/Fatal, unknown severity, foreign or missing evidence
cannot pass. Additional gates do not downgrade severities or reinterpret
SourceLinkMatched. Proposed conceptual outcomes: ReadyForDeclaredEvaluation,
Blocked (all known reasons), EvidenceUnavailable (missing/unsupported producer).
These are design labels, not new enum values or existing eligibility states.
Unknown evidence is never treated as a successful negative check. Consumer must
not execute or emit numeric/feature output for either non-ready outcome.

Each proof must be immutable, issued only by the trusted capture/import pipeline,
owned by the same session and bound to document hash, producer/policy revision,
target identity and evaluator requirement revision where relevant. Equal hashes
alone do not make cross-session proofs interchangeable. No caller-supplied flags,
public clean-proof constructors, metadata-only factories or test result bypasses.

| Gate | Proposed required proof | Non-ready examples |
| --- | --- | --- |
| Header | Parse header from exactly the captured bytes; explicit supported-schema/implementation policy for this evaluator | Missing/malformed header, unknown schema, no same-capture producer |
| Units | Resolve units in the evaluated representation context, include unresolved/conflicting candidates, positive finite scale and verified normalization contract | Unknown/conflicting length units, unsupported conversion-based unit, mixed contexts without validated transforms, missing angle units when required |
| Target | Explicit declared evaluation target and unique same-session root identity | No roots, ambiguous roots, stale/foreign selection, selected entity not a supported solid root |
| Coverage | Every geometric/topological dependency required by the declared evaluator is accounted for and mapped/validated; positive coverage ledger, not absence of diagnostics | Dropped semantic records, unsupported required surface/curve, unresolved references, mapping failure, unverified topology/required material side |

Initial scope proposal: whole-file single-solid evaluation only. A file with
multiple candidate solid roots is Blocked/ScopeNotSupported by this initial
capability, not classified as malformed. Do not automatically choose the first,
largest or visually selected body. Multi-solid or selected-body evaluation needs
a separately reviewed design for context, dependencies, exclusions, assembly
transforms and result labeling; it is not silently enabled by this proposal.
An explicit target identity is required even in the single-solid case; automatic
binding of the unique supported root must be produced by the trusted importer.

Coverage must distinguish permitted non-geometric metadata from required geometry;
an unknown entity is not automatically harmless. Before implementation, review
an explicit evaluator-specific classification/coverage manifest. Do not claim
all-file semantic understanding from one successfully mapped root. Header schema
is declared provenance, not proof of actual content support. Unit warnings remain
warnings; the independent requirements gate can still block dependent evaluation.
Require angle proof only when the evaluator contract actually depends on it.
No supported schema allowlist, unit conversion policy or coverage manifest is
ratified by this document. Their concrete revisions are implementation prerequisites.

## Proposed acceptance matrix

| Case | Expected before evaluation |
| --- | --- |
| Inventory Error/Fatal or unknown severity anywhere | Non-ready under unchanged A, including unreferenced record |
| InventoryClear but absent same-capture header proof | EvidenceUnavailable; no output |
| Header read from a later modified source path | Reject foreign evidence; retain original capture identity |
| Unsupported/malformed schema/header | Non-ready with distinct reason; never guess from extension |
| Known length unit plus unsupported/conflicting unit candidate in target context | Non-ready; filtering unknown candidates cannot establish readiness |
| Known units and declared metre/radian metadata but unproven normalization | EvidenceUnavailable; no dimensional output |
| Two contexts with different units and no validated transform | Non-ready; no global-unit inference |
| Zero, multiple, foreign or unknown roots | Non-ready; no implicit body choice |
| Corrected BACK PLATE with four bodies | ScopeNotSupported in proposed initial single-solid slice; inventory observation remains successful |
| Original BACK PLATE with one BREP record | Target ambiguity removed only; all other gates still required, no automatic pass |
| Supported root with unsupported required dependency or incomplete topology | Non-ready despite inventory/mapping success elsewhere |
| Unsupported unclassified record without diagnostic | EvidenceUnavailable until coverage classification proves its role |
| All required same-session proofs, complete coverage and valid revision | Ready only for the declared evaluator/target, never blanket manufacturing approval |
| SourceLinkMatched or Worker success alone | Insufficient; cannot start evaluation |
| Capture/provenance/parse failure | Preserve original cause/stage (A4), no fake clean counts or generic success |
| Production capture invocation | Omit afterSourceCaptured checkpoint (N2); tests cannot mint proofs/decisions |

## Delivery sequence and explicit limits

1. Independent design review ratifies or amends schema/units/target/coverage
   requirements and identifies the first evaluator contract. No consumer yet.
2. Small implementation slice produces same-capture header/context/unit evidence
   through the existing session, with tests; no second snapshot/temp lifecycle.
   A narrowly shared parser entry may be necessary: inspect/review its final diff.
3. Coverage/target evidence for that concrete evaluator, manifest and negative
   fixtures; no general geometry expansion merely to turn observations green.
4. Consumer enforcement with deterministic reason aggregation and A4/N2 tests;
   Release/full Windows CI and applicable Senior review before merge. Scope any
   physical acceptance from actual Host/Worker changes, not assumed here.

M2 closure requires implemented gates and consumer verification, not acceptance
of this document. N1 is separate and pending; this design does not close it.
No CAD repair, multi-body merge, wire/DTO/shared enum changes, Host/Worker/IPC,
performance cache, feature/time/cost implementation or existing importer rewrite.

Risk triggers: trust/fail-closed YES because this proposes future evidence
sufficiency and rejection semantics; wire NO; shared enum values NO; IPC NO;
runtime persistence NO; production UI NO. Independent final-diff Senior design
review required before this proposal merges. CI cannot ratify the design.
Validation: read-only discovery and documentation checks; required PR CI pending.
Reviewed/Closing: pending design review and delivery; no implementation PASS claimed.
