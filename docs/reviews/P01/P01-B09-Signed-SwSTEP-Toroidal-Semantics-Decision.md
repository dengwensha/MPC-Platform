# P01-B09 — Signed SwSTEP Toroidal Semantics Decision

## Classification

DISCOVERY / VERIFY-FIRST

## Traceability

- Review Date: 2026-09-06
- Start Commit: d6151ac2b7f77a10a13b800a8074e202b5a8ff4f
- Reviewed HEAD: d6151ac2b7f77a10a13b800a8074e202b5a8ff4f
- Branch: main
- Production-Code Mutation: NONE
- Test-Code Mutation: NONE

## Objective

Determine, using geometric and topological evidence, the meaning of the
12 exporter-specific signed `TOROIDAL_SURFACE` entities observed in
`P01-R03-CurvedSurfaces.STEP`.

The sprint does not implement signed-radius support. Its purpose is to
produce an evidence-backed semantic decision for a possible future
build sprint.

## Starting Position

P01-B04 established that:

- the corpus contains 120 `TOROIDAL_SURFACE` entities;
- 12 entities use a negative major radius;
- all 12 use:
  - major radius `-178.5`;
  - minor radius `229.5000000000000284`;
- every signed surface is referenced by one `ADVANCED_FACE`;
- all 12 associated faces use `same_sense = .F.`;
- `same_sense` alone does not explain the signed radius;
- negative radii are not valid standard `TOROIDAL_SURFACE` semantics.

P01-B05 through P01-B08 subsequently implemented and composed the
standard positive-radius toroidal contracts while intentionally leaving
the signed SwSTEP representation unsupported.

## Corpus Identity

- File: `P01-R03-CurvedSurfaces.STEP`
- SHA256:
  `319DD894747CAF95AC1100C6283B55C014D40AEAC4ECCC00EF54648ADF9AEA12`
- Schema: STEP AP203 / `CONFIG_CONTROL_DESIGN`
- Exporter: SwSTEP 2.0
- Authoring system: SolidWorks 2019
- Parsed entities: 135828
- `ADVANCED_FACE` entities: 2912
- `TOROIDAL_SURFACE` entities: 120
- Signed candidates: 12

The corpus hash matched the previously accepted P01 baseline.

## Standards Baseline

ISO 10303-42 defines both radii of `TOROIDAL_SURFACE` as positive length
measures.

When the positive major radius is smaller than the positive minor
radius, the full toroidal surface is self-intersecting.
`DEGENERATE_TOROIDAL_SURFACE` restricts the parameter domain to select
one manifold portion:

- `select_outer = .T.` selects the outer apple-shaped portion;
- `select_outer = .F.` selects the inner lemon-shaped portion.

For:

- `R = 178.5`;
- `r = 229.5`;

the limiting angle is defined by:

`r cos(phi) = -R`

which gives:

- `phi = 141.057558731 degrees`;
- standard inner-domain range:
  `141.057558731 <= v <= 218.942441269 degrees`.

Reference:

https://www.steptools.com/stds/smrl/data/resource_docs/geometric_and_topological_representation/sys/4_schema.htm

## Candidate Interpretations

### H1 — Invalid data with no recoverable meaning

The negative radius is arbitrary malformed STEP data and cannot be
related to a valid standard surface.

Result: REJECTED for the observed corpus.

The 12 entities form a deterministic and geometrically consistent
pattern.

### H2 — Generic absolute-value normalization to an outer torus

The negative major radius may be replaced by its absolute value and
treated as a regular outer torus.

Result: REJECTED.

`abs(majorRadius) = 178.5` remains smaller than
`minorRadius = 229.5`. A regular `Torus3D` is therefore not obtained,
and all measured boundary geometry lies on the inner lemon branch
rather than the outer apple branch.

### H3 — `ADVANCED_FACE.same_sense` selects the manifold portion

The signed meaning may be inferred directly from
`ADVANCED_FACE.same_sense`.

Result: REJECTED.

All 12 signed faces use `.F.`, but positive-radius toroidal faces in the
same corpus use both `.T.` and `.F.`. Face sense controls orientation;
it is not a substitute for `select_outer`.

### H4 — Signed major radius encodes the inner lemon branch

The observed negative major radius represents:

- standard positive major radius `abs(majorRadius)`;
- the same positive minor radius;
- `select_outer = .F.`.

