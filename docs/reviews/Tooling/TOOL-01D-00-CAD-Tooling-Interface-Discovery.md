# TOOL-01D-00 — CAD / Tool Library Trust Boundary Discovery

Status: VERIFY-FIRST / DOCUMENTATION ONLY — review draft, not implementation approval.
Prepared: 2026-10-26; reconciled: 2026-10-27 (Europe/Istanbul).
Reviewed PR baseline: #84, head 717449ec7b16e59c1802ef00c168ff805f9c5f71.
Base: f0281cc241b76019b2846677f98fdcc932e768d8.
No production code, DTO, database, merge or deployment is authorized by this document.

## Reconciliation after TOOL-01C-04..07 (2026-10-27)

Verified main: `53e87d33dbd4fdcbeacee45d886d7994266352e9`. PRs #85/#86/#87/#88 merged; full post-merge CI #169 succeeded: https://github.com/dengwensha/MPC-Platform/actions/runs/40500846940 . This does not qualify CAD material orientation or authorize runtime integration.

- #85: actual production evaluator rejects Unknown hand and null mounting, exposes InterfaceOnly; raw EAN is retained with separate GTIN format status. Known Neutral behavior and numeric mounting tokens remain uncertified.
- #86: 19 cases exercise actual production models using controlled mapped fixtures. No raw-source importer, source revision validation or unit conversion is implemented.
- #87: three additional test-local contract cases prove old/new revision coexistence and explicit reference resolution. No durable history or transaction guarantee.
- #88: CatalogSourceSnapshot copies bytes, hashes that retained copy and checks expected SHA-256. No supplier authentication, source revision association, persistence or importer wiring.

The historical review trace below is retained as reported evidence. Its acceptance applies to the earlier revised text, not to this reconciliation or its final commit. Final CI for this document is required; prior main CI does not validate this revision.

## Evidence and limits (original discovery baseline)

- PR #81 defines proposed catalog/source/product/assembly identities; it does not implement persistence.
- PR #82 contains test-local in-memory assembly invariants. These do not demonstrate durable history, transactions or physical compatibility.
- PR #83 is the independent review brief, merged at f0281cc; merging it is not architectural approval.
- Independent report MPC_TOOL-01C-SR01_Senior_Review-1.docx, dated 2026-10-25, reviews f0281cc and returns REWORK. Pinned source: Library ID libfile_a359a701ed648191812a7c5a922ec5fa, version 0, original DOCX bytes SHA-256 `9682DE43CA744C0E95C84DAA93B4E26810E73E5E09E6981223CFA9D6364B4D23`. This identifies the reviewed report, not the truth of its claims. The DOCX is a conversation-provided exported Word copy, not a repository artifact; its Library ID is not a repository-resolvable reference. No archival repository path is claimed. Its findings are static review findings; it did not run build/tests or independently verify supplier pages.
- PR #84 head 717449e has successful full-validation workflow run 40224642532, including Release build/regression. This does not close review findings or physical acceptance.
- ManufacturingRegionImportService computes SHA-256 from the same private STEP byte snapshot used for mapping. ManufacturingRegion carries DocumentFingerprint, SourceFaceId and CanonicalFaceId, with planar-face geometry.
- The inspected ManufacturingRegion does not carry a complete per-claim verification, unit or transform-provenance contract. Orientation/SurfaceNormal is not material-side certification.
- ToolProductIdentity carries manufacturer/product/order/EAN information, not source revision or source hash.
- InsertHolderCompatibilityEvaluator compares bounded shape/seat/dimension/angle/mount/hand properties. Its existing 0.001 mm and 0.001 degree behavior is unchanged.
- The physical checklist records R01 0/11 and R02 0/23 independently qualified faces. This is repository evidence, not a claim that later workstation observations have been checked here.
- Application currently has no Tooling project reference; Tooling has no project references. A runtime bridge is not established.

## Existing decisions versus implementation evidence

Previously accepted direction is retained: turning and milling share a core with separate tool models; specification and production tests are reported separately; interface match is distinct from physical suitability; current tolerance behavior is preserved, with detailed tolerance/rounding validation deferred.

These are accepted planning decisions, not evidence that code or tests already implement them. This document does not create or close TOOL-01C-03.

