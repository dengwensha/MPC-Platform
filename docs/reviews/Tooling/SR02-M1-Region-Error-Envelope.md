# SR02 M1 region request error envelope

Class: HARDEN. Date: 2026-10-28 Europe/Istanbul.
Baseline: `3ba5f9c034305e3427b3aaa54aab836f57f34a6e`.
Independent Senior review REQUIRED before merge: wire/serialization, trust
boundary and interprocess response semantics triggers are Yes. Enum numeric
change, persistence and UI triggers are No. See PR #97 for final reviewed SHA,
RED/GREEN CI and review disposition; no independent approval is claimed here.

## Problem and corrected boundary

A syntactically valid JSON object can be identified as a region-source request
by existing routing rules, then fail typed DTO deserialization (for example
Requirement is the string "SourceLinkOnly" instead of numeric 1). Previously the
outer read catch wrote a legacy v1.0 WorkerOperationResult and exit 11.
The Host failed closed, but lost the v1.1 rejection semantics and correlation.

After routing to the region operation, JsonException or NotSupportedException
from DTO deserialization produces RegionSourceValidationResult with version
1.1, operation ValidateRegionSource, outcome RequestRejected (8), exit 2.
The catch encloses only deserialization, not import/processor execution.
No unexpected parser exception is broadly swallowed by this correction.

A string RequestId is retained exactly, including invalid/empty strings.
Missing, null or non-string RequestId yields null, never a generated ID.
This is extraction, not identity validation: Host correlation validation remains
authoritative, and unusable identity cannot pass its result validator.
No string-enum converter or permissive type coercion is introduced.

Malformed JSON that cannot be parsed/routed and legacy read failures still use
the established v1.0 read-error envelope and exit 11. An output write failure
still returns exit 12; there is no success or retry as a legacy result.
No DTO schema, enum number, version or source-link success criterion changes.

## Verification and limits

The new 18-case runner suite covers seven malformed field/type combinations;
seven missing/non-string/invalid string identity cases; rejected-result write
failure; and three unroutable/legacy read failures. Wrong typed fields with a
valid ID must produce an envelope accepted by the existing v1.1 result validator
as a correlated rejection, with no legacy Status/Diagnostics.
Invalid identities must remain unacceptable to that validator.

Existing v1.0 golden import and v1.1 golden/blocking/process tests remain part
of full regression. Standalone acceptance additionally executes five real Worker
process cases. New malformed cases directly exercise the production runner;
they are not claimed as new executable-subprocess tests or live SOLIDWORKS UI.

M2 MissingRegion diagnosis, M3 unreachable FaceMismatch documentation, M5
synthetic fixture cleanup and the H1/H2 record disposition are separate work.
M1 is implemented only after recorded RED and requires final full Release/CI
plus independent review before merge. Passing CI alone does not close review.
