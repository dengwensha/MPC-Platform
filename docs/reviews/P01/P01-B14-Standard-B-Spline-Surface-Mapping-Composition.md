# P01-B14 — Standard B-Spline Surface Mapping Composition

## Classification and Outcome

Classification: NEW BUILD, explicitly approved after current-code verification.
Outcome: IMPLEMENTED — TARGETED AND FULL REGRESSION GREEN.

## Traceability

- Branch: main
- Start Commit: 4d1c559bb4e4489dde99f0382cc2414d37f8bbe8
- Implementation / Reviewed HEAD: d270eaebe240ff444cbfd45ae56a29a9ed787cc6
- Record Creation Date: 2026-09-09T21:31:24+03:00
- Closing Commit: the commit introducing this document.
- Publication: verified separately after push.

## Authorization and Scope

SC01 did not authorize B14 implementation. The project owner separately
approved this standalone mapping scope after B14 verification.

Adds StepBSplineSurfaceWithKnotsGeometryMapper, implementing
IEntityMapper<StepBSplineSurfaceWithKnotsEntity, BSplineSurface3D>.

Composition is tested through the real semantic resolver and MappingContext.
No canonical resolver dispatch or solid-import runtime integration was added.

## Design

Control points resolve through the existing semantic reference path:
Resolve<StepCartesianPointEntity>, followed by a three-coordinate check
and direct Point3D construction.

Point3D remains a readonly record struct. Generic resolver and result
class constraints remain unchanged.

The mapper preserves U rows, V columns, repeated references, equal-valued
points in distinct positions, knots and multiplicities. Degree and
multiplicity conversions from long to int use checked casts.

STEP metadata remains owned by the semantic entity. No normalization,
sorting, deduplication, grid transposition or geometry repair is introduced.

Missing and wrong-type references preserve resolver diagnostics.
Invalid coordinate dimensions report MAP-STEP-BSPLINE-SURFACE-001 with
surface identity, point reference and zero-based U/V position.
Canonical construction or integer-range failures use
MAP-STEP-BSPLINE-SURFACE-002.

Failure results contain no partial surface. Diagnostic checkpoints isolate
each operation from earlier context diagnostics.

## Validation Evidence

Build and test results were executed locally by the project owner and
supplied in this session.

- RED: exit code 1; two CS0246 errors for the absent new mapper.
- Release solution build: PASS, 0 warnings / 0 errors.
- Targeted mapping tests: PASS, 18/18; skipped 0.
- Full regression: PASS, 1261/1261; failed 0; skipped 0.
- Baseline regression: 1243; delta: +18 B14 test cases.
- Staged whitespace check: PASS.
- Working tree after implementation commit: clean.

Commands:
- dotnet build src/MPC.sln -c Release
- dotnet test tests/MPC.Tests/MPC.Tests.csproj -c Release --no-build --no-restore --filter "FullyQualifiedName~StepBSplineSurfaceWithKnotsGeometryMapperTests"
- dotnet test src/MPC.sln -c Release --no-build --no-restore

Static implementation review found no scope deviation. This review was
performed by the implementation assistant, not an independent senior reviewer.

## Changed Files

- src/MPC.Mapping.Step/StepBSplineSurfaceWithKnotsGeometryMapper.cs
- tests/MPC.Tests/Mapping/Step/StepBSplineSurfaceWithKnotsGeometryMapperTests.cs
- This closure document.

## Boundaries and Product Status

Advanced-face integration, canonical resolver dispatch, rational/complex
surfaces, B-spline curves, evaluation, tessellation, healing and optimization
remain outside B14.

No new physical R01/R02/R03 execution was performed for B14.
Unit/composition success does not establish physical R03 compatibility.

CL01 assessment remains COMPLETE WITH FINDINGS.
Last physically validated corpus compatibility remains PARTIAL — 2/3.
Full R03 compatibility remains OPEN.

Further runtime integration requires its own verified scope and decision.
