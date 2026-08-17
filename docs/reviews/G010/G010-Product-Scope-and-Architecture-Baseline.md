# G010 Product Scope and Architecture Baseline

## Record Identity

- Work Item: G010-PL01 / G010-PL02
- Classification: VERIFY / DISCOVERY
- Review Date: 2026-08-17
- Branch: main
- Start Commit: 7b88c6f96f2812fb76a244131d4a626e8d71af65
- Reviewed HEAD: 7b88c6f96f2812fb76a244131d4a626e8d71af65
- Production Code Change: None
- Previous Gate: G009 PASS
- Performance State: PERF-13 accepted stop point

## Purpose

G010 establishes the next product capability on top of the verified
STEP-to-canonical topology pipeline delivered by G009.

The selected product direction is a minimum working SOLIDWORKS add-in
vertical slice using the real Option B ISwAddin architecture.

This baseline defines product scope, runtime boundaries, dependency
direction, compatibility constraints, and the execution sequence before
production implementation begins.

## Evidence Baseline

Repository verification established:

- local main and origin/main were synchronized;
- the working tree was clean;
- G009 closed without blockers;
- the independent senior review identified no mandatory remediation;
- PERF-13 formally stopped the current parser optimization phase;
- no additional performance work is justified without new measurement;
- no SOLIDWORKS or interop dependency exists in the current source tree;
- the canonical projects currently target net8.0;
- the existing dependency direction remains inward toward canonical
  geometry and topology.

## Canonical Readiness Decision

The current STEP-to-canonical solid pipeline is ready for controlled
product consumption.

Verified properties include:

- Shell determines closure from canonical edge identity.
- Every edge must be used exactly twice for Shell.IsClosed to be true.
- StepManifoldSolidBrepMapper rejects open canonical outer shells.
- StepClosedShellMapper rejects duplicate source face references.
- StepClosedShellMapper rejects duplicate canonical face instances.
- CanonicalTopologyAssembler owns topology verification and
  transactional publication.
- Failed topology verification does not publish incident edges.
- STEP geometry conversion remains inside MPC.Mapping.Step.
- Canonical core projects contain no SOLIDWORKS interop references.

Solid remains a lightweight canonical entity.

Shell closure must not be added to the Solid constructor because
closure enforcement belongs to the source semantic operation that
requires it, while CanonicalTopologyAssembler owns topology validation
and publication.

No prerequisite canonical hardening item is required before G010
product implementation.

## Selected Product Goal

G010 will deliver a minimum working SOLIDWORKS integration vertical
slice using a real ISwAddin host.

The vertical slice must:

1. load and unload through the SOLIDWORKS add-in lifecycle;
2. expose one controlled user command;
3. preserve SOLIDWORKS interop isolation;
4. invoke a host-independent MPC application operation;
5. execute the existing STEP-to-canonical solid pipeline;
6. return deterministic status and diagnostics to the host;
7. avoid mutation of the active SOLIDWORKS model.

## Installed Host Baseline

The verified development environment contains:

- SOLIDWORKS 2024 SP1;
- SLDWORKS executable version 32.1.0.0123;
- SOLIDWORKS interop assemblies version 32.1.0.123;
- .NET Framework 4.8;
- 64-bit SOLIDWORKS installation.

The canonical interop source is:

C:\Program Files\SOLIDWORKS Corp\SOLIDWORKS\api\redist

Only the following assemblies may be referenced by the host:

- SolidWorks.Interop.sldworks.dll
- SolidWorks.Interop.swconst.dll
- SolidWorks.Interop.swpublished.dll

Interop assemblies found in CircuitWorks, DriveWorksXpress, Simulation,
Treehouse, Scheduler, or other product subdirectories are not canonical
references for MPC.

## Runtime Compatibility Decision

The existing MPC projects target net8.0.

The SOLIDWORKS 2024 in-process host baseline is:

- Target Framework: net48
- Platform Target: x64
- Prefer 32-bit: false
- Host Model: in-process COM ISwAddin

Direct reference from the net48 host to the existing net8.0 MPC core is
not permitted.

Multi-targeting the existing MPC core was rejected for G010 because the
compatibility inventory found widespread modern runtime API usage,
including:

- 154 ArgumentNullException.ThrowIfNull usages;
- 74 double.IsFinite usages;
- 5 ReferenceEqualityComparer.Instance usages;
- additional modern framework and language dependencies.

Changing this verified surface would create a broad framework migration
with insufficient product value and excessive regression risk.

## Process Boundary Decision

G010 will use:

- a thin net48 in-process SOLIDWORKS host;
- a versioned host/worker contract;
- an out-of-process net8.0 MPC worker;
- a net8.0 application orchestration layer;
- the existing net8.0 canonical projects without framework migration.

The initial transport will use a single-operation child-process model.

Long-running workers, named pipes, retries, caching, parallel execution,
and worker pooling require separate evidence and are not part of the
initial vertical slice.

## Planned Project Boundaries

### MPC.Host.SolidWorks

Target:

- net48
- x64

Responsibilities:

