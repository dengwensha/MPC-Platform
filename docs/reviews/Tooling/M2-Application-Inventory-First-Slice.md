# M2 first isolated Application inventory boundary

Date: 2026-11-19 Europe/Istanbul. Class: NEW BUILD.
Start: `b2b0877f5bbab130903e19c7671cf7efb612e3ab`.
Branch: `feature/M2-application-inventory-evidence`.
Reviewed/Closing: pending implementation commit and independent Senior review.
State: IMPLEMENTATION FOR REVIEW; M2 OPEN; no product consumer.
Owner: Mehmet İlkay Kınacı; implementation assistant maintains evidence.

## Authority and one goal

BUILD JUSTIFIED: the existing region bool/out API and source-link decision do not
carry trustworthy whole-inventory evidence. Implement the Senior-approved #115
boundary with section 20 A1-A5 conditions, entirely inside Application and its tests.
Owner explicitly accepted the revised workshop order on 2026-11-19 (“evet
onaylıyorum”) after explanation: this isolated slice may precede workshop sampling;
sampling remains mandatory before first evaluator/integration consumption. Policy A
is unchanged. No calculation results or product integration are authorized here.

## Implemented boundary and concrete review choices

- `InventoryEvidence.ImportSession.CaptureFromFile` is the only evidence issuer.
  It captures actual bytes once, hashes them, creates a private CreateNew STEP
  snapshot, invokes the concrete reader/validator and copies result diagnostics
  once. The session privately retains the inventory for future same-session
  composition. A failed reader result remains an observed Failed/Fatal inventory;
  expected source access and FormatException failures retain the cause without
  minting a session. Other unexpected exceptions propagate.
- Evidence, revision and diagnostic objects have getter-only fields. Evidence has
  no inventory/session back-reference. Evidence/session constructors are private.
  No caller bytes/hash/reader/diagnostic factory or deserialization entry exists.
  Equal hashes from two captures do not confer ownership.
- The pure policy has context/structural precedence and collects all diagnostic
  blockers deterministically, preserving duplicates. InventoryClear means only
  the inventory-error condition is clear, never manufacturing/completeness approval.
- A1: cleanup has two immediate bounded attempts. Persistent cleanup failure is
  secondary `CleanupFailure`; a valid observation survives, and an original read
  exception is not replaced. No background retry or forced GC. A residual private
  temp file may remain for OS/operator temp housekeeping; no deletion guarantee or
  antivirus-timing physical test is claimed. Only a snapshot created by this call
  is deleted; CreateNew collision must not delete a pre-existing file. A neutral
  generic observation/cleanup helper tests orchestration without issuing proofs.
- A2: `InventoryDecision.InventoryEvidenceGate.Evaluate(evidence, session)` is
  nested inside the decision and can call its private constructor. The neutral
  policy returns only status/reason data. No other method returns InventoryDecision.
  Reflection/API tests check private constructors, getters, issuance path and lack
  of public exports. The only friend assembly added is MPC.Application.Tests.
- A3: separate header/unit and geometry-coverage gates remain unimplemented and
  mandatory before dependent whole-file consumers. Unknown-unit policy P2 is not
  retroactively ratified. InventoryClear cannot substitute for these gates.
- A4: this slice supports file-backed reader assemblies only. Empty Assembly.Location
  produces controlled unavailable capture (NotSupportedException). No single-file
  packaging support, guessed path/hash or alternate revision provider is claimed.
  Identity/version/informational revision and actual assembly SHA256 are captured;
  missing revision fields cannot pass the pure gate.
- A5: the session is an operation-scoped managed owner; the future caller must
  release it after composition/evaluation. There is no static inventory cache,
  disposable external resource, finalizer or forced collection. Retaining evidence
  alone cannot retain the inventory. API/field tests inspect this relationship.
  The complete normalized source of both raw producers is hash-pinned along with
  the full eight-code manifest and real-branch severity tests. Any source change
  fails this conservative audit, including a new/computed code or unfamiliar
  construction the literal scan cannot understand. Updating hashes/manifest needs
  eligibility-impact review; this is a review alarm, not production code filtering.

Expected snapshot-write/file access catches are confined to capture IO. There is
no new catch around existing region import, Worker or Host. Neutral test projection
can express impossible metadata states but cannot produce evidence or a decision.
The ordinary typed-call threat model excludes hostile reflection/unsafe/process
compromise, as in the reviewed design. Hash identity is not authentication.

## Acceptance and validation

Tests cover real golden capture/hash/revision; golden plus unreferenced Error;
Failed/Fatal versus unavailable IO/format; missing/locked input; two identical but
foreign captures; null context/evidence; contradictory/unknown metadata and revisions;
explicit severity projection; Info/Warning versus Error/unknown blockers; duplicate
ordering under tr-TR; source replacement after capture; immutable collections/API;
empty reader location; bounded cleanup with successful/failed observations and
preserved unexpected read cause; full producer source/code/severity audit.

Local checks: Application Release build 0 warnings / 0 errors; Application 247/247 (28 new cases), core 1366/1366 and Contracts 94/94 PASS; git diff --check PASS. Exact-head Windows full CI is recorded in the PR. The existing Windows workflow remains the full build/regression/Worker
acceptance and real-corpus gate. Local Linux checks do not claim net48/SOLIDWORKS
physical acceptance. No physical test was performed.

## Review and remaining gates

Risk triggers: trust/fail-closed YES (new actual evidence issuer and decision).
Wire/serialization NO; shared enum numeric values NO (new enums are internal
Application-only); IPC NO; persistence NO; production UI entry NO.
Independent Senior review of the exact implementation head is required BEFORE
merge. #115 approval covers design only. No implementation approval is inferred.

M2 remains OPEN: workshop sampling, detailed cause separation, consumer validation,
coverage and header/unit policy/design/implementation are not completed. Existing
v1.0/v1.1 and SourceLinkMatched meanings and runtime paths are unchanged. This
slice has no product consumer and releases no feature/time/cost calculation result.