## Information ownership and layer boundaries

Ownership below means proposed technical responsibility, not an assigned person.

| Claim | Producer / authority | Composition responsibility | Fail-closed boundary |
| --- | --- | --- | --- |
| STEP document and face identity | CAD import and mapping evidence | Application preserves source linkage | Reject missing or ambiguous linkage; never use enumeration index as identity |
| Material side / concavity and evidence scope | CAD verification evidence | Application checks evidence required by the requested operation | Block dependent choice if unverified; point evidence is not whole-face evidence |
| Units and coordinate provenance | Source intake and verified conversion/transform evidence | Application checks spatial comparability | Block dependent dimensional/spatial evaluation if unknown |
| Supplier revision and reviewed field mapping | Catalog intake / tooling validation | Application retains exact revision references | Unresolved vocabulary must not become a match |
| Insert-holder interface result | Tooling evaluator, within its bounded scope | Application must preserve that scope | Never elevate interface match to physical or machine approval |
| Physical/machine suitability | Separate verification process, not yet established here | Future composition may consume explicit evidence | No physical approval from catalog or interface match alone |

Geometry/Topology must not select products or own catalog policy. Tooling must not acquire SolidWorks Host/COM dependencies. Application is the proposed composition location; this is not authorization to add references or contracts.

## Proposed handoff vocabulary and assembly decision

AnalysisRequirement, ToolCandidate and BlockedByUnverifiedGeometry are design labels only, not existing APIs.

A catalog candidate requires an identified product revision and source lineage. Merely listing a catalog candidate does not require an assembly revision.

An evaluation that relies on an assembled tool requires an explicit immutable, resolvable assembly revision and component references. Absence blocks that assembly-dependent evaluation. Neither case grants physical suitability.

This assembly distinction is a review proposal until explicitly dispositioned in TOOL-01C-03; it is not silently treated as an accepted contract.

## Six acceptance scenarios — specification only

These rows define expected behavior for later verification. They are not executed tests or PASS claims.

| Scenario | Input / control | Required outcome | Evidence gap |
| --- | --- | --- | --- |
| Face list reordered | Same exact source bytes and source face ID; different enumeration | Same source linkage; no list-index identity | ManufacturingRegionContractTests.ContentChange_ChangesFingerprint_AndSessionFaceIdIsTransient explicitly asserts different CanonicalFaceId values on repeat imports of identical bytes. Use (DocumentFingerprint, SourceFaceId) as the source identity pair; CanonicalFaceId is session-local. List-order invariance itself is not independently tested here. No continuity across a new STEP export is inferred |
| Material side unverified | Operation requires inside/outside or concavity; evidence unverified | Block dependent choice with explicit reason | CAD physical evidence and future composition gate |
| Catalog revision refreshed | Old selection pins revision r1; r2 introduced | Old selection still resolves unchanged r1; new choice is explicit | PR #87 now supplies positive test-local coexistence proof; production history remains unimplemented |
| Unsupported units | Required dimension has unknown unit | No guessed conversion or dependent match | Production validation path not proven by test-local rules |
| Transform absent | Spatial evaluation needs model-to-STEP alignment | Block spatial evaluation; report missing provenance | Transform evidence and handoff representation |
| Vendor code or hand unresolved | Unmapped mounting vocabulary or unknown hand | No interface-match approval | PR #85 rejects Unknown/null in production; known Neutral and unverified integer mounting semantics remain open |

## TOOL-01C finding disposition linkage

Assignments are planned work boundaries, not verified closure or authorization to implement.

| Finding | Related work | Requirement at this boundary | Closure evidence |
| --- | --- | --- | --- |
| TL-01 / TL-02 | TOOL-01C-04 | Unknown hand/unmapped mounting cannot approve match | Negative tests against actual production types |
| TL-03 / TL-07 | TOOL-01C-05 | Accepted envelope must actually construct validated domain objects | Production-path mapping tests; specification/production results separated |
| TL-04 | TOOL-01C-03 policy; TOOL-01C-05 validation | Explicit field-wise identity policy and conflict detection | Normalization decision and source/interface conflict tests |
| TL-05 | TOOL-01C-06 | Historical assembly remains resolvable at its pinned revision | Positive old/new revision coexistence and immutable-reference tests |
| TL-08 | TOOL-01C-04 | Result visibly scoped to interface matching | Delivered by PR #85; physical qualification remains separate |
| TL-09 | TOOL-01C-04 minimum policy preparation; separate tolerance sprint | Preserve current behavior; do not redefine rounding here | Dedicated later tolerance/rounding verification |
| TL-10 | TOOL-01C-04 | EAN/GTIN validity and master-insert text rules require explicit disposition; no inferred SKU resolution | Invalid identifier tests against production types and documented raw/normalized text rules |
| TL-11 / TL-12 | TOOL-01C-07; source identity also informs earlier work | Define hashed source object, atomicity, retry, concurrency and authorization | Source trace and fault-injection evidence before persistence |

