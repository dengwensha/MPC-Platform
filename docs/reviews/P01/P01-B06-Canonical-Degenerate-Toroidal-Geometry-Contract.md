# P01-B06 — Canonical Degenerate Toroidal Geometry Contract

## Classification

`[NEW BUILD / GEOMETRY CONTRACT]`

## Review Date

2026-09-04

## Traceability

- Branch: `main`
- Start Commit: `22a44f4ee4c9ce97fd50d0131f9cc00527d2653f`
- Implementation Commit: `427188e39d704641168a9a76bc741ead640db2b1`
- Closing Commit: commit containing this report

## Objective

Define a canonical geometry contract for the standard STEP
`DEGENERATE_TOROIDAL_SURFACE` semantics introduced during P01-B05.

The sprint establishes the geometry-level representation only. It does not
compose STEP mapping or claim physical P01-R03 import compatibility.

## Starting Evidence

At the start of P01-B06:

- `main` matched `origin/main`;
- the worktree was clean;
- HEAD was the P01-B05 closing commit;
- the standard STEP semantic entity and handler already existed;
- the existing canonical `Torus3D` accepted only
  `majorRadius > minorRadius`;
- `Torus3D` was a sealed record and did not contain `select_outer`.

## Architecture Decision

A separate canonical geometry type, `DegenerateTorus3D`, was introduced.

The existing `Torus3D` contract was not weakened or generalized because the
two types represent distinct valid state spaces:

- regular `Torus3D`: `majorRadius > minorRadius`;
- `DegenerateTorus3D`: `majorRadius < minorRadius`.

The standard STEP `select_outer` value is represented explicitly by the
canonical `SelectOuter` property.

This prevents regular and degenerate toroidal geometry from being silently
normalized, reclassified, or conflated.

## Canonical Contract

`DegenerateTorus3D`:

- implements `ISurface3D`;
- is a sealed record;
- stores `Center`;
- stores `AxisDirection`;
- stores `MajorRadius`;
- stores `MinorRadius`;
- stores `SelectOuter`;
- preserves all accepted values without normalization.

## Invariants

The constructor requires:

- finite center coordinates;
- finite axis-direction coordinates;
- a unit axis direction within tolerance `1e-12`;
- a finite, positive major radius;
- a finite, positive minor radius;
- `majorRadius < minorRadius`.

The equality case `majorRadius == minorRadius` is rejected.

The regular-torus relationship `majorRadius > minorRadius` is also rejected.

## Exception Contract

- Non-finite or non-positive scalar values produce
  `ArgumentOutOfRangeException`.
- Non-unit axis direction produces `ArgumentException`.
- Invalid major/minor radius relationship produces `ArgumentException`.

These choices follow the existing canonical geometry conventions used by
`Torus3D`, `Cylinder3D`, `Cone3D`, and `Sphere3D`.

## Test-First Evidence

The canonical contract tests were created before the production type.

The initial targeted run failed with `CS0246` because
`DegenerateTorus3D` did not yet exist.

This was the expected RED gate and demonstrated that the new tests exercised
a genuinely absent production contract.

## Implemented Files

- `src/MPC.Geometry/Primitives/DegenerateTorus3D.cs`
- `tests/MPC.Tests/Geometry/DegenerateTorus3DTests.cs`

Implementation commit scope:

- 2 files changed;
- 315 insertions;
- no existing production file modified.

## Targeted Validation

The targeted geometry gate covered:

- all new `DegenerateTorus3DTests`;
- all existing `Torus3DTests`.

Result:

- Passed: 24
- Failed: 0
- Skipped: 0

Of these, 14 cases validate the new degenerate torus contract and 10 preserve
the existing regular torus contract.

## New Contract Coverage

The new tests verify:

- preservation of `SelectOuter = true`;
- preservation of `SelectOuter = false`;
- storage of center, axis and radii;
- `ISurface3D` implementation;
- acceptance of a unit axis within tolerance;
- rejection of a non-finite center;
- rejection of a non-finite axis;
- rejection of a non-unit axis;
- rejection of zero major radius;
- rejection of negative minor radius;
- rejection of non-finite major radius;
- rejection of non-finite minor radius;
- rejection of equal radii;
- rejection of `majorRadius > minorRadius`;
- record value equality;
- inequality when `SelectOuter` differs.

## Scope Audit

The pre-commit scope audit passed with exactly two expected files.

No changes were made to:

- `Torus3D`;
- STEP semantic entities;
- STEP semantic handlers;
- handler catalogs;
- canonical reference resolution;
- STEP geometry mappers;
- application composition;
- worker behavior;
- topology.

## Normalization Audit

The implementation contains no:

- absolute-value conversion of either radius;
- radius swapping;
- sign inversion;
- subtype inference;
- automatic reclassification.

Both radii and `SelectOuter` are preserved exactly as supplied after
validation.

## Release Build

Release build result:

- Warnings: 0
- Errors: 0
- Duration: 00:00:07.13

## Full Regression

All test projects passed:

| Test project | Passed | Failed | Skipped |
|---|---:|---:|---:|
| MPC.Tests | 1014 | 0 | 0 |
| MPC.Contracts.Tests | 35 | 0 | 0 |
| MPC.Worker.Tests | 16 | 0 | 0 |
| MPC.Application.Tests | 8 | 0 | 0 |
| MPC.Host.SolidWorks.Tests | 69 | 0 | 0 |
| **Total** | **1142** | **0** | **0** |

The prior P01-B05 baseline was 1128 tests. P01-B06 added 14 test cases.

## Explicitly Out of Scope

P01-B06 does not include:

- STEP-to-canonical degenerate torus mapping;
- resolver or composition registration;
- physical P01-R03 success;
- signed `TOROIDAL_SURFACE` normalization;
- conversion between regular and degenerate torus types;
- topology mutation;
- healing;
- sewing;
- performance optimization.

## Acceptance Assessment

| Acceptance criterion | Result |
|---|---|
| Separate canonical degenerate torus type | PASS |
| Implements canonical surface contract | PASS |
| Finite and positive radius validation | PASS |
| Enforces `majorRadius < minorRadius` | PASS |
| Preserves `select_outer` | PASS |
| Preserves existing `Torus3D` contract | PASS |
| No normalization or radius swapping | PASS |
| Test-first RED evidence | PASS |
| Targeted geometry tests | PASS — 24/24 |
| Release build | PASS — 0 warnings, 0 errors |
| Full regression | PASS — 1142/1142 |
| Scope limited to canonical geometry contract | PASS |

## Outcome

**PASS**

P01-B06 establishes a distinct, deterministic, and tested canonical geometry
contract for standard degenerate toroidal surfaces.

No STEP mapping capability or P01-R03 compatibility claim is made by this
sprint.
