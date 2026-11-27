# MPC Master Roadmap — October 2026 reconciliation
> **Document control:** 2026-10-30 | Baseline: `main@857808225aa9a27e98c51e9a4839d8227a5fddee` (2026-10-28) | Status: **DRAFT / monthly reconciliation**. This is a proposed repository documentation baseline; it does not supersede previously approved architecture decisions until reviewed/merged. GitHub-only evidence, not a new local build or physical acceptance.


## 1. Vision and product boundary
MPC is a **CAD-independent manufacturing intelligence platform**. CAD systems are inputs and optional hosts; MPC consumes, validates and reasons over engineering data. SOLIDWORKS is a thin .NET Framework 4.8 x64 host; canonical geometry, topology and manufacturing logic are CAD-independent. Standalone STEP remains a first-class mode. The strategic dependency is **STEP → semantics → canonical geometry/topology → manufacturing evidence/features → tooling/process planning → CAM**.

Locked principles: Verify-First; minimum intervention; one objective per micro-sprint; no invented evidence; deterministic fail-closed boundaries; measured optimization; documentation and independent verification as engineering deliverables. Governance references: DP-001, VD-001, RM-001, EKS-001 and risk-based Senior review.

## 2. October status — do not flatten distinct gates
| Workstream | Status at review | Evidence / limitation |
| --- | --- | --- |
| Canonical STEP / B-Rep | Established baseline; compatibility incomplete | P01 closed **with findings**, frozen real corpus R01 PASS, R02 PASS, R03 controlled FAIL; later B14/B15 mapping additions do not retroactively make R03 PASS. |
| CAD analysis / GE03 | Implemented numerical/geometry safeguards; physical qualification OPEN | No independently verified material-side classification; `Indeterminate` remains correct where evidence is absent. R01 physical acceptance 0/11, R02 0/23 in recorded checklist. |
| Tool library / TOOL-01A..C | Controlled domain and specification proof advanced | Unknown hand and unmapped mounting fail closed; `InterfaceOnly` is not physical suitability. Production catalog intake, durable revisions and mapping remain OPEN (TL-03/05/07). |
| CAD source-link / TOOL-01D | Bounded Application → Contracts v1.1 → Worker → Host path merged; standalone acceptance available | This is **source-link only**, not CAD/tooling integration or material approval. |
| TOOL-01D-06 menu | **OPEN PR #94, NOT MERGED** | Final branch CI #201 PASS at `144d2b6` (1848/1848); independent changed-diff Senior review and SOLIDWORKS physical acceptance still required. |
| SR02 Tooling review | **CONDITIONAL**, not project-wide closed | H1/H2 documentation reconciliation delivered; M1/M3/M5 corrected/clarified; M2 diagnosis deferred with explicit trigger, M4/TL production gaps and L4 remain open. |
| SR02 documentation reconciliation | **OPEN PR #100, NOT MERGED** | CI #200 PASS at `677f9a0` (1815/1815); no production change. |
| Older P01 Mandatory M-01..M-04 | **VERIFIED CLOSED** | Distinct historical September SR02; do not confuse with October Tooling SR02-M1..M5. |

## 3. Project critical path and sequencing
**Locked workstream priority:** CAD analysis → Tool library → Cutting/CNC analysis. A small parallel tooling stream is allowed, but CAD physical evidence and verification remain the highest product-readiness dependency.

**November gate order:**
1. Close October documentation reconciliation without claiming overall SR02 closure; review/merge #100 if approved and green.
2. Independent Senior review of the **final #94 diff** (UI entry, IPC/result interpretation), then controlled SOLIDWORKS 2024 SP1 physical acceptance, then merge decision; CI alone is insufficient.
3. CAD-GE03B physical one-face pilot: independently map one R01 native CAD face, provenance, point/normal, transform, material-side occupancy, uncertainty; preserve Unverified unless independently demonstrated.
4. Maintain bounded M2/L4 diagnostic decision before catalog importer work or any end-user release, whichever occurs first.
5. Only after explicit gates: production catalog-intake/revision architecture decision, then narrow implementation; CNC cutting analysis remains downstream of reliable CAD and tool evidence.

## 4. Product milestone gates
- **Architecture gate:** no vendor-specific identity in canonical core; no Tooling→SOLIDWORKS dependency; Application composition does not imply authorization to add a Tooling reference.
- **Trust gate:** exact document SHA-256 and source STEP face ID; no index-as-identity; no physical/material/machine approval from `SourceLinkMatched`.
- **Quality gate:** Release build, targeted/full regression, current-head CI, clean diff, decision trace and independent review **only when a written risk trigger applies**.
- **Physical gate:** native CAD evidence and independently reviewed measurement; CI and mocked UI are not a physical PASS.
- **Documentation gate:** DP-001 publication readiness; current decisions and findings must match the actual code/PR baseline.

## 5. Explicit exclusions
No generalized catalog importer, persistence, automatic tool selection, machining simulation, physical material-side certification, toolpaths, NC output, deployment, automatic selected-face mapping or broad UI scope is authorized by this monthly document. P01-R03 remains a bounded compatibility gap.

## 6. Roadmap change log
- 2026-10-30: Added TOOL-01A..D, CAD-GE03B physical gate, current SR02 findings, risk-triggered Senior rule, and separated main CI from unmerged PR CI.
- This document is a **review proposal**, not a retroactive approval or a change to production code.

## References
- [P01 real-part closure](../reviews/P01/P01-CL01-Final-Real-Part-Revalidation-and-Closure-Assessment.md)
- [CAD-GE03 open gates](../reviews/CAD-GE03/CAD-GE03-SR05A-Evidence-and-Open-Gates.md)
- [TOOL-01D scope and SR02](../reviews/Tooling/TOOL-01D-01-07-Scope-and-SR02-Disposition.md)
- [Risk-based Senior review](../reviews/Risk-Based-Senior-Review.md)
- [Monthly review](../monthly/2026-10-review.md)
