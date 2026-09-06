# P01-B07 — Standard Degenerate Toroidal Surface Mapping Composition

## Classification

`[NEW BUILD / STEP MAPPING COMPOSITION]`

## Review Date

2026-09-06

## Traceability

- Branch: `main`
- Start Commit: `fb841f865ab41ffef5880932cd63c741f36bf97c`
- Implementation Commit: `e642bf0c3ca64bec6468f8c908c677d4f23b227d`
- Closing Commit: commit containing this report

## Objective

Map the standard STEP `DEGENERATE_TOROIDAL_SURFACE` semantic entity
introduced in P01-B05 to the canonical `DegenerateTorus3D` geometry contract
introduced in P01-B06.

Compose that mapper into the real `ADVANCED_FACE` surface mapping path without
adding normal `TOROIDAL_SURFACE` support or changing canonical reference
resolution.

## Starting Evidence

At the start of P01-B07:

- `main` matched `origin/main`;
- the worktree was clean;
- HEAD was the P01-B06 closing commit;
- `StepDegenerateToroidalSurfaceEntity` already represented the standard STEP
  semantic form;
- `DegenerateTorus3D` already represented the canonical geometry contract;
- no degenerate toroidal surface geometry mapper existed;
- `StepAdvancedFaceGeometryMapper` supported plane, cylinder, cone and sphere;
- a degenerate toroidal surface therefore entered the unsupported-surface
  diagnostic path;
- `StepCanonicalReferenceResolver` did not represent the general face-surface
  composition path.

The targeted starting baseline passed 29 of 29 tests.

## Architecture Decision

A dedicated
`StepDegenerateToroidalSurfaceGeometryMapper` was introduced.

The production mapping route is:

1. `StepAdvancedFaceEntity`;
2. `StepAdvancedFaceGeometryMapper`;
3. `StepDegenerateToroidalSurfaceEntity`;
4. `StepDegenerateToroidalSurfaceGeometryMapper`;
5. `DegenerateTorus3D`;
6. `StepAdvancedFaceBuilderMapper`;
7. canonical `FaceBuilder`.

`StepCanonicalMappingOperation.Create` explicitly constructs the new mapper
and supplies it to `StepAdvancedFaceGeometryMapper`.

`StepCanonicalReferenceResolver` was intentionally not changed because the
real face-surface mapping route resolves the semantic surface through
`StepAdvancedFaceGeometryMapper`.

This avoids creating a second, unnecessary canonical surface-resolution path.

## Constructor Compatibility

The existing four-dependency `StepAdvancedFaceGeometryMapper` constructor was
preserved and delegates to the new five-dependency constructor.

The five-dependency constructor permits explicit composition at the operation
root while preserving existing callers and tests.

The new dependency is validated against `null` in the same manner as the
existing surface-mapper dependencies.

## Mapping Contract

The new mapper:

- resolves the referenced `AXIS2_PLACEMENT_3D`;
- resolves the placement location;
- resolves the optional axis direction;
- resolves the optional reference direction;
- uses positive Z as the default axis when the STEP placement omits an axis;
- constructs a deterministic canonical coordinate system;
- maps the coordinate-system origin to `DegenerateTorus3D.Center`;
- maps the normalized coordinate-system Z axis to
  `DegenerateTorus3D.AxisDirection`;
- preserves `MajorRadius`;
- preserves `MinorRadius`;
- preserves `SelectOuter`;
- returns the canonical `DegenerateTorus3D` as an `ISurface3D` through the
  advanced-face mapping chain.

No radius inference or subtype conversion is performed.

## Diagnostic Contract

The mapper introduces entity-scoped mapping diagnostics:

| Code | Meaning |
|---|---|
| `MAP-STEP-DEGENERATE-TORUS-001` | Invalid location coordinate count |
| `MAP-STEP-DEGENERATE-TORUS-002` | Invalid axis ratio count |
| `MAP-STEP-DEGENERATE-TORUS-003` | Invalid reference-direction ratio count |
| `MAP-STEP-DEGENERATE-TORUS-004` | Canonical degenerate torus or coordinate-system construction failed |

Unresolved STEP references continue to use the existing
`STEP_REFERENCE_RESOLUTION_FAILED` diagnostic.

Diagnostic checkpoints ensure that pre-existing context diagnostics do not
leak into the result of the current successful mapping operation and do not
replace the current failure diagnostic.

## Test-First Evidence

P01-B07 used two independent RED gates.

### Direct Mapper RED

The direct mapper tests were created before the production mapper.

The initial build failed with `CS0246` because
`StepDegenerateToroidalSurfaceGeometryMapper` did not exist.

This demonstrated that the tests exercised a genuinely absent mapping
capability.

Additional `CS0619` messages were compiler cascades caused by the unresolved
mapper type and disappeared when the production type was added.

### Composition RED

After the direct mapper passed, a separate advanced-face composition test was
created.

The test compiled and ran but failed with:

- expected status: `Succeeded`;
- actual status: `Failed`.

This proved that the direct mapper existed but was not yet registered in the
advanced-face surface dispatch.

After composition was added, the same test passed.

## Implemented Files

Production:

- `src/MPC.Mapping.Step/StepDegenerateToroidalSurfaceGeometryMapper.cs`
- `src/MPC.Mapping.Step/StepAdvancedFaceGeometryMapper.cs`
- `src/MPC.Mapping.Step/StepCanonicalMappingOperation.cs`

Tests:

