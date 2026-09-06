# P01-B12 — Standard B-Spline Surface Semantic Contract

## Classification

`NEW BUILD`

## Outcome

`PASS — IMPLEMENTED / SENIOR REVIEWED / FULL REGRESSION GREEN`

P01-B12 adds a lossless semantic contract for standalone
`B_SPLINE_SURFACE_WITH_KNOTS` STEP entities.

The sprint does not add canonical B-spline geometry or mapping composition.

## Traceability

| Field | Value |
|---|---|
| Branch | `main` |
| Start Commit | `37324d4c28a8b4be9a2b3e05c10e0928a613c406` |
| Implementation Commit | `fe37280f42586c3d1a989cdacc6340401ebce15f` |
| Closure Commit | This document's commit |
| Review Date | `2026-09-06` |
| Predecessor | `P01-B11 — R03 B-Spline and Bounded-Entity Resolution Discovery` |

The implementation commit is a direct child of the locked start commit.

## Build Justification

P01-B11 established that P01-R03 contains 696 standalone non-rational
`B_SPLINE_SURFACE_WITH_KNOTS` records.

Those records are active surface dependencies in the canonical topology path,
but the semantic handler catalog previously contained no handler for the
entity family.

P01-B11 therefore concluded:

`BUILD JUSTIFIED`

The minimum approved build boundary was standalone non-rational B-spline
surface semantics.

## Delivered Production Capability

P01-B12 adds:

- `StepBSplineSurfaceWithKnotsEntity`;
- `StepBSplineSurfaceWithKnotsEntityHandler`;
- `StepLogicalValue`;
- registration in `StepSemanticHandlerCatalog`.

The semantic entity preserves:

- source entity identity;
- source entity type;
- name;
- U degree;
- V degree;
- ordered rectangular control-point reference grid;
- surface form;
- U-closed logical state;
- V-closed logical state;
- self-intersection logical state;
- ordered U multiplicities;
- ordered V multiplicities;
- ordered U knots;
- ordered V knots;
- knot specification.

No source sequence is sorted, deduplicated, clamped, approximated, or otherwise
normalized.

## STEP Parameter Contract

A standalone `B_SPLINE_SURFACE_WITH_KNOTS` record is accepted only with the
standard 13-parameter layout:

1. name;
2. U degree;
3. V degree;
4. control-point list of lists;
5. surface form;
6. U-closed logical state;
7. V-closed logical state;
8. self-intersection logical state;
9. U multiplicities;
10. V multiplicities;
11. U knots;
12. V knots;
13. knot specification.

The handler distinguishes structural format failure from semantic value
failure.

## Logical-State Contract

The STEP `LOGICAL` values are represented without boolean collapse:

| STEP token | Semantic value |
|---|---|
| `.F.` | `StepLogicalValue.False` |
| `.T.` | `StepLogicalValue.True` |
| `.U.` | `StepLogicalValue.Unknown` |

Unsupported tokens remain deterministic format failures.

## Standard Surface Forms

The semantic contract recognizes and preserves:

- `PLANE_SURF`;
- `CYLINDRICAL_SURF`;
- `CONICAL_SURF`;
- `SPHERICAL_SURF`;
- `TOROIDAL_SURF`;
- `SURF_OF_REVOLUTION`;
- `RULED_SURF`;
- `GENERALISED_CONE`;
- `QUADRIC_SURF`;
- `SURF_OF_LINEAR_EXTRUSION`;
- `UNSPECIFIED`.

## Standard Knot Specifications

The semantic contract recognizes and preserves:

- `UNIFORM_KNOTS`;
- `QUASI_UNIFORM_KNOTS`;
- `PIECEWISE_BEZIER_KNOTS`;
- `UNSPECIFIED`.

## Semantic Invariants

The entity enforces the applicable standalone B-spline surface constraints
independently for U and V:

- degree is at least one;
- the control-point grid is non-empty and rectangular;
- every control-point row contains at least two references;
- the control-point count in each direction is at least degree plus one;
- knot and multiplicity lists each contain at least two entries;
- knot and multiplicity list sizes match;
- every multiplicity is positive;
- the multiplicity sum equals control-point count plus degree plus one;
- first and last multiplicities do not exceed degree plus one;
- interior multiplicities do not exceed degree;
- knot values are finite;
- consecutive distinct knot values are strictly increasing.

These checks implement the relevant
`constraints_param_b_spline` boundary without creating canonical geometry.

## Representative P01-R03 Coverage

Two real P01-R03 standalone record shapes are represented directly in tests:

| Source Entity | U Degree | V Degree | Grid Shape |
|---|---:|---:|---:|
| `#75` | 3 | 1 | 7 × 2 |
| `#371` | 3 | 1 | 11 × 2 |

The tests verify:

- grid row and column order;
- first and last control-point references;
- degree preservation;
- knot preservation;
- multiplicity preservation;
- surface-form preservation;
- knot-specification preservation;
- source entity identity.

This is representative semantic coverage. It is not a claim that all 696
records were mapped to canonical geometry.

## Failure Contract

Structural failures produce deterministic `FormatException` behavior,
including:

- incorrect parameter count;
- incorrect degree type;
- non-nested control-point data;
- non-reference control-point entries;
- invalid logical tokens;
- non-integer multiplicities;
- non-numeric knots.

