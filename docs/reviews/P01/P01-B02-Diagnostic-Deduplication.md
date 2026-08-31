# P01-B02 — Diagnostic Deduplication

## 1. Record

- Review date: 2026-08-31
- Classification: HARDEN / VERIFY-FIRST
- Branch: `main`
- Start commit: `8d13fdb6833137c43be563880d4ef2b78e91e822`
- Reviewed HEAD: `1ea1a8aa0b55b1e199c464f25302104db1aae599`
- Implementation commit: `1ea1a8aa0b55b1e199c464f25302104db1aae599`
- Outcome: PASS

## 2. Objective

Prevent equivalent mapping diagnostics from being returned more than
once while preserving deterministic first-seen ordering and distinct
diagnostic contexts.

No STEP semantic normalization, geometry correction, topology mutation,
parser expansion, mapping-authority duplication, cache expansion, or
signed/spindle toroidal interpretation was authorized.

## 3. Verify-First Findings

The existing diagnostic flow was inspected before product-code
mutation.

The inspection established that:

- `MappingContext` records diagnostics produced during a mapping
  operation;
- its checkpoint operations depend on insertion count and order;
- `MappingResult<T>` defensively copies diagnostics but did not
  deduplicate equivalent entries;
- nested STEP mappers propagate child result diagnostics toward the
  final mapping result;
- successful canonical values are cached, but failed mapping paths can
  be reached repeatedly through different parent references;
- `StepImportApplicationService` publishes
  `mappingResult.Diagnostics`;
- deduplicating only `MappingContext` would not guarantee deduplicated
  application output;
- general connector and inventory duplicate detection serves a
  different validation purpose and was not an appropriate mutation
  point.

The verified intervention point was therefore the
`MappingResult<T>` construction boundary.

## 4. Deduplication Semantics

A mapping diagnostic is considered equivalent when all of the following
values match:

- diagnostic code;
- severity;
- message.

The first diagnostic for an equivalent key is retained. Later
equivalent diagnostics are omitted.

A `HashSet<DiagnosticKey>` provides key membership checks while a
separate ordered list preserves deterministic first-seen ordering.

The implementation does not use unordered grouping as the returned
sequence.

## 5. Source and Context Preservation

`MappingDiagnostic` does not currently contain a separate structured
source-entity field. Existing STEP mapping diagnostics encode source
identity and mapping context in their messages.

Using code, severity, and complete message as the identity therefore
provides the required current behavior:

- equivalent diagnostics for the same encoded source and context are
  deduplicated;
- the same code for different source entities remains distinct because
  the messages remain distinct;
- the same code and message with different severity remains distinct;
- the first diagnostic object is preserved;
- first-seen order is preserved.

Adding structured entity ID, parameter name, rejected value, or semantic
type fields is not part of this sprint. That work remains assigned to
P01-B03 — Entity-Scoped Semantic Failure.

## 6. Product-Code Change

Changed file:

- `src/MPC.Mapping/MappingResult.cs`

The change:

1. materializes and validates the supplied diagnostic sequence;
2. rejects null diagnostic entries exactly as before;
3. removes equivalent diagnostics using
   `Code + Message + Severity`;
4. preserves the first diagnostic instance and insertion order;
5. validates the final result state after deduplication;
6. preserves defensive copying and read-only result exposure.

`MappingContext`, its checkpoint semantics, and all STEP mapper
composition remain unchanged.

## 7. Test Change

Changed file:

- `tests/MPC.Tests/Mapping/MappingResultTests.cs`

Added tests verify that:

- equivalent diagnostics are reduced to one result entry;
- the first diagnostic object is retained;
- different source-entity messages are preserved;
- different severity values are preserved;
- interleaved duplicates do not change first-seen ordering.

Existing tests continue to verify:

- successful result state;
- failure result state;
- mandatory diagnostics for diagnostic-bearing states;
- defensive copying;
- read-only diagnostic exposure.

## 8. Validation Evidence

### Pre-mutation targeted baseline

- Passed: 37
- Failed: 0

### Post-mutation targeted validation

- Passed: 41
- Failed: 0

### Mapping regression

- `MPC.Tests`: 979 / 979
- Failed: 0

### Application regression

- `MPC.Application.Tests`: 6 / 6
- Failed: 0

### Final full regression

- `MPC.Tests`: 979 / 979
- `MPC.Contracts.Tests`: 35 / 35
- `MPC.Worker.Tests`: 15 / 15
- `MPC.Application.Tests`: 6 / 6
- `MPC.Host.SolidWorks.Tests`: 69 / 69
- Total: 1104 / 1104
- Failed: 0
- Skipped: 0

### Release build

- Warnings: 0
- Errors: 0

The final build and regression were executed against the exact content
committed as the implementation.

## 9. Scope Integrity

Implementation commit:

`1ea1a8aa0b55b1e199c464f25302104db1aae599`

Implementation files:

- `src/MPC.Mapping/MappingResult.cs`
- `tests/MPC.Tests/Mapping/MappingResultTests.cs`

Implementation diff:

- 2 files changed;
- 132 insertions;
- 4 deletions.

Final implementation file hashes before commit:

- `MappingResult.cs`:
  `8BFC488C0E3E96E3AD2C5D031822573125B49370AEEFD3580D0D3FD944BDA8C8`
- `MappingResultTests.cs`:
  `3D98924DBA29DAAFD07F033EFCC25B47C0515F746F3E05D061F608B9C8EA6928`

No unrelated product, test, contract, worker, host, application, topology,
geometry, or connector files were changed.

## 10. Explicitly Deferred Work

The following remain outside P01-B02:

- structured source-entity diagnostic fields;
- preservation of parameter name and rejected semantic value;
- conversion of semantic constructor exceptions into entity-scoped
  diagnostics;
- interpretation or normalization of negative toroidal major radii;
- signed/spindle toroidal canonical modeling;
- physical-worker revalidation of R03.

These remain governed by P01-B03 and P01-B04.

## 11. Acceptance Decision

P01-B02 acceptance criteria are satisfied:

- equivalent mapping diagnostics are deduplicated;
- deterministic first-seen order is preserved;
- the first diagnostic instance is retained;
- different source contexts are not merged;
- different severity values are not merged;
- `MappingContext` checkpoint behavior is unchanged;
- no unrelated scope was modified;
- targeted tests, mapping regression, application regression, Release
  build, and full regression are green;
- implementation commit traceability is complete.

Outcome: **PASS**.

The sprint is technically ready for documentation commit and remote
closure.
