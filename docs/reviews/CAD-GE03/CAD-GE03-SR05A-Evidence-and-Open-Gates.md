# CAD-GE03 — SR05A Senior Review evidence and open gates

Baseline verified: `main` at `85699a2199387577737ad6aa445a9a57d25b22b6` (2026-10-24). Documentation-only inventory; not independent production approval.

## Independently reported findings vs delivered mitigations

| Review finding | Delivered evidence | Closure limitation |
| --- | --- | --- |
| R01 — B-spline normal scale / regularity | PR #27, relative normal regularity guard and regressions | Independently re-evaluate other endpoint degeneracies before unrestricted use |
| R02 — unverified outward normal claim | PR #29, public ungated numerical classifier removed, Evaluate fails closed | **Material-side convexity/concavity is NOT enabled**; outward orientation proof still unavailable (U-1/U-2) |
| R03 — both-plane edge membership | PR #28, shared-edge endpoint membership validation | Validated within implemented planar scope |
| R04 — real STEP accuracy evidence | PRs #31–#36: R01/R02 observed histograms, T001 right-angle oracle, R03 finite difference normal comparisons | Source/count golden gates alone do not prove every true signed dihedral or all geometric accuracy |
| R05 — signed dihedral regression coverage | PR #30 angle set, reversal/invariance tests | Does not prove material-side classification |
| R06 — local tangent status ambiguity | PR #38 local unoriented tangent-plane coincidence terminology | Does not certify oriented G1 or edge-wide continuity |
| R07 — eligible samples misreported | PR #37 rename to eligible interior sample count | Three eligible samples are not three measured G0/tangency witnesses |
| R08 — mismatched tolerances | PRs #39–#41 bounded G0, analytic normals, local angle propagation | Global universal tolerance policy not proved; legacy overloads remain |
| R09 — duplicated / dead B-spline evaluation | PR #42 removed dead Coordinate helpers; PR #44 adds degree 2/3 normal benchmarks | **Local benchmark timings and allocation data pending**; no optimization decision |
| R10 — implicit layers and CI | PR #43 adds main push full validation trigger; project references reviewed below | No proof that all architectural boundaries are statically enforced; post-merge push CI success was user-reported, not independently retrieved here |

## R10 repository architecture inspection

- `src/MPC.Application/MPC.Application.csproj` directly references Connectors, Connectors.Step, Mapping, Mapping.Step and Topology, without an explicit Geometry ProjectReference.
- `src/MPC.Mapping/MPC.Mapping.csproj` references Geometry and Topology. Geometry availability to Application is therefore transitive; that is a *maintenance risk*, not proof of runtime failure.
- `src/MPC.Worker/MPC.Worker.csproj` references Application and Contracts; it remains the out-of-process worker project.
- `src/MPC.sln` includes the Host, Application, Worker, geometry/mapping, tests and benchmark projects.
- `.github/workflows/mpc-full-validation.yml` triggers `push` on main, `pull_request` on main and manual dispatch, Windows Release/full regression/real STEP probes. The GitHub commit-runs connector used in this review filters to PR runs, so post-merge *push* run cannot be independently verified via that endpoint.

## Gates before senior closure / manufacturing release

1. Collect same-machine .NET 8 Release BenchmarkDotNet results for R09B, with `Mean`, `Error`, `Allocated`, runtime/CPU details. No rewrite without evidence.
2. Obtain independent, attributable GitHub Actions main push run link, commit SHA and SUCCESS evidence (user reported successful; independent reference pending).
3. Decide whether explicit Geometry ProjectReference is desired and test independently in a dedicated, risk-bounded PR; do not change architecture silently.
4. Independently review R04 oracle coverage and R01 singular edge cases.
5. Resolve or explicitly defer U-1/U-2 verified outward material orientation, retaining `Indeterminate` for unproven classifications.
6. Conduct final independent Senior Review on a frozen, identified `main` commit; production acceptance is NOT implied by prior green CI.

No software, CI, or public API changes made in this document-only sprint.
