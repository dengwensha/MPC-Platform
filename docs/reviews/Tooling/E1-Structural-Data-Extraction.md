# E1 — structural DATA selection in existing imports

Date: 2026-11-23 Europe/Istanbul. Owner: Mehmet İlkay Kınacı.
Start: 67ce97ec29e3be11bf50018d9f6e643d3b8e89bc.
Measured implementation: cc6dc5ed346f1139a25c20f434dc19c99fd81c6a.
Branch: fix/E1-structural-data-extraction. Refs #130, Senior §31.
State: implemented; independent final-diff Senior review and exact-head Windows CI pending.
E1 remains open until review, merge and main validation. M2 OPEN.

## One goal and affected behavior

The old substring search could read a DATA block inside a comment or header string,
then return a clean inventory while ignoring the real section. Both existing
StepImportApplicationService and ManufacturingRegionImportService share this reader;
this correction is prioritized independently of the future M2 consumer.

Selection now scans structural statements, skipping comments, doubled-quote strings
and binary literals. Only one unparameterized DATA section is selected. Multiple or
parameterized DATA, forbidden ANCHOR/REFERENCE/SIGNATURE sections, unfinished lexical
boundaries and trailing structural content are rejected with the EXISTING fatal
STEP_DATA_SECTION_NOT_FOUND result and no inventory. This deliberately changes
these inputs from first-section success to failure, without adding a catch or code.
Comments are separators, never identifier concatenation. DATA-body comments are
removed before record/reference parsing, so fake references inside comments cannot
become inventory records. Raw statements containing comments consequently change;
ordinary real/golden fixtures have identical full inventory output.

The selector keeps at most a 128-character statement prefix. The existing reader
still holds the input and selected DATA string; this is not a memory/performance
benchmark or a streaming-reader claim. The reader deliberately still accepts DATA
fragments and case variants used by the existing API. It is NOT full Part 21
conformance validation. Parenthesis/record semantics remain with the existing
inventory and semantic parser; partial-record diagnostics remain available.
Application's separate envelope/header evidence still owns complete exchange/header
policy. Neither a successful read nor InventoryClear establishes calculation readiness.

No new consumer, combined gate, DTO/wire/enum, Host, IPC, geometry repair, automatic
body selection, header/unit policy or severity policy change is included.

## Eight-code manifest audit

InventorySeverityManifestTests pins the new normalized reader source and unchanged
validator source, enumerates exactly the same eight codes and exercises their
severity branches. Reader normalized SHA-256:
547008BCB29C5F7AB60262B4EC09341675AA1E5B9997935D772F466C20BAD537.

| Severity | Codes |
| --- | --- |
| Fatal | STEP_FILE_NOT_FOUND, STEP_FILE_EXTENSION_UNSUPPORTED, STEP_FILE_READ_FAILED, STEP_DATA_SECTION_NOT_FOUND |
| Error | STEP_COMPLEX_ENTITY_COMPONENTS_NOT_FOUND, STEP_DATA_ENTITY_HEADER_UNRECOGNIZED, STEP_DUPLICATE_SOURCE_ENTITY_ID, STEP_MISSING_ENTITY_REFERENCE |

The fourth Fatal now also covers an ambiguous/unsupported section layout. The
message and severity are unchanged, but this expanded classification is explicit.
The reserved simple COMPLEX_ENTITY FormatException remains propagated. No model
invariant, validator source, header/unit producer or envelope source was changed.

## Red/green and current imports

Before correction, the initial focused matrix failed 19 of 20 cases (one passed),
including actual existing import service paths. After correction those 20 passed;
the final E1 matrix contains 30 passing cases. These include comment/header-string/
binary decoys, quoted/comment ENDSEC, forbidden/multiple/parameterized sections,
tails, missing closings, DATA fragment/case compatibility and preserved partial
record diagnostics. The committed original C01 fixture is linked into the tests.

Both existing import services are exercised with T001_PRISM containing comment,
header-string, body-comment and inline-comment decoys. They retain the real root
#63 and source face #8. Both reject multiple/parameterized DATA. Reader helpers
compare full serialized inventory and diagnostics for Read(path) and ReadCaptured.
HeaderPolicyV2Tests now expects blocked inventory for unsupported sections while
retaining separate envelope rejection. Historical #129 evidence is left intact.

Local .NET SDK 8.0.422/Linux: Application 430/430, Core 1366/1366; committed-source
Application Release build 0 warnings/errors. Windows full regression, Worker
acceptance and real-corpus controls must pass at the final PR head before merge.

## Bounded equivalence evidence

[Validation.json](evidence/E1-Structural-Data/Validation.json), the profiler source,
31 baseline summaries and two sets of 31 current summaries are committed. Full
geometry-bearing JSON is hashed in memory; no raw workshop/Festo CAD is committed.
All 15 real inputs were checked against the previous profile's input hashes.
There are 15 real files (R01–R03, 11 workshop versions, one unique Festo), five
goldens and 11 synthetic controls. Workshop versions/families are correlated;
this is not a claim of 15 independent production jobs or false-rejection frequency.
Existing local-use permission remains the basis; no new distribution right is claimed.

Each current process creates a fresh reader/validator for each of two read routes.
The same private bytes are passed to ReadCaptured and hashed before/after the file
reads. All 31 inputs match across four current observations (124 observations),
including complete result/record/raw-statement/diagnostic JSON hashes. Both runs
record the same reader and Application DLL hashes. Build command, SDK, source pins
and exact measured code commit are recorded. The final documentation-only descendant
is source-identical; its CI DLL is not claimed byte-identical to the measured DLL.

The 15 real files and five goldens have unchanged full result hashes versus start.
C01 changes from fake #99 alone (1) to actual #1/#2 (2); the quoted DATA control
also reads the real content. Multiple/parameterized controls change from success
to Fatal failure. The reserved discriminator control records FormatException and
unavailable observation, never zero diagnostics/success. Four BOM encodings and
invalid UTF-8 are included. This comparison requires an existing readable file;
Read(path)'s file validation is not claimed equivalent to an absent stream.

This is new bounded Linux equivalence evidence for the changed reader, not an
expansion of the older N1 acceptance or Windows real-file proof. Windows real-file
repetition remains due before the first Windows consumer. No geometric eligibility
or arbitrary malformed-input completeness is inferred from hash equality.

## Review and remaining gates

| Senior trigger | Decision |
| --- | --- |
| Wire/serialization | No — transport and representation unchanged |
| Enum numeric values | No |
| Trust/fail-closed | Yes — existing import section selection and rejection changes |
| IPC | No |
| Persistence | No |
| UI entry | No — existing callers only |

Independent review of the final diff is required before merge. Review should confirm
structural selection in both existing paths, deliberate failure classification,
eight-code audit, bounded path/memory evidence and sufficiency of automated acceptance.

M2 remains open. Future combined gate: §29 reflection scope/constructor expansions,
U1 cone angle requirement, target/context/root and positive coverage proof. Optional
envelope DATA record-count comparison is defense in depth, not identity proof, and
is not implemented here. B2, A4, N2, N3/B6 and Windows repetition remain tracked.
Three-pass parsing cost must be measured with B6; no optimization claim is made.
