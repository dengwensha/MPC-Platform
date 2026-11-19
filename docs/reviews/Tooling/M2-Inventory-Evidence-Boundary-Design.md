# M2 Application inventory evidence and decision boundary design

Date: 2026-11-19 Europe/Istanbul. Class: VERIFY / design for independent review.
Baseline: `5ad02dc79bff9d8b1a744432d78bb1dd22d42813`, main CI #240 successful.
State: PROPOSED DESIGN, NOT IMPLEMENTED OR MERGED. M2 remains OPEN.
Owner: Mehmet İlkay Kınacı; implementation assistant maintains design/evidence.
Authority: owner-accepted policy A and Senior section 19 acceptance of the bounded
next DESIGN step. Neither approval is sign-off on this new concrete design.

## One technical goal and exclusions

Specify an importer-owned, immutable inventory observation and a pure Application
inventory decision that cannot be obtained from a caller-manufactured clean DTO.
The next implementation slice, if reviewed and authorized, stays inside Application
and its tests. Existing Application references to Connectors/Connectors.Step suffice.
No public CAD objects, SOLIDWORKS types or Tooling reference enter the evidence API.

No production source is changed in this design PR. No v1.0/v1.1 routing, DTO,
MPC.Contracts enum value, Worker/Host/UI, parser, severity assignment, product
storage, catalog intake or whole-file evaluator is implemented or changed.
The current region importer and SourceLinkMatched route remain untouched in the
first slice; do not overload their bool/out-region result with the new eligibility.

## Verified baseline constraints

- ManufacturingRegionImportService captures bytes once, hashes them, reads a
  temporary snapshot, then constructs semantic entities from that inventory.
  Its bool/out-region API does not carry inventory diagnostics.
- RegionSourceGate checks the region's existing fingerprint/face linkage; the
  current result is not machining or whole-file completeness approval.
- StepDataEntityReader collects record and validator diagnostics in the result.
  SucceededWithDiagnostics can contain Error, not Fatal. Unknown simple types can
  produce no diagnostic, so neither successful status nor zero errors is sufficient.
- Application currently has no InternalsVisibleTo attribute. Tests of new internal
  types would need a narrowly scoped friend entry for MPC.Application.Tests only.
- [Direct R01/R02/R03 inventory profiles](M2-Real-Inventory-Profile.md): 329/649/135828
  records, zero Error/Fatal/read failures in 3 fixed inputs. Workshop sample is 0.
  This is not a representative rejection rate or semantic-completeness certificate.

These are inspected source facts and existing measured evidence, not proof that
the proposed producer/gate already exists. All names below are proposed local
Application names; no shared wire numeric values are assigned by this document.

## Proposed type and ownership shape

All new types/methods are internal to MPC.Application in the first slice. Values
use Application-owned primitives/enums, not connector interfaces or mutable records.

| Proposed element | Required shape and responsibility |
| --- | --- |
| InventoryEvidence | Sealed reference type, private constructor, getter-only fields. No record/with/init mutation, deserialization constructor, metadata factory or caller-set clean flag. |
| InventoryEvidence.ImportSession | Sealed nested producer/session with private constructor. Its static CaptureFromFile(path) is the only issuance path. Nesting grants access to the outer evidence's private constructor without an assembly-wide metadata constructor. |
| ImportSession.Evidence and DocumentSha256 | Readonly, minted together from the actual capture. Session retains the private inventory needed for later same-capture geometry composition; never exposes mutable source bytes, inventory collections, snapshot path or setters to consumers. |
| ImportSession.Owns(evidence) | Side-effect-free identity check using reference identity of the evidence minted by this session. It cannot be overridden. Equal hashes from different sessions do not satisfy ownership. |
| InventoryCaptureResult | Readonly captured-session or unavailable result. Unavailable has no session/evidence and retains original failure evidence. It never substitutes an empty diagnostic collection for a failed observation. |
| InventoryEvidenceGate.Evaluate(evidence, expectedSession) | Pure deterministic function; no file/process/clock/configuration access. Returns the inventory-only decision described below. Neither argument is reconstructed from JSON, a Worker success flag or the non-production probe. |
| InventoryDecision | Immutable reference result with three statuses, ordered typed blocking reasons and a fixed local policy revision. No public construction or writable approval flag. It is not a reusable authorization token or manufacturing approval. |

