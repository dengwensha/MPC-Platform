# P01-B11 — R03 B-Spline and Bounded-Entity Resolution Discovery

## Classification

`DISCOVERY / VERIFY-FIRST`

## Status

`PASS — DISCOVERY COMPLETE / BUILD JUSTIFIED`

## Scope

P01-B11 investigates the B-spline and bounded-entity compatibility
boundary reached by the physical P01-R03 worker path after completion of
the exporter-scoped signed toroidal compatibility policy in P01-B10.

This sprint determines:

- the exact B-spline and bounded-entity inventory in P01-R03;
- whether those entities are active topology dependencies;
- the existing structural and semantic parsing capabilities;
- the missing semantic and canonical resolution capabilities;
- the surface-versus-curve execution order;
- the minimum information that future canonical contracts must preserve;
- the minimum justified next build boundary.

This sprint does not add production or test capability.

## Repository Baseline

| Item | Value |
|---|---|
| Branch | `main` |
| Start commit | `478cb255647825f73dac9644157d18b691eb05fa` |
| Start commit subject | `docs(p01): record B10 compatibility closure` |
| Initial local/remote relationship | `HEAD == origin/main` |
| Initial working tree | Clean |
| Production-code mutation | None |
| Test-code mutation | None |

## Accepted Upstream Boundary

P01-B10 established exporter-scoped compatibility for the 12 signed
SwSTEP toroidal entities in P01-R03.

Accepted interpretation:

`negative major radius -> positive degenerate torus / select_outer = false`

That policy remains restricted to the verified SwSTEP 2.0 /
SolidWorks 2019 exporter boundary.

P01-B11 does not modify, widen, normalize, or reinterpret that policy.

## Corpus Identity

| Item | Value |
|---|---|
| File | `P01-R03-CurvedSurfaces.STEP` |
| Bytes | `14003088` |
| Parsed entities | `135828` |
| Schema | `CONFIG_CONTROL_DESIGN` |
| Preprocessor | `SwSTEP 2.0` |
| Originating system | `SolidWorks 2019` |
| SHA256 | `319DD894747CAF95AC1100C6283B55C014D40AEAC4ECCC00EF54648ADF9AEA12` |

The corpus identity matched the accepted P01-R03 baseline.

## Corrected B-Spline Record Inventory

Token occurrence counts were separated from actual STEP record
classification because bounded and rational names occur as components of
complex entity records.

| Record family | Records |
|---|---:|
| Standalone `B_SPLINE_CURVE_WITH_KNOTS` | 1910 |
| Complex rational B-spline curve | 4 |
| Standalone `B_SPLINE_SURFACE_WITH_KNOTS` | 696 |
| Complex rational B-spline surface | 276 |
| **Total** | **2886** |

`BOUNDED_CURVE` and `BOUNDED_SURFACE` are not additional independent
records in this inventory. They are component types inside the 4 curve
and 276 surface complex entities.

## Topology Reachability

All 2886 B-spline records are direct active dependencies of the
canonical topology path.

| Record family | Records | Referenced | Unreferenced | Direct owner |
|---|---:|---:|---:|---|
| Standalone B-spline curve | 1910 | 1910 | 0 | `EDGE_CURVE` |
| Complex rational B-spline curve | 4 | 4 | 0 | `EDGE_CURVE` |
| Standalone B-spline surface | 696 | 696 | 0 | `ADVANCED_FACE` |
| Complex rational B-spline surface | 276 | 276 | 0 | `ADVANCED_FACE` |
| **Total** | **2886** | **2886** | **0** | — |

Each B-spline curve record has one direct `EDGE_CURVE` reference.

Each B-spline surface record has one direct `ADVANCED_FACE` reference.

The records are therefore not passive file metadata or unreachable
geometry. They are required inputs to R03 canonical topology assembly.

## Corpus Degree Profile

### Curves

| Family | Degree | Records |
|---|---:|---:|
| Standalone non-rational | 2 | 6 |
| Standalone non-rational | 3 | 1904 |
| Complex rational | 3 | 4 |
| **Total** | — | **1914** |

### Surfaces

| Family | U degree | V degree | Records |
|---|---:|---:|---:|
| Standalone non-rational | 3 | 1 | 696 |
| Complex rational | 3 | 2 | 252 |
| Complex rational | 2 | 3 | 24 |
| **Total** | — | — | **972** |

The corpus does not justify a degree-specific canonical model.

In particular, an implementation must not assume that every curve is
cubic or that every surface uses a single degree pair.

## Complex-Entity Structural Capability

The connector already contains structural support for complex STEP
entities.

`StepDataEntityReader` preserves complex records with:

- `SourceEntityType = COMPLEX_ENTITY`;
- ordered `ComponentTypes`;
- the complete raw STEP statement;
- referenced entity identifiers;
- complete read status when component discovery succeeds.

Existing tests include:

