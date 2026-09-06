# P01-B13 — Canonical B-Spline Surface Geometry Contract

## Classification

`NEW BUILD`

## Outcome

`PASS — IMPLEMENTED / SENIOR REVIEWED / FULL REGRESSION GREEN`

P01-B13 adds a STEP-independent canonical geometry contract for non-rational
B-spline surfaces.

The sprint builds directly on the lossless standalone
`B_SPLINE_SURFACE_WITH_KNOTS` semantic boundary delivered by P01-B12 without
introducing STEP, connector, mapping, evaluation, or tessellation concerns
into `MPC.Geometry`.

## Traceability

| Field | Value |
|---|---|
| Branch | `main` |
| Start Commit | `d80e522554a58bafa16a59abedd43093e33c729f` |
| Implementation Commit | `2cb9e31bf08e115e5ca5279759edc62106b953c7` |
| Closure Commit | This document's commit |
| Review Date | `2026-09-06` |
| Predecessor | `P01-B12 — Standard B-Spline Surface Semantic Contract` |

The implementation commit is a direct child of the locked start commit.

## Build Justification

P01-B12 established a validated, immutable, lossless semantic contract for
standalone non-rational `B_SPLINE_SURFACE_WITH_KNOTS` entities.

That sprint intentionally stopped before canonical geometry and identified the
canonical B-spline surface geometry contract as the next explicit capability
gate.

P01-B13 therefore implements the minimum canonical mathematical representation
needed before mapping composition can be introduced.

## Delivered Production Capability

P01-B13 adds:

- `BSplineSurface3D`;
- implementation of the existing `ISurface3D` contract;
- immutable ordered control-point storage;
- immutable U/V knot and multiplicity storage;
- canonical B-spline invariant enforcement;
- explicit structural value equality;
- equality-consistent structural hashing.

The public canonical contract exposes:

- U degree;
- V degree;
- ordered rectangular `Point3D` control-point grid;
- ordered U multiplicities;
- ordered V multiplicities;
- ordered distinct U knot values;
- ordered distinct V knot values.

## Canonical Boundary Decision

The canonical geometry deliberately excludes:

- STEP source entity identity;
- STEP source entity type;
- STEP entity name;
- `StepLogicalValue`;
- STEP surface-form enumeration values;
- STEP knot-specification enumeration values.

Those values remain owned by the STEP semantic layer.

The canonical surface carries only the data required to define the
non-rational B-spline surface mathematically. Explicit degrees, control points,
knots, and multiplicities are sufficient for that boundary.

This decision keeps dependency direction intact and prevents source-format
metadata from becoming a dependency of `MPC.Geometry`.

## Control-Grid Direction Contract

The control-point grid preserves the direction contract established and
verified in P01-B12:

- outer grid rows represent the U direction;
- points within each row represent the V direction;
- U control-point count equals the outer row count;
- V control-point count equals the common row length.

No grid transposition, sorting, deduplication, or reindexing occurs.

## Canonical Invariants

`BSplineSurface3D` enforces the applicable B-spline constraints independently
for U and V:

- degree is at least one;
- the control-point grid is non-null and non-empty;
- every control-point row is non-null and non-empty;
- the control-point grid is rectangular;
- all control-point coordinates are finite;
- each direction contains at least degree plus one control points;
- knot and multiplicity collections are non-null;
- knot and multiplicity collections contain at least two entries;
- knot and multiplicity counts match;
- every multiplicity is positive;
- multiplicity sum equals control-point count plus degree plus one;
- first and last multiplicities do not exceed degree plus one;
- interior multiplicities do not exceed degree;
- every knot value is finite;
- consecutive distinct knot values are strictly increasing.

Invalid construction fails deterministically through standard argument
exceptions.

## Immutability Contract

The canonical type performs defensive copies of:

- the outer control-point grid;
- every control-point row;
- U multiplicities;
- V multiplicities;
- U knots;
- V knots.

Published collections are read-only.

Mutation of constructor inputs after creation cannot change the canonical
surface. Consumers also cannot mutate the published outer grid, individual
rows, multiplicities, or knot collections.

`Point3D` is already an immutable readonly record struct.

## Equality Contract

Existing canonical primitives use value semantics.

Because the B-spline surface contains nested collections, compiler-generated
record equality would compare collection objects rather than their contents.
P01-B13 therefore uses explicit structural equality.

Two `BSplineSurface3D` instances are equal only when they contain equal:

- U and V degrees;
- grid dimensions, ordering, and control-point values;
- U and V multiplicity sequences;
- U and V knot sequences.

The equality operators and `GetHashCode` follow the same canonical value
contract.

Floating-point values are compared exactly. No tolerance, rounding, or
normalization is introduced at the representation boundary.

## Normalization Policy

The implementation does not:

- sort knots;
- sort control points;
- deduplicate values;
- clamp values;
- apply absolute-value normalization;
- transpose the grid;
- approximate the surface;
- repair invalid input.

Invalid mathematical input is rejected rather than silently changed.

## Test Coverage

`BSplineSurface3DTests` provides 29 targeted test cases covering:

