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
The composite decision has a private constructor and a single issuing Application
gate, as with InventoryDecision; no other component or neutral test core may mint
it. Bind separate revisions for schema allowlist, unit policy, candidate-root
definition and evaluator coverage manifest, in addition to producer revisions.

After the unchanged inventory prerequisite, evaluate in dependency order:
Header -> Target -> units of the target's representation context -> Coverage.
Evidence production may precede target selection: produce a separate immutable
unit observation for every context, never a global unit or a preselected target.
Multiple contexts with identical units are normal and do not themselves block.

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

E1 candidate/occurrence definition: enumerate geometry-bearing representations
and their root items across the file, including solid forms such as
MANIFOLD_SOLID_BREP, BREP_WITH_VOIDS and FACETED_BREP (including applicable complex
forms), surface models and wireframe/geometric representations. Traverse mapping
and assembly placement relationships (including MAPPED_ITEM) to count physical
occurrences, not just distinct root IDs. These examples are not an exhaustive
allowlist: an unclassified geometry-bearing representation or relationship blocks
positive scope proof until classified by the versioned manifest. Initial readiness
requires exactly one physical occurrence, exactly one supported solid root, and
no other geometry-bearing representation. Additional occurrences of the same root
are additional physical instances. Alternate/duplicate representations cannot be
silently ignored or assumed equivalent. BREP_WITH_VOIDS, FACETED_BREP, surface and
wireframe support is not introduced here; their presence cannot disappear from
the scope check merely because today's importer counts only MANIFOLD_SOLID_BREP.

E2 conversion-unit definition: follow the unit assigned to each representation
context, including CONVERSION_BASED_UNIT references and conversion factors. A base
SI unit used to define a conversion unit must never substitute for that context's
actual unit. INCH must not become millimetre merely because its factor references
millimetres; DEGREE must not become radian because its factor references radians.
Unit policy v1 must explicitly choose validated-factor support or blocking for
each conversion form before implementation; no choice is ratified here.

Coverage must distinguish permitted non-geometric metadata from required geometry;
an unknown entity is not automatically harmless. Before implementation, review
an explicit evaluator-specific classification/coverage allowlist: unknown entity
types block by default, including those without diagnostics, until their supported
role is explicitly classified. Do not claim
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
| BREP_WITH_VOIDS or FACETED_BREP without an approved supported mapping | Non-ready; count as candidate geometry, never as absence of a root |
| One supported solid plus a surface body or wireframe model | ScopeNotSupported; other geometry cannot be omitted |
| One distinct root placed twice through MAPPED_ITEM/assembly relationships | ScopeNotSupported; two physical occurrences despite one root ID |
| INCH context whose conversion factor refers to millimetres | Verified inch conversion under reviewed v1 policy or explicit block; never infer millimetres |
| DEGREE context whose conversion factor refers to radians | Verified degree conversion under reviewed v1 policy or explicit block; never infer radians |
| Two contexts with the same valid units | Separate context evidence; multiplicity alone does not block |

## Delivery sequence and explicit limits

1. Independent design review ratifies or amends schema/units/target/coverage
   requirements and identifies the first evaluator contract. No consumer yet.
2. Small implementation slice produces same-capture header and separate unit
   evidence for EVERY representation context through the existing session, with
   tests; no target selection, consumer or second snapshot/temp lifecycle.
   A narrowly shared parser entry may be necessary: inspect/review its final diff.
3. Target selection binds its context's existing unit evidence, then coverage
   evidence for that concrete evaluator, manifest and negative
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

## Senior section 23 — conditional design acceptance

Source: owner-supplied independent review of `39cc95480e81a562ab563d97f55662a8519bde9f`
in the working conversation, dated 2026-11-22. No GitHub review submission or
independent CI verification is claimed. Decision 1: conditional merge approval
after E1 candidate/physical-instance definition and E2 conversion-unit additions.
Both are now incorporated above with six additional acceptance rows (22 total).
The existing-code table was independently checked by the reviewer. Predicted
legacy inch/degree misresolution is a static finding, not yet fixture-reproduced;
no runtime regression result or legacy resolver correction is claimed here.

Decision 2: first header/unit implementation is conditional. Before starting:
this amended design must merge with green CI; schema allowlist v1 and unit policy
v1 must each be reviewed; scope is per-context evidence with no target selection.
Before that implementation merges: N1 must close; captured-header decoding/BOM
must match ReadCaptured with parity tests; pin full header/unit producer sources,
diagnostic codes and severities. Required synthetic fixtures: inch, degree, two
contexts with different units, two with the same units, unitless context,
unsupported prefix, malformed header and unknown schema. Preserve existing
StepUnitResolver behavior, omit afterSourceCaptured in production (N2), add no
consumer. These conditions are recorded requirements, not evidence of completion.

Deferred follow-ups, owner Mehmet İlkay Kınacı: GLOBAL_UNCERTAINTY evidence at
precision-dependent evaluator design; multi/selected-body design before enabling
that scope; importer BREP_WITH_VOIDS support before claiming it supported;
workshop ScopeNotSupported incidence when a scope classifier exists and before
product eligibility claims; evaluator coverage allowlist before that evaluator's
implementation/consumer. No count is inferred from inventory diagnostics alone.
M2 stays OPEN, N1 separate and pending. No implementation starts in this PR.
