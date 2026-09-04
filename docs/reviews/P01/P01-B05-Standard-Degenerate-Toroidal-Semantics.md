# P01-B05 — Standard Degenerate Toroidal Semantics

## Classification

NEW BUILD

## Traceability

- Review Date: 2026-09-04
- Start Commit: `f4744be017f1747c9e5ef7ae85e307600fe8ba22`
- Reviewed / Implementation Commit: `efef3f1c9ea9569f396c69e6db1df82b3c980037`
- Branch: `main`
- Closing Commit: documentation commit containing this report; recorded by the remote closure evidence

## Objective

Add first-class STEP semantic support for the standard
`DEGENERATE_TOROIDAL_SURFACE` entity without accepting or normalizing
the signed non-standard `TOROIDAL_SURFACE` representation observed in
P01-R03.

## Standards Contract

The implemented semantic contract is:

- entity type: `DEGENERATE_TOROIDAL_SURFACE`;
- subtype of `TOROIDAL_SURFACE`;
- flattened STEP parameters:
  1. name;
  2. placement;
  3. major radius;
  4. minor radius;
  5. select outer;
- major radius must be finite and greater than zero;
- minor radius must be finite and greater than zero;
- major radius must be strictly smaller than minor radius;
- select outer must be encoded as `.T.` or `.F.`;
- `.T.` and `.F.` are preserved as `true` and `false`;
- source entity ID, source entity type, name, placement reference and
  numeric radius values are preserved.

Reference:

- ISO 10303-42:2021, geometry schema,
  `degenerate_toroidal_surface`, clause 4.5.64
- https://www.steptools.com/stds/smrl/data/resource_docs/geometric_and_topological_representation/sys/4_schema.htm

## Scope Delivered

### Semantic entity

Added:

- `StepDegenerateToroidalSurfaceEntity`

The entity derives from `StepToroidalSurfaceEntity`, matching the
existing semantic subtype pattern used by entities such as
`FACE_OUTER_BOUND`.

The existing public `StepToroidalSurfaceEntity` constructor remains
available with the same parameters and behavior. A protected
source-entity-type constructor was added to support the standard
subtype.

### Semantic handler

Added:

- `StepDegenerateToroidalSurfaceEntityHandler`

The handler:

- recognizes only `DEGENERATE_TOROIDAL_SURFACE`;
- requires exactly five flattened parameters;
- accepts integer or real radius parameters;
- parses select outer through the existing STEP boolean enumeration
  parser;
- rejects invalid parameter shapes deterministically;
- converts radius invariant failures into
  `StepSemanticValueException`;
- preserves rejected radius values using invariant round-trip
  formatting.

### Application catalog

Registered the new handler explicitly in:

- `StepSemanticHandlerCatalog`

Application-level evidence confirms that an invalid standard
degenerate torus now reaches the registered semantic handler and is
reported with `APP-STEP-SEMANTIC-001`.

## Normalization Prohibition

The implementation contains no:

- `Math.Abs()` conversion of either radius;
- sign inversion;
- major/minor radius swap;
- inferred `select_outer`;
- conversion from signed `TOROIDAL_SURFACE` to
  `DEGENERATE_TOROIDAL_SURFACE`;
- other implicit normalization.

A negative radius remains invalid and its original negative value is
preserved in the semantic failure evidence.

## Test-First Evidence

### Connector RED

Before production types existed, the connector test project failed to
compile with `CS0246` for:

- `StepDegenerateToroidalSurfaceEntity`;
- `StepDegenerateToroidalSurfaceEntityHandler`.

Exit code: `1`.

### Catalog RED

Before catalog registration, the application test executed but failed.
The entity was skipped by semantic reading and processing continued to
missing-reference and missing-solid-root diagnostics instead of
`APP-STEP-SEMANTIC-001`.

Exit code: `1`.

### Targeted GREEN

Initial targeted gates:

- connector semantic tests: `2/2`;
- application catalog test: `1/1`.

### Hardened semantic gates

Final focused gates:

- degenerate toroidal semantic reader tests: `20/20`;
- application catalog test: `1/1`;
- toroidal near-regression tests: `38/38`.

Covered behavior includes:

- `.T.` and `.F.` preservation;
- integer and real radii;
- zero and negative radius rejection;
- strict `major_radius < minor_radius`;
- entity-scoped rejected parameter and value evidence;
- incorrect parameter counts;
- incorrect parameter types;
- unsupported boolean enumerations;
- handler type isolation;
- null argument behavior;
- deterministic application diagnostics.

## Final Validation

### Release build

- Solution: `src/MPC.sln`
- Configuration: `Release`
- Result: succeeded
- Warnings: `0`
- Errors: `0`

### Full regression

- `MPC.Tests`: `1000/1000`
- `MPC.Contracts.Tests`: `35/35`
- `MPC.Worker.Tests`: `16/16`
- `MPC.Application.Tests`: `8/8`
- `MPC.Host.SolidWorks.Tests`: `69/69`
- Total: `1128/1128`
- Failed: `0`
- Skipped: `0`

The Host test project was executed with `Platform=x64`, matching its
solution output path. An earlier command without the platform argument
looked in the non-x64 output directory; this was a gate-command
configuration issue, not a product or test failure.

### Repository hygiene

- staged implementation scope: exactly six files;
- staged whitespace check: passed;
- final newline check: passed for all six implementation files;
- existing UTF-8 BOM on `StepToroidalSurfaceEntity.cs`: preserved;
- prohibited-normalization check: passed.

## Explicit Out of Scope

The following were not implemented:

- successful import of P01-R03;
- signed SolidWorks 2019 / SwSTEP 2.0
  `TOROIDAL_SURFACE` compatibility;
- absolute-value normalization;
- radius reordering;
- inference of `select_outer` from `ADVANCED_FACE.same_sense`;
- canonical degenerate-torus geometry;
- canonical surface mapping;
- topology mutation;
- healing or sewing.

P01-R03 therefore remains a deterministic controlled semantic failure
under the existing signed `TOROIDAL_SURFACE` policy.

## Corpus Impact

P01-R03 contains:

- 120 `TOROIDAL_SURFACE` entities;
- 12 observed signed/spindle candidates;
- 0 `DEGENERATE_TOROIDAL_SURFACE` entities.

Accordingly, this standard semantic support does not change the
P01-R03 compatibility result and does not require a change to the
real-part validation matrix.

## Outcome

PASS — STANDARD SEMANTIC SUPPORT COMPLETE

P01-B05 establishes explicit, standard-compliant semantic support for
`DEGENERATE_TOROIDAL_SURFACE` while preserving the B04 decision that
signed non-standard `TOROIDAL_SURFACE` input must not be normalized or
silently reclassified.