- `tests/MPC.Tests/Mapping/Step/StepDegenerateToroidalSurfaceGeometryMapperTests.cs`
- `tests/MPC.Tests/Mapping/Step/StepDegenerateToroidalSurfaceGeometryMapperFailureTests.cs`
- `tests/MPC.Tests/Mapping/Step/StepDegenerateToroidalSurfaceCompositionTests.cs`

Implementation commit scope:

- 6 files changed;
- 1196 insertions;
- 5 deletions.

## New Test Coverage

P01-B07 added 16 test cases:

- 7 direct mapper contract cases;
- 8 failure and diagnostic cases;
- 1 advanced-face composition case.

The tests cover:

- `SelectOuter = true`;
- `SelectOuter = false`;
- center preservation;
- axis normalization;
- default positive-Z axis;
- major-radius preservation;
- minor-radius preservation;
- unresolved placement;
- unresolved location;
- unresolved axis;
- unresolved reference direction;
- invalid location coordinate count;
- invalid axis ratio count;
- invalid reference-direction ratio count;
- zero-length axis;
- pre-existing diagnostic isolation;
- null source;
- null context;
- advanced-face dispatch to `DegenerateTorus3D`.

## Targeted Validation

### Direct Mapper Gate

- Passed: 7
- Failed: 0
- Skipped: 0

### Composition Gate

The mapper, composition, advanced-face geometry and face-builder selection
passed:

- Passed: 19
- Failed: 0
- Skipped: 0

### Hardened Degenerate-Toroidal Gate

The broader name-based gate included the 16 new B07 tests together with
existing B05 semantic and B06 canonical contract coverage:

- Passed: 36
- Failed: 0
- Skipped: 0

## Scope Audit

The pre-commit audit confirmed exactly six expected implementation files.

No changes were made to:

- `StepCanonicalReferenceResolver`;
- `Torus3D`;
- `DegenerateTorus3D`;
- STEP semantic entities;
- STEP semantic handlers;
- handler catalogs;
- topology;
- application contracts;
- worker contracts;
- host code.

## Normalization Audit

The mapper contains no:

- `Math.Abs` conversion;
- sign conversion;
- radius swapping;
- radius reassignment;
- regular-torus inference;
- subtype reclassification.

`MajorRadius`, `MinorRadius` and `SelectOuter` are supplied directly from the
validated semantic entity to the canonical constructor.

## Normal Toroidal Surface Exclusion

The production audit found no mapper or dispatch registration for the base
`StepToroidalSurfaceEntity`.

Only `StepDegenerateToroidalSurfaceEntity` is supported by this sprint.

This preserves the prior decision that standard degenerate support must be
implemented before any investigation or treatment of exporter-specific signed
`TOROIDAL_SURFACE` values.

## Release Build

Release build result:

- Warnings: 0
- Errors: 0
- Duration: 00:00:26.67

## Full Regression

All test projects passed:

| Test project | Passed | Failed | Skipped |
|---|---:|---:|---:|
| MPC.Tests | 1030 | 0 | 0 |
| MPC.Contracts.Tests | 35 | 0 | 0 |
| MPC.Worker.Tests | 16 | 0 | 0 |
| MPC.Application.Tests | 8 | 0 | 0 |
| MPC.Host.SolidWorks.Tests | 69 | 0 | 0 |
| **Total** | **1158** | **0** | **0** |

The P01-B06 baseline was 1142 tests. P01-B07 added 16 test cases.

## Explicitly Out of Scope

P01-B07 does not include:

- normal `TOROIDAL_SURFACE` canonical mapping;
- signed-radius normalization;
- `Math.Abs` treatment of exporter-specific values;
- conversion between regular and degenerate torus types;
- canonical resolver registration;
- physical P01-R03 success;
- topology mutation;
- healing;
- sewing;
- coincidence processing;
- performance optimization;
- UI or host changes.

P01-R03 success is not an acceptance requirement because that file contains
exporter-specific signed `TOROIDAL_SURFACE` values rather than the standard
`DEGENERATE_TOROIDAL_SURFACE` semantic form delivered by this sprint.

## Acceptance Assessment

| Acceptance criterion | Result |
|---|---|
| Dedicated standard degenerate-torus mapper | PASS |
| Maps to canonical `DegenerateTorus3D` | PASS |
| Preserves center and normalized placement axis | PASS |
| Preserves major and minor radii | PASS |
| Preserves `SelectOuter` | PASS |
| Composed into `ADVANCED_FACE` mapping | PASS |
| Explicitly composed at operation root | PASS |
| Canonical resolver remains unchanged | PASS |
| Normal torus mapping remains out of scope | PASS |
| No normalization or radius swapping | PASS |
| Direct mapper RED evidence | PASS |
| Composition RED evidence | PASS |
| Direct mapper tests | PASS — 7/7 |
| Hardened degenerate-toroidal tests | PASS — 36/36 |
| Release build | PASS — 0 warnings, 0 errors |
| Full regression | PASS — 1158/1158 |
| Exact six-file implementation scope | PASS |

## Outcome

**PASS**

P01-B07 provides deterministic, diagnostic-aware mapping composition for the
standard STEP `DEGENERATE_TOROIDAL_SURFACE` representation.

The standard semantic entity now reaches the canonical `DegenerateTorus3D`
contract through the real `ADVANCED_FACE` mapping path.

No claim is made for normal toroidal mapping, signed exporter-specific
normalization, or physical P01-R03 import compatibility.