Result: ACCEPTED for the observed exporter/corpus scope.

## Signed Entity Inventory

| Signed torus | Face | Placement | Geometric centre group |
|---|---|---|---|
| `#2349` | `#21358` | `#52780` | A |
| `#90166` | `#74800` | `#94608` | A |
| `#29109` | `#30614` | `#57639` | B |
| `#106843` | `#3708` | `#8240` | B |
| `#30640` | `#135494` | `#19679` | C |
| `#71594` | `#126969` | `#23451` | C |
| `#35856` | `#90477` | `#11799` | D |
| `#128956` | `#99670` | `#67678` | D |
| `#77376` | `#68653` | `#73050` | E |
| `#122903` | `#111604` | `#24334` | E |
| `#107272` | `#109755` | `#10935` | F |
| `#114164` | `#111908` | `#13464` | F |

The 12 surfaces form six geometric pairs.

All placements use:

- global `+Y` as the torus axis;
- global `+Z` as the reference direction.

The six centre groups are arranged regularly in the global XZ plane.

## Local Topology Evidence

For the 12 signed surfaces:

- signed surfaces: 12;
- associated `ADVANCED_FACE` entities: 12;
- `ADVANCED_FACE.same_sense = .F.`: 12/12;
- bounds: 12;
- all bounds are `FACE_OUTER_BOUND`;
- bound orientation `.T.`: 12/12;
- loops: 12;
- edge uses: 48;
- edge uses per loop: 4;
- unique topological edges: 36;
- all underlying edge geometries are `CIRCLE`.

No topology parse failures were observed.

## Vertex Branch Measurement

Every unique boundary vertex was transformed into the local coordinate
system of its signed torus.

Two residuals were evaluated:

Outer apple candidate:

`sqrt((rho - R)^2 + axial^2) - r`

Inner lemon candidate:

`sqrt((rho + R)^2 + axial^2) - r`

Results:

| Measurement | Count |
|---|---:|
| Face-local vertex observations | 48 |
| Unique vertex entities | 24 |
| Inner lemon matches | 48 |
| Outer apple matches | 0 |
| Both/self-intersection matches | 0 |
| Neither matches | 0 |

The inner residual was between zero and approximately
`2.84E-14`.

The outer residual was approximately `-100.36` or `-102.0`.

## Underlying Circle Discovery

Sampling each complete underlying circle initially produced:

| Full-circle sample result | Count |
|---|---:|
| Total | 768 |
| Inner lemon | 456 |
| Outer apple | 312 |
| Both | 0 |
| Neither | 0 |

This full-circle result was not treated as an edge result because an
`EDGE_CURVE` uses only the directed interval between its vertices.

The mixed result exposed two boundary-curve families:

1. Circles with radius approximately `50.411162981` or `51.0`;
2. Generatrix circles with radius `229.5`.

The complete `229.5` circles cross both portions of the
self-intersecting toroidal surface. Only a short directed arc belongs
to each topological edge.

## Directed Edge-Arc Measurement

The actual curve interval was reconstructed from:

- edge start vertex;
- edge end vertex;
- circle placement;
- circle parametrisation;
- `EDGE_CURVE.same_sense`.

Each directed arc was divided into 32 segments, producing 33 samples
per face-local edge observation.

Two deterministic arc families were found:

- radius approximately `50.411162981` or `51.0`:
  `180-degree` arcs;
- radius `229.5`:
  `4.105223-degree` arcs.

Results:

| Measurement | Count |
|---|---:|
| Face-local edge observations | 48 |
| Unique topological edges | 36 |
| Directed arc samples | 1584 |
| Inner lemon matches | 1584 |
| Outer apple matches | 0 |
| Both matches | 0 |
| Neither matches | 0 |
| Parse failures | 0 |

Maximum measured inner residual:

`2.84E-14`

The actual trimmed boundary geometry therefore lies entirely on the
inner lemon manifold portion.

## Adjacency and Orientation Evidence

All 36 unique boundary edges were traced through:

`EDGE_CURVE -> ORIENTED_EDGE -> EDGE_LOOP -> FACE_BOUND -> ADVANCED_FACE`

Results:

| Check | Result |
|---|---:|
| Unique signed-boundary edges | 36 |
| Edges with exactly two face uses | 36 |
| Opposite raw `ORIENTED_EDGE` orientation | 36 |
| Opposite effective boundary direction | 36 |
| Adjacency anomalies | 0 |
| Parse failures | 0 |

