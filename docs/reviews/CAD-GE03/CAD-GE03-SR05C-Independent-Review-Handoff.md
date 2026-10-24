# CAD-GE03 — SR05C Independent Senior Review handoff

**Candidate freeze:** `main` `156c1b60b27376d31a30a80c0e30ba1b7956198c` (verified unchanged at preparation).
**Decision:** REVIEW READY WITH OPEN GATES — **NOT production accepted**.
**Scope:** read-only evidence reconciliation and reviewer instructions; no implementation edits.

## Delivered change sets and evidence

| Review item | Delivery | Scope / reservation |
| --- | --- | --- |
| R01 | PR #27 | Regular normal guard; edge/singular cases still require reviewer scrutiny |
| R02 | PR #29 | Fail-closed `Indeterminate` when outward material proof is absent; U-1/U-2 remain unresolved |
| R03 | PR #28 | Both-plane endpoint membership |
| R04 | PR #31–#36 | Real STEP count/histogram gates, analytic golden and finite-difference normal comparisons; does not prove all signed edge angles |
| R05 | PR #30 | Signed-angle and invariance regressions |
| R06 | PR #38 | Unoriented tangent plane naming, no oriented G1 claim |
| R07 | PR #37 | Eligible sample count distinguished from measured witnesses |
| R08 | PR #39–#41 | Explicit tolerance propagation in bounded local queries, not system-wide proof |
| R09 | PR #42 and #44 | Dead helper removal and BenchmarkDotNet fixture; performance measurement still pending |
| R10 | PR #43 and #46 | Main push CI trigger and explicit Application -> Geometry reference; full static architecture enforcement not established |
| Review evidence | PR #45 | Previous evidence matrix; historical architecture finding updated by PR #46 |

## Verified integration evidence and known limits

- PR #46 updated head `37b902bf4ca026e4e52584e1908e68e1ebc665c0`, PR CI run `40081296406` **SUCCESS**, merged into `156c1b60b27376d31a30a80c0e30ba1b7956198c`.
- PR #45 documentation CI `40080762827` **SUCCESS**, merged immediately prior to PR #46.
- `main` comparison to `156c1b60b27376d31a30a80c0e30ba1b7956198c` returned identical when this handoff was prepared.
- Main push CI event SUCCESS has been reported by operator, but no independent event-specific workflow run URL/ID is available through the GitHub connector used here. Do not relabel that report as independently verified.
- Private real STEP corpus CI supports mapped-case validations; independent geometric oracle coverage remains bounded.

## Review questions / required judgments

1. Is the relative-regularity guard adequate for B-spline singular endpoints outside tested fixtures?
2. Are R04's actual independently known angle oracles sufficient, and which signed-angle edges still need a trusted reference?
3. Confirm fail-closed R02/U-1/U-2: no positive material concavity/convexity classification may be claimed without verified outward normals and orientation mapping.
4. Review bounded R08 linear tolerance API and legacy overload policy for remaining inconsistencies.
5. Review direct Application -> Geometry dependency and CI main push trigger; separately inspect whether broader architecture boundary enforcement is warranted.
6. Do R09B results reveal material performance issue? **Pending same-machine BenchmarkDotNet measurements.**
7. Is the post-merge main push validation attributable to exact SHA? **Pending independently accessible Actions run.**

## Deferred local test package

- On prepared machine with .NET 8 SDK and Git, checkout exactly `156c1b60b27376d31a30a80c0e30ba1b7956198c` and run:
  `dotnet run -c Release --project benchmarks/MPC.Benchmarks -- --filter "*BSplineNormalQueryBenchmarks*"`
- Capture hardware/OS/.NET version, BenchmarkDotNet summary (Mean, Error, Allocated) and result artifacts; do not infer a speedup from a baseline alone.
- SOLIDWORKS physical verification is a separate host/interop acceptance gate, not fulfilled by the synthetic benchmark.

## Acceptance boundary

This package authorizes an **independent review request only**. It is not release approval, not an assertion of edge-wide G1, and not verified material-side classification. Any additional required corrections must be separate micro-sprints with fresh build/full test and commit-specific traceability.
