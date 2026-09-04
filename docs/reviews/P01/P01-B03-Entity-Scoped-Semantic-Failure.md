# P01-B03 — Entity-Scoped Semantic Failure

## Classification

`HARDEN / DIAGNOSTIC RELIABILITY / VERIFY-FIRST`

## Traceability

| Record | Value |
|---|---|
| Start commit | `bad5652d3e7472591f92b23c3f9574e849203f6b` |
| Implementation commit | `7243335f5df0c2733bbff6eedf269cc461713624` |
| Closing commit | Commit containing this closure report |
| Branch | `main` |
| Review date | `2026-09-01` |
| Closure date | `2026-09-04` |

## Objective

Replace the generic `APP-STEP-UNEXPECTED-001` result produced by a
known rejected STEP semantic value with a deterministic,
entity-scoped semantic diagnostic.

The diagnostic must preserve:

- STEP source entity ID
- STEP semantic entity type
- Parameter name
- Rejected value

## Locked Boundaries

- The rejected major radius is not converted to an absolute value.
- Exporter data is not silently normalized.
- Toroidal or spindle geometry support is not introduced.
- Worker controlled-failure behavior is preserved.
- `APP-STEP-UNEXPECTED-001` remains available for genuinely
  unexpected failures.
- Existing diagnostic deduplication and deterministic ordering
  behavior remains unchanged.
- Worker contract version remains `1.0`.
- No diagnostic contract DTO expansion is included.

## Implementation

The implementation introduces
`StepSemanticValueException` as the semantic transport exception.

`StepToroidalSurfaceEntityHandler` wraps the known
`ArgumentOutOfRangeException` for an invalid `majorRadius` and
preserves the source entity context.

`StepImportApplicationService` handles this exception before the
generic unexpected-failure path and emits:

`APP-STEP-SEMANTIC-001`

The worker receives the application result through the existing
contract transport without crashing.

## Files Changed

- `src/MPC.Application/Step/StepImportApplicationService.cs`
- `src/MPC.Connectors.Step/Semantics/StepSemanticValueException.cs`
- `src/MPC.Connectors.Step/Semantics/StepToroidalSurfaceEntityHandler.cs`
- `tests/MPC.Application.Tests/Step/StepImportSemanticFailureTests.cs`
- `tests/MPC.Tests/Connectors/StepToroidalSurfaceSemanticFailureTests.cs`
- `tests/MPC.Tests/Connectors/StepToroidalSurfaceSemanticReaderTests.cs`
- `tests/MPC.Worker.Tests/WorkerSemanticFailurePropagationTests.cs`

Implementation size:

- 7 files changed
- 425 insertions
- 30 deletions

## Red-State Evidence

Before implementation:

- Connector test expected `StepSemanticValueException` but received
  `ArgumentOutOfRangeException`.
- Application test expected `APP-STEP-SEMANTIC-001` but received
  `APP-STEP-UNEXPECTED-001`.

This verified both the connector boundary and application transport
path before the implementation was applied.

## Targeted Validation

| Validation | Result |
|---|---:|
| New connector semantic-failure test | 1/1 passed |
| Toroidal semantic-reader regression | 17/17 passed |
| Application semantic-failure test | 1/1 passed |
| Worker semantic propagation test | 1/1 passed |
| Application regression | 7/7 passed |

The existing zero and negative major-radius tests were reconciled to
expect the new semantic exception while preserving the original
`ArgumentOutOfRangeException` as the inner exception.

## Full Regression

Final Release regression:

| Test assembly | Passed | Failed | Skipped |
|---|---:|---:|---:|
| MPC.Tests | 980 | 0 | 0 |
| MPC.Contracts.Tests | 35 | 0 | 0 |
| MPC.Worker.Tests | 16 | 0 | 0 |
| MPC.Application.Tests | 7 | 0 | 0 |
| MPC.Host.SolidWorks.Tests | 69 | 0 | 0 |
| **Total** | **1107** | **0** | **0** |