Surface adjacency distribution:

- signed torus to signed torus: 12;
- signed torus to another positive-radius toroidal surface: 12;
- signed torus to plane: 12.

The signed patches are therefore part of a consistently closed
topological neighbourhood. They are not isolated malformed geometry.

## Parametric Equivalence

Let:

`e(u) = cos(u)x + sin(u)y`

and let `R` and `r` be positive.

The observed signed surface is represented algebraically as:

`S_signed(u,v) = C + (-R + r cos(v))e(u) + r sin(v)z`

For the standard positive-radius inner degenerate torus, apply:

- `u' = u + pi`;
- `v' = pi - v`.

Then:

`e(u + pi) = -e(u)`

and:

`S_inner(u',v') = S_signed(u,v)`.

The corresponding standard surface normal also satisfies:

`N_inner(u',v') = N_signed(u,v)`.

Therefore the conversion:

- preserves the point set;
- selects the standard inner lemon parameter domain;
- preserves the surface-normal direction;
- requires no additional normal inversion.

Consequently, the original `ADVANCED_FACE.same_sense` value must be
preserved.

## Semantic Decision

For the specifically observed SolidWorks 2019 / SwSTEP 2.0 encoding:

- a negative `TOROIDAL_SURFACE.major_radius`;
- a positive `minor_radius`;
- and `abs(major_radius) < minor_radius`;

represent a standard degenerate toroidal surface with:

- `major_radius = abs(source major_radius)`;
- `minor_radius = source minor_radius`;
- `select_outer = .F.`.

The original face `same_sense` is preserved.

This is an exporter-specific semantic reinterpretation. It is not
permission to apply generic `Math.Abs()` normalization to standard
`TOROIDAL_SURFACE` input.

## Scope Boundary

This decision does not establish semantics for:

- other SolidWorks versions;
- other SwSTEP versions;
- other STEP exporters;
- negative minor radii;
- zero radii;
- `abs(major_radius) >= minor_radius`;
- an exporter-specific encoding of the outer apple branch;
- generic negative-radius normalization;
- inference of `select_outer` from face sense.

Until an explicit compatibility path is implemented, existing
deterministic `APP-STEP-SEMANTIC-001` failure remains correct.

## Evidence Quality Controls

Several intermediate discovery outputs were explicitly rejected:

- a single-line entity parser produced only 132942 entities and was
  replaced by a multiline parser;
- an initial PowerShell vector calculation failed because of array
  expression parsing, and its measurements were excluded;
- a strict trailing-whitespace regex caused 48 edge-sense parse
  failures, and that run was excluded;
- complete-circle samples were retained as discovery evidence but were
  not treated as trimmed-edge samples.

Accepted measurements came only from runs with:

- complete 135828-entity parsing;
- zero duplicate entity IDs;
- no incomplete entity at EOF;
- zero measurement parse failures;
- deterministic branch classification.

## Validation

Release build:

- Result: succeeded
- Warnings: 0
- Errors: 0
- Duration: 00:00:20.22

Full Release regression:

- `MPC.Tests`: 1033/1033
- `MPC.Contracts.Tests`: 35/35
- `MPC.Worker.Tests`: 16/16
- `MPC.Application.Tests`: 8/8
- `MPC.Host.SolidWorks.Tests`: 69/69
- Total: 1161/1161
- Failed: 0
- Skipped: 0

Repository state before report creation:

- `main`
- synchronized with `origin/main`
- clean working tree

## Outcome

PASS — DISCOVERY COMPLETE / BUILD JUSTIFIED

The 12 signed SwSTEP toroidal surfaces have an evidence-backed
geometric and topological interpretation:

`negative major radius -> positive degenerate torus / select_outer = false`

No production-code or test-code change was made in P01-B09.

A future NEW BUILD sprint may implement an explicit, narrowly scoped
SwSTEP compatibility policy without weakening the standard
`TOROIDAL_SURFACE` contract.

## Recommended Next Sprint

`P01-B10 — Exporter-Scoped Signed Toroidal Compatibility Policy`

Recommended classification:

`NEW BUILD`

The future sprint should define the compatibility boundary before
implementation, preserve standard semantic rejection outside that
boundary, map the proven signed representation to
`DegenerateTorus3D(selectOuter: false)`, preserve face sense, and
validate the result against the physical R03 worker path.
