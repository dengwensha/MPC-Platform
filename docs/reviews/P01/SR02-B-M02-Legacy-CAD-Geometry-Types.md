# SR02-B - M-02 Legacy CAD Geometry Types

## Status

IMPLEMENTED - INDEPENDENT VERIFICATION PENDING

Engineering cleanup and local validation are complete.
M-02 is not yet independently Verified Closed.
This record does not replace SR02 Final Independent Verification.

## Traceability

| Field | Value |
|---|---|
| Classification | CLEANUP |
| Finding | M-02 - Legacy CAD Geometry Types |
| Review Date | 2026-09-06 |
| Branch | main |
| Start Commit | f0912e3b8065755cb296bce1e01f2a8e287a684f |
| Reviewed Source Baseline | f0912e3b8065755cb296bce1e01f2a8e287a684f |
| Implementation Commit | 3faf77af15a1d7255395075db8c6801039d4582e |
| Implementation Parent | f0912e3b8065755cb296bce1e01f2a8e287a684f |
| Documentation Commit | The Git commit introducing this record |
| Working Tree After Implementation Commit | Clean |
| Remote Publication | Pending verification at record creation |

Complete source files, the obsolete test file, project files and
repository reference-search output were reviewed before removal.
The implementation contains only the four reviewed whole-file deletions.
No independent review is claimed.

## Governing Input

P01-SR01-Senior-Review-Findings-Reconciliation.md classified M-02 as
CLEANUP - CONFIRMED OPEN.

It requires an explicit retain/remove decision, removal of obsolete
tests when applicable, and regression evidence for supported behavior.

## Current-Code Evidence

At the start commit:

- CadPoint3 and CadVector3 are coordinate-storage record structs.
- CadAxis3 composes CadPoint3 and CadVector3.
- Production references are limited to these declarations and composition.
- The only test consumer is CadGeometryValueTests, containing four Facts.
- Repository searches found no other consumers of the types or their
  MPC.Connectors.Snapshots.Geometry namespace outside documentation.
- Inspected project files require no explicit compile-item changes.

This revalidates the finding against the current source baseline.

## Disposition

REMOVE.

The three unused legacy types and their four storage-only tests are
removed. No replacement types or migration layer are introduced.

These are public types, so removal reduces the public API surface.
The no-consumer evidence is repository-scoped; external consumers were
not audited, and external source or binary compatibility is not claimed.

## Implementation Scope

Removed, recoverable from Git history:

- src/MPC.Connectors/Snapshots/Geometry/CadPoint3.cs
- src/MPC.Connectors/Snapshots/Geometry/CadVector3.cs
- src/MPC.Connectors/Snapshots/Geometry/CadAxis3.cs
- tests/MPC.Tests/Connectors/CadGeometryValueTests.cs

Implementation diff: four files removed, 83 deletions, no additions.

No project references, canonical geometry, topology, STEP composition,
application, worker or host implementation changes were made.
Analyzer enforcement and MPC.Foundation disposition remain outside
this sprint.

Historical review records are preserved as evidence of their baselines.

## Acceptance Evidence

| Criterion | Evidence | Result |
|---|---|---|
| Explicit retain/remove decision | REMOVE, justified by current-code usage | PASS |
| Remove unused types | Three complete production files deleted | PASS |
| Remove obsolete tests | Four Facts removed with their sole test file | PASS |
| No remaining code/configuration references | Repository text search excluding docs | PASS |
| Exact implementation scope | Four deletions, 83 lines, no additions | PASS |
| Release build | 0 warnings, 0 errors | PASS |
| Retained regression suite | 1243 passed, 0 failed, 0 skipped | PASS |
| Clean state after implementation commit | Git status verification | PASS |
| Independent current-HEAD code verification | SR02 Final Independent Verification | PENDING |

Targeted verification is the absence of legacy type, test-class and
namespace references outside documentation, together with exact
deletion-scope verification.

Full regression supports preservation of covered repository behavior.
It is not a claim about untested behavior or external consumers.

## Release and Full Regression

Build command: dotnet build src/MPC.sln -c Release

Build result: PASS - 0 warnings, 0 errors.
Elapsed time: 00:00:22.59.

Test command: dotnet test src/MPC.sln -c Release --no-build --no-restore

| Project | Passed | Failed | Skipped |
|---|---:|---:|---:|
| MPC.Tests | 1112 | 0 | 0 |
| MPC.Contracts.Tests | 35 | 0 | 0 |
| MPC.Worker.Tests | 16 | 0 | 0 |
| MPC.Application.Tests | 11 | 0 | 0 |
| MPC.Host.SolidWorks.Tests | 69 | 0 | 0 |
| Total | 1243 | 0 | 0 |

Test reconciliation against SR02-A:

- MPC.Tests: 1116 - 4 = 1112.
- Full suite: 1247 - 4 = 1243.
- Other test-project totals are unchanged.
- No retained tests were modified or skipped.

The four removed tests are:

- CadPoint3_StoresSourceValuesWithoutModification
- CadVector3_AllowsNonNormalizedDirection
- CadVector3_AllowsZeroVector
- CadAxis3_StoresOriginAndDirection

Validation ran on the cleanup working tree before the implementation
commit. HEAD remained at the start commit during validation.
The four-deletion scope was checked before staging and committing.

## Remaining Governance Gates

- Verify remote publication of the implementation and documentation chain.
- Perform SR02 Final Independent Verification against the resulting HEAD.
- Do not classify M-02 as Verified Closed from this record alone.
- M-03 and M-04 remediation remains outstanding.
- This sprint does not approve P01-B14 or new PRODUCT scope.
- P01 final-planned-item and scope-freeze decisions are not supplied here.