The accessibility pattern was checked outside the repository with .NET 8.0.422:
a nested producer can instantiate its containing sealed type's private constructor;
consumer construction of either private evidence or private session was rejected
with CS0122. A positive identity check also rejected a second fixture session with
the same hash. These three feasibility checks tested ACCESSIBILITY/IDENTITY ONLY;
that fixture producer did not read STEP and is not production code or an acceptance
suite. No CaptureFixture or arbitrary-field factory may ship with the real boundary.

The threat boundary is ordinary typed callers and accidental API misuse. It is
not a sandbox against hostile code executing with reflection/unsafe access inside
the process, a compromised assembly or filesystem administrator. Hashes are
content identity, not signatures, supplier authentication or proof of geometry.

## Required immutable evidence fields

| Field | Source and invariant |
| --- | --- |
| DocumentSha256 | SHA256 of the one captured byte array; canonical uppercase 64 ASCII hex characters. Never accepted as an evidence-factory input. |
| InventoryReadStatus | Explicit Application projection of reader result status: Succeeded, SucceededWithDiagnostics, Failed or Unknown. This is result status, not per-record Complete/Partial. Unknown cannot pass. |
| HasInventory | Actual presence of the private inventory; not inferred from diagnostics. No exposed mutable inventory. |
| Diagnostics | Complete, defensively copied immutable sequence from result.Diagnostics only, not added again from each record. Each entry retains Code, known Severity, original numeric severity value, SourceEntityId (nullable), and original index. Copy message/exception-type/source-type strings when provided for trace; they never determine eligibility. No filtering, downgrading or silent deduplication. |
| ReaderRevision | Actual reader assembly identity/version, informational/source revision and assembly SHA256 captured by the producer. Default assembly version alone is inadequate provenance. No caller-supplied reader version. |
| EvidenceSchemaRevision / SeverityMappingRevision | Producer-owned local supported revisions, pinned by tests. Unsupported/missing revision blocks; a new code-to-severity interpretation requires reviewed revision/expectation changes. No wire version is introduced. |

Strings are immutable; diagnostic entries have no setters. Copy the input sequence
and wrap private storage without leaking an array/list alias. Do not store a mutable
connector result/diagnostic collection as the public evidence payload. Preserve
Info/Warning as well as blockers. Unknown severity must retain its raw value and
map to Unknown, never default to Warning or disappear.

## Capture sequence and snapshot binding

1. Validate a nonempty source path and supported STEP/STP extension without culture-
   dependent comparisons. Capture actual bytes once. No overload accepts bytes/hash,
   diagnostics, an inventory or an injected caller-provided success reader.
2. Hash those bytes, create an unexposed private temporary STEP copy and invoke the
   concrete existing reader/validator. File access before read belongs in the producer,
   never the pure gate. Use the reader assembly's actual revision information.
3. Project the result and all diagnostics; retain the resulting inventory privately.
   A normal Failed reader result with Fatal diagnostics is still an observed failed
   inventory: create a session/evidence with Failed and no inventory, not clean data.
4. Complete snapshot cleanup and issue the immutable session/evidence. No caller can
   choose its identity or combine a hash from one capture with diagnostics from another.
   The future geometry composition must use this session's private inventory, not
   reopen the original file or accept an unrelated caller-built ManufacturingRegion.

Expected path/file-access failures yield unavailable capture with original cause.
Known reader FormatException may yield unavailable capture, preserving its cause;
there is no observed inventory to manufacture. Keep such catches confined to the
specific path/file/read calls. Do not blanket-catch InvalidOperationException or
reinterpret programming/configuration invariants as input failures. Unexpected
exceptions escaping the existing reader propagate under the bounded exception policy; no session is
published. Snapshot cleanup must not mask the original exception. A cleanup-only
failure leaves capture unavailable; secondary cleanup evidence stays secondary.

