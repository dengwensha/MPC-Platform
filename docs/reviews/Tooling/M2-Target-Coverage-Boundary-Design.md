# M2 — target, occurrence and coverage evidence boundary (proposal)

Date: 2026-11-24 Europe/Istanbul. Class: VERIFY / architecture discovery.
Start: e2ae853acd34e5adfb8e2fe5d7a6c68ce741383b.
Branch: docs/M2-target-coverage-boundary. Owner: Mehmet İlkay Kınacı.
State: PROPOSED; independent Senior design review required, no implementation.
M2 OPEN. N1 bounded acceptance unchanged; C1 accepted in its consumer-free scope.
E1/#130 bounded CLOSED under Senior §32 after #131 and main CI #283.

## One goal and verified starting point

Specify the missing target/physical-occurrence and positive coverage evidence before
implementing a combined gate or evaluator. This is a refinement of #122/Senior §23,
not permission to start feature recognition, time/cost estimation or tool selection.

| Source inspected at Start | Verified boundary |
| --- | --- |
| Inventory/InventoryEvidence.cs | Private capture, retained inventory, owned header/unit and envelope observations; no target/coverage producer |
| Inventory/HeaderUnitObservation.cs | Header policy 2, unit policy 1; per-context observations; conversion units blocked; no normalization proof |
| Manufacturing/ManufacturingRegionImportService.cs | Filters semantic MANIFOLD_SOLID_BREP roots and requires exactly one; no complete candidate/occurrence proof |
| Step/StepSemanticHandlerCatalog.cs | Explicit semantic dispatch catalogue; handler existence is not whole-file classification or occurrence support |
| MPC.Mapping.Step/StepConicalSurfaceGeometryMapper.cs | Copies source.SemiAngle into its calculations; no context-angle conversion at this boundary |
| Manufacturing/ConeLocalNormalQuery.cs | Uses Math.Tan/Cos/Sin on cone.SemiAngle, hence dependent on radian interpretation |

Source paths above are under src/MPC.Application except the explicit mapping path.
Header v2/envelope and E1 do not themselves supply target or coverage readiness.
CI #283 at Start: Windows Release 0 warnings/errors, 2147/2147; Worker 5/5;
inventory measurement controls 6/6. This validates the starting code, not this proposal.

## Proposed initial scope: one direct physical solid occurrence

Initial target capability accepts only a positively classified direct, single 3D
MANIFOLD_SOLID_BREP occurrence. No first/largest/visible-body choice. A unique root
ID alone is insufficient. Target identity contains document hash, root ID,
representation ID, context ID and occurrence identity, plus separately pinned
root-definition and occurrence-classification revisions. A bare entity not bound
to a supported representation/context is not a usable target.

Enumerate the WHOLE retained inventory before selecting a target. Classify every
record using an exact, versioned type/component-role manifest. Recognize unsupported
candidates such as BREP_WITH_VOIDS, FACETED_BREP, surface and wireframe forms, including
complex records. The most specific complex form controls classification: a component
named MANIFOLD_SOLID_BREP cannot hide a void/faceted/unsupported extension. Unknown
components or conflicting roles prevent positive proof. Handler absence cannot
make geometry disappear. Unreferenced geometry also counts against whole-file scope.

Direct occurrence proof requires one supported representation of one supported root
in one resolved 3D context, with no second geometry-bearing representation, root,
placement instance or unclassified relationship. Two representations sharing one
root are not assumed equivalent. Repeated item membership is not silently deduped.
Multiple contexts alone are harmless; a second unused unit context is not itself
another physical body. Context binding must come from typed representation fields,
not the first context, proximity, matching units or a global reference search.

For v1, MAPPED_ITEM/representation mapping, assembly placement and transformed
representation paths are explicit ScopeNotSupported, even if only one occurrence
might exist. This avoids pretending that unimplemented assembly traversal proves
one instance. Preserve offending IDs/relationships; two placements of one root
must never pass. Supporting even one mapped instance later requires a separately
reviewed traversal/transform policy with cycle, multiplicity and unit tests.
An unrecognized relationship is unavailable classification, never a direct-instance
proof. Existing importer behavior remains unchanged by this design.

## Positive ledger and evaluator-specific coverage

