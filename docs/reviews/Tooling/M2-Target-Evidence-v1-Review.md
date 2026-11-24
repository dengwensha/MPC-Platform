# M2 — consumer-free owned target producer v1

Date: 2026-11-24 Europe/Istanbul. Class: NEW BUILD after VERIFY.
Start: 264b16e548d77fcf1d2365670b7f97803fa3450c (#139).
Branch: feature/M2-target-evidence-v1. Owner: Mehmet İlkay Kınacı.
State: implementation for independent review, NOT merged. M2 OPEN.

## Goal, justification and scope

BUILD JUSTIFIED: Senior sections 37/38 ratified the exact v1 role/binding/compatibility
contract, including A1–A3, but no owned target producer existed. Existing R-U is
only a fail-fast import precheck and cannot issue owned observations or the full
all-record target result. Implement only this missing internal Application slice.

No consumer, combined eligibility gate, selected-body UI, normalization, physical-
occurrence, angle-absence, geometry-validity or Ready proof. Header/envelope and
Policy A remain independent; no existing importer, Worker, wire, Host, SOLIDWORKS,
semantic mapper or parser behavior is changed. #137 transforms remain unsupported.
No new friend assembly. The existing payload-free hook is used only by tests.

## Issuance and policy execution

InventoryEvidence creates private TargetData from the exact retained inventory in
its private constructor. The owning ImportSession holds the private bound
TargetObservation. ReadTarget/ReadTargetData and the binder require reference owner
identity; equal source hashes never substitute. Evidence has no session back-reference.
No captured bytes, connector inventory, mutable stream or authority constructor is
exposed. The final TargetEvidenceGate takes only evidence + owner, requires Policy A
InventoryClear and approved revisions, and returns TargetBoundOnly/Blocked/NoEvidence.
NoEvidence is missing owner/evidence; foreign ownership blocks without reading it.

The reviewed contract is embedded in Application, not loaded from a caller path.
Its normalized UTF-8/LF SHA-256 remains
`F6C55BD1B7E84CE00683BBB2BC018DC3871EDD6FF2AD0BAFD1DCABFE7227BBB1`.
Load failure/hash drift is an unexpected implementation/deployment fault, not fake
clean evidence. All 86 whole signatures, 54 conditional rows/196 fields, 83 profiles,
12 invariants and 20 Error codes retain the ratified definitions. Record signatures
use StepEvidenceSyntax.Components, never the tooling splitter or base containment.
Profile kind/cardinality/enum/reference checks run across all admitted records;
degree/grid/knot/weight checks are explicit code, not inferred geometry support.

Typed graph checks cover direct root/representation/context, bounded T2, unique
selected SDR/PDS/product chain and unknown incoming paths. All presentation rows
receive catalogue/target/reference/side/RGB checks; iterative cycle detection avoids
recursive presentation graph traversal. Root geometry and admitted placement
dependencies must account for every geometry row. When root/binding is unavailable,
dependent ownership is reported unavailable instead of falsely declaring every
geometry record orphaned. Unknown/partial/duplicate/unresolved rows always block.

- A1: direct ImportUnitPrecheck.EnsureCompatible call on the same inventory and
  uniquely established root. Only its InvalidDataException input rejection is
  recorded as CurrentImportIncompatible; original message/inner cause strings are
  retained. No broad catch or copied rule substitute; independent reasons continue.
- A2: only exact integer revisions 1/1/1/1/2/1 (role/binding/compatibility/unit/header/
  envelope) can pass. No proposal labels, coercion or latest-version fallback.
- A3: every context has structural unit checks; presentation-only metre/cm is not
  forced to mm. R-U compatibility still demands mm+radian at binding endpoints.

Reasons retain layer/code/source/component/nested numeric indices/related IDs/stable
occurrence and original guard failure. Whole-entity/global graph findings use empty
component/index when no single field applies; no guessed child index is fabricated.
Sort is ordinal layer/code/source/component, numeric index tuple, related tuple,
then stable occurrence. Data and nested collections are readonly copies. Unexpected
exceptions propagate under the existing policy. Header/envelope/coverage failures
are not silently reclassified as target success or replaced by a target decision.

## Tests and pins

65 new tests: five golden positive captures; 22 existing R-U negative cases through
the real capture/gate; structural metre/cm versus compatibility; A3 presentation
cases; source replacement after capture; foreign same/different hash ownership;
approved-revision tuple; full-Application method/constructor/wrapper reflection lock;
immutability; complete deterministic independent failures; 196 field-kind mutations
at exact field paths; cardinality/enum/numeric/optional-placement boundaries;
B-spline knot/degree/multiplicity cases; original/corrected cone metadata; source,
contract/reason/severity pins. The 196-case loop is one test, not 196 extra tests.

The existing InventoryEvidence source pin is explicitly updated for the private
target capture/read addition; reader/parser/header/envelope/guard sources and their
reason codes are unchanged. New Target*.cs sources are exhaustively pinned, plus
the immutable contract hash. No existing assert is weakened.

## Measurement and bounded evidence

The M2TargetProbe is a tooling-only reflection adapter into the real internal
CaptureFromFile, owner read and TargetEvidenceGate; no public seam or friend access.
It separately captures/hashes the same input into ReadCaptured, then directly calls
the unchanged ImportUnitPrecheck for independent parity. All three hashes (initial,
real captured session, independent R-U capture) must agree and input remains stable.
Only names/hashes/IDs/counts/technical results/reasons are emitted. Inner cause text
can contain STEP tokens and is excluded from uploaded JSON. Output must be empty;
unavailable captures or unexpected exceptions fail measurement, never become zero
diagnostics. Reflection adapter/source and exact SDK/commands/build hashes are pinned.

The complete historical 20-file corpus and 12 controls will be compared with the
historical draft and independent R-U twice in separate processes. One additional
positive control is separate from the historical set. Full reason snapshots and
comparison hashes are retained; repeat equality excludes no decision fields.
Linux is the full private corpus; Windows CI repeats nine public inputs and 13
controls. Private workshop/Festo Windows acceptance is NOT claimed. Families and
file versions remain correlated, not independent jobs. Raw private CAD/neutral ASTs
are not committed. No successful-import-loss approval is inferred from green CI.

### Historical cone metadata discrepancy

The old independent analytic cone is valid for the prior geometric oracle, but
PRODUCT #1009.frame_of_reference points directly to APPLICATION_CONTEXT #1011,
not a PRODUCT_CONTEXT. The draft only checked a reference shape and R-U did not
type-check that tail. Full v1 field rules therefore correctly block historical
C11/RU-QuarterCone, with ReferenceRoleMismatch #1009[3][0] -> #1011. The original
file and 12 historical controls are unchanged. This is a stricter metadata finding,
not a new conical geometry failure or an undisclosed policy relaxation.

RU-QuarterCone-TargetMetadata.step is an ADDITIONAL control: change only #1009 to
reference new PRODUCT_CONTEXT #1012, whose application context is #1011. Every
geometric record is byte-identical after line-ending normalization (test pinned).
This corrected control must pass target binding and is C13 in the audit. It does
not replace C11 or inflate the 20-file real corpus.

Measured code: `8a2eb1a48d7bc5e9e2147bb4ee59540c112a84a6`. Full source/DLL/contract
hashes, SDK/build commands and 66 complete metadata snapshots are recorded in
[Validation.json](evidence/M2-Target-v1/Validation.json). Final evidence-only descendant
retains identical producer/test sources; final-head CI is recorded in the PR.

- Full corpus: 20/20 snapshots byte-identical across separate processes; all 444,497
  records/counts match historical T3. 17 TargetBoundOnly, three Blocked (corrected
  BACK PLATE, Festo, R03). No loss from the 17 historical candidates, including
  B-spline workshop files. This is not a new successful geometry-import claim.
- Controls: original 12 plus separate C13, repeated 13/13 byte-identically. Three
  target-bound (C01, C03, C13), ten blocked. Historical C11 is the one explained
  stricter metadata finding. Every independently rejected R-U call blocks, with the
  actual producer guard call recorded; no positive target on an R-U rejection.
- Measurements: 66 real target captures/gates and 66 independently captured R-U
  calls. Initial/session/independent hashes and final unchanged-file check agree.
  Full reason snapshots are retained, not only code counts. No scalar CAD tokens
  or inner cause text is uploaded.
- Linux whole-audit 20-file runs: 20.9923 and 22.6891 seconds, peak RSS 598,152 and
  699,848 KiB. These include capture/target, a SECOND independent R-U inventory and
  JSON emission; they are NOT isolated production latency or a Host memory budget.
  This cost remains a B6/first-consumer measurement concern; no optimization claim.

Local results:
Application 526/526, Core 1366/1366, Contracts 94/94; Windows-only Worker executable
fixture cannot run on Linux. Final Windows full regression and Worker acceptance
remain mandatory, not waived by these local results.

## Senior decisions and remaining work

1. Does issuance/read/gate ownership and the all-Application reflection boundary
   preserve C1, with no detached data authority or new consumer?
2. Are the exact field, full product/T1 graph and all-record ownership checks a
   faithful implementation of v1, including dependent unavailable findings?
3. Are A1 direct same-inventory guard, A2 approved tuple and A3 presentation scope
   implemented without weakening current R-U or conflating binding/compatibility?
4. Is the complete 20+12 comparison and separate corrected metadata control enough;
   are every stricter difference and the original C11 gap honestly accounted for?
5. With green final-head Windows CI, may this consumer-free slice merge? No M2
   closure, occurrence/angle-absence, geometry validity or Ready approval requested.

Trust/fail-closed trigger YES; independent final-diff Senior acceptance before merge.
Wire/shared numeric enums/IPC/persistence/UI entry NO. Host/Worker/SOLIDWORKS unchanged;
no new physical gate claimed waived beyond this actual diff. Final CI/review pending;
no merge until required review. M2 OPEN, existing follow-ups and bounded N1/R-U
acceptances unchanged. U1 positive-coverage waiver, evaluator coverage/normalization,
combined gate and first consumer remain later work.
