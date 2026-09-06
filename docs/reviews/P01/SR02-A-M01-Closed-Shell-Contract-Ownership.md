# SR02-A - M-01 CLOSED_SHELL Contract Ownership

## Status

IMPLEMENTED - INDEPENDENT VERIFICATION PENDING

Engineering implementation and local validation are complete.
M-01 is not yet independently Verified Closed.
This record does not replace SR02 Final Independent Verification.

## Traceability

| Field | Value |
|---|---|
| Classification | HARDEN |
| Finding | M-01 - CLOSED_SHELL Contract Ownership |
| Review Date | 2026-09-06 |
| Branch | main |
| Start Commit | 5e50746af3cac3e6bf256d30cfc4b3b4060de3f3 |
| Implementation Commit | 91b4d5b22be0324b7daf38f9aa10ab71f598d5a2 |
| Implementation Parent | 5e50746af3cac3e6bf256d30cfc4b3b4060de3f3 |
| Documentation Commit | The Git commit introducing this record |
| Working Tree After Implementation Commit | Clean |
| Remote Publication | Pending verification at record creation |

The internal code review examined the pre-commit diff and complete new
test files. Reviewed file hashes were checked before staging.
The implementation commit identifies the delivered changes.
No independent review is claimed.

## Governing Input

P01-SR01-Senior-Review-Findings-Reconciliation.md classified M-01 as
HARDEN - CONFIRMED OPEN and required closure enforcement at the
CLOSED_SHELL boundary with deterministic diagnostic propagation.

## Ownership Decision

- Shell may represent an open or closed canonical shell.
- Shell.IsClosed retains its existing reference-based rule: every canonical
  edge must occur exactly twice across outer and inner boundary loops.
- StepClosedShellMapper checks IsClosed before either successful return path.
- An open shell returns Failure with Error diagnostic
  MAP-STEP-CLOSED-SHELL-004 and the CLOSED_SHELL source identity.
- StepManifoldSolidBrepMapper propagates failed child mapping diagnostics.
- Its former MAP-STEP-MANIFOLD-SOLID-BREP-001 closure check was removed.

## Dependency Contract and Limits

StepManifoldSolidBrepMapper relies on its injected
IEntityMapper<StepClosedShellEntity, Shell> to supply a non-null closed
shell on successful mapping. Production canonical composition supplies
StepClosedShellMapper.

The generic interface does not enforce this postcondition through its
type system. An alternative mapper that incorrectly reports success for
an open shell is no longer rejected by a defensive manifold closure check.

Solid constructor behavior is unchanged. This change does not establish
new orientation, connectedness, geometric validity, or complete manifold
validity guarantees. It enforces the existing Shell.IsClosed predicate.

## Acceptance Evidence

| Criterion | Evidence | Result |
|---|---|---|
| Reject open shell at CLOSED_SHELL boundary | Two direct ownership tests | PASS |
| Correct code, severity and source identity | Diagnostic assertions | PASS |
| Same diagnostic in result and context | Identity and count assertions | PASS |
| Isolate previous diagnostics | Direct and composed cases | PASS |
| Propagate through real shell and solid mappers | Two composed cases | PASS |
| Preserve valid closed-shell mapping | Tetrahedron and golden tests | PASS |
| Preserve duplicate and reference rejection | Existing mapper tests retained | PASS |
| Preserve topology and canonical composition | No changes in these paths | PASS |
| Release build | 0 warnings, 0 errors | PASS |
| Full regression | 1247 passed, 0 failed, 0 skipped | PASS |
| Independent current-HEAD verification | SR02 Final Independent Verification | PENDING |

The composed negative tests use real StepClosedShellMapper and
StepManifoldSolidBrepMapper instances with a stub face mapper and resolver.
They are not full STEP parser/application/worker import tests.

## RED and GREEN Evidence

Before production changes, both new direct tests failed with:
Expected: Failed; Actual: Succeeded.

After the ownership transfer, both passed. Four existing success tests
were changed to closed tetrahedron fixtures instead of open-face fixtures.

Two former manifold-owned closure tests were removed. Valid-pipeline
rejection and diagnostic isolation are covered at the new boundary and
through composed mapping tests. Protection against a contract-violating
injected shell mapper was intentionally removed as described above.

Expanded targeted regression: 23/23 passed.
Composed diagnostic propagation: 2/2 passed.

## Release and Full Regression

Build command: dotnet build src/MPC.sln -c Release

Test command: dotnet test src/MPC.sln -c Release --no-build --no-restore

| Project | Passed | Failed | Skipped |
|---|---:|---:|---:|
| MPC.Tests | 1116 | 0 | 0 |
| MPC.Contracts.Tests | 35 | 0 | 0 |
| MPC.Worker.Tests | 16 | 0 | 0 |
| MPC.Application.Tests | 11 | 0 | 0 |
| MPC.Host.SolidWorks.Tests | 69 | 0 | 0 |
| Total | 1247 | 0 | 0 |

These gates ran before the implementation commit. HEAD and the six-path
mutation inventory remained unchanged during validation.

Test reconciliation: previous 1245 - 2 removed + 4 added = 1247.

## Implementation Scope

Modified:

- src/MPC.Mapping.Step/StepClosedShellMapper.cs
- src/MPC.Mapping.Step/StepManifoldSolidBrepMapper.cs
- tests/MPC.Tests/Mapping/Step/StepClosedShellMapperTests.cs

Added:

- tests/MPC.Tests/Mapping/Step/StepClosedShellClosureOwnershipTests.cs
- tests/MPC.Tests/Mapping/Step/StepClosedShellDiagnosticPropagationTests.cs

Removed, recoverable from Git history:

- tests/MPC.Tests/Mapping/Step/StepManifoldSolidBrepClosureTests.cs

No topology, Solid constructor, canonical composition, healing, sewing,
tolerance policy, performance optimization, or STEP surface support
changes were introduced. M-02, M-03 and M-04 remain outside this scope.

## Remaining Governance Gates

- Verify remote publication of the implementation and documentation chain.
- Perform independent current-HEAD code verification.
- Do not infer M-01 Verified Closed from this record alone.
- Do not infer approval for P01-B14 or new PRODUCT scope.
- P01 final-planned-item and scope-freeze decisions are not supplied here.
