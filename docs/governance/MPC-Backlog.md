# MPC Backlog — November 2026

**Document control:** 2026-11-27 | main `7aebe6d2de57916d76ea2d55aa88937120f5c6e0` | DRAFT, monthly reconciliation. October [PR #101](https://github.com/dengwensha/MPC-Platform/pull/101) remains open/unmerged; this proposed November baseline supersedes its current-status sections only upon review. Historical records remain intact. GitHub records inspected; no new local build, physical test or independent Senior review performed. CI316 status directly checked; other CI/test counts and Senior dispositions are attributed to PR records.


| Priority | Item | State | Acceptance / dependency |
|---|---|---|---|
| P0 | B6-A Windows20 owner 16GB | NOT EXECUTED per [#147](https://github.com/dengwensha/MPC-Platform/pull/147) | Pinned paired trials, exact projections/reasons, peak/time/allocation/retained/startup and headroom; private CAD stays private |
| P0 | B6-A owner tradeoff | OPEN | Explicit acceptance/rejection of ~35.36% allocation increase vs 25–28% peak gain; no automatic product acceptance |
| P0 | Startup interpretation | OPEN | Senior/owner clarify total vs incremental 1MiB |
| P0 | B6 product slice | GATED | Separate PR, exact parity, Windows CI, independent review; experiment never merged |
| P0 | Real Worker resource limits/headroom | OPEN | Before first consumer; no budget increase by inference |
| P0 | M2 remaining coverage/evaluator/consumer | OPEN / GATED | Verify-first design, A1/C1, physical occurrence/geometry evidence, fail closed; no Ready without proof |
| P1 | CAD-GE03 one native face | OPEN | Independent identity/transform/material occupancy/uncertainty witness |
| P1 | VD-001 | PLANNED | Golden/real model inventory, stage coverage, hashes, controlled failures; private data excluded |
| P1 | TL-03/05/07 and M4 | OPEN | Approved production intake, durable revisions, mapping; negative tests |
| P2 | P01 R03 coverage | OPEN | Narrow discovery and fresh full-pipeline rerun; raw inventory success insufficient |
| P2 | Tool policy/rounding | OPEN | Explicit ratification and boundary tests |
| P3 | CNC/CAM/P10 UI | DEFERRED | Separate scope after CAD/tooling evidence |
## Closed / historical
#94 menu, #100 factual reconciliation, #102 regression pins merged; C1 accepted bounded; R-U and target producer merged. B rejected/reverted, not a shipped optimization. Older P01 Mandatory M-01..04 remain historical closed. October #101 open; reconcile drafts before merging either documentation PR.
Each task needs owner, exact commit, acceptance/out-of-scope, file/test plan, build/regression, clean git and risk-triggered review. No unexecuted physical PASS.
