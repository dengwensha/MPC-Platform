# P01-SC01 — Scope Freeze and Final Planned Work Decision

## Classification and Outcome

Classification: VERIFY

Outcome: PASS — SCOPE DECISION RECORDED / FINAL VALIDATION PENDING

This record compares P01-SR01 and P01-B11 through P01-B13 with
current code and records the remaining P01 boundary.

It does not declare P01 complete or claim current physical corpus
execution. No production, test or build-configuration change is included.

## Traceability

| Field | Value |
|---|---|
| Branch | main |
| Start Commit | b82e4be90cd76ea46770182ac0b2e3cfe567cafc |
| Reviewed HEAD | b82e4be90cd76ea46770182ac0b2e3cfe567cafc |
| Verified Remote main at Start | b82e4be90cd76ea46770182ac0b2e3cfe567cafc |
| Record Creation Date, Local | 2026-09-07 |
| Initial Working Tree | Clean |
| Post-Validation Working Tree | Clean |
| Closing Commit | The commit introducing this document |
| Publication | Pending verification at record creation |

Static code comparison used files pinned to the reviewed commit.
Build and test evidence was executed locally by the project owner
and supplied in the session. SC01 is not a new independent senior review.

## Governing Evidence

- P01-Real-Step-Part-Validation-Report.md
- P01-B03-Entity-Scoped-Semantic-Failure.md
- P01-B10-Exporter-Scoped-Signed-Toroidal-Compatibility-Policy.md
- P01-B11-R03-B-Spline-and-Bounded-Entity-Resolution-Discovery.md
- P01-B12-Standard-B-Spline-Surface-Semantic-Contract.md
- P01-B13-Canonical-B-Spline-Surface-Geometry-Contract.md
- P01-SR01-Senior-Review-Findings-Reconciliation.md
- SR02-Final-Independent-Verification-Reconciliation.md

Historical records remain unchanged. Their earlier status statements
apply to their own baselines.

SR02 independently verified M-01 through M-04 closed at
b2281654a7f02e46f6ada111a24746339087eda8.
Its reconciliation is included in the SC01 starting HEAD.

The independent review was static and did not execute build or tests.
Mandatory remediation closure does not itself approve B14 or close P01.

## Current-Code Reconciliation

| Capability | Current evidence | Disposition |
|---|---|---|
| Standalone non-rational surface semantics | StepBSplineSurfaceWithKnotsEntity and catalog registration exist | B12 delivered |
| Canonical non-rational surface | BSplineSurface3D implements ISurface3D | B13 delivered |
| Standalone B-spline surface mapping | No B-spline composition reference in Mapping.Step; advanced-face dispatch has no B-spline case | Missing |
| Rational/complex B-spline semantics | Semantic reader dispatches by SourceEntityType; Application catalog has no applicable handler | Missing in the composed path |
| B-spline curves | No B-spline curve handler in the catalog or dispatch in the canonical resolver | Missing in the composed path |
| Face traversal | Surface builder must succeed before boundaries are traversed | Surface-first order remains |

Reviewed production files include:

- src/MPC.Application/Step/StepSemanticHandlerCatalog.cs
- src/MPC.Application/Step/StepImportApplicationService.cs
- src/MPC.Connectors.Step/Semantics/StepSemanticEntityReader.cs
- src/MPC.Connectors.Step/Semantics/StepBSplineSurfaceWithKnotsEntity.cs
- src/MPC.Geometry/Primitives/BSplineSurface3D.cs
- src/MPC.Geometry/Primitives/Point3D.cs
- src/MPC.Mapping.Step/StepAdvancedFaceGeometryMapper.cs
- src/MPC.Mapping.Step/StepAdvancedFaceBuilderMapper.cs
- src/MPC.Mapping.Step/StepAdvancedFaceMapper.cs
- src/MPC.Mapping.Step/StepCanonicalReferenceResolver.cs
- src/MPC.Mapping.Step/StepReferenceResolver.cs

### Metadata Ownership Reconciliation

B11 listed source logical states, form and knot specification among
future surface information. B13 explicitly assigned those STEP fields
to the semantic layer and retained mathematical representation in Geometry.

Current code follows that decision: the semantic entity retains these
fields, while BSplineSurface3D contains degrees, ordered control points,
knots and multiplicities.

This is an explicit ownership refinement, not evidence of lost
standalone semantic fields. No end-to-end mapped preservation is claimed.

### B14 Design Qualification

B13 recommends resolving control-point references through the existing
canonical reference resolver. That recommendation is not directly
implementable as Resolve<Point3D>:

- StepCanonicalReferenceResolver.Resolve<T> requires T to be a class;
- Point3D is a readonly record struct;
- the canonical resolver has no StepCartesianPointEntity dispatch.

Any future B14 design must verify the existing semantic reference path
and point conversion conventions before implementation. SC01 does not
authorize a generic resolver redesign.

## Final Planned Work Decision

P01 FINAL PLANNED WORK ITEM:

P01-CL01 — Final Real-Part Revalidation and Closure Assessment [VERIFY]

This is a new explicit planning decision recorded by SC01.
It is not a claim that a previously approved final-item record was found.
B13 remains the last delivered P01 B-item.

CL01 has one goal: establish the current physical-worker result for
the fixed R01/R02/R03 corpus and close the validation assessment with
accurate compatibility limits and traceable evidence.

## Scope Freeze

P01 SCOPE FROZEN

The remaining planned P01 scope is:

- verify corpus identity;
- establish a clean, current execution baseline;
- execute Release build and full regression;
- run each corpus part three times through the physical worker;
- reconcile results, diagnostics and topology signatures;
- record unresolved compatibility work explicitly;
- complete closure evidence and commit/publication verification.

