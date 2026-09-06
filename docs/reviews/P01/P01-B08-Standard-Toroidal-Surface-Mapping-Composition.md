# P01-B08 — Standard Toroidal Surface Mapping Composition

## Classification

`[NEW BUILD / STEP MAPPING COMPOSITION]`

## Review Date

2026-09-06

## Traceability

- Branch: `main`
- Start Commit: `34e06441b2134ecd2453b595212e779a9eb327e6`
- Implementation Commit: `586f4d4b1f583d4a7c5be0a54c5641e72c7f1518`
- Closing Commit: commit containing this report

## Objective

Map the standard positive-radius STEP `TOROIDAL_SURFACE` semantic entity to
the canonical `Torus3D` geometry contract.

Compose that mapper into the real `ADVANCED_FACE` surface mapping path while
preserving the P01-B07 degenerate-torus behavior and leaving signed
exporter-specific toroidal semantics unresolved.

## Starting Evidence

At the start of P01-B08:

- `main` matched `origin/main`;
- the worktree was clean;
- HEAD was the P01-B07 closing commit;
- `StepToroidalSurfaceEntity` already represented standard positive-radius
  STEP toroidal semantics;
- `Torus3D` already represented the canonical regular-torus contract;
- `Torus3D` required `majorRadius > minorRadius`;
- no `StepToroidalSurfaceGeometryMapper` existed;
- `StepAdvancedFaceGeometryMapper` supported the P01-B07
  `StepDegenerateToroidalSurfaceEntity` subtype but did not support the base
  `StepToroidalSurfaceEntity`;
- a regular toroidal surface therefore entered the unsupported-surface
  diagnostic path;
- `StepCanonicalReferenceResolver` did not represent the general face-surface
  composition path.

P01-R03 contains 120 `TOROIDAL_SURFACE` entities:

- 108 standard positive-radius regular torus entities;
- 12 exporter-specific signed variants with `majorRadius = -178.5`.

Only the standard positive-radius form was included in P01-B08.

## Architecture Decision

A dedicated `StepToroidalSurfaceGeometryMapper` was introduced.

The production mapping route is:

1. `StepAdvancedFaceEntity`;
2. `StepAdvancedFaceGeometryMapper`;
3. `StepToroidalSurfaceEntity`;
4. `StepToroidalSurfaceGeometryMapper`;
5. `Torus3D`;
6. `StepAdvancedFaceBuilderMapper`;
7. canonical `FaceBuilder`.

`StepCanonicalMappingOperation.Create` explicitly constructs the regular
toroidal mapper and supplies it to `StepAdvancedFaceGeometryMapper`.

`StepCanonicalReferenceResolver` was intentionally not changed because
advanced-face surface composition resolves the semantic surface through
`StepAdvancedFaceGeometryMapper`.

This preserves a single face-surface composition path.

## Dispatch Ordering

`StepDegenerateToroidalSurfaceEntity` derives from
`StepToroidalSurfaceEntity`.

The advanced-face type-pattern dispatch therefore preserves this order:

1. `StepDegenerateToroidalSurfaceEntity`;
2. `StepToroidalSurfaceEntity`.

This prevents a standard degenerate entity from being captured by the base
regular-toroidal branch.

The targeted B07 protection test confirmed that a degenerate toroidal surface
continues to map to `DegenerateTorus3D`.

## Constructor Compatibility

The existing four-dependency `StepAdvancedFaceGeometryMapper` constructor was
preserved.

The existing five-dependency constructor accepting
`StepDegenerateToroidalSurfaceGeometryMapper` was also preserved.

Both constructors delegate to the new six-dependency constructor and create a
default `StepToroidalSurfaceGeometryMapper`.

The six-dependency constructor permits explicit composition at
`StepCanonicalMappingOperation.Create`.

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
- maps the coordinate-system origin to `Torus3D.Center`;
- maps the coordinate-system Z axis to `Torus3D.AxisDirection`;
- supplies `MajorRadius` directly to the canonical constructor;
- supplies `MinorRadius` directly to the canonical constructor;
- returns the canonical `Torus3D` as an `ISurface3D` through the real
  advanced-face mapping chain.

The mapper does not reinterpret the semantic subtype and does not infer any
signed-radius meaning.

