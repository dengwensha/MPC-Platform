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