TL-06 turning/milling scope is accepted planning direction; implementation adequacy remains to be verified. Other report findings remain in TOOL-01C disposition; their omission from the six scenarios is not closure.

## Accepted / Open decisions

Accepted below records the prior planning instructions supplied in this conversation. Those instructions are not independently verifiable from the repository; PR #81/#83 establish design boundaries but do not prove these user approvals. Treat these entries as reported planning context, not repository acceptance evidence or implementation PASS. Proposed entries remain unapproved.

| Decision | Status | Related work / remaining evidence |
| --- | --- | --- |
| Turning and milling: shared core, separate tool models | Accepted planning direction | TL-06; production model adequacy still to verify |
| Specification and production tests reported separately | Accepted planning direction | TOOL-01C-05: #86 adds production-model tests; importer/source mapping remains open |
| Interface match distinct from physical suitability | Accepted planning direction | TOOL-01C-04: InterfaceOnly delivered by #85; no physical suitability claim |
| Preserve existing tolerance behavior; minimum policy preparation only | Accepted planning direction | TOOL-01C-04 |
| Detailed tolerance and rounding policy | Deferred | Separate tolerance micro-sprint; no new policy adopted here |
| Assembly revision: catalog candidate versus assembly-dependent evaluation | Open proposal | TOOL-01C-03; explicit disposition required |
| Unknown units block dependent evaluation | Proposed, not accepted | TOOL-01C-05; unit policy and production validation |
| First bounded bridge inside Application | Accepted planning direction in the continuation instruction | Dependency discovery below supports this location; separate project deferred. No reference or runtime implementation added |
| EAN/GTIN validity and master-insert text policy | Open in this discovery record | TOOL-01C-04; explicit policy disposition remains required; do not infer acceptance from TL-10 linkage |

## Actual dependency discovery and minimum gate (2026-10-27)

Inspected at main `53e87d33dbd4fdcbeacee45d886d7994266352e9`:
- `src/MPC.Application/MPC.Application.csproj`: net8.0; references Connectors, STEP, Geometry, Mapping and Topology, but not Tooling.
- `src/MPC.Tooling/MPC.Tooling.csproj`: net8.0 with no project references.
- Worker references Application and Contracts; net48 SOLIDWORKS Host references Contracts, not Application or Tooling.
- `ManufacturingRegionImportService` reads STEP bytes once and hashes the snapshot used for mapping. `ManufacturingRegion` exposes source hash/face ID and planar geometry, without a complete evidence/unit/transform contract.
- `tests/MPC.Application.Tests/Manufacturing/ManufacturingRegionContractTests.cs`, test `ContentChange_ChangesFingerprint_AndSessionFaceIdIsTransient`, asserts equal fingerprints and unequal canonical GUIDs for repeated imports. This is an existing executable test assertion, covered by the successful main full suite; no new dedicated test was run during discovery.

Planning choice: place the first bounded eligibility composition inside Application; defer a separate bridge project. Preserve Tooling's CAD/COM independence and the Host/Contracts process boundary. This is a location decision, not approval to implement a complete catalog or spatial contract.

The first proposed outcome means evidence sufficiency for a named evaluation only. It never means tool selection, interface compatibility, physical fit, machining safety or NC authorization. No new API names or DTO are established by this document.

