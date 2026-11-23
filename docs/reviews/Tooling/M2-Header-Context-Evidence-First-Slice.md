# M2 header/context unit observation implementation — first slice

Date: 2026-11-23 Europe/Istanbul. Class: NEW BUILD, consumer-free Application slice.
Start: `542feb9a5e8f91bf0fa3bd456b8d35a988459fac`.
Branch: feature/M2-header-context-evidence. State: IMPLEMENTED, REVIEW/PRE-MERGE
ACCEPTANCE PENDING. M2 OPEN; N1 bounded closure delivered in #124.
Owner: Mehmet İlkay Kınacı.

## Build justification and scope

BUILD JUSTIFIED: #122/#123 design/policies merged; existing path header API cannot
provide same-private-capture evidence and global StepUnitResolver cannot preserve
context assignments/unsupported conversion units. This slice adds neither an
evaluator nor a composite readiness decision. Legacy connector APIs, inventory
policy A, manifest and current callers/Host/Worker/wire remain unchanged.

Only CaptureFromFile creates bound InventoryEvidence. Its private constructor
now produces immutable HeaderUnits observations from a new non-writable stream
over exactly the already-hashed private bytes and the same inventory. No temp file,
reopen or retained source byte array; no production consumer uses the new data.
Session.Owns applies to the encompassing evidence object: standalone neutral
observations cannot mint evidence/decisions or establish same-session ownership.
The new classes' internal constructors are neutral observation/test values, not
independent trusted proof issuers. Future composite gate must verify ownership of
the encompassing evidence before considering them; no caller-created neutral
snapshot is sufficient. This concrete issuance choice requires Senior review.

Header/unit producer assembly identity/version/info/hash is captured separately
from the connector reader revision. Both require file-backed assembly provenance;
failure remains unavailable at ReaderProvenance, no made-up revision. Source pins
cover the new producer, capture/issuer and reused StepParameterParser. Any pin
change requires explicit review. Policy revisions are separate constants (1/1).
No public metadata factory, new friend assembly, callback argument or consumer.

## Parsing and reasons

Application's structural boundary scanner protects comments, quoted/doubled quoted
strings and binary literals. Existing StepParameterParser parses parameter grammar.
The original StepHeaderReader and StepUnitResolver are untouched. Required header
records appear exactly once; required lists retain dollar/empty entries and reject
them. Schema raw content is ASCII [A-Z0-9_] and exactly AUTOMOTIVE_DESIGN; level
exactly 1. All 2;1/3;1 cases have separate UnsupportedImplementationLevel reason.
No escape/alias/qualified-schema inference. This narrow scanner additionally
requires structurally balanced exchange statements; it is not a general STEP validator.

Contexts are geometric 3D representation contexts, emitted in ordinal ID order.
Malformed/duplicate/missing identities or assignments remain explicit reasons,
never silently dropped. Unit observations preserve assignment order and every
assigned reference, separate required length/plane-angle and other dimensions.
Direct SI metre/centi/milli and unprefixed radian use exact rational scales.
Conversion-based units block immediately, including cyclic conversion definitions;
no factor traversal or silent base-SI substitution. Unsupported extra unit forms
remain reasons. Other solid-angle metadata is preserved as OtherDimension, never
dimensional readiness. Unsupported dimensions/roles are not coverage proof.

All observation reason codes have fixed Error severity in the new local manifest;
SupportedUnit/OtherDimension are classifications, not diagnostic severity changes.
The connector's existing eight diagnostic codes/severities remain unchanged.
No output means overall Ready: missing angle remains MissingAngleUnit regardless
of evaluator desire, and cone degree/missing-angle fixtures remain non-ready data.
Target selection, no-angle coverage exemption and normalization are not implemented.

## Validation and actual data profile

Local SDK 8.0.422 Linux Release: Application 308/308 (net +46), core 1366/1366,
Contracts 94/94; Worker 85/85 excluding Windows executable integration locally.
Release Application build 0 warnings/errors. Windows CI must cover full Host and
Worker including executable test. Final exact-head CI pending; local results do
not claim Windows or physical SOLIDWORKS acceptance. git diff --check required.

Tests include five goldens with both contexts and legacy header parity, actual
GE03B01 2;1 rejection, H2 structural cases, schema escapes/qualified/multiple/dollar
entries, exact SI scales, inch/degree, cone degree/missing-angle, same/different
contexts, missing/duplicate/unresolved assignment, unsupported prefix, unitless
context, cyclic conversion, BOM variants/invalid UTF8, actual source replacement,
producer-source pins and reason/severity/revision pins. No test-first failing
behavioral run is claimed: these are new capability acceptance tests.

[Profiles](evidence/M2-HeaderUnit-First-Slice/) use the actual capture entry via an
external observation-only reflection harness; no production API/test backdoor is
added. Hashes/build command/SDK/source pins identify the measured working diff;
base commit is not falsely declared to contain these new sources. Raw STEP/header
texts or geometry are not published. The historical probe source is included as
text; not an eligibility issuer and not part of production or CI.

14/14 observations available. Workshop 11 versions: no header or context-unit
blocking reasons. R01/R02 likewise none. R03 schema CONFIG_CONTROL_DESIGN, level 1:
UnsupportedSchema under the reviewed v1 list; its units have no blocking reasons.
Thus reference header non-ready 1/3, workshop 0/11 versions, unavailable 0 in those
sets. Two correlated families and converted STEP origin are retained; not a
population rejection estimate. Corrected BACK PLATE has five representation
contexts, not five bodies; no conflation with the separately observed four solids.
These are header/unit observations, not gate-complete or manufacturing readiness.

