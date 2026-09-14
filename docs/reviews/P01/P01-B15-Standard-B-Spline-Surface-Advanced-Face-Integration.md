# P01-B15 — Standard B-Spline Surface Advanced-Face Integration

## Classification and Outcome

Classification: NEW BUILD, approved after current-code verification.
Outcome: IMPLEMENTED — TARGETED AND FULL REGRESSION GREEN.

## Traceability

- Branch: main
- Start Commit: 2b035876a4b4127c66e5a7c7915e924932308351
- Implementation Commit: b0f7276e9dbf9594a00a83aba9db54fce282f56b
- Validated implementation: the three files delivered by that commit.
- Record Creation Date: 2026-09-14T19:53:12+03:00
- Closing Commit: the commit introducing this document.
- Publication: verified separately after push.

## Verified Need and Authorization

B14 delivered standalone non-rational B-spline surface mapping.
At the B15 baseline, advanced-face surface dispatch did not recognize
StepBSplineSurfaceWithKnotsEntity and returned MAP-STEP-FACE-001.

The project owner explicitly approved B15 integration after verification.

## Delivered Scope

StepAdvancedFaceGeometryMapper now delegates standalone non-rational
B-spline surfaces to StepBSplineSurfaceWithKnotsGeometryMapper.

Existing four-, five- and six-argument constructor calls remain supported.
A seven-argument constructor supports explicit B-spline mapper composition.

StepCanonicalMappingOperation creates and supplies the B-spline mapper
through the production advanced-face composition path.

StepAdvancedFaceBuilderMapper and StepAdvancedFaceMapper required no
production changes. Their existing ISurface3D, orientation and boundary
responsibilities remain intact.

No canonical reference resolver dispatch was added.

## Validation Evidence

Builds and tests were executed locally by the project owner and supplied
in the session.

- RED: three existing-constructor dispatch tests failed as expected.
- Targeted GREEN: 12/12; failed 0; skipped 0.
- Release solution build: 0 warnings / 0 errors.
- Full regression: 1273/1273; failed 0; skipped 0.
- Previous baseline: 1261; delta: 12 B15 test cases.
- Staged whitespace check: PASS.

Regression breakdown:
- MPC.Tests: 1142
- MPC.Contracts.Tests: 35
- MPC.Worker.Tests: 16
- MPC.Application.Tests: 11
- MPC.Host.SolidWorks.Tests: 69, net48 x64

Commands used for final validation:
- dotnet build tests/MPC.Tests/MPC.Tests.csproj -c Release --no-restore --no-incremental
- dotnet test tests/MPC.Tests/MPC.Tests.csproj -c Release --no-build --no-restore --filter "FullyQualifiedName~StepBSplineAdvancedFaceIntegrationTests"
- dotnet build src/MPC.sln -c Release --no-incremental
- dotnet test src/MPC.sln -c Release --no-build --no-restore

## Test Coverage and Evidence Limits

Tests verify:
- B-spline dispatch through existing constructor variants;
- preservation of the asymmetric U/V grid and degree 3/1 surface data;
- canonical Face creation and both SameSense orientations;
- surface failures stopping before boundary traversal;
- preservation of control-point resolution diagnostics;
- isolation from earlier context diagnostics;
- production composition reaching a missing boundary after surface success;
- production propagation of an invalid control-point failure.

Successful face tests use a valid canonical triangle loop supplied by a
test boundary mapper. They do not demonstrate full STEP boundary import.

Production-operation tests deliberately use incomplete topology and expect
controlled failure. They verify wiring and traversal order, not solid success.

## Execution Deviation

An earlier package run built successfully but its targeted tests still
reported unsupported B-spline surfaces. After a backed-up restart and
explicit non-incremental builds, all targeted and regression tests passed.

Stale build artifacts were suspected; the exact mechanism was not proven.
No test expectations were weakened to obtain GREEN.

## Review

The implementation assistant performed static scope and composition review.
This is not an independent senior-review result.

## Changed Files

- src/MPC.Mapping.Step/StepAdvancedFaceGeometryMapper.cs
- src/MPC.Mapping.Step/StepCanonicalMappingOperation.cs
- tests/MPC.Tests/Mapping/Step/StepBSplineAdvancedFaceIntegrationTests.cs
- This closure document.

## Product Boundary

Rational/complex B-spline surfaces, B-spline curves, evaluation, tessellation,
healing and optimization remain outside B15.

No physical R01/R02/R03 revalidation was performed in B15.
Surface dispatch may expose downstream boundary-curve failures and alter
R03 diagnostics; no new physical diagnostic counts are asserted.

CL01 assessment remains COMPLETE WITH FINDINGS.
Last physically validated corpus compatibility remains PARTIAL — 2/3.
Full R03 compatibility remains OPEN.
