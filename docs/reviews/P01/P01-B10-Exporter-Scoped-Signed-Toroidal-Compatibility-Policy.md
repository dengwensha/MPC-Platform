# P01-B10 — Exporter-Scoped Signed Toroidal Compatibility Policy

## Classification

NEW BUILD

## Traceability

- Review Date: 2026-09-06
- Start Commit: `16bb8a85ac8372768ca03c4b4be3b6f6ea67b6f8`
- Implementation Commit: `2afe3f991339a00ffd461727c7be8c6b3a898c50`
- Reviewed HEAD: `2afe3f991339a00ffd461727c7be8c6b3a898c50`
- Branch: `main`
- Production-Code Mutation: YES
- Test-Code Mutation: YES

## Objective

Implement the exporter-scoped signed toroidal interpretation established
by P01-B09 without weakening the standard STEP `TOROIDAL_SURFACE`
contract.

The compatibility boundary is limited to:

- preprocessor version: `SwSTEP 2.0`;
- originating system: `SolidWorks 2019`;
- finite negative source major radius;
- finite positive source minor radius;
- positive major-radius magnitude smaller than the minor radius.

The compatible canonical result uses:

- positive major radius;
- unchanged positive minor radius;
- `selectOuter: false`.

## Decision Baseline

P01-B09 established that the 12 signed SwSTEP toroidal surfaces in the
accepted R03 corpus represent the inner lemon portion of a standard
degenerate toroidal surface.

Accepted interpretation:

`negative major radius -> positive degenerate torus / select_outer = false`

P01-B09 rejected:

- generic absolute-value normalization;
- interpretation as an ordinary outer torus;
- inference of `select_outer` from `ADVANCED_FACE.same_sense`;
- extension to other exporters or versions.

## Implemented Policy Boundary

| Header field | Required value |
|---|---|
| `PreprocessorVersion` | `SwSTEP 2.0` |
| `OriginatingSystem` | `SolidWorks 2019` |

The policy does not activate for:

- different SwSTEP versions;
- different SolidWorks versions;
- missing exporter information;
- other STEP exporters;
- negative or zero minor radius;
- zero major radius;
- `-sourceMajorRadius >= sourceMinorRadius`.

No generic `Math.Abs()` normalization is used.

## Architecture

### Standard path

The parameterless `StepToroidalSurfaceEntityHandler` remains strict and
continues to reject negative standard `TOROIDAL_SURFACE.major_radius`
values.

### Compatibility policy

`SwStepSignedToroidalCompatibilityPolicy` owns the exact exporter and
radius eligibility decision.

The policy is supplied explicitly to the toroidal semantic handler.

### Application composition

`StepImportApplicationService` reads header information only to select
the optional compatibility policy.

Header reading is best-effort:

- a successfully parsed header may enable the policy;
- an invalid or missing header does not enable compatibility;
- no new mandatory header-validation gate was introduced.

### Source identity and mapping

The compatible result is a `StepDegenerateToroidalSurfaceEntity`.

The following source information is preserved:

- original source entity ID;
- source entity type `TOROIDAL_SURFACE`;
- original `ADVANCED_FACE.same_sense`.

The existing degenerate-toroidal geometry mapper is reused with:

- positive major radius;
- original positive minor radius;
- `SelectOuter = false`.

## Changed Files

Production:

1. `SwStepSignedToroidalCompatibilityPolicy.cs`
2. `StepToroidalSurfaceEntityHandler.cs`
3. `StepDegenerateToroidalSurfaceEntity.cs`
4. `StepSemanticHandlerCatalog.cs`
5. `StepImportApplicationService.cs`

Tests:

1. `StepSignedToroidalCompatibilityPolicyTests.cs`
2. `StepSignedToroidalApplicationPolicyTests.cs`

## TDD Evidence

### Semantic policy RED/GREEN

The initial connector contract failed because the exporter-scoped policy
and policy-aware handler composition did not yet exist.

After implementation:

- focused compatibility tests: 10/10;
- targeted toroidal regression: 67/67;
- failures: 0;
- skipped: 0.

### Application composition RED/GREEN

Before Application composition, the exact supported fingerprint still
returned `APP-STEP-SEMANTIC-001`.

After header-scoped composition:

- exact fingerprint passed signed-toroidal semantics;
- a different SwSTEP version preserved semantic rejection;
- focused Application tests: 2/2;
- full Application regression: 10/10.

## Safety Audit

The implementation was checked for:

- generic `Math.Abs()` conversion;
- unsigned conversion;
- `selectOuter: true`;
- widened SwSTEP support;
- widened SolidWorks support.

Result:

`PASS — scoped signed conversion only`