Changing the original path after capture cannot alter this session's observation.
This does NOT certify that a later original file still matches. Existing document/
source-link checks remain separately required. Binding a future geometry result
to its creating session is a later integration requirement, not a claimed feature
of this design PR or a consequence of supplying a matching hash string.

## Pure decision and precedence

Evaluate receives the evidence and the expected producing session. It inspects only
immutable values/reference ownership; it does not call CaptureFromFile or inspect
the current filesystem. The expected session is retained by the Application owner
of the evaluated artifact, not chosen by a Host success response. No future consumer
may present a decision object as substitute evidence; it must evaluate its own
artifact's retained session at the relevant boundary.

| Order | Condition | Status and primary reason |
| --- | --- | --- |
| 1 | Expected session absent | NoEvidence / MissingSnapshotContext |
| 2 | Evidence absent | NoEvidence / MissingInventoryEvidence |
| 3 | Expected session does not own this exact evidence | Blocked / SnapshotOwnerMismatch, even if content hashes match |
| 4 | Either fingerprint invalid, then fingerprints unequal | Blocked / InvalidFingerprint, otherwise FingerprintMismatch |
| 5 | Missing reader revision or unsupported schema/severity-mapping revision | Blocked / UnsupportedEvidenceRevision |
| 6 | Unknown result status or inconsistent status/presence/diagnostic metadata | Blocked / InconsistentInventoryEvidence |
| 7 | Observed Failed inventory | Blocked / InventoryReadFailed; retain diagnostic blocker reasons too |
| 8 | Any unknown diagnostic severity | Blocked / UnknownDiagnosticSeverity; never a warning default |
| 9 | Any Error/Fatal anywhere in the captured inventory | Blocked / InventoryDiagnostic; unreferenced entities included |
| 10 | None of the above | InventoryClear: ONLY the inventory-error condition is satisfied |

Orders 1–6 short-circuit structural/context failure; do not interpret diagnostics
from a foreign session as this artifact's blockers. For 7–9 collect all applicable
blocking reasons with primary priority in that order, then stable ordering by code
Ordinal, source ID Ordinal (null first), raw severity and original diagnostic index.
Keep repeated occurrences; human messages, culture, timestamps and mutable global
configuration must not determine status or reason ordering. Retain all evidence
Info/Warning for inspection, without turning them into blocking reasons.

Consistency includes: Succeeded requires inventory/no diagnostics;
SucceededWithDiagnostics requires inventory/nonempty diagnostics/no Fatal;
Failed requires no inventory and at least one Fatal. Known severity values are
mapped explicitly. Reader build/source revisions are trace fields, not a dynamic
commit allowlist: the supported evidence/severity schema is fixed in the compiled
producer and policy. Missing revision fields cannot be replaced with a guess. Unknown severity gets its explicit blocking rule; no coercion
into a compatible status. The wrapper cannot create successful metadata from an
unavailable capture. No new partial-record blocking rule replaces policy A's
severity rule; unsupported geometry remains the separate coverage-gate concern.

InventoryClear intentionally does not mean EligibleForMachining or AllGeometryRead.
A no-diagnostic unsupported entity may pass this inventory-only rule while still
failing a separately required supported-geometry/completeness decision. No overall
success boolean is exported, and there is no bypass that drops unreferenced Errors.

## Minimum meaningful acceptance before implementation closure