Two proofs are distinct: global record-role/occurrence classification establishes
scope; evaluator-specific reachable dependency coverage establishes usable geometry.
An unreachable record cannot be ignored to bypass inventory policy A or global
scope. Each global record has exactly one reviewed role (supported target structure,
required dependency, permitted non-geometric metadata, recognized unsupported
geometry/relationship). Unknown type/component/role blocks by default, even with
zero inventory diagnostics. Non-geometric status requires a manifest entry and
validated structure; a name suffix or no incoming reference is insufficient.

The manifest must list exact simple/complex signatures, parameter and reference
roles, cardinality, supported context/placement behavior and disposition. It must
be ratified with fixtures before the first positive target producer merges.
The examples in this proposal are requirements, NOT an exhaustive approved list.
No generic geometry allowlist or evaluator revision is ratified by this PR.

The coverage ledger for a declared evaluator records every required dependency,
its source ID/type/role, typed reference edges, mapped/validated disposition,
normalization revision and reasons. Unresolved, duplicated, partial, dropped or
unsupported required records prevent complete coverage. A semantic reader returning
null, a map succeeding for one root or absence of diagnostics is not coverage proof.
Graph traversal uses typed edges, detects cycles where prohibited and accounts for
shared dependencies without inventing duplicate physical occurrences. Results and
reason lists are deterministically ordered by ordinal source ID/code; retain all
relevant findings, not only the first error.

A coverage manifest declares the exact evaluator ID/revision, supported geometry,
topology/material-side and precision requirements and angular-valued parameter
roles. A future evaluator must receive only the corresponding trusted combined
decision. There is no universal Ready/manufacturable result. Until a concrete
manifest and normalization implementation are reviewed, coverage is unavailable.
Do not infer existing mapper coverage from its handler catalogue.

## U1 — angular geometry cannot waive angle proof

Before mapping any angle-bearing target dependency, require an unambiguous supported
angle unit in its bound context. At minimum CONICAL_SURFACE.SemiAngle requires
radian evidence under ContextUnitPolicy 1. Degree/conversion units remain blocked;
the factor's base radian record cannot substitute. Missing, conflicting, unknown
or unsupported angle evidence prevents angle-dependent geometry mapping/evaluation.
If context attribution of a dependency is ambiguous, do not borrow another context.

An evaluator saying it does not need angles cannot waive an angle used to construct
geometry. Waiver is possible only when the complete typed coverage ledger positively
proves absence of ALL angular-valued fields in the admitted dependency set AND the
evaluator requires none. Unknown geometry cannot support this absence proof. Circular
geometry with no admitted angular-valued field is not automatically a cone, but its
actual typed dependencies still need audited classification.

Units are bound after target/context selection; production order is inventory A +
envelope/header prerequisites, then target, its units, normalization and coverage.
Per-context observations may be produced earlier. Length scale metadata is not
coordinate normalization; dimensioned output requires a separately validated same-
capture conversion contract. Radian/millimetre metadata never repairs old mapped
coordinates implicitly. No legacy StepUnitResolver change is authorized here.

## Same-session issuance and non-ready semantics

Proposed target/coverage observations are immutable with private constructors, issued
only from the trusted retained inventory after Owns(evidence) succeeds. Neutral
parser data cannot mint bound proof. Every path to content requires ownership,
including binders and internal helpers. Equal hashes do not allow foreign sessions.
No caller-supplied target ID, clean flag, detached observation, cache or public
metadata factory can authorize evaluation. Keep proofs free of inventory/session/
stream/byte-array back-references; owning session may retain private bound views.

The future combined decision has a private constructor and one Application issuer
accepting session + evidence + reviewed evaluator identity, not detached observation
parameters. It reads owned observations itself, verifies producer and independent
policy/root/occurrence/manifest revisions and preserves capture cause/stage (A4).
No host/Worker success flag substitutes. Missing/foreign/unknown evidence is non-ready;
known unsupported scope is distinct from malformed input. These are conceptual
reasons/outcomes, not shared enum values or a wire contract change.

Before combined-gate merge, extend the §29 reflection lock to ALL MPC.Application
methods AND constructors, recursively checking arrays/by-ref/generic wrappers;
exclude only each bound type's own necessary private constructor. Extend protected
types to target/coverage observations and neutral data. Test same-hash foreign
sessions, missing owner, private construction, no back-reference and issuer-only
decision creation. Production must omit afterSourceCaptured (N2).