## Canonical Radius Contract

`StepToroidalSurfaceEntity` validates that both radii are finite and positive.

`Torus3D` additionally requires:

`majorRadius > minorRadius`

The mapper does not duplicate or weaken this canonical rule. It supplies the
source values directly to `Torus3D`.

When the relationship is invalid, canonical construction fails and the mapper
returns `MAP-STEP-TORUS-004`.

The verified invalid cases were:

| Major radius | Minor radius | Expected result |
|---:|---:|---|
| 178.5 | 229.5 | Controlled failure |
| 229.5 | 229.5 | Controlled failure |

The first case is a behavioral guard against radius swapping. The second is a
guard against accepting equality.

## Diagnostic Contract

The mapper introduces entity-scoped mapping diagnostics:

| Code | Meaning |
|---|---|
| `MAP-STEP-TORUS-001` | Invalid location coordinate count |
| `MAP-STEP-TORUS-002` | Invalid axis ratio count |
| `MAP-STEP-TORUS-003` | Invalid reference-direction ratio count |
| `MAP-STEP-TORUS-004` | Canonical torus or coordinate-system construction failed |

Unresolved STEP references continue to use the existing
`STEP_REFERENCE_RESOLUTION_FAILED` diagnostic.

Diagnostic checkpoints prevent pre-existing context diagnostics from being
returned as diagnostics belonging to the current mapping operation.

The B08 behavioral tests directly verify `MAP-STEP-TORUS-004` through the
advanced-face composition path.

## Test-First Evidence

### Initial Composition RED

The first regular-torus advanced-face test was created before the production
mapper.

The test compiled and ran but failed with:

- expected status: `Succeeded`;
- actual status: `Failed`.

This demonstrated that a standard regular torus entered the existing
unsupported-surface path.

### Mapper-Isolated RED

`StepToroidalSurfaceGeometryMapper` was then introduced without registering it
in `StepAdvancedFaceGeometryMapper`.

The mapping project built successfully, but the same composition test remained
RED.

At that stage:

- B07 degenerate composition passed;
- B08 regular composition failed.

This separated mapper existence from real composition registration.

### Strengthened Radius RED

Before production dispatch was connected, two canonical-radius cases were
added.

All three B08 cases failed:

- the valid regular torus expected `Succeeded` but received `Failed`;
- the `major < minor` case expected `MAP-STEP-TORUS-004` but received
  `MAP-STEP-FACE-001`;
- the `major == minor` case expected `MAP-STEP-TORUS-004` but received
  `MAP-STEP-FACE-001`.

This proved that neither valid regular mapping nor canonical radius validation
was reachable through `ADVANCED_FACE`.

After composition was added, the same three cases passed.

## Implemented Files

Production:

- `src/MPC.Mapping.Step/StepToroidalSurfaceGeometryMapper.cs`
- `src/MPC.Mapping.Step/StepAdvancedFaceGeometryMapper.cs`
- `src/MPC.Mapping.Step/StepCanonicalMappingOperation.cs`

Tests:

- `tests/MPC.Tests/Mapping/Step/StepToroidalSurfaceCompositionTests.cs`

Implementation commit scope:

- 4 files changed;
- 589 insertions;
- 2 deletions.

## New Test Coverage

P01-B08 added three test cases:

- one valid standard regular-torus composition case;
- one `majorRadius < minorRadius` rejection case;
- one `majorRadius == minorRadius` rejection case.

The tests verify:

- real `ADVANCED_FACE` semantic surface resolution;
- mapping to exactly `Torus3D`;
- center preservation;
- placement-axis preservation;
- major-radius preservation;
- minor-radius preservation;
- successful results contain no diagnostics;
- canonical radius failure propagation;
- `MAP-STEP-TORUS-004`;
- no radius swapping;
- no acceptance of equal radii.

The existing P01-B07 composition test independently protects the
`DegenerateTorus3D` dispatch path.

## Targeted Validation

The combined regular and degenerate toroidal composition gate passed:

- B08 regular torus cases: 3;
- B07 degenerate-torus protection cases: 1;
- Passed: 4;
- Failed: 0;
- Skipped: 0.

## Scope Audit

The pre-commit audit confirmed exactly four expected implementation files.

