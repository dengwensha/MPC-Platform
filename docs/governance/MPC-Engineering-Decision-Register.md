# MPC Engineering Decision Register — October 2026
> **Document control:** 2026-10-30 | Baseline: `main@857808225aa9a27e98c51e9a4839d8227a5fddee` (2026-10-28) | Status: **DRAFT / monthly reconciliation**. This is a proposed repository documentation baseline; it does not supersede previously approved architecture decisions until reviewed/merged. GitHub-only evidence, not a new local build or physical acceptance.


Status vocabulary: **Accepted** (explicit recorded direction), **Implemented bounded** (observed code behavior, not policy ratification), **Proposed**, **Open**, **Deferred with trigger**, **Historical closed**. These are deliberately not interchangeable.

| ID | Decision / boundary | Status | Evidence / action |
| --- | --- | --- | --- |
| GOV-01 | Verify-First, one objective, build/full tests, clean git and commit trace | Accepted | Historical engineering governance; use in each sprint. |
| GOV-02 | DP-001, VD-001, RM-001, EKS-001 documentation/verification principles | Accepted project direction | Preserve in Master Roadmap; publication-ready documents, evidence and trace. |
| GOV-03 | Independent Senior review is **risk-triggered**, not every PR | **Accepted 2026-10-28** | [Rule and six triggers](../reviews/Risk-Based-Senior-Review.md): wire/serialization, enum numeric, trust/fail-closed, IPC, persistence, UI production entry. |
| ARCH-01 | CAD-independent canonical kernel; thin SOLIDWORKS host + standalone STEP | Accepted | Host net48 x64, Worker .NET 8, Contracts boundary. |
| ARCH-02 | STEP source identity = document SHA-256 + STEP source face ID; canonical face index is session-local | Implemented bounded | [TOOL-01D discovery](../reviews/Tooling/TOOL-01D-00-CAD-Tooling-Interface-Discovery.md); no cross-export continuity claim. |
| ARCH-03 | Source-link verification must not become physical/material/machine approval | Accepted + implemented fail-closed | `SourceLinkOnly` success only; MaterialSideDependent blocked. |
| ARCH-04 | Application is composition location, but **no Application-to-Tooling reference approved** | Accepted bounded location | [Scope record](../reviews/Tooling/TOOL-01D-01-07-Scope-and-SR02-Disposition.md). |
| TOOL-01 | Turning and milling share core, separate tool models | Accepted planning direction | TL-06; production sufficiency remains unverified. |
| TOOL-02 | Tool compatibility result is `InterfaceOnly`; Unknown hand and null mounting fail closed | Implemented bounded | PR #85; no physical suitability. |
| TOOL-03 | Preserve 0.001 mm / 0.001 degree tolerance behavior | Accepted | Dedicated tolerance/rounding verification remains separate. |
| TOOL-P3 | Raw EAN preserved; GTIN format state separate | Implemented bounded; historical policy approval trace open | [Policy record](../reviews/Tooling/TOOL-01C-03-Policy-Decision-Record.md); format ≠ authenticity/uniqueness. |
| TOOL-P6 | Unknown hand/mounting rejection and stable enum interpretation | Implemented bounded; historical policy approval trace open | `Unknown=0, Left=1, Right=2, Neutral=3`; future enum change triggers Senior. |
| TOOL-P1/P2/P4/P5 | Assembly history, unknown-unit intake, master insert raw text, identity normalization | Proposed / **ratification not evidenced** | Do not convert to accepted production policies without current explicit decision. |
| TOOL-P7 | Bounded Application source-link location | Reported accepted direction; limited implementation | No full CAD/tooling bridge authorization. |
| SR02-H1/H2 | Reconcile discovery NO-GO and policy record with delivered code | Documentation reconciliation delivered; historical gaps not retroactively approved | PR #96 merged; #100 open status update. |
| SR02-M1 | Malformed v1.1 DTO yields v1.1 RequestRejected | Implemented/merged | PR #97; full main CI #199. |
| SR02-M2 | Aggregate `MissingRegion` retained; diagnostic separation | **Deferred with trigger / OPEN** | Revisit before catalog importer begins or end-user release, whichever first; linked L4. |
| SR02-M3 | Worker v1.1 does not emit `FaceMismatch` or `InvalidSourceIdentity` | Documented bounded behavior | PR #99; no enum change. |
| SR02-M4 / TL-03/05/07 | Production intake, durable revision and real source mapping | **OPEN** | Test-local evidence is not production closure. |
| SR02-M5 | Remove supplier identity from synthetic evaluator fixtures | Implemented/merged for evaluator fixtures | PR #98; residual other fixture labels separate. |
| SR02-L4 | Unexpected parser exception diagnostics | **OPEN / linked to M2** | Fail-closed Host does not mean rich error classification; no broad catch without decision. |
| HOST-94 | Controlled SOLIDWORKS menu/callback | **Proposed unmerged** | PR #94; independent final diff review + physical acceptance required. |
| CAD-GE03 | Material-side/normal orientation certification | **OPEN / fail closed** | R01 0/11, R02 0/23 independently qualified in physical checklist; no certification. |
| HIST-SR02 | September P01 M-01..M-04 | **Historical independently verified closed** | [September reconciliation](../reviews/P01/SR02-Final-Independent-Verification-Reconciliation.md); not October Tooling findings. |

## Decision quality and changes
A merged implementation proves delivered behavior, not a retroactive policy approval. A current owner ratification can set a new effective date, never fabricate a past one. Open risk triggers and Senior decisions are preserved until explicit closure evidence exists.

## November decision requests (not pre-approved)
1. #94 independent changed-head review disposition and physical acceptance/merge authorization.
2. M2/L4 diagnostic separation versus bounded continued deferral **before** importer/release trigger.
3. TL-03/05/07 production intake/revision design; decide owner, provenance, persistence and negative-case acceptance before coding.
4. CAD-GE03 native CAD material-side evidence sufficiency, including point-local vs full-face qualification.
5. #100 documentation merge decision; factual update does not close SR02.

Source-of-truth crosslinks: [Roadmap](MPC-Master-Roadmap.md) · [Backlog](MPC-Backlog.md).