- `StepComplexEntityReaderTests`;
- `StepComplexEntityStructuralParserTests`;
- complex-entity state coverage in `StepDataEntityRecordTests`;
- golden inventory coverage for `COMPLEX_ENTITY`.

A new general-purpose STEP file reader is therefore not justified.

## Verified Rational Curve Component Structure

All 4 rational curve records contain the complete component family:

1. `BOUNDED_CURVE`
2. `B_SPLINE_CURVE`
3. `B_SPLINE_CURVE_WITH_KNOTS`
4. `CURVE`
5. `GEOMETRIC_REPRESENTATION_ITEM`
6. `RATIONAL_B_SPLINE_CURVE`
7. `REPRESENTATION_ITEM`

Coverage: `4/4` for every component.

## Verified Rational Surface Component Structure

All 276 rational surface records contain the complete component family:

1. `BOUNDED_SURFACE`
2. `B_SPLINE_SURFACE`
3. `B_SPLINE_SURFACE_WITH_KNOTS`
4. `GEOMETRIC_REPRESENTATION_ITEM`
5. `RATIONAL_B_SPLINE_SURFACE`
6. `REPRESENTATION_ITEM`
7. `SURFACE`

Coverage: `276/276` for every component.

## Semantic Resolution Gap

Although complex records are preserved structurally,
`StepSemanticEntityReader` selects handlers only by
`StepDataEntityRecord.SourceEntityType`.

All rational B-spline complex records therefore enter semantic
processing as `COMPLEX_ENTITY`.

No B-spline semantic handler is registered, and no component-aware
complex B-spline semantic interpretation is currently composed.

The current production catalog contains handlers for supported analytic
curves, analytic surfaces, topology entities, and the accepted toroidal
contracts, but it contains no handler for:

- standalone `B_SPLINE_CURVE_WITH_KNOTS`;
- standalone `B_SPLINE_SURFACE_WITH_KNOTS`;
- rational B-spline curve complex entities;
- rational B-spline surface complex entities.

## Canonical Geometry Gap

The geometry layer currently has canonical curve contracts including
`Line3D`, `Circle3D`, and `Ellipse3D`, and canonical surface contracts
including plane, cylinder, cone, sphere, torus, and degenerate torus.

It contains no canonical B-spline or NURBS curve type.

It contains no canonical B-spline or NURBS surface type.

No approximation, tessellation, analytic substitution, or degree
normalization is accepted as a replacement for the missing contracts.

## Surface Resolution Path

The verified surface path is:

`ADVANCED_FACE`
→ `StepReferenceResolver.Resolve<StepSemanticEntity>`
→ `StepAdvancedFaceGeometryMapper`
→ surface-type dispatch
→ surface geometry mapper
→ canonical `ISurface3D`

B-spline surfaces are absent from both the semantic-handler catalog and
the advanced-face geometry dispatch.

Surface resolution does not use `StepCanonicalReferenceResolver`.

## Curve Resolution Path

The verified curve path is:

`EDGE_CURVE`
→ `StepCanonicalReferenceResolver.Resolve<ICurve3D>`
→ semantic-type dispatch
→ curve geometry mapper
→ canonical `ICurve3D`

Future B-spline curve support therefore requires:

- a semantic contract and handler;
- a canonical curve contract;
- a geometry mapper;
- canonical resolver registration;
- cache and type-mismatch coverage;
- edge-curve composition coverage.

## Execution Order

`StepAdvancedFaceMapper` creates the `FaceBuilder` before mapping face
boundaries.

`StepAdvancedFaceBuilderMapper` first resolves and maps the face
surface.

When surface geometry cannot be resolved, face mapping stops before its
boundary loops and edge curves are visited.

The first runtime build boundary is therefore B-spline surface support,
not B-spline curve support.

## Physical Diagnostic Reconciliation

P01-B10 produced 2772 deterministic
`STEP_REFERENCE_RESOLUTION_FAILED` diagnostics:

| Reported unresolved family | Count |
|---|---:|
| `B_SPLINE_CURVE_WITH_KNOTS` | 1796 |
| `B_SPLINE_SURFACE_WITH_KNOTS` | 696 |
| `BOUNDED_SURFACE` | 276 |
| `BOUNDED_CURVE` | 4 |
| **Total** | **2772** |

B11 independently identified 2886 active B-spline records.

The difference is reconciled by the verified surface-first mapping
order:

| Group | Active records | Reported failures | Deferred |
|---|---:|---:|---:|
| B-spline surfaces | 972 | 972 | 0 |
| B-spline curves | 1914 | 1800 | 114 |
| **Total** | **2886** | **2772** | **114** |

The 114 deferred curves are behind faces whose B-spline surfaces fail
before boundary traversal.

They are active topology dependencies even though they do not yet
produce physical worker diagnostics.

## Minimum Future Surface Information

A lossless canonical B-spline surface contract must be capable of
preserving at least:

