# MPC Architecture Documentation — October 2026 boundary snapshot
> **Document control:** 2026-10-30 | Baseline: `main@857808225aa9a27e98c51e9a4839d8227a5fddee` (2026-10-28) | Status: **DRAFT / monthly reconciliation**. This is a proposed repository documentation baseline; it does not supersede previously approved architecture decisions until reviewed/merged. GitHub-only evidence, not a new local build or physical acceptance.


## 1. Layered responsibility
```text
SOLIDWORKS 2024 SP1 (optional)       Standalone STEP caller
       |                                    |
MPC.Host.SolidWorks (net48 x64)             |
       |       versioned IPC                |
       +------------> MPC.Contracts <-------+
                            |
                       MPC.Worker (net8)
                            |
                     MPC.Application
                       /          \
              Mapping.Step     source-link gate
                 |                  |
            Mapping + STEP      ManufacturingRegion
                 |
       Topology / Geometry / Validation

MPC.Tooling (independent bounded domain)
[NO approved runtime Application-to-Tooling bridge]
```
Diagram conveys ownership, not every direct project reference. Actual `MPC.Application` references Connectors, Connectors.Step, Mapping, Mapping.Step and Topology; Geometry is transitive through Mapping at the inspected CAD-GE03 baseline. The transitive reference is a maintainability decision, **not** proof of runtime failure.

## 2. Canonical and STEP boundary
- Canonical Geometry/Topology must remain CAD-independent; STEP entities are parsed/semantic inputs, not a general CAD vendor API.
- Geometry and topology are source-linked without coincidence-based topology mutation/healing by default.
- Standard STEP toroidal semantics and exporter-scoped signed-toroidal compatibility remain separate.
- R03 failure is a capability/coverage gap, not permission to silently normalize or certify unsupported geometry.

## 3. CAD analysis evidence boundary
- `ManufacturingRegion` records the exact imported STEP document fingerprint, source STEP face ID and session-local canonical face ID; enumeration index is **not** source identity.
- A planar face/source-link match does not certify material side, outward orientation, machine fit, physical tooling suitability or full-face accuracy.
- Independent native CAD occupancy/oracle evidence is not yet present for R01/R02; `Indeterminate` and `Unverified` must remain visible.
- T001 synthetic and real STEP numerical checks are supporting evidence, not a substitute for native CAD physical review.

## 4. Tooling domain boundary
- `MPC.Tooling` models manufacturer/product/grade/holder/interface and bounded insert-holder interface compatibility.
- `ToolHand.Unknown=0`; unmapped mounting cannot approve match. Known Neutral is characterized, not physically certified.
- EAN/GTIN format checking does not prove supplier authenticity or product identity; do not deduplicate products from raw record equality.
- The evaluator returns `InterfaceOnly`; no collision-free, machine, material-side, assembly or toolpath approval.
- Catalog source snapshot hashes retained bytes; this does not provide a production importer, supplier authentication, persistent historical revision or catalog merge policy.

## 5. v1.0 vs v1.1 process boundary
- Legacy v1.0 `AnalyzeModel` remains separate and is not a fallback path for v1.1 source-link requests.
- v1.1 `ValidateRegionSource` is explicit; requests/results are correlated by version, operation and exact RequestId; Host also checks exit/result consistency.
- Source validation reads a private STEP byte snapshot once for hashing and mapping; rejects mismatched digest or source face; material-dependent evaluation remains blocked.
- `SourceLinkMatched` means only a supported imported region from the supplied bytes and explicit source face ID matched the expected link.
- `MissingRegion` aggregates several construction failures; it is **not** proof the face alone is absent. `FaceMismatch` and `InvalidSourceIdentity` are defined but not currently emitted by Worker v1.1; no enum renumbering.
- M1 malformed typed v1.1 requests now use the v1.1 rejection envelope; no invented RequestId or legacy relaxation.
- Unexpected parser exceptions (L4) and diagnostic separation (M2) are tracked under a bounded release/importer trigger; no fabricated detailed reason codes.

## 6. UI and physical gate
- Main at `8578082` has **no merged SOLIDWORKS menu entry** for this source-link operation.
- PR #94 (`144d2b6`) introduces controlled explicit STEP ID/menu callback and guarded messages, but is OPEN.
- CI #201 confirms automated cases, **not** an installed/registered/live SOLIDWORKS interaction. Final diff independent Senior review and local physical acceptance are pre-merge gates.
- No automatic selected-face mapping, broad UI framework, deployment or CAD mutation is authorized.

## 7. Architectural non-goals / deferred changes
No speculative B-spline optimization, healing/sewing, broad exception swallowing, hash-before-parse behavior change, new enum, persistence, supplier importer, automatic tool selection, NC or machine qualification without separate Verify-First decision.

## 8. Decision and review linkage
See [Decision Register](MPC-Engineering-Decision-Register.md), [Risk-Based Senior Review](../reviews/Risk-Based-Senior-Review.md), [TOOL-01D scope](../reviews/Tooling/TOOL-01D-01-07-Scope-and-SR02-Disposition.md), and [CAD-GE03 gates](../reviews/CAD-GE03/CAD-GE03-SR05A-Evidence-and-Open-Gates.md).
