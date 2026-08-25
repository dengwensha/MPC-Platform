# G010 Independent Architecture Review

## Review Record

- Work Item: G010-CR01
- Classification: VERIFY / INDEPENDENT ARCHITECTURE REVIEW
- Review Date: 2026-08-25
- Branch: main
- Start Commit: b3d5046a28ad1a8f4271b9d067bab80019678922
- Reviewed HEAD: b3d5046a28ad1a8f4271b9d067bab80019678922
- Production Code Change: None
- Decision: PASS

## Review Purpose

This review independently evaluates whether G010 delivered the minimum
SOLIDWORKS-to-canonical STEP vertical slice without violating the approved
runtime, dependency, transport, lifecycle, and domain-isolation boundaries.

The review does not treat prior closure statements as proof by themselves.
Project references, production source, tests, commit history, and GitHub
repository state were cross-checked at the reviewed commit.

## Reviewed Architecture

The delivered execution path is:

1. SOLIDWORKS loads the net48 x64 COM host.
2. The host exposes one controlled STEP import command.
3. The host writes a versioned JSON request.
4. The host launches and observes one net8 x64 worker process.
5. The worker validates the request and invokes MPC.Application.
6. MPC.Application reads STEP data, creates semantic entities, performs
   canonical mapping, and assembles canonical topology.
7. The worker emits a transport-safe result, diagnostics, and summary.
8. The host validates result identity and presents the outcome.

No canonical domain object crosses the process boundary.

## Independent Findings

### IR-01 Host Isolation — PASS

MPC.Host.SolidWorks targets net48 and x64. Its only MPC project reference is
MPC.Contracts. It does not reference MPC.Application, MPC.Mapping,
MPC.Mapping.Step, MPC.Topology, MPC.Geometry, or MPC.Connectors.Step.

The host references only the approved SOLIDWORKS interop assemblies from the
configured API redist path:

- SolidWorks.Interop.sldworks;
- SolidWorks.Interop.swconst;
- SolidWorks.Interop.swpublished.

### IR-02 Contract Neutrality — PASS

MPC.Contracts targets netstandard2.0 and has no project dependency.

Requests, results, diagnostics, statuses, and topology summaries contain only
transport-safe values. Diagnostic collections are defensively copied.
Undefined status and negative summary counts are rejected. JSON round-trip
coverage exists.

### IR-03 Worker Boundary — PASS

MPC.Worker targets net8.0 x64 and references only MPC.Contracts and
MPC.Application.

The worker does not reference SOLIDWORKS or MPC.Connectors.Step directly.
It validates the request before application execution, produces one result,
and maps terminal operation states to deterministic exit codes.

### IR-04 Application Orchestration — PASS

MPC.Application owns the STEP use case and directly composes the connector,
semantic reader, canonical mapping operation, and topology assembler.

Its project-reference surface is wider than the simplified dependency line in
the planning record, but every direct reference is used by the orchestration
implementation. No reverse dependency toward the host exists. This is a
documentation-precision observation, not an architecture violation.

### IR-05 Canonical Pipeline Reuse — PASS

The vertical slice uses StepCanonicalMappingOperation and
CanonicalTopologyAssembler. It does not introduce a parallel topology model or
host-specific mapping authority.

Representative golden inputs T001_PRISM.step, T101_BASIC_POCKET.step, and
T103_MULTIPLE_ISLANDS.step are covered at the application boundary.
T001_PRISM.step deterministically reports one solid, one outer shell, six
faces, six loops, twelve edges, and eight vertices.

### IR-06 Result Identity and Failure Semantics — PASS

The host validates both ContractVersion and RequestId before accepting a
worker result.

Launch failure, timeout, observation failure, result-read failure, transport
failure, non-zero exit, rejected request, and failed operation are
distinguishable. Temporary request/result transport is isolated per operation
and cleanup is attempted on every host-command exit path.

### IR-07 SOLIDWORKS Mutation Boundary — PASS

The command selects an external STEP file, invokes the worker, and presents a
message. No active SOLIDWORKS document is created, healed, sewn, or mutated.

### IR-08 Lifecycle and COM Boundary — PASS

The primary add-in retains ClassInterfaceType.None and stable COM identity.
A dedicated callback proxy provides the required dispatch surface.
Connect/disconnect behavior and command registration cleanup are isolated and
tested. Registration uses the explicit 64-bit development workflow.

### IR-09 Verification Evidence — PASS WITH EVIDENCE LIMITATION

The G010-A04 closure record reports:

- solution build succeeded with zero warnings and zero errors;
- 1091 of 1091 tests passed;
- 66 of 66 host tests passed;
- a live SOLIDWORKS 2024 smoke test passed;
- COM registration and command execution passed.

GitHub contains no workflow run or commit status for the reviewed commit.
The automated-test totals are therefore accepted as recorded local evidence,
not independently reproduced CI evidence.

## Risk Assessment

### R-01 Worker Timeout Termination — Follow-up

A timeout is correctly reported, but disposing the host-side Process wrapper
does not terminate a still-running worker. A03 explicitly documented automatic
worker termination as out of scope.

This does not violate the locked G010 acceptance criteria. It should be handled
before workloads can become long-running so that timeout cannot leave an
orphan worker attempting to use removed transport files.

### R-02 Successful Physical-Process Automation — Follow-up

The repository contains a real executable process-boundary test, but its
current input intentionally exercises operation failure. Successful canonical
STEP execution is proven by application tests and the manual live SOLIDWORKS
smoke test.

Add a successful golden STEP test through the actual worker executable in a
future reliability gate.

### R-03 Windows CI — Follow-up

No GitHub Actions or commit-status evidence exists for the reviewed commit.
Introduce a Windows CI gate capable of building net48/x64 host projects and
running the non-live automated regression suite.

### R-04 Development Deployment — Accepted Limitation

The host is unsigned and RegAsm /codebase warning RA0000 is accepted for local
development only. Strong-name signing, installer work, and enterprise
deployment remain outside G010.

## Blocker Classification

No mandatory architecture, isolation, contract, lifecycle, canonical mapping,
or SOLIDWORKS mutation blocker was found.

The follow-up risks do not require reopening G010 because they were either
explicitly outside the approved scope or concern strengthening reproducible
verification beyond the accepted minimum vertical slice.

## Final Decision

G010 satisfies its approved product and architecture baseline.

Decision: PASS

Recommended next state:

- close G010;
- preserve b3d5046 as the reviewed product baseline;
- carry worker termination, successful physical-process automation, and
  Windows CI into explicitly scoped future reliability work;
- do not begin performance optimization without new representative
  measurements.
