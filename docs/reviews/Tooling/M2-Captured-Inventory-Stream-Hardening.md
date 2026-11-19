# M2 captured inventory hardening — Senior section 21 B1-B5

Date: 2026-11-19 Europe/Istanbul. Class: HARDEN.
Start: `62a22a3ddc50a914d49eeed6d6276328d8d77b81`.
Branch: `fix/M2-captured-inventory-stream`.
Reviewed: `73aebc89454102aaf135b768e5c4a0dd9914531a` (Senior section 22).
Closing (implementation merge): `d6f6a01e29f5de0ddad973dc48a2e8686255f3aa`.
State: SENIOR-APPROVED AND MERGED bounded B1-B5 fix; M2 OPEN; no production consumer.
Owner: Mehmet İlkay Kınacı; implementation assistant maintains evidence.

## One goal and smallest boundary change

Prevent capture infrastructure failures from being attributed to STEP source
inventory and eliminate the private temp-file reopen/residual-data mechanism in
this isolated Application capture and its non-production measurement probe.
The prior file-only reader forced the duplicate on-disk copy; it has no stream
entry. The existing Read(path) behavior and eight diagnostic severities must stay
unchanged. This is bounded hardening, not a new evaluator or product integration.

Concrete choice for separate Senior review: expose ReadCaptured(Stream) on the
concrete StepDataEntityReader, leaving IStepDataEntityReader and all existing
callers unchanged. Both entries invoke the same private ReadContent parser.
ReadCaptured reads from current position, detects BOM with UTF-8 replacement
fallback exactly as File.ReadAllText, leaves caller stream open, and propagates
stream access failures to its owner. It cannot issue Application evidence.

Application privately captures original bytes once, hashes exactly that array,
and invokes the concrete reader/validator on a non-writable MemoryStream over it.
Neither the byte array nor stream is passed to callers or retained by evidence.
There is no generated STEP path, snapshot reopen, write, delete or cleanup fallback.
The measurement probe uses the same entry after verifying the captured hash.
Its JSON remains observation only, not eligibility evidence; six existing control
outcomes must remain unchanged. Old immutable measurement artifacts are historical
and are not rewritten to claim a new reader build.

## Bounded disposition proposed, not yet Senior-closed

| Finding | Proposed resolution / remaining limit |
| --- | --- |
| B1 attribution | No private file exists to be locked/quarantined and mislabeled as source evidence. SourceCapture, ReaderProvenance and InventoryParse failures carry separate Application-only FailureStage. Missing DATA is still genuine observed Failed/Fatal; actual source IO is unavailable. New stream IO exceptions propagate, not synthetic STEP_FILE_READ_FAILED. Old path reader behavior remains. |
| B2 byte identity | Hash and concrete parse consume the same privately owned array through a non-writable stream. Source replacement/deletion/recreation after capture cannot change either. No caller-provided bytes/reader/result or metadata evidence factory is introduced. |
| B3 private path | No generated private path can enter inventory diagnostics. Actual original source errors may still contain their meaningful source path in the capture cause. No global message redaction policy is silently introduced. |
| B4 residual STEP | This capture and measurement probe no longer write an additional STEP file, so their failed-delete residual mechanism is removed. This is not secure erasure of RAM/OS storage, nor remediation of existing importer temp files or a claim that every customer data-sharing agreement is satisfied. Other existing import paths are out of scope. |
| B5 tests | Test original-source mutation through actual CaptureFromFile, propagation of the same unexpected exception through that entry, stream failure propagation, all five golden path/stream parity and BOM/UTF-8 fallback. Temp-deletion and CreateNew-collision cases are superseded by removal of that mechanism, not claimed as passing deletion/collision tests. |
| B6 | Assembly hash still recomputed per capture; remains OPEN, no unmeasured cache. |
| A4 consumer | Empty reader location still controlled unavailable; FailureStage now distinguishes provenance. First consumer must emit the retained cause; no consumer implemented. |

A narrow internal payload-free Action checkpoint after source capture supports the
actual capture-entry fault/replacement tests. It receives no data/stream/hash/result
and has no return value. It executes outside expected IO catches: its exceptions
propagate. Returning normally cannot skip parsing, replace the producer or mint clean
evidence; tests mutate an erroneous source to clean after capture and still get
Blocked. All default callers omit it. This concrete test seam needs Senior review;
it is not a caller-supplied successful reader or a metadata evidence factory.