- property and sequence preservation;
- `ISurface3D` compatibility;
- U/V control-grid direction and ordering;
- defensive-copy behavior;
- read-only published collections;
- structural equality;
- equality operators;
- hash-code consistency;
- invalid U/V degrees;
- empty and non-rectangular grids;
- insufficient U/V control-point counts;
- non-finite control-point coordinates;
- mismatched knot and multiplicity counts;
- non-positive multiplicities;
- invalid multiplicity sums;
- excessive endpoint multiplicity;
- excessive interior multiplicity;
- non-finite knots;
- duplicate distinct knots;
- descending knot sequences.

The initial RED gate failed only because `BSplineSurface3D` did not yet exist.
The implemented production contract then passed all 29 targeted cases.

## Senior Review

The staged implementation received a senior-level architecture and code review
before commit.

The review examined:

- canonical versus STEP-specific ownership;
- `ISurface3D` integration;
- U/V direction consistency;
- B-spline mathematical invariants;
- nested collection immutability;
- record-versus-class equality behavior;
- equality and hash-code consistency;
- normalization risks;
- test adequacy;
- scope discipline.

No material finding remained open.

Senior-review result:

`PASS — NO MATERIAL FINDINGS`

## Changed Files

### Production

- `src/MPC.Geometry/Primitives/BSplineSurface3D.cs`

### Tests

- `tests/MPC.Tests/Geometry/BSplineSurface3DTests.cs`

No connector, mapping, topology, application, worker, contract, or host
production file was changed.

## Quality Evidence

### RED Contract Gate

`EXPECTED RED — BSplineSurface3D production type did not exist`

### Targeted Canonical Geometry Tests

`PASS — 29/29`

### Normalization Audit

`PASS`

Confirmed absent:

- `OrderBy`;
- `Sort`;
- `Distinct`;
- `Math.Abs`;
- `Clamp`.

### Dependency Boundary Audit

`PASS`

The canonical geometry has no STEP, connector, or mapping dependency.

### Encoding and Whitespace

- UTF-8 BOM absent;
- final newline present;
- staged whitespace check clean.

### Release Solution Build

`PASS — 0 warnings / 0 errors`

### Full Regression

| Test Project | Passed | Failed | Skipped |
|---|---:|---:|---:|
| `MPC.Tests` | 1114 | 0 | 0 |
| `MPC.Contracts.Tests` | 35 | 0 | 0 |
| `MPC.Worker.Tests` | 16 | 0 | 0 |
| `MPC.Application.Tests` | 11 | 0 | 0 |
| `MPC.Host.SolidWorks.Tests` | 69 | 0 | 0 |
| **Total** | **1245** | **0** | **0** |

The Host tests passed under the required `x64` platform.

## Acceptance Criteria

| Criterion | Result |
|---|---|
| Locked start commit independently verified | Passed |
| Canonical B-spline surface type added | Passed |
| Existing `ISurface3D` contract implemented | Passed |
| U/V degrees preserved | Passed |
| Ordered rectangular control-point grid preserved | Passed |
| U/V knot values preserved | Passed |
| U/V multiplicities preserved | Passed |
| Finite control-point coordinates enforced | Passed |
| Standard U/V B-spline constraints enforced | Passed |
| Constructor inputs defensively copied | Passed |
| Published collections read-only | Passed |
| Structural value equality implemented | Passed |
| Equality-consistent hashing implemented | Passed |
| No source sequence normalization introduced | Passed |
| STEP-independent geometry boundary preserved | Passed |
| Mapping and application layers unchanged | Passed |
| Targeted tests completed at 29/29 | Passed |
| Senior review completed | Passed |
| Release build completed with zero warnings and errors | Passed |
| Full regression completed at 1245/1245 | Passed |

## Out of Scope Confirmed

P01-B13 did not:

- add STEP-to-canonical mapping composition;
- resolve STEP control-point references;
- change advanced-face mapping composition;
- add rational B-spline or NURBS support;
- interpret complex STEP entity instances;
- add B-spline curve support;
- evaluate B-spline basis functions;
- calculate surface points, derivatives, or normals;
- add approximation or tessellation;
- add healing or knot repair;
- change toroidal compatibility policy;
- claim physical P01-R03 import success.

## Architectural Assessment

The implementation preserves the established dependency direction.

`MPC.Geometry` remains independent of:

- STEP;
- connectors;
- mapping;
- application;
- worker;
- host concerns.

The result is a reusable canonical mathematical representation suitable for
future CAD-integrated and standalone workflows.

## Product Assessment

P01-B13 closes the canonical representation gap between the standalone
B-spline semantic reader and future mapping composition.

The implementation does not yet make P01-R03 physically importable. It
provides the minimum validated canonical surface contract required for the next
explicit mapping gate.

## Recommended Next Sprint

`P01-B14 — Standard B-Spline Surface Mapping Composition`

Recommended classification:

`NEW BUILD`

The next sprint should resolve the semantic control-point references through
the existing canonical reference resolver and compose a `BSplineSurface3D`.

Rational or complex B-spline interpretation, B-spline curves, evaluation,
tessellation, advanced-face integration, and physical P01-R03 success should
remain outside that mapping-composition sprint.