Unary negation is used only after verifying that the source major radius
is finite and negative.

## Release Validation

Full solution Release build:

- Result: succeeded
- Warnings: 0
- Errors: 0
- Duration: 00:00:09.44

Full regression:

| Test project | Passed | Failed | Skipped |
|---|---:|---:|---:|
| `MPC.Tests` | 1043 | 0 | 0 |
| `MPC.Contracts.Tests` | 35 | 0 | 0 |
| `MPC.Worker.Tests` | 16 | 0 | 0 |
| `MPC.Application.Tests` | 10 | 0 | 0 |
| `MPC.Host.SolidWorks.Tests` | 69 | 0 | 0 |
| **Total** | **1173** | **0** | **0** |

The Host test project was executed with the required `x64` setting.

## Physical R03 Worker Validation

Corpus:

- File: `P01-R03-CurvedSurfaces.STEP`
- Bytes: `14003088`
- Parsed entities: `135828`
- Schema: `CONFIG_CONTROL_DESIGN`
- Preprocessor: `SwSTEP 2.0`
- Originating system: `SolidWorks 2019`
- SHA256:
  `319DD894747CAF95AC1100C6283B55C014D40AEAC4ECCC00EF54648ADF9AEA12`

Three physical worker runs:

| Run | Request ID | Exit | Duration | Diagnostics |
|---|---|---:|---:|---:|
| 1 | `90deb033-4950-49c0-a5bd-2db935a77fd3` | 3 | 3500 ms | 2772 |
| 2 | `fe3487ba-5b0f-4c38-bdb4-08da72d66f45` | 3 | 2675 ms | 2772 |
| 3 | `8b15835e-fa3c-4141-9563-32d71ddc784c` | 3 | 2704 ms | 2772 |

Physical assertions:

- worker crashes: 0;
- deterministic result: 3/3;
- unique result fingerprints: 1;
- RequestId preserved: 3/3;
- `APP-STEP-SEMANTIC-001`: absent 3/3;
- signed toroidal IDs unresolved: 0/12;
- unresolved toroidal entities: 0.

Result:

`SIGNED TOROIDAL BLOCKER REMOVED`

## Downstream Finding

The worker advanced to the next controlled compatibility boundary.

All 2772 diagnostics used:

`STEP_REFERENCE_RESOLUTION_FAILED`

Source-type distribution:

| Entity type | Count |
|---|---:|
| `B_SPLINE_CURVE_WITH_KNOTS` | 1796 |
| `B_SPLINE_SURFACE_WITH_KNOTS` | 696 |
| `BOUNDED_SURFACE` | 276 |
| `BOUNDED_CURVE` | 4 |
| **Total** | **2772** |

Controls:

- unique unresolved IDs: 2772;
- unique messages: 2772;
- unclassified entities: 0;
- unresolved `TOROIDAL_SURFACE`: 0;
- unresolved `DEGENERATE_TOROIDAL_SURFACE`: 0.

These non-toroidal entity families are outside P01-B10. No B-spline or
bounded-entity implementation was added.

## Scope Exclusions Preserved

P01-B10 does not implement:

- B-spline curves;
- B-spline surfaces;
- bounded curves;
- bounded surfaces;
- healing or sewing;
- topology mutation;
- generic coincidence resolution;
- caching or parallel parsing;
- UI behavior;
- other exporter or version compatibility;
- generic negative-radius normalization;
- signed outer-apple interpretation;
- face-sense-derived manifold selection.

## Outcome

`PASS WITH FINDING — POLICY IMPLEMENTED / PHYSICAL TOROIDAL BLOCKER REMOVED`

P01-B10 successfully implements the P01-B09 decision as an explicit,
exporter-scoped compatibility policy.

The standard `TOROIDAL_SURFACE` contract remains strict outside the
approved SwSTEP 2.0 / SolidWorks 2019 boundary.

The physical R03 worker path confirms that all 12 signed toroidal
entities now pass semantic resolution.

Full R03 success remains blocked by 2772 non-toroidal B-spline and
bounded-entity references. This downstream finding does not invalidate
the P01-B10 policy result.

## Recommended Next Sprint

`P01-B11 — R03 B-Spline and Bounded-Entity Resolution Discovery`

Recommended classification:

`DISCOVERY / VERIFY-FIRST`

The next sprint should determine:

- the complex-entity structures behind the four unresolved type groups;
- which missing semantic handlers cause the unresolved references;
- reference ownership and topology impact;
- the minimum required canonical curve and surface contracts;
- whether the reported types are leaf geometry, complex-entity
  supertypes, or parser projection artifacts.

No production implementation should begin until that discovery is
complete.
