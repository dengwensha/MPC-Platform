# P01-B04 - Signed Toroidal Surface Semantics Discovery

## Classification

DISCOVERY / VERIFY-FIRST

## Traceability

- Review Date: 2026-09-04
- Start Commit: f303610430f49e8a51cadfe11d0105c925bbfc08
- Branch: main
- Production-Code Mutation: NONE

## Corpus Evidence

Source file:

- P01-R03-CurvedSurfaces.STEP
- SHA256: 319DD894747CAF95AC1100C6283B55C014D40AEAC4ECCC00EF54648ADF9AEA12
- Schema: STEP AP203 / CONFIG_CONTROL_DESIGN
- Exporter: SwSTEP 2.0
- Authoring system: SolidWorks 2019

Observed:

- 120 TOROIDAL_SURFACE entities
- 12 signed candidates
- All 12 use major radius -178.5 and minor radius 229.5
- Each signed torus is referenced by one ADVANCED_FACE
- All 12 signed faces use same_sense .F.
- Positive torus distribution: 72 .T. and 36 .F.
- Therefore face sense alone does not explain the signed radius

## Contract Findings

- STEP semantic entities require finite positive radii.
- Torus3D additionally requires major radius greater than minor radius.
- StepToroidalSurfaceEntity is not mapped to Torus3D in production code.
- DEGENERATE_TOROIDAL_SURFACE is unsupported and absent from the corpus.
- The failure occurs during semantic entity construction, before canonical mapping.

## Standards Finding

ISO 10303-42 defines TOROIDAL_SURFACE radii as positive length measures.

When major radius is smaller than minor radius, the surface is
self-intersecting. DEGENERATE_TOROIDAL_SURFACE adds select_outer to
identify the required manifold portion.

Reference:

https://www.steptools.com/stds/smrl/data/resource_docs/geometric_and_topological_representation/sys/4_schema.htm

## Decision

The signed representation observed in this specific SolidWorks 2019 /
SwSTEP 2.0 corpus is not accepted as standard TOROIDAL_SURFACE semantics.

- Do not apply Math.Abs normalization.
- Do not infer select_outer from ADVANCED_FACE same_sense.
- Preserve deterministic APP-STEP-SEMANTIC-001 failure.
- Make no production-code change in P01-B04.
- Treat degenerate torus support as a separate NEW BUILD decision.

This finding applies to the observed corpus and does not claim that all
SolidWorks 2019 STEP exports use the same representation.

## Validation

Targeted Release tests:

- Passed: 28
- Failed: 0
- Skipped: 0

Full Release gate:

- Build: succeeded
- Warnings: 0
- Errors: 0
- MPC.Tests: 980/980
- MPC.Contracts.Tests: 35/35
- MPC.Worker.Tests: 16/16
- MPC.Application.Tests: 7/7
- MPC.Host.SolidWorks.Tests: 69/69
- Total: 1107/1107

## Outcome

PASS - DISCOVERY COMPLETE / NO PRODUCTION CHANGE
