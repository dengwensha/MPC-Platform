# M2 inventory-error policy proposal

Date: 2026-11-18 Europe/Istanbul. Class: VERIFY / proposed policy.
Baseline: `f7a54ad4b4e0c741220415b479e2f46d91b7658f` (main CI #233 successful).
State: PROPOSED, NOT OWNER-ACCEPTED OR IMPLEMENTED. M2 remains OPEN.
Accountable owner: Mehmet İlkay Kınacı; implementation assistant maintains evidence.

## Single goal and authority

Resolve the decision prepared by Senior section 18: should inventory errors block
future whole-file-dependent feature recognition, machining-time or cost evaluation,
or only be displayed? This proposal supplies evidence and a concrete recommendation.
User continuation authorizes this discovery/proposal; it is not recorded as a
choice between the policy alternatives. No code behavior or existing approval is
changed, and no pending work/release gate is crossed by preparing this document.

## Verified current propagation

The source references below are pinned to the baseline; current line numbers
can move. This is source inspection plus the bounded observations below.

| Boundary | Pinned source | Observed behavior / consequence |
| --- | --- | --- |
| Raw inventory | [StepDataEntityReader](https://github.com/dengwensha/MPC-Platform/blob/f7a54ad4b4e0c741220415b479e2f46d91b7658f/src/MPC.Connectors.Step/Readers/StepDataEntityReader.cs#L114) | Collects record and inventory-validator diagnostics; any diagnostics produce SucceededWithDiagnostics. |
| Result invariant | [CadConnectorResult](https://github.com/dengwensha/MPC-Platform/blob/f7a54ad4b4e0c741220415b479e2f46d91b7658f/src/MPC.Connectors/Results/CadConnectorResult.cs#L112) | SucceededWithDiagnostics permits Error but forbids Fatal. Therefore status is not a warning-only guarantee. |
| v1.1 region importer | [ManufacturingRegionImportService](https://github.com/dengwensha/MPC-Platform/blob/f7a54ad4b4e0c741220415b479e2f46d91b7658f/src/MPC.Application/Manufacturing/ManufacturingRegionImportService.cs#L35) | Rejects absent/Failed inventory, then maps supported semantic entities; it does not propagate inventory diagnostics in its bool/out-region API. |
| v1.1 result | [RegionSourceValidationResult](https://github.com/dengwensha/MPC-Platform/blob/f7a54ad4b4e0c741220415b479e2f46d91b7658f/src/MPC.Contracts/RegionSourceValidationResult.cs) | Outcome-only source-link DTO has no inventory diagnostic/completeness evidence. Host cannot reconstruct it from SourceLinkMatched. |
| v1.0 Application import | [StepImportApplicationService](https://github.com/dengwensha/MPC-Platform/blob/f7a54ad4b4e0c741220415b479e2f46d91b7658f/src/MPC.Application/Step/StepImportApplicationService.cs#L101) | Preserves inventory diagnostics and can return a summary with SucceededWithDiagnostics. |
| v1.0 Worker | [WorkerRequestProcessor](https://github.com/dengwensha/MPC-Platform/blob/f7a54ad4b4e0c741220415b479e2f46d91b7658f/src/MPC.Worker/WorkerRequestProcessor.cs#L44) | Preserves converted diagnostics but maps SucceededWithDiagnostics to Succeeded. Exit/status alone is insufficient for future evaluation eligibility. |
| Connector wrapper | [StepConnector](https://github.com/dengwensha/MPC-Platform/blob/f7a54ad4b4e0c741220415b479e2f46d91b7658f/src/MPC.Connectors.Step/StepConnector.cs#L47) | Also consumes IStepDataEntityReader by injection and preserves diagnostics in snapshot/result. Two Application sites instantiate the reader directly; they are not an exhaustive list of consumers. |

No whole-file feature/time/cost evaluator was identified in the inspected current
source tree (searches for feature/machining/time/cost estimation and reader/importer
consumers). Existing Tooling compatibility evaluation is not such a STEP evaluator.
This is a bounded source-discovery result, not a claim about future/external consumers.

## Fresh Release child-process observations

After a fresh local Worker Release build (0 warnings/errors), six actual Linux
child-process invocations confirmed the following. Each request had a fresh valid
RequestId and replies were correlated; v1.1 used the actual modified-byte hash
and face #8. Original golden bytes were preserved; ASCII records were inserted
immediately before the final ENDSEC. No production source or fixture was edited.

| Input | v1.0 result / exit | v1.1 result / exit |
| --- | --- | --- |
| Unchanged T001_PRISM | Succeeded, summary, no diagnostics / 0 | SourceLinkMatched / 0 |
| Golden plus unreferenced `#999999 = ( );` | Succeeded, summary, Error STEP_COMPLEX_ENTITY_COMPONENTS_NOT_FOUND / 0 | SourceLinkMatched / 0; no diagnostics field in DTO |
| Golden plus unreferenced `#999999 = UNSUPPORTED_ENTITY();` | Succeeded, summary, no diagnostics / 0 | SourceLinkMatched / 0 |

[Machine-readable observation evidence](evidence/M2-Inventory-Policy-Baseline.json)
records baseline, golden/modified-byte hashes, freshly built Worker DLL hash and
observed fields. These are 6/6 bounded observations, not six new regression tests,
physical SOLIDWORKS acceptance, exhaustive parser tests or policy acceptance.
The unsupported-simple control is important: zero inventory errors does not prove
all source geometry was interpreted or supported. Header-level Complete metadata
is not a semantic-completeness certificate. This proposal does not add an unsupported
entity rejection rule or silently reclassify all unsupported STEP metadata as errors.

## Decision alternatives and recommendation

| Alternative | Future whole-file-dependent evaluation | Tradeoff |
| --- | --- | --- |
| A — block on inventory Error/Fatal (recommended) | Do not produce/publish feature, time or cost outputs if full-inventory evidence contains Error/Fatal, is unavailable or belongs to another snapshot | Prevents relying on known malformed input; can block useful partial evaluations until a separately defined partial-scope contract exists |
| B — display errors and continue | Surface diagnostics but allow evaluation despite inventory Error/Fatal | More permissive, but can publish incomplete quantities/times/costs; needs an explicit separately reviewed partial-result and uncertainty contract |

Proposed owner decision: accept A for future whole-file-dependent evaluators.
Keep the already accepted v1.1 source-link-only behavior and limits unchanged.
Information/Warning diagnostics should remain available to the user and evidence
consumer; by themselves they are not this inventory-error blocker. Other supported
geometry, completeness, material, machine or tooling rules can still block the
operation. Error-free inventory is necessary under A, never sufficient approval.

Unknown/missing evidence must not be reconstructed as zero errors from v1.1's
DTO or a v1.0 successful exit. This is a proposed rule for future evaluators,
not a new interpretation or restriction of today's wire outcomes. No new enum,
DTO field, automatic retry, legacy fallback, UI flow or current source-match
behavior is introduced by this document.

## Proposed implementation boundary and acceptance

If A is owner-accepted and independently reviewed, the next micro-sprint should
first define the smallest CAD-independent Application evidence/decision boundary.
Preserve typed severity/code/source context from the same private snapshot used
for geometry and fingerprint; do not independently reread a changing original file
or decide eligibility in the Host from a success flag. No Application-to-Tooling
reference is authorized. API shape, producer/consumer integration and wire/version
impacts require a concrete design review before implementation; this proposal does
not pretend the existing bool/out-region API or v1.1 DTO transports the evidence.

| Required acceptance case for that later implementation | Proposed expected behavior |
| --- | --- |
| Error on an unreferenced empty complex record beside valid geometry | Whole-file feature/time/cost output withheld; diagnostic retained; current source-link check remains independently scoped |
| Failed/Fatal inventory, missing evidence, unknown severity or wrong snapshot | Whole-file evaluation not approved; no synthetic clean evidence |
| Only Info/Warning | Diagnostics retained; this inventory-error rule alone does not block; all other required gates still apply |
| No diagnostics but unsupported/unmapped geometry | No automatic completeness or machining approval; applicable capability gate still required |
| Valid supported golden input | Same-snapshot evidence usable; evaluation succeeds only when its other prerequisites pass |
| Legacy and v1.1 compatibility controls | Existing contracts/outcomes retained unless a separate versioned change is explicitly reviewed |

These are acceptance criteria for a future implementation, not tests claimed to
exist or pass now. This sprint changes documentation/evidence only. Full M2 cause
separation and error precedence remain separately OPEN; choosing A alone closes
neither that gap nor this policy's implementation/evidence gate.

## Gates and next decisions

1. Owner explicitly selects A or B (or supplies a different concrete policy).
2. Independent Senior review assesses the proposed boundary meaning and limits.
3. Define and review minimal evidence propagation plus required compatibility tests.
4. Implement only the approved micro-sprint, with Release/full-test/corpus gates,
   exact commit trace and a clean checkout; do not publish a whole-file evaluator
   until its evidence gate is implemented and verified.

Deadline remains BEFORE catalog intake/importer work OR end-user release, whichever
first; the earlier whole-file feature/time/cost-consumption trigger also applies.
This discovery is not catalog importer implementation, source-link expansion or
release approval. M2 remains OPEN and the four-guard closure is not reopened.

Risk assessment: trust/fail-closed meaning Yes because the proposal defines future
evaluation eligibility; independent Senior review is required before policy
ratification/merge even though the diff is documentation-only. Wire shape No;
enum values No; IPC mechanics No; persistence No; production UI entry No. No owner
selection or independent acceptance is fabricated from user continuation or CI.
