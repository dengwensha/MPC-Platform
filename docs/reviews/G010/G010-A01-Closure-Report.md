# G010-A01 — Real ISwAddin Thin Host Skeleton Closure Report

## Traceability

- Classification: `[BUILD]`
- Branch: `main`
- Start Commit: `2d7e3375e3cced1870b037291c503b9c6d1b01ef`
- Reviewed HEAD: `2d7e3375e3cced1870b037291c503b9c6d1b01ef`
- Closing Commit: resolved from the commit containing this report
- Review Date: `2026-08-17`

## Scope

G010-A01 introduced the minimum real SOLIDWORKS in-process add-in
host skeleton defined by the G010 product scope and architecture
baseline.

The delivery contains:

- a `net48` host project;
- explicit `x64` platform configuration;
- a real `ISwAddin` implementation;
- deterministic connect and disconnect lifecycle behavior;
- explicit and stable COM identity;
- canonical SOLIDWORKS API redist references;
- lifecycle and COM contract tests executable outside SOLIDWORKS.

Command creation, worker transport, application orchestration, STEP import,
document mutation, and broad user-interface development remain outside
G010-A01.

## Delivered Projects

### MPC.Host.SolidWorks

The host project:

- targets `.NET Framework 4.8`;
- targets `x64`;
- disables 32-bit preference;
- implements `SolidWorks.Interop.swpublished.ISwAddin`;
- owns the add-in GUID and ProgID;
- exposes no generated COM class interface;
- references no MPC canonical project;
- references only canonical SOLIDWORKS API redist assemblies.

Canonical interop source:

`C:\Program Files\SOLIDWORKS Corp\SOLIDWORKS\api\redist`

Referenced assemblies:

- `SolidWorks.Interop.sldworks.dll`, version `32.1.0.123`;
- `SolidWorks.Interop.swpublished.dll`, version `32.1.0.123`.

### MPC.Host.SolidWorks.Tests

The separate `net48/x64` test project verifies:

- successful lifecycle connection;
- invalid application and cookie rejection;
- callback registration failure behavior;
- callback exception containment;
- duplicate connection rejection;
- deterministic and idempotent disconnection;
- real `ISwAddin` contract implementation;
- COM visibility;
- stable GUID;
- stable ProgID;
- disabled generated class interface;
- rejection of non-SOLIDWORKS connection objects.

## COM Identity

- GUID: `9A54C5C5-5457-4B41-9147-184FF399B057`
- ProgID: `MPC.Host.SolidWorks.Addin`
- Class interface: `None`
- COM visibility: explicitly enabled for the add-in class

The identity is protected by automated contract tests.

## Dependency Boundary

The host has no project-to-project reference.

It does not reference:

- `MPC.Geometry`;
- `MPC.Topology`;
- `MPC.Mapping`;
- `MPC.Mapping.Step`;
- `MPC.Connectors`;
- `MPC.Connectors.Step`;
- `MPC.Application`.

SOLIDWORKS interop references remain isolated to the host project.
The host test project receives the canonical interop assemblies only as
runtime outputs through its host project reference.

## Validation Evidence

### Solution Build

- Configuration: `Release`
- Solution orchestration: `Any CPU`
- Host mapping: `Release|x64`
- Result: successful
- Warnings: `0`
- Errors: `0`

### Existing Regression

- Project: `MPC.Tests`
- Framework: `net8.0`
- Passed: `969`
- Failed: `0`
- Skipped: `0`

### Host Regression

- Project: `MPC.Host.SolidWorks.Tests`
- Framework: `net48`
- Platform: `x64`
- Passed: `16`
- Failed: `0`
- Skipped: `0`

### Platform Verification

The evaluated host build properties are:

- `TargetFramework`: `net48`
- `Platform`: `x64`
- `PlatformTarget`: `x64`
- `Prefer32Bit`: `false`
- `OutputPath`: `bin\x64\Release\net48\`

## Acceptance Assessment

| Acceptance criterion | Result |
|---|---|
| net48 x64 host project exists | Pass |
| ISwAddin is implemented | Pass |
| Connect and disconnect paths are deterministic | Pass |
| COM identity is explicit and stable | Pass |
| Only canonical API redist assemblies are referenced | Pass |
| No MPC canonical project is referenced | Pass |
| Lifecycle behavior is unit-testable outside SOLIDWORKS where practical | Pass |

## Completion Decision

G010-A01 satisfies its acceptance criteria and is accepted as complete.

The next planned delivery is G010-A02 — Versioned Host/Worker Contract.