## Release Build

Post-implementation-commit Release build:

- Result: Passed
- Warnings: 0
- Errors: 0
- Duration: `00:00:11.43`

The build was executed with HEAD at:

`7243335f5df0c2733bbff6eedf269cc461713624`

## R03 Physical Worker Acceptance

Input:

| Property | Value |
|---|---|
| File | `P01-R03-CurvedSurfaces.STEP` |
| Bytes | `14003088` |
| SHA-256 | `319DD894747CAF95AC1100C6283B55C014D40AEAC4ECCC00EF54648ADF9AEA12` |
| STEP schema | `CONFIG_CONTROL_DESIGN` |

Three independent physical worker executions produced:

| Run | Exit code | Status | Duration ms | Diagnostics | Code | Request ID preserved |
|---:|---:|---|---:|---:|---|---|
| 1 | 3 | Failed | 2451 | 1 | `APP-STEP-SEMANTIC-001` | Yes |
| 2 | 3 | Failed | 1380 | 1 | `APP-STEP-SEMANTIC-001` | Yes |
| 3 | 3 | Failed | 1418 | 1 | `APP-STEP-SEMANTIC-001` | Yes |

Deterministic diagnostic:

> STEP semantic entity '#2349' of type 'TOROIDAL_SURFACE' rejected parameter 'majorRadius' with value '-178.5'.

Observed behavior:

- Exactly one diagnostic was returned per run.
- `APP-STEP-UNEXPECTED-001` was not returned.
- The rejected value remained `-178.5`.
- Entity ID `#2349` was preserved.
- The worker returned a controlled failure and did not crash.
- Request identity was preserved.

## R01 and R02 Physical Regression

Three physical worker executions were performed for each part.

| Part | Runs | Exit code | Status | Diagnostics |
|---|---:|---:|---|---:|
| R01 | 3/3 | 0 | Succeeded | 0 |
| R02 | 3/3 | 0 | Succeeded | 0 |

Deterministic topology signatures:

| Part | Solid | Outer shell | Inner shell | Faces | Loops | Edges | Vertices |
|---|---:|---:|---:|---:|---:|---:|---:|
| R01 | 1 | 1 | 0 | 11 | 12 | 24 | 16 |
| R02 | 1 | 1 | 0 | 23 | 28 | 48 | 32 |

R02's `InternalBoundaries` name is a corpus label. It does not assert
the presence of a topological inner shell.

The five additional loops relative to face count are consistent with
face-level internal boundaries such as the central pocket opening and
four hole openings. These open features remain part of the outer shell;
therefore `InnerShell = 0` is expected.

`AUTOMOTIVE_DESIGN` is the STEP file's `FILE_SCHEMA` declaration, not
an application-generated topology or manufacturing classification.

## Out-of-Scope Review

A transient line-ending-only modification in
`WorkerExecutableIntegrationTests.cs` was inspected with an
ignore-EOL comparison and excluded before commit.

No out-of-scope file remained in the implementation commit.

## Acceptance Decision

| Criterion | Result |
|---|---|
| Known invalid STEP semantic value produces entity-scoped diagnostic | Passed |
| Entity ID, parameter, rejected value and semantic type are preserved | Passed |
| Expected semantic rejection avoids `APP-STEP-UNEXPECTED-001` | Passed |
| Duplicate diagnostic is not produced | Passed |
| Worker returns deterministic controlled failure | Passed |
| R01 and R02 behavior remains valid and deterministic | Passed |
| Targeted tests, Release build and full regression pass | Passed |
| Real R03 physical worker execution is verified | Passed |

## Closure

P01-B03 is accepted and closed.

The implementation converts the known R03 semantic rejection from a
generic unexpected failure into a deterministic, entity-scoped
controlled failure without normalizing the source value or expanding
geometry support.