Validly parsed but semantically inconsistent values produce
`StepSemanticValueException` carrying:

- source entity ID;
- source entity type;
- semantic parameter name;
- rejected source parameter text;
- original argument exception.

The Application catalog integration test verifies deterministic propagation as
`APP-STEP-SEMANTIC-001`.

## Immutability

The semantic entity performs defensive copies of:

- the outer control-point grid;
- every control-point row;
- U and V multiplicity collections;
- U and V knot collections.

Mutation of constructor inputs after entity creation cannot change the
published semantic state.

## Senior Review

The staged implementation received a senior-level review before commit.

Two findings were raised:

| Finding | Severity | Resolution |
|---|---|---|
| Degree-based multiplicity upper bounds were incomplete | Material | First/last and interior bounds added for both directions |
| Three new production files lacked final newline | Hygiene | Final newlines added before commit |

Additional standard verification confirmed:

- the complete B-spline surface-form enumeration;
- the complete knot-type enumeration;
- the 13-parameter standalone layout;
- the U/V control-grid direction;
- the multiplicity-sum rule;
- endpoint and interior multiplicity bounds;
- strictly increasing distinct knots.

The corrected targeted B-spline suite passed `42/42`.

Senior-review result:

`PASS — FINDINGS CORRECTED`

## Changed Files

### Production

- `src/MPC.Application/Step/StepSemanticHandlerCatalog.cs`
- `src/MPC.Connectors.Step/Semantics/StepBSplineSurfaceWithKnotsEntity.cs`
- `src/MPC.Connectors.Step/Semantics/StepBSplineSurfaceWithKnotsEntityHandler.cs`
- `src/MPC.Connectors.Step/Semantics/StepLogicalValue.cs`

### Tests

- `tests/MPC.Application.Tests/Step/StepBSplineSurfaceWithKnotsCatalogTests.cs`
- `tests/MPC.Tests/Connectors/StepBSplineSurfaceWithKnotsConstraintTests.cs`
- `tests/MPC.Tests/Connectors/StepBSplineSurfaceWithKnotsSemanticReaderTests.cs`

## Quality Evidence

### Targeted Semantic Reader and Constraint Tests

`PASS — 42/42`

### Targeted Application Catalog Test

`PASS — 1/1`

### Release Solution Build

`PASS — 0 warnings / 0 errors`

### Full Regression

| Test Project | Passed | Failed | Skipped |
|---|---:|---:|---:|
| `MPC.Tests` | 1085 | 0 | 0 |
| `MPC.Contracts.Tests` | 35 | 0 | 0 |
| `MPC.Worker.Tests` | 16 | 0 | 0 |
| `MPC.Application.Tests` | 11 | 0 | 0 |
| `MPC.Host.SolidWorks.Tests` | 69 | 0 | 0 |
| **Total** | **1216** | **0** | **0** |

The Host tests passed under the required `x64` platform.

## Acceptance Criteria

| Criterion | Result |
|---|---|
| Locked start commit independently verified | Passed |
| Standalone semantic entity added | Passed |
| Standard 13-parameter layout preserved | Passed |
| U/V degrees preserved | Passed |
| Ordered rectangular control grid preserved | Passed |
| U/V knot values preserved | Passed |
| U/V multiplicities preserved | Passed |
| Three-state STEP logical values preserved | Passed |
| Surface form preserved | Passed |
| Knot specification preserved | Passed |
| Source entity identity preserved | Passed |
| Standard U/V spline constraints enforced | Passed |
| Deterministic format failures covered | Passed |
| Entity-scoped semantic failures covered | Passed |
| Handler registered in the Application catalog | Passed |
| Representative R03 shapes verified | Passed |
| Defensive-copy immutability verified | Passed |
| Normalization audit clean | Passed |
| Canonical and mapping layers unchanged | Passed |
| Senior-review findings corrected | Passed |
| Release build completed with zero warnings and errors | Passed |
| Full regression completed at 1216/1216 | Passed |

## Out of Scope Confirmed

P01-B12 did not:

- add canonical `ISurface3D` B-spline geometry;
- change advanced-face mapping composition;
- interpret rational or complex B-spline entities;
- add B-spline curve support;
- add approximation or tessellation;
- alter the accepted toroidal compatibility policy;
- claim physical P01-R03 import success.

## Architectural Assessment

The implementation preserves the established dependency direction:

`MPC.Application`
→ `MPC.Connectors.Step`

No reverse dependency was introduced.

The STEP connector owns source-format parsing and semantic validation.
The Application layer owns composition through the semantic handler catalog.
Canonical geometry and mapping remain separate future gates.

## Product Assessment

P01-B12 removes the standalone B-spline surface semantic-resolution gap without
prematurely coupling the connector to a canonical geometry representation.

The result is a stable source-semantic boundary suitable for the next explicit
capability gate.

## Recommended Next Sprint

`P01-B13 — Canonical B-Spline Surface Geometry Contract`

Recommended classification:

`NEW BUILD`

Initial scope should be derived from the lossless semantic information
established here.

Mapping composition, rational complex interpretation, B-spline curves, and
physical P01-R03 success should remain outside that geometry-contract sprint.