| Case | Required evidence / expected behavior |
| --- | --- |
| Golden supported file | Genuine capture, same-session ownership and copied metadata; InventoryClear only, no overall geometry/manufacturing approval |
| Golden plus unreferenced empty complex entity | Original Error code/severity/source ID retained; Blocked; exactly one count per result occurrence |
| Raw input without DATA | Failed/Fatal observed inventory; Blocked, distinguish from unavailable capture |
| Missing/unreadable input; reserved simple COMPLEX_ENTITY format failure | Capture unavailable with cause, no minted session; gate NoEvidence |
| No evidence or no expected context | NoEvidence with deterministic reason precedence |
| Same bytes captured twice | Cross-session evidence Blocked even with equal hashes; own-session evidence evaluated normally |
| Invalid/unequal fingerprint; unknown schema/status/severity; contradictory metadata | Never InventoryClear, with the ordered typed reason specified above |
| Info/Warning only | Retained unchanged; do not block this rule if metadata otherwise valid |
| Unsupported simple entity without diagnostics | Do not claim semantic completeness; inventory-only decision cannot authorize whole-file publication |
| External construction, record-with/init and mutation attempts | Cannot mint evidence/session from arbitrary fields; private constructor/sealed API checks and collection-alias mutation tests |
| Determinism and original-file replacement | Repeat same immutable inputs, including under tr-TR, gives same status/reason sequence; original file replacement does not mutate captured evidence |
| Current v1.0/v1.1 routes | Existing real Worker/source-link outcomes remain unchanged because first slice is not connected to those routes |
| Snapshot failure/cleanup | No false clean evidence, stale-session reuse or masking of original cause |

Authentic cases must use actual temporary file text and the concrete producer.
Defensive states impossible through the normal factory may be tested through an
internal neutral input projection for the pure policy core, using only the narrowly
scoped MPC.Application.Tests friend assembly. Such a projection must not construct
an InventoryEvidence, be publicly callable, deserialize probe JSON, or serve as
consumer authorization. Do not add a public CreateClean or metadata constructor
just to make tests convenient. Access/immutability tests cover the real wrapper.

## Pin the current raw-reader diagnostic severity map

The reviewed baseline contains these eight codes. This is a proposed explicit test
manifest for the next slice, not a severity change or claim tests already exist.

| Code | Required current severity |
| --- | --- |
| STEP_FILE_NOT_FOUND | Fatal |
| STEP_FILE_EXTENSION_UNSUPPORTED | Fatal |
| STEP_FILE_READ_FAILED | Fatal |
| STEP_DATA_SECTION_NOT_FOUND | Fatal |
| STEP_COMPLEX_ENTITY_COMPONENTS_NOT_FOUND | Error |
| STEP_DATA_ENTITY_HEADER_UNRECOGNIZED | Error |
| STEP_DUPLICATE_SOURCE_ENTITY_ID | Error |
| STEP_MISSING_ENTITY_REFERENCE | Error |

Pin all code/severity pairs and the explicit projection of Info/Warning/Error/Fatal,
not only a fixture's final blocked flag. Include real branch fixtures where feasible;
file-read failure needs a deterministic read-failure setup, not a flaky OS permission
assumption. A new diagnostic code, removal, severity change or mapping default needs
review of inventory eligibility impact and applicable independent Senior review.
Do not silently add an unrecognized code to a clean allowlist.

## Delivery gates and review request

This PR delivers the design and current-code severity manifest only. It adds no
production code, test-only minting API, runtime gate or implemented acceptance cases.
Existing full CI remains mandatory but cannot validate the unimplemented design.

Required decisions from independent Senior review:
1. Is the private nested producer/session ownership, same-snapshot binding and
   immutable schema suitable for the first Application-only implementation?
2. Are capture/unavailable behavior, three-result precedence, defensive test seam
   and severity-map pinning adequate without changing existing wire/source-link meaning?

Implementation may begin only after this concrete design is reviewed/authorized
and the required workshop-input measurement is completed or an explicit reviewed
revised disposition is recorded. No such waiver is requested or inferred now.
R01/R02/R03 measurement alone does not complete the workshop prerequisite.
No numerical feature/time/cost output can be released before consumer verification
and the separate completeness gate are defined and implemented. M2 detailed-cause
separation remains a separate OPEN item; all existing catalog/importer/end-user-
release and earlier whole-file-use gates continue to apply.

Risk triggers: trust/fail-closed Yes (new concrete evidence sufficiency/ownership
and decision semantics), independent review of final head required BEFORE merge.
Wire shape No; existing/shared enum numeric values No; IPC mechanics No;
persistence No; production UI entry No. #112's policy approval and #114's measurement
validation are not approval of this new concrete design. No physical PASS claimed.