| Required evidence | Minimum rule | Evidence owner / remaining gap |
| --- | --- | --- |
| Exact STEP and face linkage | Require the document fingerprint and resolvable source face together; reject a mismatched source | CAD import/Application; do not use canonical GUID as persistent identity |
| Units | Dimensional evaluation requires explicit supported units and conversion lineage where applicable | Source intake; no production catalog unit boundary yet |
| Transform | Require verified transform lineage only when the evaluation compares spatial frames; irrelevant checks are explicitly not applicable | CAD/Application; missing representation is not an identity transform |
| Geometry verification | Each requested claim requires evidence with matching face and spatial scope; Unknown blocks dependent evaluation | CAD verification; point evidence does not qualify a whole face |
| Catalog lineage | Require uniquely identified product revision, source linkage and reviewed mappings relevant to the requested evaluation | Catalog intake; snapshot digest alone does not supply this lineage |
| Assembly revision | Required only for assembly-dependent evaluation; candidate listing does not imply an assembly | Proposal still pending TOOL-01C policy disposition |
| Result scope | Preserve InterfaceOnly; return reasons for missing evidence, without issuing physical approval | Future Application composition; trusted evidence producers must be defined |

Before production implementation, define the smallest trusted evidence input and validation path, its owner, and the permitted named evaluation. Do not replace absent producers with caller-supplied “verified=true” flags or synthetic revision strings. Positive tests must use the actual production evidence path; test-local approvals cannot establish a production GO. Unknown, missing, conflicting and not-applicable states must remain distinct.

## Independent discovery review response (2026-10-26)

Review revision: MPC_TOOL-01D-00_Discovery_Inceleme_Raporu.docx, dated 2026-10-26, including section 7 post-revision assessment; original uploaded DOCX bytes SHA-256 `D8BD2E6CCA7EA77E4E7CFA796EC2C75132BB702A770B54B75724797669EC9FC6`. This is a conversation-provided copy, not a repository artifact. Initial verdict ACCEPT WITH CONDITIONS; section 7 verdict ACCEPT for the revised discovery text, with CLOSED dependent on final commit and full CI. The reviewer did not independently verify PR CI or physical checklist evidence. Those limitations remain explicit.

| Condition | Draft response | Closure state |
| --- | --- | --- |
| K1 / D1 | Accepted/Open decision table added, including alternative bridge location | Closed at document-text level by independent review section 7 |
| K2 / D2 | CanonicalFaceId determinism gap and source-identity pair added to scenario 1 | Historical text accepted by review section 7; later source inspection found an existing repeat-import test proving session-local IDs (see dependency discovery below) |
| K3 / D3 | TL-10 added to disposition table | Closed at document-text level by review section 7; no production fix claimed |
| K4 / D4 | Original SR01 DOCX version and byte hash pinned; trace clarified below | Source trace accepted by review section 7; final commit and full CI remain pending |

## Discovery acceptance and implementation gates

Discovery acceptance requires:
1. Existing versus proposed fields/APIs explicitly separated.
2. Each scenario names evidence, owning layer and blocking behavior.
3. Assembly optionality is no longer contradictory; the proposed distinction remains visibly pending disposition.
4. Senior findings link to work and evidence, without converting static findings or synthetic fixtures into production PASS.
5. Source/face identity is scoped to exact document bytes; local and whole-face evidence are separated.
6. Final documentation diff stays within this one file. Changed final commit requires full CI; existing baseline CI is reusable only for unchanged baseline.
7. Accepted decisions and unresolved decisions are separately recorded before declaring the discovery CLOSED.

GO: documentation review and finding disposition only.
NO-GO: production contracts/DTO, runtime bridge, persistence/importer, selection algorithm, UI, machine qualification, toolpath and NC output.

Implementation requires relevant TOOL-01C findings dispositioned and verified, required CAD evidence accepted at the scope of the requested operation, and dedicated actual dependency/DTO discovery. No provider or independent-review recommendation is automatically adopted here.

## Review trace

Start/base: f0281cc241b76019b2846677f98fdcc932e768d8.
Reviewed PR head: 717449ec7b16e59c1802ef00c168ff805f9c5f71.
This deliverable: replacement documentation proposed on PR #84, including 2026-10-27 reconciliation.
Closing commit: pending final documentation commit and CI. Discovery acceptance does not close production findings.
Target PR branch: docs/TOOL-01D-00-interface-discovery. The PR delivery record must identify the actual final commit and its full CI; baseline CI does not validate this revised text.