Festo: zero actual STEP inputs supplied, profiling unavailable. This mandatory
section 24 pre-merge evidence remains OPEN; do not report it as zero blockers or
waive it. Request actual Festo STEP inputs before completing that acceptance.
Test-plan Permission entry is a separate open follow-up from N1; owner's local-test
permission declaration exists, external redistribution is not inferred.

## Pre-merge gates and review

Independent final-diff Senior review required: capture issuance extension,
neutral-observation binding, producer revision, structural grammar and unit
classification must be reviewed. #123 policy approval is not implementation
approval. Festo profile and exact-head Windows CI must complete before merge;
no consumer/afterSourceCaptured production invocation is added. Existing source
replacement tests alone invoke the internal checkpoint. Initial N2 remains an
obligation for the eventual real consumer; A4 detailed consumer cause propagation,
N3/B6 measured performance and first Windows consumer real-file repeat remain open.

Risk triggers: trust/fail-closed YES (new evidence producer/binding); serialization
wire NO (neutral profile JSON is not protocol); shared enums NO; IPC NO; runtime
persistence NO; UI entry NO. No merge authorized by implementation self-review.
Reviewed/Closing: pending final-diff Senior review and delivery. M2 OPEN.

## Senior section 26 — accepted consumer-free implementation, conditional merge

Date: 2026-11-23 Europe/Istanbul. Independent review of
`6835ff2b9c3c4b748b8066c5b2ca9bc4cdba59df`, supplied by project owner in the
conversation. Decision 1: implementation/trust boundary ACCEPTED for this
consumer-free slice; no implementation merge blocker. Reviewer checked same-byte
capture, immutable observation issuance in the private InventoryEvidence constructor,
three source pins, 46 tests, H1/H2, per-context units, 17 reasons/severity/revisions
and unchanged legacy APIs/inventory policy. Reviewer did not verify CI #265.
Implementation session separately verified that exact-head run 45126890277/job
137533166082 succeeded: Release 0 warnings/errors, 2025/2025 regression, Worker5/5,
targeted3/3, controls6/6, corpus checks. No physical acceptance is claimed.

Decision 2: merge permitted only after Festo profile and recorded conditions:
same reviewed producer source hashes, recorded Application assembly hash, local
usage permission record, every observed reason reported without policy broadening,
and verified final-head CI. If Festo exposes 2D/parametric-context classification
(B2), resolve its policy classification BEFORE merge, not as a silent tolerance.
Festo inputs/permission/profile remain pending; PR stays draft and unmerged.

Owner of every follow-up below: Mehmet İlkay Kınacı; implementation assistant
maintains technical evidence. Conditions do not imply delivered fixes:

| Item | Disposition and trigger |
| --- | --- |
| B1 header attribution/allocation | OPEN, fix BEFORE first consumer. Stop scan at HEADER ENDSEC; DATA structural faults must not become HeaderMalformed. Include malformed-DATA/valid-header test and retain same-byte/BOM behavior. |
| B2 2D contexts | OPEN Festo profiling check; current producer accepts geometric dimension 3 only. Other context roles can receive ContextMalformed/missing-unit reasons. Decide classification before merge if observed; no widening accepted by this review. |
| B3 short component body | OPEN, fix BEFORE first consumer. Guard a body consisting only of '(' before slicing; test actual capture path so malformed source cannot unexpectedly prevent inventory evidence. |
| B4 other dimensions | Documented conservative limit: beyond length/plane angle, only the existing recognized solid-angle metadata path is retained without a blocking reason. Other forms can be UnsupportedUnit, stricter than the policy's general separate-dimension recording. This is not a claim those files are malformed; no silent relaxation. |
| C1 neutral observation construction | OPEN, close BEFORE composite gate/first consumer. Restrict observation construction and prove by reflection test that the gate reads header/unit data only from an encompassing InventoryEvidence whose session ownership it has verified. Current neutral producer can accept unrelated inventory; no consumer may trust its standalone return. |
| U1 cone angle | OPEN, implement in coverage gate BEFORE angle-bearing geometry is used. Evaluator-only angle waiver cannot override cone/context angle requirement. |
| R03/AP203 | OPEN v2 candidate before expanding schema list; review CONFIG_CONTROL_DESIGN support or select an explicitly compatible additional reference. Current R03 rejection stays visible. |
| Windows profile repeat | At first Windows consumer acceptance with N1 platform follow-up; no Linux profile is relabeled Windows evidence. |

Evidence clarification: reviewer narrative mentions four BACK PLATE contexts, but
committed profile JSON contains FIVE IDs: #13379, #14404, #21251, #22408, #3878.
The separately observed four solid bodies do not establish one-to-one context/body
mapping. Keep the five-context observation unchanged; no body ownership inference
or silent rewrite to four. R03 remains CONFIG_CONTROL_DESIGN/UnsupportedSchema.

This addition records received acceptance and conservative limits only. Source
pins and measured implementation are unchanged; no reviewed behavior is broadened.
All six review triggers No for this factual reconciliation diff, while the
implementation's trust trigger and applicable review remain as recorded above.
N1 is closed only in its existing bounded scope; N2/A4, N3/B6 and M2 stay OPEN.
