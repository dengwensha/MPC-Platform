# P01-B01 — CIRCLE Resolver Composition

## 1. Record

- Review date: 2026-08-31
- Classification: VALIDATE / VERIFY-FIRST / HARDEN
- Branch: `main`
- Start commit: `36835c5e9a97c0ce00959309f80df442b5348163`
- Reviewed HEAD: `83cb77915193f03de2389b326b0f4f9b8f24f866`
- Closing commit: `83cb77915193f03de2389b326b0f4f9b8f24f866`
- Outcome: PASS WITH VERIFIED SCOPE AMENDMENT

## 2. Objective

Register the existing canonical CIRCLE mapper in the STEP canonical
reference resolver and verify `P01-R02-InternalBoundaries.STEP` through
the physical worker executable.

No parallel mapping authority, alternative geometry construction path,
healing, sewing, feature recognition, performance optimization, active
SOLIDWORKS mutation, or general STEP expansion was authorized.

## 3. Initial Finding

The real-part validation baseline showed that:

- `P01-R02-InternalBoundaries.STEP` was rejected deterministically;
- the worker returned `MAP-STEP-CANONICAL-REFERENCE-001`;
- the unsupported semantic entity type was `CIRCLE`;
- `StepCircleGeometryMapper` and its direct tests already existed;
- `StepCanonicalReferenceResolver` did not compose that mapper.

The missing behavior was therefore resolver composition rather than a
missing CIRCLE geometry mapper.

## 4. Implemented CIRCLE Composition

The implementation:

1. creates `StepCircleGeometryMapper` in
   `StepCanonicalMappingOperation.Create`;
2. injects it into `StepCanonicalReferenceResolver`;
3. registers `StepCircleEntity` in the resolver mapping switch;
4. preserves existing result conversion, diagnostic propagation, cache,
   and type-mismatch behavior;
5. makes `Circle3D` implement the existing `ICurve3D` contract.

`StepCircleGeometryMapper` remains the only canonical CIRCLE geometry
mapping authority.

## 5. Verified Scope Amendment

The first physical worker execution after CIRCLE composition passed the
CIRCLE resolution stage and exposed a deeper canonical topology
incompatibility.

Eight valid STEP `EDGE_LOOP` entities contained two `ORIENTED_EDGE`
references each. Inspection confirmed that:

- no oriented edges were lost by the mapper;
- each loop contained a continuous and closed two-edge cycle;
- the two edges used opposite vertex pairs;
- all sixteen referenced edge geometries were `CIRCLE`;
- `Loop` and `TopologyBuilder` incorrectly required at least three edge
  uses.

The canonical loop minimum was therefore changed from three to two
directed edge uses.

Continuity and closure validation remain mandatory. A single-edge loop
is still rejected. Open and discontinuous two-edge chains remain
rejected.

## 6. Product-Code Changes

- `src/MPC.Geometry/Primitives/Circle3D.cs`
  - implements `ICurve3D`.

- `src/MPC.Mapping.Step/StepCanonicalMappingOperation.cs`
  - creates and injects `StepCircleGeometryMapper`.

- `src/MPC.Mapping.Step/StepCanonicalReferenceResolver.cs`
  - accepts the circle mapper dependency;
  - maps `StepCircleEntity` through the existing mapper.

- `src/MPC.Topology/Primitives/Loop.cs`
  - accepts continuous, closed loops with at least two edge uses.

- `src/MPC.Topology/Building/TopologyBuilder.cs`
  - verifies registered loops using the same minimum.

## 7. Test Changes

New tests:

- `StepCanonicalCircleCompositionTests`
- `StepCanonicalCircleResolutionTests`
- `TwoEdgeLoopSupportTests`

Updated tests:

- `StepEdgeLoopMapperTests`
  - an open two-edge chain remains an invalid loop;
- `LoopTests`
  - fewer than two edge uses remains invalid.

The tests verify:

- CIRCLE to `Circle3D` resolution;
- CIRCLE to `ICurve3D` resolution;
- canonical cache identity;
- wrong-type diagnostics;
- production operation composition;
- valid two-edge closed cycles;
- builder registration and verification;
- rejection of single-edge loops;
- rejection of open or discontinuous two-edge chains.

## 8. Validation Evidence

### Targeted CIRCLE tests

- Passed: 3
- Failed: 0

### Initial near regression

- Passed: 42
- Failed: 0

### Topology and CIRCLE regression

- Passed: 90
- Failed: 0

### Final full regression

- `MPC.Tests`: 975 / 975
- `MPC.Contracts.Tests`: 35 / 35
- `MPC.Worker.Tests`: 15 / 15
- `MPC.Application.Tests`: 6 / 6
- `MPC.Host.SolidWorks.Tests`: 69 / 69
- Total: 1100 / 1100
- Failed: 0
- Skipped: 0

The final Release solution build completed successfully.

## 9. Physical Worker Evidence

Input:

- Part: `P01-R02-InternalBoundaries.STEP`
- Bytes: 46,323
- STEP entities: 649
- Schema: `AUTOMOTIVE_DESIGN`
- SHA-256:
  `0E9E2D6E15E1328C068E30703AD621AE883B6FE94A5DB51353A0D26F82E1578F`

Final physical runs:

| Run | Exit code | Status | Duration (ms) | Diagnostics |
|---:|---:|---:|---:|---:|
| 1 | 0 | Succeeded | 358 | 0 |
| 2 | 0 | Succeeded | 267 | 0 |
| 3 | 0 | Succeeded | 275 | 0 |

Deterministic canonical summary:

- source solid entity: `#125`
- solids: 1
- outer shells: 1
- inner shells: 0
- faces: 23
- loops: 28
- edges: 48
- vertices: 32

The canonical counts exactly match the raw STEP inventory for the
manifold solid, closed shell, advanced faces, edge loops, edge curves,
and vertex points.

## 10. Setup Attempts Excluded From Acceptance Matrix

Two controlled setup or discovery attempts are not counted as final
physical acceptance runs:

1. a request using a non-GUID request identifier was rejected with
   `CONTRACT-REQUEST-003` and exit code 2;
2. the first valid-GUID run passed CIRCLE resolution and exposed the
   verified two-edge loop invariant incompatibility.

Neither attempt crashed the worker. Both failures were controlled and
diagnostic.

## 11. Acceptance Decision

P01-B01 acceptance criteria are satisfied:

- the existing CIRCLE mapper is registered in canonical composition;
- no parallel mapping authority was introduced;
- `Circle3D` participates in the canonical curve contract;
- valid two-edge CIRCLE boundary loops are supported;
- invalid loop structures remain rejected;
- R02 passes through the physical worker 3 / 3;
- exit code, status, diagnostics, and canonical summary are deterministic;
- Release build and full regression are green.

Outcome: **PASS WITH VERIFIED SCOPE AMENDMENT**.

The sprint is technically ready for controlled commit and remote
closure.