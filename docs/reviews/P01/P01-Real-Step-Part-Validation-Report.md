# P01 — Real STEP Part Validation Report

## 1. Review Record

- Review date: 2026-08-25
- Classification: VALIDATE / VERIFY-FIRST
- Branch: main
- Start commit: `196eda878e0461d300fc1c8d1bfe10159e0ea26d`
- Product-code mutation during validation: none
- Physical worker runs: 9
- Successful runs: 3
- Controlled failed runs: 6
- Worker crashes: 0
- Deterministic parts: 3/3

## 2. Objective

Validate real production STEP parts through the physical product path:

`STEP → Connector → Semantic Reader → Canonical Mapping → Topology Assembly → Application Service → MPC.Worker`

The sprint evaluates current product capability. It does not silently repair source geometry, expand canonical geometry semantics, or introduce unrelated feature work.

## 3. Corpus

The validation corpus was held outside the repository to prevent proprietary production files from being committed.

| Part | Classification | Bytes | Entities | Schema | SHA-256 |
|---|---|---:|---:|---|---|
| P01-R01-Prismatic | Real prismatic part | 23,222 | 329 | AUTOMOTIVE_DESIGN | `E518DB3332DC665B16CC31DA960C46B75B8D75128234129922A25146BE1C33AD` |
| P01-R02-InternalBoundaries | Real part with internal boundaries | 46,323 | 649 | AUTOMOTIVE_DESIGN | `0E9E2D6E15E1328C068E30703AD621AE883B6FE94A5DB51353A0D26F82E1578F` |
| P01-R03-CurvedSurfaces | Large real part with curved surfaces | 14,003,088 | 135,828 | CONFIG_CONTROL_DESIGN | `319DD894747CAF95AC1100C6283B55C014D40AEAC4ECCC00EF54648ADF9AEA12` |

## 4. Execution Matrix

| Part | Runs | Exit code | Result | Duration (ms) | Canonical summary |
|---|---:|---:|---|---|---|
| R01 | 3 | 0 | Succeeded | 270 / 272 / 256 | 1 solid, 1 outer shell, 0 inner shells, 11 faces, 12 loops, 24 edges, 16 vertices |
| R02 | 3 | 3 | Controlled failure | 266 / 263 / 264 | No summary; canonical reference resolution failed |
| R03 | 3 | 3 | Controlled failure | 1382 / 1552 / 1540 | No summary; semantic toroidal-surface construction failed |

All three parts produced the same status, topology result, or diagnostic classification across repeated runs.

## 5. R01 Result

R01 passed the complete physical-worker pipeline.

The canonical result was stable across all runs:

- solid count: 1
- outer shell count: 1
- inner shell count: 0
- face count: 11
- loop count: 12
- edge count: 24
- vertex count: 16
- diagnostics: none

Classification: PASS.

## 6. R02 Finding

R02 failed deterministically with:

`MAP-STEP-CANONICAL-REFERENCE-001`

The file contains 16 unique CIRCLE semantic entities required by edge-curve mapping. Each entity was reported twice, producing 32 diagnostics per run.

The canonical circle mapper exists, but CIRCLE is not registered in the current `StepCanonicalReferenceResolver` composition.

This is a composition gap, not a missing parser or circle-geometry algorithm.

Classification: CONTROLLED FAIL — canonical resolver composition gap.

## 7. R03 Finding

R03 contains:

- 1 MANIFOLD_SOLID_BREP
- 1 CLOSED_SHELL
- 2,912 ADVANCED_FACE entities
- 120 TOROIDAL_SURFACE entities
- 12 repeated signed/spindle toroidal surfaces
- 0 DEGENERATE_TOROIDAL_SURFACE entities

The incompatible records use:

- major radius: `-178.5`
- minor radius: `229.5`

The semantic constructor rejects the negative major radius. The current application boundary converts the exception into:

`APP-STEP-UNEXPECTED-001`

The worker remains alive and returns a deterministic operation failure. However, the diagnostic does not preserve the source entity ID, parameter name, or rejected value.

Taking the absolute value is not an acceptable correction because it would discard source semantics and would still violate the current canonical `majorRadius > minorRadius` invariant.

Classification: CONTROLLED FAIL — exporter-specific signed/spindle toroidal representation.

## 8. Additional Reliability Precondition

Before the real-part runs, worker timeout cleanup was hardened and independently committed as:

`196eda878e0461d300fc1c8d1bfe10159e0ea26d`

Evidence:

- targeted tests: 20/20
- physical timeout stress: 10/10
- host tests: 69/69
- full regression: 1094/1094
- Release build: 0 warnings, 0 errors
- remote reachability: confirmed

## 9. Backlog Classification

### P01-B01 — CIRCLE Resolver Composition

Register the existing canonical circle mapper in `StepCanonicalReferenceResolver` and verify R02 through the physical worker.

### P01-B02 — Diagnostic Deduplication

Prevent duplicate mapping diagnostics for the same diagnostic code and source entity while preserving deterministic ordering.

### P01-B03 — Entity-Scoped Semantic Failure

Convert unsupported or invalid STEP semantic values into diagnostics that preserve source entity ID, parameter name, rejected value, and semantic type.

### P01-B04 — Signed/Spindle Toroidal Research

Determine whether the exporter-specific signed/spindle toroidal representation requires:

- a new canonical surface type;
- an extension of `Torus3D`;
- explicit unsupported-geometry classification; or
- a documented exporter compatibility rule.

No geometry normalization or absolute-value conversion is authorized without a separate architecture decision.

## 10. Scope Exclusions Preserved

The sprint did not implement:

- feature recognition;
- manufacturability analysis;
- healing or sewing;
- coincidence-based topology mutation;
- active SOLIDWORKS model mutation;
- caching;
- parallel parsing;
- parser optimization;
- broad user-interface work.

## 11. Closure Decision

P01 validation outcome: PASS WITH FINDINGS.

Current real-part compatibility outcome: PARTIAL — 1 of 3 parts completed the canonical physical-worker pipeline.

The sprint successfully established:

- a three-part real corpus;
- physical-worker execution evidence;
- deterministic success and failure behavior;
- zero worker crashes;
- two reproducible compatibility boundaries;
- an evidence-backed remediation backlog.

P01 must not be represented as full real-part compatibility. The correct conclusion is that the current architecture is stable and observable, while canonical coverage remains incomplete for circle-reference composition and signed/spindle toroidal surfaces.
