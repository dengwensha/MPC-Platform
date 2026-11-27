# MPC Sprint Timeline — November 2026

**Document control:** 2026-11-27 | main `7aebe6d2de57916d76ea2d55aa88937120f5c6e0` | DRAFT, monthly reconciliation. October [PR #101](https://github.com/dengwensha/MPC-Platform/pull/101) remains open/unmerged; this proposed November baseline supersedes its current-status sections only upon review. Historical records remain intact. GitHub records inspected; no new local build, physical test or independent Senior review performed. CI316 status directly checked; other CI/test counts and Senior dispositions are attributed to PR records.


| Sequence | Evidence | Status / bounded outcome |
|---|---|---|
| October baseline | [#101](https://github.com/dengwensha/MPC-Platform/pull/101) | Open draft; historical main CI199 1815/1815 |
| Host menu and SR02 records | [#94](https://github.com/dengwensha/MPC-Platform/pull/94), [#100](https://github.com/dengwensha/MPC-Platform/pull/100), [#102](https://github.com/dengwensha/MPC-Platform/pull/102) | Merged; physical menu acceptance bounded; wire/hand regression pins |
| SR-L4 / parser hardening | PRs 103–111 (search records) | Controlled malformed/duplicate input and local diagnostics; bounded closure, not universal parser coverage |
| M2 policy and capture | PRs 112–124 (search records) | Policy A, immutable evidence, same-byte capture; N1 bounded closure; no consumer |
| C1 / header and unit evidence | [#127](https://github.com/dengwensha/MPC-Platform/pull/127) | Merged; §29 acceptance, reported CI274 2048/2048 |
| R-U / target contract | [#136](https://github.com/dengwensha/MPC-Platform/pull/136), [#139](https://github.com/dengwensha/MPC-Platform/pull/139), [#140](https://github.com/dengwensha/MPC-Platform/pull/140) | Merged; mm+radian precheck; exact manifest and owner-bound producer |
| B6 measurement/optimization | [#142](https://github.com/dengwensha/MPC-Platform/pull/142), [#143](https://github.com/dengwensha/MPC-Platform/pull/143), [#144](https://github.com/dengwensha/MPC-Platform/pull/144), [#145](https://github.com/dengwensha/MPC-Platform/pull/145) | Merged; narrow allocation improvement, heap observation and current-source guard |
| B experiment | [#146](https://github.com/dengwensha/MPC-Platform/pull/146) | Merged negative-result documentation; runtime B reverted; CI313 reported 2253/2253 |
| A experiment | [#147](https://github.com/dengwensha/MPC-Platform/pull/147) | OPEN, DO NOT MERGE; CI316 success directly checked; reported 2253/2253 and Worker5/5 |
## December rolling plan (proposed)
| Window | Objective | Acceptance / exclusion |
|---|---|---|
| Week 1 | Owner Windows20/16GB paired run | Exact pins, manifest, per-trial parity, natural peak, allocation/time/retained/startup, headroom; no active checkout/COM change |
| Week 1–2 | Owner/Senior B6 disposition | Explicit allocation tradeoff and startup interpretation; no ceiling increase by default |
| Week 2 | Separate product PR if authorized | Exact source/reason parity, C1/A1 locks, full Windows CI, independent review; no consumer |
| Later | M2 remaining coverage/evaluator and real Worker resource gate | Verify-first design, no Ready claim; conditional on earlier gates |
| Parallel | Native CAD one-face evidence / VD-001 | Independent oracle and provenance; no material certification without evidence |
Historical actual hours not inferred. Replan every two weeks. See [review](../monthly/2026-11-review.md).
