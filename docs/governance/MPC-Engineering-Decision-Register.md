# MPC Engineering Decision Register — November 2026

**Document control:** 2026-11-27 | main `7aebe6d2de57916d76ea2d55aa88937120f5c6e0` | DRAFT, monthly reconciliation. October [PR #101](https://github.com/dengwensha/MPC-Platform/pull/101) remains open/unmerged; this proposed November baseline supersedes its current-status sections only upon review. Historical records remain intact. GitHub records inspected; no new local build, physical test or independent Senior review performed. CI316 status directly checked; other CI/test counts and Senior dispositions are attributed to PR records.


| Record | Status | Scope / evidence |
|---|---|---|
| GOV | Accepted historical | Verify-First, DP-001, VD-001, RM-001, EKS-001, SRG-001; risk-triggered Senior review |
| HOST-94 | Merged, bounded physical acceptance | [#94](https://github.com/dengwensha/MPC-Platform/pull/94) §11; no material approval |
| SR02-H1/H2/M1/M3/M5 | Bounded reconciliation/closure | [#100](https://github.com/dengwensha/MPC-Platform/pull/100) merged; historical policy gaps not retroactively ratified |
| SR-L4 | Bounded closure reported | No universal malformed-input or diagnostic guarantee |
| Policy A | Owner accepted, bounded implementation | Whole-inventory Error/Fatal blocks future consumer; no completeness proof |
| N1 | Bounded closed per PR records | Pinned R01/R02/R03 path/capture parity; no generalization |
| C1 | Accepted bounded | [#127](https://github.com/dengwensha/MPC-Platform/pull/127) §29, owner-bound same-capture evidence |
| Header v2 | Bounded policy | Level 1/2;1 and exact AP214 qualifier; no broad schema inference |
| R-U | Implemented merged | [#136](https://github.com/dengwensha/MPC-Platform/pull/136) mm+radian reject-before-map |
| M2 manifest v1 | Ratified | [#139](https://github.com/dengwensha/MPC-Platform/pull/139) §38 A1–A3; producer [#140](https://github.com/dengwensha/MPC-Platform/pull/140) merged, M2 OPEN |
| B6 source guard | Accepted | [#145](https://github.com/dengwensha/MPC-Platform/pull/145) §43 |
| B option | Rejected, reverted | [#146](https://github.com/dengwensha/MPC-Platform/pull/146) §44 supersedes §43 wording: no meaningful natural peak reduction; tiny startup allocation informational |
| A experiment | Authorized experiment only | [#147](https://github.com/dengwensha/MPC-Platform/pull/147) DO NOT MERGE; owner tradeoff and private Windows evidence pending |
| Startup bound | OPEN | Clarify 1MiB total vs incremental; baseline sometimes exceeds total |
| M4/TL-03/05/07, historical P1/P2/P4/P5 | OPEN / ratification not evidenced | No production intake/revision closure |
## §44 acceptance bounds
R03 natural peak ≥15% reduction EACH trial; time ≤20% R03 / ≤25% baseline >1s files; first ≤10s; warm allocation ≤30%; retained ±1MiB; initialization ≤1MiB (interpretation open). Noise below inter-trial variation is not speedup. If peak passes but time/allocation exceeds, owner tradeoff required. Linux and Windows separate. No budget increase or product approval inferred.
## Pending decisions
Owner allocation tradeoff, startup definition, Windows20/16GB and real Worker headroom/resource limits; separate reviewed product PR; first consumer/evaluator design and CAD physical oracle. No decision fabricated.