- U and V degree;
- ordered rectangular control-point grid;
- U and V knot values;
- U and V knot multiplicities;
- U-closed and V-closed state;
- self-intersection state;
- surface form;
- knot specification;
- rational weights when present.

The concrete canonical type design remains the responsibility of its
dedicated build sprint.

## Minimum Future Curve Information

A lossless canonical B-spline curve contract must be capable of
preserving at least:

- degree;
- ordered control points;
- knot values;
- knot multiplicities;
- closed state;
- self-intersection state;
- curve form;
- knot specification;
- rational weights when present.

The concrete canonical type design remains outside P01-B11.

## Build Decision

`BUILD JUSTIFIED`

R03 cannot complete canonical topology assembly without B-spline surface
and curve support.

However, surface and curve support follow different composition paths
and must not be treated as a single undifferentiated implementation
change.

The minimum first build boundary is standalone non-rational B-spline
surface semantics.

## Recommended Next Sprint

`P01-B12 — Standard B-Spline Surface Semantic Contract`

Recommended classification:

`NEW BUILD`

Initial scope:

- add a semantic entity for standalone
  `B_SPLINE_SURFACE_WITH_KNOTS`;
- parse and preserve all standard source parameters without
  normalization;
- register the handler in `StepSemanticHandlerCatalog`;
- verify representative R03 degree and control-grid shapes;
- verify knot and multiplicity preservation;
- preserve source entity identity;
- provide deterministic format and semantic failure coverage.

Out of scope:

- canonical `ISurface3D` implementation;
- advanced-face mapping composition;
- rational or complex-entity interpretation;
- B-spline curve support;
- approximation or tessellation;
- physical R03 success.

## Subsequent Capability Order

After B12, capability should advance through explicit gates:

1. canonical B-spline surface geometry contract;
2. standalone B-spline surface mapping composition;
3. rational complex B-spline surface interpretation and mapping;
4. B-spline curve semantic and canonical contracts;
5. curve canonical resolver composition;
6. rational complex B-spline curve interpretation;
7. physical R03 validation and P01 closure assessment.

Each gate must retain its own acceptance criteria and full regression
evidence.

## Out of Scope Confirmed

P01-B11 did not:

- change production code;
- change test code;
- implement a B-spline entity handler;
- implement a canonical B-spline type;
- change complex-entity parsing;
- change mapping composition;
- add approximation or tessellation;
- modify the accepted toroidal compatibility policy;
- claim successful physical import of P01-R03.

## Verification Evidence

### Initial Incorrect Build Invocation

An initial attempt supplied `Release|x64` at solution level.

`MPC.sln` does not define that solution configuration, so MSBuild
rejected the invocation before compilation with `MSB4126`.

This was a command-configuration error, not a source-code or product
failure.

The corrected gate used the solution's valid Release configuration and
executed the Host test project explicitly with `Platform=x64`.

### Corrected Release Build

| Item | Result |
|---|---|
| Solution | `src/MPC.sln` |
| Configuration | `Release` |
| Result | Succeeded |
| Warnings | 0 |
| Errors | 0 |
| Duration | 22.25 seconds |

### Full Regression

| Test project | Passed | Failed | Skipped |
|---|---:|---:|---:|
| `MPC.Tests` | 1043 | 0 | 0 |
| `MPC.Contracts.Tests` | 35 | 0 | 0 |
| `MPC.Worker.Tests` | 16 | 0 | 0 |
| `MPC.Application.Tests` | 10 | 0 | 0 |
| `MPC.Host.SolidWorks.Tests` | 69 | 0 | 0 |
| **Total** | **1173** | **0** | **0** |

The Host test project passed under the required `x64` platform.

## Acceptance Criteria

| Criterion | Result |
|---|---|
| Repository baseline independently verified | Passed |
| P01-R03 corpus identity verified | Passed |
| B-spline record families and counts determined | Passed |
| Topology ownership and reachability determined | Passed |
| Standalone and complex records distinguished | Passed |
| Existing complex structural parser verified | Passed |
| Missing semantic handler boundary verified | Passed |
| Missing canonical curve and surface contracts verified | Passed |
| Surface and curve resolution paths distinguished | Passed |
| Physical diagnostic count reconciled | Passed |
| First runtime build boundary identified | Passed |
| Production-code mutation remains none | Passed |
| Test-code mutation remains none | Passed |
| Release build succeeds with zero warnings and errors | Passed |
| Full regression succeeds | Passed |
| Working tree remains clean before report creation | Passed |

## Final Assessment

`PASS — DISCOVERY COMPLETE / BUILD JUSTIFIED`

P01-R03 contains 2886 active B-spline geometry records.

The existing connector preserves complex entity structure, but neither
standalone nor rational B-spline records have semantic or canonical
geometry support.

All 972 B-spline surfaces block face creation directly.

Of the 1914 active B-spline curves, 1800 are currently reached by the
physical worker and 114 remain deferred behind failed B-spline surface
faces.

The evidence establishes standalone B-spline surface semantics as the
minimum justified next build boundary.
