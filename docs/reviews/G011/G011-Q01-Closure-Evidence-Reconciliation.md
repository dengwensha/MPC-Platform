# G011-Q01 Closure Evidence Reconciliation

## Review Record

- Work Item: G011-Q01
- Classification: VERIFY
- Review Date: 2026-08-25
- Branch: main
- Start Commit: b3d5046a28ad1a8f4271b9d067bab80019678922
- Reviewed HEAD: b98f0f35936cc7dafeab09429ee4ef3581005319
- Reviewed Product Baseline: b3d5046a28ad1a8f4271b9d067bab80019678922
- Closing Commit: assigned by this documentation commit
- Production Code Change: None
- Final Decision: PASS

## Purpose

G011-Q01 reconciles the G010 closure claims against the repository, GitHub state, project dependency graph, production source, test discovery, and newly reproduced local Windows build and regression results.

This VERIFY sprint introduces no production-code or product-scope change.

## Repository Reconciliation

The sprint began with local `main` at `b3d5046`. After fetch, `origin/main` was found at `b98f0f3`. The clean local branch was advanced by fast-forward.

GitHub independently confirmed that `b98f0f3` exists and adds only:

- `docs/reviews/G010/G010-CR01-Final-Closure-Report.md`
- `docs/reviews/G010/G010-Independent-Architecture-Review.md`

No production code was changed by the G010 closure commit.

## Evidence Classification

Four evidence classes are kept distinct:

1. repository evidence;
2. reproduced local Windows execution evidence;
3. previously recorded live SOLIDWORKS evidence;
4. GitHub CI workflow or commit-status evidence.

Local execution evidence must not be represented as CI reproduction.

## Evidence Ledger

| ID | Claim | G011-Q01 result | Disposition |
|---|---|---|---|
| EV-01 | Real net48 x64 ISwAddin host | PASS | Confirmed |
| EV-02 | Host depends only on MPC.Contracts | PASS | Confirmed |
| EV-03 | Versioned transport-neutral contract | PASS | Confirmed |
| EV-04 | Contract has no project dependency | PASS | Confirmed |
| EV-05 | Isolated net8 x64 worker | PASS | Confirmed |
| EV-06 | Worker has no SOLIDWORKS dependency | PASS | Confirmed |
| EV-07 | Application owns STEP orchestration | PASS | Confirmed |
| EV-08 | Canonical mapping and topology authority are reused | PASS | Confirmed |
| EV-09 | ContractVersion and RequestId validation | PASS | Confirmed |
| EV-10 | No active SOLIDWORKS model mutation | PASS | Confirmed |
| EV-11 | Release solution build | PASS | 0 warnings, 0 errors |
| EV-12 | Host regression | PASS | 66 of 66 passed |
| EV-13 | Full regression | PASS | 1091 of 1091 passed |
| EV-14 | Live SOLIDWORKS 2024 smoke | PASS WITH QUALIFIER | Recorded G010 evidence; not rerun here |
| EV-15 | COM registration validation | PASS WITH QUALIFIER | Recorded G010 evidence; not rerun here |
| EV-16 | GitHub CI reproduction | NOT PRESENT | Evidence limitation |
| EV-17 | G010 closure commit exists remotely | PASS | Confirmed |
| EV-18 | G010 requires reopening | NO | Closure preserved |

## Architecture Reconciliation

### Host Boundary

`MPC.Host.SolidWorks` targets net48 x64 with `Prefer32Bit` disabled. Its only MPC project reference is `MPC.Contracts`. The host uses the approved SOLIDWORKS interop assemblies:

- `SolidWorks.Interop.sldworks`
- `SolidWorks.Interop.swconst`
- `SolidWorks.Interop.swpublished`

### Contract Boundary

`MPC.Contracts` targets netstandard2.0 and has no project dependency. `ContractVersion` and `RequestId` are explicit and validated.

### Worker Boundary

`MPC.Worker` targets net8.0 x64 and references only `MPC.Application` and `MPC.Contracts`. No SOLIDWORKS dependency was found.

### Canonical Pipeline

`StepImportApplicationService` uses `StepCanonicalMappingOperation` and `CanonicalTopologyAssembler`. No parallel host-specific mapping or topology authority was identified.

### Mutation Boundary

The host production source contains no candidate active-model mutation API usage. The command remains limited to external STEP selection, worker invocation, result validation, and user presentation.

## Test-Count Reconciliation

The host test project contains:

- 10 test source files;
- 47 Fact methods;
- 5 Theory methods;
- 52 declared test methods;
- 66 discovered and executed test cases.

Five Theory methods expand into 19 parameterized cases and create 14 cases beyond the declared-method count:

```text
52 declared test methods
+14 additional parameterized cases
=66 discovered test cases
```

The counts 52 and 66 measure different things. This is a terminology reconciliation, not a missing-test defect.

## Reproduced Verification

Environment:

- Windows 10.0.19045 x64;
- .NET SDK 8.0.422;
- MSBuild 17.11.48;
- VSTest 17.11.1 x64.

Release solution build succeeded with zero warnings and zero errors.

| Test assembly | Passed | Failed | Skipped |
|---|---:|---:|---:|
| MPC.Tests | 969 | 0 | 0 |
| MPC.Contracts.Tests | 35 | 0 | 0 |
| MPC.Worker.Tests | 15 | 0 | 0 |
| MPC.Host.SolidWorks.Tests | 66 | 0 | 0 |
| MPC.Application.Tests | 6 | 0 | 0 |
| Total | 1091 | 0 | 0 |

The G010 build and automated regression totals were reproduced locally at the G011-Q01 reviewed HEAD.

## GitHub and CI Reconciliation

GitHub inspection confirmed:

- `b98f0f3` exists on `main` and is the documentation-only G010 closure commit;
- no workflow run was found for `b3d5046` or `b98f0f3`;
- no commit status was found for either commit;
- the repository has no `.github/workflows` directory.

The build and test results are verified local Windows evidence, not CI reproduction evidence. Windows CI remains future reliability work.

## Discrepancy Disposition

### D-01 Closure Commit

The earlier absence finding was caused by stale repository state. GitHub and `origin/main` both contain `b98f0f3`. No remediation is required.

### D-02 Host Test Count

The independent count of 52 represented declared test methods. The closure count of 66 represented discovered test cases. Theory expansion fully explains the difference.

### D-03 Local Reproduction

The earlier independent environment could not execute the tests. G011-Q01 reproduced the build, 66 host tests, and 1091 full regression tests on Windows at `b98f0f3`.

### D-04 CI Evidence

No GitHub workflow or commit-status evidence exists. This remains an accepted evidence limitation and separately scoped reliability backlog.

## Follow-up Classification

Future reliability work remains:

1. worker termination after timeout;
2. successful golden STEP execution through the physical worker;
3. Windows CI regression gate;
4. architecture-document precision for MPC.Application references;
5. strong-name signing and deployment hardening.

These items do not reopen G010 and are not implementation scope for G011-Q01.

No authorization is implied for long-running workers, retries, named pipes, caching, parallel parsing, healing, sewing, active model mutation, or performance optimization without new measurements.

## Closure Decision

All material G010 closure claims were mapped to evidence and assigned an explicit verification status. The apparent commit and test-count conflicts were fully reconciled. The recorded build and regression totals were reproduced locally.

No architecture, dependency, contract, canonical-authority, lifecycle, or SOLIDWORKS mutation blocker was found.

G010 remains closed.

G011-Q01 Final Decision: PASS