No changes were made to:

- `StepCanonicalReferenceResolver`;
- `Torus3D`;
- `DegenerateTorus3D`;
- `StepToroidalSurfaceEntity`;
- `StepDegenerateToroidalSurfaceEntity`;
- STEP semantic handlers;
- `StepSemanticHandlerCatalog`;
- topology;
- application contracts;
- worker contracts;
- host code.

## Normalization Audit

The implementation audit found no:

- `Math.Abs` conversion;
- `Math.Min` radius selection;
- `Math.Max` radius selection;
- radius swapping;
- sign conversion;
- negative-radius normalization;
- signed-value inference;
- `SelectOuter` inference.

`MajorRadius` and `MinorRadius` are supplied directly from
`StepToroidalSurfaceEntity` to `Torus3D`.

## Canonical Resolver Audit

`StepCanonicalReferenceResolver` remained unchanged.

Regular toroidal support was added only to the real advanced-face surface
composition route and its operation-root construction.

## Release Build

Release build result:

- Warnings: 0
- Errors: 0
- Duration: 00:00:03.75

## Full Regression

All test projects passed:

| Test project | Passed | Failed | Skipped |
|---|---:|---:|---:|
| MPC.Tests | 1033 | 0 | 0 |
| MPC.Contracts.Tests | 35 | 0 | 0 |
| MPC.Worker.Tests | 16 | 0 | 0 |
| MPC.Application.Tests | 8 | 0 | 0 |
| MPC.Host.SolidWorks.Tests | 69 | 0 | 0 |
| **Total** | **1161** | **0** | **0** |

The P01-B07 baseline was 1158 tests. P01-B08 added three test cases.

## Explicitly Out of Scope

P01-B08 does not include:

- conversion of the 12 signed SwSTEP toroidal surfaces;
- negative-radius normalization;
- `Math.Abs` treatment of exporter-specific values;
- signed-radius interpretation;
- signed-value-to-`SelectOuter` inference;
- conversion from signed regular torus to `DegenerateTorus3D`;
- canonical resolver registration;
- physical P01-R03 success;
- full P01-R03 worker validation;
- topology mutation;
- healing;
- sewing;
- coincidence processing;
- performance optimization;
- UI or host changes.

P01-R03 success is not an acceptance requirement because its remaining 12
signed `TOROIDAL_SURFACE` entities require a separate semantic decision.

## Acceptance Assessment

| Acceptance criterion | Result |
|---|---|
| Dedicated standard regular-torus mapper | PASS |
| Maps to canonical `Torus3D` | PASS |
| Preserves center and placement axis | PASS |
| Preserves major and minor radii | PASS |
| Applies `majorRadius > minorRadius` canonical contract | PASS |
| Rejects `majorRadius < minorRadius` without swapping | PASS |
| Rejects equal radii | PASS |
| Composed into real `ADVANCED_FACE` mapping | PASS |
| Explicitly composed at operation root | PASS |
| Degenerate dispatch precedes regular dispatch | PASS |
| B07 degenerate mapping remains unchanged | PASS |
| Canonical resolver remains unchanged | PASS |
| No normalization or radius swapping | PASS |
| Test-first RED evidence | PASS |
| Targeted toroidal composition gate | PASS — 4/4 |
| Release build | PASS — 0 warnings, 0 errors |
| Full regression | PASS — 1161/1161 |
| Exact four-file implementation scope | PASS |

## Outcome

**PASS**

P01-B08 provides deterministic, diagnostic-aware mapping composition for the
standard positive-radius STEP `TOROIDAL_SURFACE` representation.

Standard regular toroidal semantics now reach the canonical `Torus3D`
contract through the real `ADVANCED_FACE` mapping path.

The P01-B07 `DEGENERATE_TOROIDAL_SURFACE` path remains protected by ordered
type dispatch.

No claim is made for signed exporter-specific toroidal interpretation or
physical P01-R03 import compatibility.

## Recommended Next Sprint

`P01-B09 — Signed SwSTEP Toroidal Semantics Decision`

P01-B09 should determine the meaning of the 12 observed signed SwSTEP
`TOROIDAL_SURFACE` entities before any adapter, normalization or subtype
conversion is implemented.