The pinned complete producer source hash changes because the reader gained the
stream entry and shared parser wrapper. The eight codes/severities and severity
mapping revision remain unchanged; no new diagnostic or downgrade is introduced.
Updating the pin in this PR is an eligibility-impact review item, not a test waiver.
No parsing algorithm, status interpretation or policy A rule is intentionally changed.

## Verification and delivery gate

Required: Release build; full regression including Windows Host/Worker; real corpus
and six measurement controls; parity/encoding/capture-fault tests; diff --check;
exact-head independent Senior review before merge. Local validation: Application 262/262 (net +15 cases), core 1366/1366, Contracts 94/94; measurement controls 6/6; Application Release 0 warnings/errors; diff --check PASS. The old source pin first failed alone (261 pass / 1 pin failure); after checking the unchanged eight-code/severity manifest, the new source hash was pinned and 262/262 passed. This is a producer-change audit demonstration, not a claim of a test-first reproduction of B1. Exact commit and Windows CI are recorded in the PR. No physical PASS is claimed. The isolated capture
has no SOLIDWORKS or product consumer; Windows automated validation exercises the
runtime change. Physical acceptance is not made a precondition here; if Senior
identifies a concrete uncovered runtime requirement, scope it explicitly before
merge. Existing separate SOLIDWORKS diagnostic follow-up remains OPEN.

Trust/fail-closed YES: byte identity, cause attribution and capture test seam.
Wire/serialization NO; shared enum values NO (failure stage is internal Application
only); IPC NO; persistence YES for removal of the private on-disk STEP lifecycle;
UI entry NO. These triggers require independent review of the final diff, even
though the change removes disk persistence. #117 approval does not cover this diff.
M2, workshop sampling, coverage/header/unit gates and consumer validation remain OPEN.


## Senior section 22 acceptance and remaining follow-ups

Date: 2026-11-19 Europe/Istanbul. Source: independent review supplied by project
owner in the working conversation; no GitHub review submission is fabricated.
Reviewer approved both decisions: B1-B5 addressed for the isolated capture and
measurement probe, evidence trust boundary preserved, merge allowed, no physical
acceptance required. Reviewer recomputed the new reader source pin, confirmed the
unchanged validator pin, eight diagnostic codes/severities, 21-line reader diff
and net +15 tests. No independent CI execution/verification by reviewer is claimed.
Implementation session separately verified Actions #249 run 44533217724 head_sha
exactly equals Reviewed above, conclusion success, and job 135591731675 logs:
Release 0 warnings/errors, 94+1366+262+171+86 = 1979/1979; Worker acceptance 5/5,
targeted 3/3, measurement controls 6/6, real-corpus steps success. Merge used an
expected-head lease. Main merge CI is separately verified in the delivery PR.

B1-B5 are CLOSED within these two capture paths after approved implementation
merge; no closure of temp behavior in other importers, general data erasure or
M2 is implied. Former proposed-fix wording above describes the reviewed change.
The prior A1 cleanup mechanism is superseded by removal of its extra file.

Owner of all remaining items: Mehmet İlkay Kınacı; implementation assistant
maintains technical evidence. Follow-ups are OPEN, not measured results:

| Item | Required evidence / trigger |
| --- | --- |
| N1 — real-input path/capture parity | At the NEXT measurement, compare Read(path) and ReadCaptured for R01/R02/R03; record new reader assembly/source hashes, source SHA256 and inventory/diagnostic comparison. Existing golden parity and CI probe observations are not this explicit comparison. |
| N2 — checkpoint usage | In the FIRST consumer PR, verify production calls omit afterSourceCaptured and preserve genuine same-session ownership. No consumer exists now. |
| N3 / B6 — assembly hash cost | Still hashes on each capture. Address only at the next measured performance work; no unmeasured cache or global session/inventory retention. |
| A4 consumer diagnostics | First consumer must carry Failure/FailureStage provenance or source/parse cause to diagnostics; NoEvidence alone does not explain the cause. |

Workshop sample remains 0. Measurement must preserve unavailable observations
and separate causes, never report unavailable counts as zero Error/Fatal or treat
SourceLinkMatched as overall eligibility. Header/unit, coverage, detailed-cause
separation and consumer verification remain OPEN. No physical test performed;
Senior explicitly waived an additional physical acceptance for this exact scope,
not for the separate existing SOLIDWORKS diagnostic follow-up.
This reconciliation changes no runtime behavior or policy; all new-diff review
triggers No for this factual record. Future consumer/capture boundary changes
still need applicable independent review. M2 remains OPEN.