Optional envelope/inventory DATA-record count consistency may reject a mismatch;
equal counts cannot establish entity identity or replace E1/global classification.
Do not use naive textual counts including comments. Any count producer needs its
own reviewed lexical rules/source pin; it is not part of the first target slice.

## Proposed acceptance matrix (implementation prerequisites, not test results)

| Case | Required observation / action |
| --- | --- |
| One direct supported solid, complete classified graph, unique representation/context | Target proof only; no evaluation readiness |
| No root; root has no representation/context | No target / unavailable binding, never choose a context |
| Four BACK PLATE bodies | ScopeNotSupported, no body selection or repair |
| BREP_WITH_VOIDS or FACETED_BREP (simple or complex) | Recognized unsupported geometry, never hidden by base component |
| Solid plus surface, wireframe or unreferenced extra solid | ScopeNotSupported |
| One root referenced by two representations or duplicate item entries | No equivalence/dedup assumption; no positive direct proof |
| One root placed twice; even one mapped/assembly instance | ScopeNotSupported under direct-only v1 |
| Unknown record/component/relationship, even diagnostic-free | Classification unavailable; no positive scope/absence proof |
| One target context plus unused context with same/different valid units | Bind actual target context; no global unit or multiplicity-only rejection |
| Missing/ambiguous/2D target context | No supported 3D target; report limitation, never invent 3D |
| Cone + radian + proven required normalization/coverage | Angular requirement satisfied only, not blanket Ready |
| Cone + degree with radian base / missing angle / evaluator angle waiver | Non-ready before angle-dependent mapping |
| Classified complete no-angular-fields graph and no-angle evaluator | Angle waiver only; all other gates still required |
| Unknown or partial dependency / dropped semantic record / unresolved reference | Coverage incomplete/unavailable |
| Invalid typed edge/cardinality or prohibited cycle | Preserve reason, no positive proof |
| Foreign equal-hash evidence or stale manifest/revision | No bound proof/decision |
| Raw data construction or observation passed to another factory | Cannot mint proof; reflection test detects detached entry |
| Capture/parse/producer failure | Original stage/cause; no fake zero diagnostics |
| Shuffled record order and repeated process | Stable target identity, ledger and ordered reasons |
| Inventory Error/Fatal in unreachable record | A still blocks; no target-level exemption |

B2's current 2D unit observation limitation remains tracked; this target proposal
must not relabel it as supported. A later fix requires scoped fixtures/review. Real
workshop/Festo rejection incidence is measured after a classifier exists, with
correlated families/versions identified, never inferred from zero diagnostics.

## Delivery and decisions requested

1. Review this direct-only boundary and the U1 prerequisites. No implementation yet.
2. Ratify an explicit record-role/root/occurrence manifest with synthetic positive
   and negative fixtures; implement only consumer-free target/context evidence.
   Pin complete producer source, reason/severity map and independent revisions.
   No whole-file Ready result or geometry support expansion. Record/profile real
   inputs without publishing raw CAD; report unsupported and unavailable separately.
3. Review a concrete evaluator coverage/normalization contract; implement consumer-
   free coverage, including U1 before angle-dependent mapping. Tests must demonstrate
   cone + degree/missing angle cannot reach radian-dependent geometry consumption.
4. Implement reviewed combined gate and reflection expansions; then a separately
   reviewed consumer with A4/N2 and Windows real-file repetition. M2 closure needs
   enforcement/consumer evidence, not this proposal or a target producer alone.

Senior decisions: (1) accept/amend direct-only v1 and fail-closed global role manifest;
(2) accept U1 waiver/ordering and ownership boundary; (3) accept/amend staged delivery
and prerequisites for the target-only implementation. No evaluator is selected here.

Risk triggers: trust/fail-closed YES (future proof sufficiency); wire, shared enum
numeric values, IPC, persistence and UI entry NO. Independent final-diff design
review before merge. Documentation-only: git diff --check and required PR Windows
CI; no new runtime test is needed for this proposal. CI cannot ratify its semantics.
Reviewed/Closing: pending independent review/merge. Scope excludes geometry healing,
assembly/selected-body support, Host/Worker/IPC/DTO changes and feature/time/cost code.
Owner of remaining follow-ups: Mehmet İlkay Kınacı; §29 scans due at combined gate,
U1 before angle-bearing mapping, Windows repetition before first Windows consumer,
B2 before claiming 2D context support, N3/B6 before performance/optimization claims.
