# MPC Master Roadmap — November 2026

**Document control:** 2026-11-27 | main `7aebe6d2de57916d76ea2d55aa88937120f5c6e0` | DRAFT, monthly reconciliation. October [PR #101](https://github.com/dengwensha/MPC-Platform/pull/101) remains open/unmerged; this proposed November baseline supersedes its current-status sections only upon review. Historical records remain intact. GitHub records inspected; no new local build, physical test or independent Senior review performed. CI316 status directly checked; other CI/test counts and Senior dispositions are attributed to PR records.


## Vision and strategy
CAD-independent manufacturing intelligence; CAD is consumed, not recreated. Standalone STEP and thin SOLIDWORKS host remain first-class access modes. Critical path: STEP → semantics → canonical geometry/topology → trusted CAD evidence → feature recognition → tooling/process planning → CNC/CAM. Locked priority CAD analysis → tool library → cutting analysis. Preserve Verify-First, Minimum Intervention, New Build Gate, Completion Gate, DP-001, VD-001, RM-001, EKS-001, SRG-001 and risk-triggered independent review.
## Current gates
- P01 complete with historical findings: R01/R02 PASS, R03 controlled FAIL; no fresh full-pipeline revalidation evidenced this month.
- Source-face menu [#94](https://github.com/dengwensha/MPC-Platform/pull/94) merged after bounded physical acceptance and Senior §11; source-link is not material-side or machine approval.
- CAD-GE03 independent material-side oracle: no new qualifying evidence retrieved; remain open.
- M2 OPEN: C1 owner-bound evidence accepted; exact target manifest ratified; consumer-free target producer merged ([#127](https://github.com/dengwensha/MPC-Platform/pull/127), [#139](https://github.com/dengwensha/MPC-Platform/pull/139), [#140](https://github.com/dengwensha/MPC-Platform/pull/140)). No Ready proof or first consumer.
- R-U reject-before-map correction merged [#136](https://github.com/dengwensha/MPC-Platform/pull/136); no unit conversion.
- B6 resource gate OPEN. Option B reverted; A throwaway [#147](https://github.com/dengwensha/MPC-Platform/pull/147) is not product approval. Private Windows20/16GB, owner allocation tradeoff, startup clarification and separate reviewed product PR remain.
- Tooling TL-03/05/07 production intake/revisions/mapping and M4 remain open absent newer closure evidence. CNC/CAM downstream; no schedule acceleration or percentage invented.
## December sequencing
First owner Windows20 evidence and B6 decision, then independently reviewed product slice only if authorized. Establish resource headroom and remaining M2 evaluator/coverage gates before consumer. Parallel bounded native CAD oracle and verification inventory documentation; no broad UI, persistence, healing, toolpaths or release implied.
## Change control
Monthly review is draft until accepted; independent review at milestones and risk triggers; no retroactive approval. See [timeline](MPC-Sprint-Timeline.md), [milestones](MPC-Milestone-Tracker.md), [decisions](MPC-Engineering-Decision-Register.md), [review](../monthly/2026-11-review.md).