No additional B-series capability is included in this frozen scope.

Excluded work includes B-spline mapping implementation, rational/complex
support, B-spline curve support, evaluation, tessellation, healing,
sewing, normalization, exporter-policy expansion, optimization, UI,
feature recognition and manufacturing analysis.

A new unexplained failure or regression stops closure and requires
explicit classification and scope disposition. It does not silently
expand CL01 into an implementation sprint.

## P01-B14 Decision

Disposition: NOT REQUIRED FOR P01 VALIDATION-ASSESSMENT CLOSURE;
NOT AUTHORIZED TO START BY SC01.

The mapping capability proposed for B14 is needed for eventual
standalone B-spline surface import. B14 alone is insufficient for
full R03 compatibility.

B13 explicitly excludes advanced-face integration from its proposed
B14 scope. Rational surfaces, non-rational and rational curves, and
their runtime composition also remain unresolved.

The B14 proposal remains traceable future work under a separate
NEW BUILD gate. It is neither implemented nor classified as unnecessary.

B11's historical corpus inventory identifies 696 standalone surfaces,
276 rational surfaces, 1910 standalone curves and 4 rational curves.
These are inventory evidence, not current execution counts.

## CL01 Acceptance Criteria

### Execution Baseline

- Verify main, clean Git state and current remote agreement.
- Record Start Commit, Reviewed HEAD, execution date and commands.
- Execute the worker built from that verified source state.
- Record corpus SHA256 and byte size before execution.
- Keep proprietary STEP inputs outside the repository.
- Perform three separate worker executions per part.
- Preserve request identity and capture exit code, result status,
  diagnostics, duration and topology summary where present.
- Compare deterministic payloads without requiring RequestId or timing
  fields to be identical.

### Corpus Identity

| Part | Bytes | SHA256 |
|---|---:|---|
| R01 | 23222 | E518DB3332DC665B16CC31DA960C46B75B8D75128234129922A25146BE1C33AD |
| R02 | 46323 | 0E9E2D6E15E1328C068E30703AD621AE883B6FE94A5DB51353A0D26F82E1578F |
| R03 | 14003088 | 319DD894747CAF95AC1100C6283B55C014D40AEAC4ECCC00EF54648ADF9AEA12 |

### R01 and R02

Both parts require 3/3 successful runs, exit code 0, no diagnostics,
preserved RequestId and these historical acceptance signatures:

| Part | Solid | Outer shell | Inner shell | Faces | Loops | Edges | Vertices |
|---|---:|---:|---:|---:|---:|---:|---:|
| R01 | 1 | 1 | 0 | 11 | 12 | 24 | 16 |
| R02 | 1 | 1 | 0 | 23 | 28 | 48 | 32 |

These values come from B03 physical regression evidence and are not
claimed as current SC01 physical measurements.

### R03

Current code indicates incomplete B-spline import support.
Assessment closure requires:

- 3/3 consistent, explained results;
- no worker crash or timeout;
- RequestId preserved for every run;
- for controlled failure, exit code 3 and Failed status;
- no successful solid summary on a failed operation;
- diagnostics attributable to identified source entities and capability
  boundaries, using message evidence where structured identity is absent;
- confirmation that the accepted signed-toroidal boundary has not regressed;
- no APP-STEP-UNEXPECTED-001 or unexplained new failure accepted as routine.

Record the actual diagnostic codes, source families, counts and ordered
result fingerprint. Do not reuse B10's 2772 count as a fixed assertion:
B12 semantic support can change the downstream diagnostic classification.

Unexpected success also requires reconciliation with current code and
corpus identity before making a compatibility claim.

### Closure Semantics

A repeatable, explained R03 controlled failure may close the validation
assessment with findings. It does not close R03's compatibility gap.

If R01/R02 pass and R03 remains controlled-failed, report:

- P01 assessment: COMPLETE WITH FINDINGS;
- corpus compatibility: PARTIAL — 2/3;
- R03 full compatibility: OPEN.

Full R03 compatibility requires a separately authorized capability
delivery and successful end-to-end physical validation. It cannot be
inferred from green unit tests or this scope-freeze decision.

### Delivery Gate

- Release build: zero warnings and errors.
- Full regression: no failures or skipped tests.
- Explain any test-count change against the verified baseline.
- Preserve independent-review evidence limits.
- Record the nine physical executions and remaining capability backlog.
- Record closing commit and verify remote publication and clean Git state.
- Do not infer unrelated PRODUCT scope approval from P01 closure.

## SC01 Local Validation Evidence

Commands:

- dotnet build src/MPC.sln -c Release
- dotnet test src/MPC.sln -c Release --no-build --no-restore

Build: PASS — 0 warnings / 0 errors.
Build elapsed time: 00:00:24.38.

| Test Project | Passed | Failed | Skipped |
|---|---:|---:|---:|
| MPC.Tests | 1112 | 0 | 0 |
| MPC.Contracts.Tests | 35 | 0 | 0 |
| MPC.Worker.Tests | 16 | 0 | 0 |
| MPC.Application.Tests | 11 | 0 | 0 |
| MPC.Host.SolidWorks.Tests | 69 | 0 | 0 |
| Total | 1243 | 0 | 0 |

Host tests executed from bin/x64/Release/net48.
Reviewed HEAD remained unchanged and the working tree remained clean.

No R01/R02/R03 physical execution was performed as part of SC01.
CL01 remains pending.

## Delivery Boundary

SC01 introduces only this decision document.
Historical records, production code, tests and build policy are unchanged.
