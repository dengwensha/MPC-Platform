# M2 — target role manifest v1 draft and T3 discovery

Date: 2026-11-24 Europe/Istanbul. Class: VERIFY / architecture discovery.
Start: 7c360c64b3adbfc8e8e2ff9851ac7a97c9feb0f3.
Branch: docs/M2-target-manifest-v1-T3. Owner: Mehmet İlkay Kınacı.
State: PROPOSED; no trusted target producer, consumer, coverage or Ready result.
M2 OPEN. R-U v1 bounded CLOSED (#133); v2 transformation design OPEN (#137).

## One goal and authority boundary

Supply the separate manifest and pre-review T3 dry run required by Senior §33–§36.
No src/ or production tests change. Tool references Connectors.Step only, not
Application; it cannot mint C1 observations/evidence or evaluation permission.
DraftCandidateOnly means structural discovery candidate, NOT valid geometry,
calculation eligibility, a supported evaluator or a working legacy import.
CORE BAS is a concrete example: draft scope candidate while legacy import fails.

The extractor uses the existing public reader and parameter parser. Its generic
complex-component splitter is tooling-only, not a production parser replacement;
unknown/partial/duplicate/unresolved records block classification. Files are hashed
before/after reading. This is measurement identity checking, not an immutable same-
capture Application proof. All scalar CAD values in the neutral AST remain private
intermediates; only signatures/roles/IDs/counts/issues and hashes are committed.
No Application code, allowed units, legacy status or R-U guard is altered here.

## Proposed machine-readable manifest

See evidence/M2-Manifest-v1-T3/Manifest-v1-DRAFT.json. Each exact simple/complex
component-name + parameter-count signature has a proposed role, ordered field
names, parameter-shape alternatives and review status. All complex components must
match together; a recognized base cannot mask an unknown subtype. Geometry reference
fields have explicit expected target component names. CONICAL_SURFACE semi_angle
is marked angular-valued at field index 3; unknown records cannot prove angle absence.
No angle-absence proof is issued by this tool.

The authoring helper derives observed serialization SHAPES from the 20 inputs and
the independent positive cone; this is NOT automatic approval of observed types.
Roles/field labels/reference roles are manually declared; unknown administrative/
validation/property fields remain explicitly Unclassified. Optional descriptions
already accepted in R-U v1 are explicit alternatives, even if absent in the corpus.
Observed shapes are a conservative draft, not exhaustive AP214/schema conformance.
Aggregate member-kind shapes do not certify all cardinality/value/geometric invariants;
these require reviewed producer/evaluator validation before any positive usable proof.
No numerical geometry mapping/support is inferred from a classified dependency.

Proposed roles: TargetRoot, ShapeRepresentation, ShapeBinding, NontransformingBridge,
GeometryDependency, Presentation, ProductMetadata, Context, UnitMetadata,
UnsupportedGeometry/UnsupportedPath and Unclassified. Unknown signatures, shapes,
fields, reference-role mismatches and unresolved/duplicate IDs remain visible gaps.
27 observed signatures remain explicitly Unclassified; do not silently label them
metadata to make Festo/R03 pass. ELLIPSE is explicitly unsupported geometry in this
initial proposal (not present in the production semantic dispatch catalogue).

Metadata has exact signature/field roles, never a suffix or no-incoming-reference
exemption. T1 distinguishes styles/layers from body occurrences; an axis placement
listed by a shape representation may own its location/direction dependencies.
All other geometric records must be reachable from the declared solid graph.
A standalone/unreferenced point remains out of scope, even diagnostic-free.
Closure membership is discovery only; typed reference roles are checked where listed,
not a claim that every semantic or topological invariant has been implemented.

## T2 and M1 — structural binding separate from current import compatibility

Adopt the bounded STRUCTURAL R-U v1 rules from #135/Senior §35 and accepted #136/§36:
one simple non-transforming SHAPE_REPRESENTATION_RELATIONSHIP, direct root in one
geometry representation, unique SDR/product binding, semantic unit equality rather
than equal IDs, either endpoint ordering; reject mapping/transformation/assembly,
chains/branches/cycles, duplicate root membership, direct-plus-bridge dual SDR and
unknown incoming binding paths. The source rule file and production guard remain
hash-bound in Validation.json as the inheritance baseline. No v2 identity-transform
exemption is proposed; #137 remains separate.

Senior §37 M1 distinguishes TWO independently versioned policies:
- Binding: endpoint units must be supported by the applicable unit policy and
  semantically equal. Millimetres are not a permanent structural binding condition.
- Current import compatibility: until a separately verified normalization or scale-
  independent pipeline exists, only mm + unprefixed radian may enter the current
  mapping path. Metre/centimetre stay rejected by R-U v1; ContextUnitPolicy 1 is
  unchanged and can still describe their evidence.

For example, equal supported metre contexts may satisfy the structural unit-equality
condition while failing import compatibility. Preserve distinct outcomes/reasons;
do not call this an invalid representation binding. No positive trusted target
evidence may be issued for a file rejected by current ImportUnitPrecheck. A future
reviewed normalization revision may change import compatibility without redefining
the structural binding rule. Neither layer alone establishes evaluator readiness.

The committed DRAFT JSON, classifier and measured T3 outputs freeze the earlier
combined mm/radian discovery predicate. They are historical draft measurements,
not the normative separated implementation; no code, manifest hash or measured
outcome is changed here. BEFORE DRAFT promotion or trusted target implementation,
encode binding and compatibility as separate revisioned requirements, specify their
reason aggregation and test equal-metre binding versus blocked import compatibility.
M1 is now explicit in this design; its machine-contract/test realization is a
mandatory pre-producer gate, not claimed implemented by this clarification.

The dry-run binding model exercises these requirements but is NOT a production-
complete implementation or attestation of equivalence to ImportUnitPrecheck.
In particular this draft does not yet fully validate every T1 style dependency or
product-formation field, nor every aggregate cardinality. Positive same-session target
evidence implementation must implement/review all inherited checks and tests; this
prototype is not to be copied into an evidence issuer without that work.

## T3 bounded inputs, complete accounting and results

15 real files (R01–R03, 11 workshop versions, one unique Festo), plus five goldens.
15 real hashes checked against previous manifest; workshops are correlated families/
versions, not independent jobs. Local-use permission unchanged; no raw private CAD
published. Source/manifest/input/reader hashes, SDK/build commands, separate-process
repeat and exact code provenance are recorded in Validation.json.

Each record is assigned a ledger group: exact signature, role and issue set. Every
source ID is losslessly stored as sorted integer deltas, unsigned base-128 varints,
base64 in EntityIdsDeltaVarintBase64. The decoder is in scope_draft.py; decode lists
are verified against all source IDs and counts, not sampled. Issue groups also have
human-readable ID ranges. Ledger SHA-256 covers the full grouped per-record mapping;
no geometry values or raw statements are in that ledger. Missing observation is
never presented as zero diagnostics or a candidate.

Initial full dry-run: 17 DraftCandidateOnly; three ScopeNotSupported:
- Corrected BACK PLATE: four solid roots, not a single target.
- Festo: unsupported ELLIPSE dependencies plus unclassified property/validation data.
- R03: transforming/assembly paths, unclassified administrative data and unreferenced
  geometry; all reasons retained rather than hiding one behind another.
No corpus file was changed to improve these results. T001's colour/layer presentation
is not mistaken for a second body. R01/R02 and five goldens are draft candidates.
Other candidates do not establish successful mapping or full support (CORE BAS).

A file can have both unsupported scope and classification gaps. ScopeNotSupported
is the display precedence only; complete per-record issues and all reasons remain.
No numeric physical occurrence count is asserted for unsupported mapping/assembly
paths. Two placed instances cannot pass merely because only one root ID exists.

## Twelve public structural controls

C01 styled/layered T001; C02 same root in a second geometry representation;
C03 equal-unit T2 bridge; C04 direct-plus-bridge double SDR; C05 different bridge
units; C06 standalone point; C07 unknown record; C08 complex void/base combination;
C09 solid plus shell surface model; C10 two mapped items referencing one root;
C11 independent analytic radian cone; C12 degree declaration on that cone.
C01/C03/C11 are draft candidates; C07 is classification unavailable; other cases
are non-ready scope. All deny production evidence, physical-occurrence proof and
angle-absence proof. Negative structural controls are not claims of full valid AP
exchange/geometry. The public generator and exact generated hashes are retained;
no altered-prism control substitutes for the independent positive cone.

## Delivery, acceptance and unresolved decisions

Independent Senior review must decide the exact manifest rows/roles, especially
unknown metadata and the finite presentation/placement exemptions. Accept/amend T2
inheritance and the distinction between global record-role classification and later
evaluator-specific dependency coverage. Unclassified signatures may remain blocking;
new role support requires a reviewed version, not an implicit "harmless" fallback.

This PR is discovery/design, not permission to run geometry evaluators. Before a
trusted target-only implementation: manifest ratified with T3 results, concrete
parameter/cardinality/value/reference checks specified, all T1/T2/R-U inherited
requirements tested, C1-owned issuance and source/reason pins; no detached data entry
or Ready consumer. The §29 all-Application/constructor scan remains a combined-gate
requirement. U1 remains mandatory before angle-bearing geometry use. Header/envelope,
normalization, evaluator coverage and consumer enforcement remain separate M2 gates.

Local extractor Release build: 0 warnings/errors, SDK 8.0.422 Linux. No new runtime
regression tests; tooling controls and full final-head Windows CI required. Windows
step repeats public references/goldens/cone and 12 controls, not private workshop/
Festo data. AST intermediates stay outside uploaded evidence. Final PR CI pending.
Trust/fail-closed trigger YES (proposed future sufficient proof/role policy); wire,
shared enum values, IPC, persistence and UI entry NO. Independent final-diff design
review before merge. Reviewed/Closing pending; R-U v1 and N1 scope not expanded.


## Senior section 37 — draft acceptance and pre-producer gates

Owner supplied independent review of dca251b546ae780b3105e3cc0d3bd5224be8b930
on 2026-11-24. Reviewer independently decoded every ledger: 444,497 records exactly
once, matching hashes; 20 repeated outputs equal; 12 controls as expected. Decisions:
draft manifest/T3 sufficient; 27 unclassified signatures may remain blocking; T1 and
angular-field list accepted within the declared type set. No Windows CI/dotnet rerun
by reviewer claimed. Merge is conditional only on green exact-head Windows CI.
M1 above is a pre-producer condition, not a document-merge blocker. This amendment
implements that explicitly requested distinction without changing measured tools.

Before any trusted target producer:
1. Ratify/promote the manifest after explicit lengths/cardinalities, allowed enum
   values, reference types and numeric conditions are specified; realize M1's
   separate binding/compatibility contract and tests. DRAFT acceptance is not this
   promotion or automatic implementation authorization.
2. Use ImportUnitPrecheck's full T1 style catalogue, style target/cycle and product-
   binding checks, shared or faithfully ported with the same tests; the draft's
   type-name-only exemptions must not become trusted evidence logic.
3. Use reviewed StepEvidenceSyntax.Components, not the tooling complex splitter.
4. Compare the trusted producer with BOTH draft and ImportUnitPrecheck over all
   20 files and 12 controls. A R-U-rejected file must never get positive target
   evidence. Explain stricter differences instead of silently relaxing policies.
5. Apply C1 owned issuance/read access, reflection guards for new types and full
   producer source/reason/severity pins. No consumer or Ready result.
6. Preserve all reasons in deterministic order. This producer must not issue
   physical-occurrence proof or angle-absence proof.

Manifest DRAFT, M2 OPEN. R-U v1 and N1 bounded acceptances unchanged. Deferred roles
remain blocking; validation properties may later support independent geometry
checks only through a separately reviewed contract, not a harmless-metadata guess.
