# M2 first isolated Application inventory boundary

Date: 2026-11-19 Europe/Istanbul. Class: NEW BUILD.
Start: `b2b0877f5bbab130903e19c7671cf7efb612e3ab`.
Branch: `feature/M2-application-inventory-evidence`.
Reviewed: `048604572cffd4c9bd2b78af6fae30f97fa7fb63` (Senior section 21).
Closing (implementation merge): `2c77cfc7fc8e8f56def49286bea2cafce1ac4ba9`.
State: MERGED ISOLATED FIRST SLICE; M2 OPEN; no product consumer.
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


## Senior section 21 acceptance and follow-up register

Date: 2026-11-19 Europe/Istanbul. Source: independent review text supplied by
project owner in this conversation, not an invented GitHub review submission.
Reviewer approved #117 at 0486045 and A1-A5 for this consumer-free slice, with no
blocking finding, conditional on CI #245 being green at the exact head. Reviewer
read code/tests and recomputed both pinned producer hashes and eight-code manifest;
reviewer did not verify CI or execute the 1964 tests. Implementation session checked
Actions run 44502659275 head_sha equals the full Reviewed SHA above and success,
and job 135486142632 logs: Release 0 warnings/errors, 94+1366+247+171+86 = 1964/1964,
Worker acceptance 5/5, targeted 3/3, inventory controls 6/6. Condition met before merge.
[Main CI #246](https://github.com/dengwensha/MPC-Platform/actions/runs/44528387573)
checks the exact implementation merge; its final result is recorded in the delivery PR.
No physical acceptance was performed or inferred.

Owner of all follow-ups below: Mehmet İlkay Kınacı; implementation assistant
maintains technical disposition and evidence. These are OPEN unless specified;
recording them is not a fix, a new waiver or permission to connect a consumer.

| Item | Finding and required disposition | Trigger |
| --- | --- | --- |
| B1 — infrastructure attribution (Medium) | Reader reopens the generated temp copy; antivirus lock/quarantine can become source read-failure evidence. Blocking is safe but attribution is wrong. Fix infrastructure/source cause separation and verify it before measurements or first consumer; do not interpret affected observations as workshop source-error incidence. | BEFORE further workshop measurement using this capture path and BEFORE first consumer; review measurement-tool applicability too |
| B2 — reopen window | Memory hash and re-read temp inventory have an intervening file window; exact hash-to-inventory byte identity across that window is not guaranteed. Reviewer suggested holding a read-only handle through reading. Treat as an input to the next design/fix, not a fix already applied or a newly ratified IO contract. | Resolve with B1 before using this path for measurement/first consumer |
| B3 — private path in diagnostic | Temp path can appear in file-not-found messages. Define correct source attribution/redaction with B1, retaining useful cause evidence. | Before measurement output or first consumer diagnostics expose this path |
| B4 — residual STEP content | Two failed deletes can leave customer STEP bytes. Windows temp cleanup is not reliable; OS housekeeping is not a deletion guarantee. Resolve residual handling against customer data-sharing requirements; current slice must not be used with customer data under an assumed cleanup guarantee. | Before customer/workshop data intake into this capture path, and before first consumer |
| B5 — capture-level test gaps | Actual deletion after successful capture, CreateNew name collision, and unexpected exception through CaptureFromFile are not behaviorally tested. Existing tests exercise the neutral cleanup helper/API; they do not close these integration gaps. | Next capture hardening with B1-B4, before measurement/first consumer |
| B6 — reader hash cost (Low) | Assembly bytes are hashed each capture. Profile if materially costly; any caching must preserve actual provenance and operation ownership. No cache is added here. | Next measured capture-performance work; not a merge blocker |
| A4 consumer diagnostics | Empty-location unavailable cause currently lives only in capture Failure. First consumer must preserve/emit that reason rather than collapse it into an unexplained NoEvidence. | First consumer design and acceptance |

The original “same private copy” design intent is qualified by B2 in this delivered
implementation. No claim of an immutable open snapshot, guaranteed deletion,
complete capture-level fault tests or infrastructure/source separation is made.
A1-A5 acceptance is limited to the isolated slice, as the reviewer explicitly states.
M2 remains OPEN, including detailed-cause separation, workshop sample (still 0),
consumer validation and complementary coverage/header/unit gates. This follow-up
record changes no runtime behavior or accepted policy; all new-diff risk triggers
are No for factual reconciliation. A future capture-boundary change needs its own
trust/fail-closed review and tests.


## B1-B5 hardening proposal

[Captured-inventory stream hardening](M2-Captured-Inventory-Stream-Hardening.md)
proposes eliminating the extra STEP copy in this isolated capture and measurement
probe. The register above remains the historical reviewed implementation's findings;
Senior section 22 approved the exact new diff at 73aebc89454102aaf135b768e5c4a0dd9914531a; it merged as d6f6a01e29f5de0ddad973dc48a2e8686255f3aa after CI #249 success. B1-B5 are closed only for the isolated capture and measurement probe; see the new record for N1-N3.
B6 and A4 consumer diagnostics remain OPEN. M2 remains OPEN.
