# G010-A04 â€” Minimum STEP Import Vertical Slice Closure Report

## Review Record

- Review Date: 2026-08-25
- Branch: main
- Start Commit: 7c15e92bc1b7f51b2b52e27a4ad57756074a6fa4
- Reviewed HEAD: 7c15e92bc1b7f51b2b52e27a4ad57756074a6fa4
- Closing Commit: assigned by this closure commit
- Classification: New Build
- Status: Accepted for closure

## Objective

Deliver the minimum real STEP import vertical slice across the established
SOLIDWORKS host and out-of-process worker boundary.

The required path was:

1. SOLIDWORKS thin host accepts a STEP import command.
2. The host creates a versioned transport request.
3. The host starts and observes a child-process worker.
4. The worker invokes the application boundary.
5. The application boundary runs the existing canonical STEP pipeline.
6. A deterministic result and diagnostic set returns to the host.
7. The host presents the completion status to the user.

## Delivered Scope

### Application Boundary

The `MPC.Application` project now owns STEP import orchestration.

The application service composes:

- STEP data reading and validation;
- semantic entity construction;
- canonical STEP mapping;
- canonical topology assembly;
- deterministic application status;
- application diagnostics;
- canonical topology summary generation.

The application project does not reference SOLIDWORKS interop assemblies.

### Transport Contract

The versioned worker result contract now supports an optional,
transport-safe operation summary.

The summary reports:

- solid count;
- outer and inner shell counts;
- face count;
- loop count;
- edge count;
- vertex count.

Contract construction, normalization, validation, defensive copying, and
JSON round-trip behavior are covered by tests.

### Worker Integration

The worker now invokes the application service for supported STEP import
requests.

The worker:

- validates the versioned request;
- executes one operation per child process;
- translates application status into worker status;
- translates diagnostics into transport diagnostics;
- returns the topology summary without transporting canonical domain
  objects;
- preserves deterministic process exit behavior.

The worker contains no SOLIDWORKS dependency.

### SOLIDWORKS Host Command

The net48 x64 host now provides one minimum STEP import command.

The command:

- opens a STEP file selection dialog;
- writes a versioned JSON request;
- launches the net8 x64 worker;
- observes process completion;
- reads the versioned JSON result;
- presents success or failure to the SOLIDWORKS user.

No active SOLIDWORKS model is created, modified, healed, or mutated.

### Runtime Compatibility

A COM-visible callback proxy is used for SOLIDWORKS command callbacks while
the primary add-in class retains `ClassInterfaceType.None`.

A constrained assembly resolver loads only the approved local net48
`System.Text.Json` dependency set from the registered host assembly
directory.

The resolver is limited to an explicit allowlist and does not perform
general probing.

### Registration Workflow

The repository now contains explicit local-development registration tools:

- `Register-MpcSolidWorksAddin.ps1`;
- `Unregister-MpcSolidWorksAddin.ps1`;
- registration workflow documentation.

The registration workflow uses 64-bit RegAsm and configures the required
SOLIDWORKS machine and current-user startup registry entries.

## Verification Evidence

### Final Solution Build

Command:

    dotnet build .\src\MPC.sln -c Debug

Result:

- succeeded;
- 0 warnings;
- 0 errors.

The solution `Any CPU` configuration orchestrates the host and worker
project-specific x64 outputs.

### Final Full Regression

Result: 1091 of 1091 tests passed.

Breakdown:

- MPC.Tests: 969 passed;
- MPC.Contracts.Tests: 35 passed;
- MPC.Application.Tests: 6 passed;
- MPC.Worker.Tests: 15 passed;
- MPC.Host.SolidWorks.Tests: 66 passed;
- failed: 0;
- skipped: 0.

### Host-Specific Regression

Result: 66 of 66 net48 host tests passed.

### Live SOLIDWORKS Smoke Test

Environment:

- SOLIDWORKS 2024;
- net48 x64 in-process host;
- net8 x64 out-of-process worker;
- registered development assembly;
- golden input `T001_PRISM.step`.

Observed result:

- command registration succeeded;
- STEP file selection succeeded;
- worker execution succeeded;
- canonical pipeline succeeded;
- result returned to SOLIDWORKS;
- success message was presented.

Observed topology summary:

- Solid: 1
- Shells: 1
- Faces: 6
- Loops: 6
- Edges: 12
- Vertices: 8

### COM Registration Validation

The registration script was executed successfully against:

    src\MPC.Host.SolidWorks\bin\x64\Debug\net48\MPC.Host.SolidWorks.dll

Verified values:

- Title: MPC Manufacturing Intelligence
- Description: MPC STEP canonical import add-in
- Startup: 1
- COM CodeBase: registered x64 Debug net48 host assembly
- registration script parse errors: 0
- unregistration script parse errors: 0

## Runtime Discoveries

### Callback Marshaling

Passing the main add-in instance directly to `SetAddinCallbackInfo2`
produced an `InvalidCastException` with the required disabled class
interface.

The accepted solution is a dedicated COM-visible callback proxy using
`ClassInterfaceType.AutoDispatch`. The primary add-in remains explicitly
`ClassInterfaceType.None`.

### Callback Registration Result

The SOLIDWORKS callback registration call is treated as exception-driven.
A completed call is accepted, while an exception causes lifecycle
connection failure.

This behavior was confirmed in the live host.

### Assembly Binding

The SOLIDWORKS process did not apply the package binding behavior expected
by the net48 `System.Text.Json` dependency graph.

A constrained local assembly resolver was required to bind the packaged
runtime dependencies, including
`System.Runtime.CompilerServices.Unsafe`.

### Output Architecture

Live registration must target:

    bin\x64\Debug\net48\MPC.Host.SolidWorks.dll

Building only a non-x64 project output can leave the registered assembly
stale. Final live validation therefore used explicit x64 host and worker
builds.

## Accepted Development Limitation

The host assembly is currently unsigned.

RegAsm consequently reports warning RA0000 when `/codebase` is used. This
is accepted only for local development and smoke testing.

Strong-name signing, installer creation, deployment hardening, and
enterprise packaging remain future work.

## Out of Scope

The following capabilities were not added:

- feature recognition;
- manufacturability analysis;
- CNC toolpath generation;
- automatic healing or sewing;
- coincidence-based topology mutation;
- active SOLIDWORKS model mutation;
- broad STEP entity expansion;
- named-pipe transport;
- long-running worker infrastructure;
- caching or parallel parsing;
- broad user-interface development;
- production installer packaging.

## Acceptance Decision

G010-A04 satisfies its minimum vertical-slice acceptance criteria.

A real STEP file travels from the SOLIDWORKS host through a versioned
child-process request, is processed through the canonical application
pipeline, and returns deterministic status, diagnostics, and topology
summary data to the host.

The solution builds without warnings or errors, all 1091 automated tests
pass, and the cleaned production implementation passes a real SOLIDWORKS
smoke test.

G010-A04 is accepted for commit and remote closure.