- implement ISwAddin;
- own COM registration metadata;
- manage connect and disconnect lifecycle;
- create and remove the MPC command;
- translate host events into contract requests;
- launch and supervise the worker process;
- present operation status to the user.

It may reference:

- MPC.Contracts;
- canonical SOLIDWORKS interop assemblies.

It must not reference:

- MPC.Geometry;
- MPC.Topology;
- MPC.Mapping;
- MPC.Mapping.Step;
- MPC.Connectors.Step;
- MPC.Application.

### MPC.Contracts

Target:

- netstandard2.0

Responsibilities:

- define versioned request DTOs;
- define versioned result DTOs;
- define operation status;
- define transport-safe diagnostics;
- contain only transport-neutral primitive data.

It must not expose canonical domain objects or SOLIDWORKS API types.

### MPC.Application

Target:

- net8.0

Responsibilities:

- own application use cases;
- orchestrate STEP reading and canonical mapping;
- translate domain results into transport-safe results;
- remain independent of SOLIDWORKS.

### MPC.Worker

Target:

- net8.0 executable

Responsibilities:

- accept a versioned operation request;
- validate the request;
- invoke MPC.Application;
- emit exactly one deterministic operation result;
- return a documented process exit code;
- avoid SOLIDWORKS interop dependencies.

## Dependency Direction

Permitted dependency direction:

SOLIDWORKS
→ MPC.Host.SolidWorks
→ MPC.Contracts

MPC.Worker
→ MPC.Contracts

MPC.Worker
→ MPC.Application
→ MPC.Mapping.Step
→ MPC.Mapping
→ MPC.Topology
→ MPC.Geometry

MPC.Mapping.Step
→ MPC.Connectors.Step
→ MPC.Connectors

MPC.Worker must not directly depend on MPC.Connectors.Step.
STEP composition belongs to MPC.Application.

No reverse dependency toward the SOLIDWORKS host is permitted.

## In Scope

- real ISwAddin implementation;
- connect and disconnect lifecycle;
- one minimum MPC command;
- explicit COM registration workflow;
- x64 host configuration;
- versioned host/worker request and result contracts;
- child-process invocation;
- application service boundary;
- STEP file import through the existing canonical pipeline;
- deterministic operation status and diagnostics;
- isolation, lifecycle, contract, and regression tests.

## Out of Scope

- feature recognition;
- manufacturability analysis;
- CNC toolpath generation;
- automatic healing or sewing;
- coincidence-based topology mutation;
- active SOLIDWORKS model mutation;
- general STEP entity expansion;
- parser optimization without new measurements;
- caching;
- parallel parsing;
- long-running worker infrastructure;
- named-pipe transport;
- installer or enterprise deployment packaging;
- broad user-interface development.

## Execution Plan

### G010-A01 — Real ISwAddin Thin Host Skeleton

Acceptance criteria:

- net48 x64 host project exists;
- ISwAddin is implemented;
- connect and disconnect paths are deterministic;
- COM identity is explicit and stable;
- only canonical API redist assemblies are referenced;
- no MPC canonical project is referenced;
- lifecycle behavior is unit-testable outside SOLIDWORKS where practical.

### G010-A02 — Versioned Host/Worker Contract

Acceptance criteria:

- contract version is explicit;
- request and result DTOs contain only transport-safe data;
- operation status is deterministic;
- diagnostics preserve code, severity, and message;
- invalid requests are rejected without worker execution;
- contract project has no domain or interop dependency.

### G010-A03 — net8 Application Worker

Acceptance criteria:

- worker accepts one request and emits one result;
- exit codes are documented and deterministic;
- application orchestration is separate from process transport;
- worker contains no SOLIDWORKS reference;
- timeout and abnormal termination are distinguishable by the host;
- unit and integration tests cover success and failure paths.

### G010-A04 — Minimum STEP Import Vertical Slice

Acceptance criteria:

- the SOLIDWORKS command can initiate one STEP import operation;
- the existing canonical mapping operation is used;
- a successful result reports canonical solid summary information;
- a failed result reports deterministic diagnostics;
- the active SOLIDWORKS document is not mutated;
- representative golden STEP inputs pass through the vertical slice.

### G010-Q01 — Isolation, Lifecycle and Regression Gate

Acceptance criteria:

- dependency boundaries are verified;
- interop references exist only in the host;
- host connect/disconnect tests pass;
- worker contract tests pass;
- vertical-slice integration tests pass;
- solution build passes;
- full regression passes;
- repository state and commit traceability are recorded.

## Governance Rules

G010 continues to apply:

- Verify-First;
- Status Freshness;
- Minimum Intervention;
- New Build Gate;
- Architecture Discovery Allowance;
- Completion Gate;
- commit traceability.

Discovery code does not count as production delivery.

No performance optimization may begin without a new measurement sprint
and representative workload evidence.

No healing, sewing, coincidence mutation, caching, or parallelization is
implicitly authorized by this baseline.

## PL01 and PL02 Decision

G010-PL01 and G010-PL02 are accepted when this record is reviewed,
committed, and the repository remains clean after the documentation
commit.

Decision: PASS
