# MPC Product & Engineering Backlog — 30 October 2026
> **Document control:** 2026-10-30 | Baseline: `main@857808225aa9a27e98c51e9a4839d8227a5fddee` (2026-10-28) | Status: **DRAFT / monthly reconciliation**. This is a proposed repository documentation baseline; it does not supersede previously approved architecture decisions until reviewed/merged. GitHub-only evidence, not a new local build or physical acceptance.


Priority order remains **CAD analysis → Tool library → Cutting/CNC analysis**. No dates or hours are represented as historical actuals unless independently recorded. A task marked planned is not authorized production scope.

| Priority | ID | Objective / scope | State | Gate / acceptance | Dependency |
| --- | --- | --- | --- | --- | --- |
| P0 | DOC-11A | Review/merge SR02 status reconciliation PR #100 | OPEN PR | Two-doc factual diff; CI #200 verified; no false closure/approval claims | Current main, owner merge decision |
| P0 | HOST-11A | Independent review of final PR #94 changed diff | OPEN / BLOCKING | Reviewer checks `144d2b6` UI entry, IPC/result trust, messages; renewed review if diff changes | #94 exact head |
| P0 | HOST-11B | Physical SOLIDWORKS 2024 SP1 menu acceptance | NOT EXECUTED | InputBox foreground, correct file/face prompt, hash check, blocked responses, cancellation, large-file behavior, logs | HOST-11A; local workstation |
| P0 | CAD-11A | One R01 native CAD face independent material-side pilot | NOT EXECUTED | Native CAD provenance, STEP hash, face identity, transform, interior sample, occupancy both sides, uncertainty; fail closed otherwise | Native SLDPRT / CAD license / reviewer |
| P0 | SR02-11A | Keep M2/L4 bounded deferral and trigger visible | OPEN / BOUNDED | Decision before importer or end-user release; neutral operator message; no guessed cause | Host-94 UI and import planning |
| P1 | TOOL-11A | TL-03 production source intake mapper and units | OPEN / DESIGN GATE | Provenance, version, exact units, reject unknown/missing critical fields; production negative tests | Policy ratification, supplier data |
| P1 | TOOL-11B | TL-05 durable historical revision/assembly resolution | OPEN / DESIGN GATE | Old revision remains resolvable after new revision; transaction/identity/retention evidence | Persistence decision |
| P1 | TOOL-11C | TL-07 source-to-domain mapping and revision binding | OPEN / DESIGN GATE | Real validated source envelope, no test-local-only proof | TOOL-11A/B |
| P1 | CAD-11B | Independent GE03 oracle coverage/singularity review | OPEN | Evaluate true signed dihedral limits, normal regularity near poles and evidence classification | CAD-11A and GE03 senior record |
| P1 | PERF-11A | Same-machine Release B-spline benchmarks | PENDING | Mean/Error/Allocated + environment; no optimization before measurements | Local workstation |
| P1 | VD-11A | Formalize verification dataset model/coverage matrix | PLANNED | R01/R02/R03 hashes, golden statuses, per-stage coverage and failures; protect private source bytes | CAD/P01 evidence |
| P2 | STEP-11A | R03 rational/complex B-spline curve/surface gap | BACKLOG | Narrow Verify-First discovery then capability-specific tests; physical rerun | R03 reproducible corpus |
| P2 | TOOL-11D | Vendor vocabulary, Neutral/mounting and EAN identity policy | OPEN | Explicit ratification, controlled fixtures, negative cases | TOOL-01C decisions |
| P2 | TOOL-11E | Separate tolerance and rounding verification | PLANNED | Preserve existing tolerance; detailed boundary tests | Tooling baseline |
| P2 | CNC-11A | Cutting/CNC analysis discovery | DEFERRED | Depends on CAD evidence and tooling qualification; no premature toolpaths | CAD-11A, TOOL-11A/B/C |
| P3 | P10-UI | Common standalone/CAD-integrated UI model | DEFERRED | Separate product scope; #94 is not general UI approval | Verified interaction boundary |
| P3 | SIM-01 | Simple CAD simulation | DEFERRED | Explicit later scope decision | Core product readiness |

## Items closed but not erased
- September P01 senior M-01..M-04: verified closed (separate historical work).
- October SR02-H1/H2 documentation reconciliation, M1 envelope correction, M3 outcome clarification, evaluator-fixture M5: delivered/merged **in their bounded scope**. The whole SR02 is **not closed**.
- TOOL-01D-01..05 and 07: bounded source-link delivery complete, not physical tool approval.

## Next two-week execution
1. Documentation PR #100 disposition; exact-head status sync.
2. #94 independent review; only then local physical test, followed by merge decision.
3. One R01 native face pilot in an evening local session.
4. In parallel, bounded M2/L4 owner decision and TL-03/05/07 discovery—no production importer before gate.

## Backlog hygiene
Each item records a single target, explicit out-of-scope, test/physical acceptance, source commit/PR, owner decision and closure evidence. No “PASS” from an unexecuted workstation check, and no “production ready” from synthetic or specification-only tests.

See [Milestone Tracker](MPC-Milestone-Tracker.md) and [Monthly Review](../monthly/2026-10-review.md).
