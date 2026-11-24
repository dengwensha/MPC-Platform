# M2 — exact target role contract v1 for ratification review

Date: 2026-11-24 Europe/Istanbul. Class: VERIFY / design preparation.
Start: 12e38d21afcc85fcccdd1a2f7e76c4fb9d030281 (#138 merge).
Branch: docs/M2-manifest-v1-contract. Owner: Mehmet İlkay Kınacı.
State: PROPOSED_PENDING_SENIOR_NOT_RATIFIED; M2 OPEN.

## One goal and authority

Specify the field and graph contract required by Senior SR02 section 37 before a
trusted, consumer-free target producer can be implemented. The companion
[JSON contract](M2-Target-Role-Manifest-v1-Contract.json) is the exhaustive proposed
signature/field/profile catalogue. This document defines interpretation, deltas and
acceptance. Neither file is loaded by production or by the T3 tool. Any conflict
between these two documents is an unresolved review defect, never a permissive
fallback. Senior acceptance and green final-head CI are required before promotion;
this PR does not declare the proposed contract ratified.

Only documentation changes. No src, production tests, tooling, reader, manifest pins,
wire/enum values, Host/Worker/IPC or private CAD content changes. No trusted target
observation, combined gate, consumer, physical-occurrence proof, angle-absence proof,
geometry support or Ready result is implemented. R-U v1's existing rejection rule
and N1's bounded acceptance remain unchanged. #137 transformation support remains
separate; even an apparently identity transform is still unsupported here.

## Verified baseline and provenance

The exact starting main commit passed Windows CI
[#295](https://github.com/dengwensha/MPC-Platform/actions/runs/45379081026): Release
zero warnings/errors, full regression 2183/2183, Worker acceptance 5/5, inventory
controls 6/6 and T3 controls 12/12. Those results validate the baseline, not the new
contract's semantics. The new documentation head requires its own Windows CI.

Historical T3 remains byte-for-byte unchanged at
`evidence/M2-Manifest-v1-T3/Manifest-v1-DRAFT.json`; SHA-256
`EE03563A63F98C79D629E8D387DD0AEBA7031BC9F021BC750FB4923E70DA1A41`.
The old 20-file/two-run evidence accounts for 444,497 records, with 17 draft
candidates and three unsupported files. It is NOT a run of this stricter contract.
CORE BAS remains the concrete warning that a draft candidate need not import or
have valid geometry. No unmeasured retention count is predicted here.

Implementation inheritance is grounded in the current files, not only prose:

| Source at Start | SHA-256, UTF-8 text with CRLF normalized to LF |
| --- | --- |
| `src/MPC.Application/Step/ImportUnitPrecheck.cs` | `64A488249CDE753A1B06E3DF5B7259AECAC296FBEB66194B3A952F8F9DEE7E6B` |
| `src/MPC.Application/Inventory/HeaderUnitObservation.cs` (including `StepEvidenceSyntax`) | `D062130ED86A69D3B0C26F3246CFF6EC3B4F72B891C9D4235F9FBD41C7041633` |

The B-spline enum/cardinality checks below were compared with the existing
`StepBSplineCurveWithKnotsEntity`, `StepBSplineSurfaceWithKnotsEntity` and rational
curve/surface entity classes. The contract is a deliberately bounded local policy,
not an assertion that these rows cover all AP214 or Part 21 conformance rules.

## Exhaustive field contract

The JSON contains exactly the same 86 whole signatures as the accepted draft:
54 conditionally admitted, five explicitly unsupported, 27 unclassified/blocking.
There are no new entity signatures or metadata exemptions. Every field of each
conditionally admitted component has a zero-based index and a named constraint.
Every reference profile expands to exact admitted target signatures; component-name
containment cannot accept an unknown subtype. Every global invariant I01–I12 is
mandatory; per-row invariant links are an index, not an exemption from other checks.

The five known unsupported signatures are ELLIPSE, NEXT_ASSEMBLY_USAGE_OCCURRENCE,
ITEM_DEFINED_TRANSFORMATION, CONTEXT_DEPENDENT_SHAPE_REPRESENTATION and the complex
transforming representation relationship. All 27 unclassified signatures remain
denied. A signature absent from the catalogue is also denied. Recognized void,
faceted, surface, wire, mapped or assembly forms cannot inherit a recognized base's
admission. Unsupported rows have no admitting field profiles: they fail immediately,
while independent parseable records still contribute their reasons.

| Field family | Proposed exact constraint |
| --- | --- |
| Text and optional text | Actual STEP string, empty permitted; `$` only at explicitly optional description fields inherited from R-U. No stringification/coercion. |
| Null and omitted | `$` and `*` are distinct; only the named profile permits either. ORIENTED_EDGE start/end are exactly `*`; NAMED_UNIT dimensions exactly `*`. |
| Numeric scalar | Actual integer/number parameter, invariant-culture finite binary64. NaN, infinity and conversion overflow fail. No silently substituted zero. |
| Analytic geometry scalar | Finite structural data only, including signed radius/magnitude/angle values. Sign, angle range, frame validity and geometry semantics remain separate mandatory checks before use; see below. |
| Integer scalar | Actual STEP integer within checked Int64 range, greater than zero for degree, multiplicity and application year. Context dimension is the literal integer `3`. A real-valued `3.0` is not an integer field. |
| Cartesian coordinates / direction | Exactly three finite members; direction has at least one nonzero component. No normalization, norm overflow or implicit 2D promotion. |
| Boolean / logical | Boolean exactly `.F.` or `.T.`. B-spline logical flags additionally permit `.U.`; `.U.` is not inferred false. |
| Other enumerations | Closed lists for curve form, surface form, knot specification, style side and make-or-buy, all explicitly enumerated in JSON. Unknown values fail. |
| References | Exactly one resolved Complete target matching its whole admitted signature and field rules. No first-match, name suffix or generic-reference inference. |
| Aggregates | Explicit min/max, member profile, order and duplicate rule. `maximum_items: null` means no schema upper bound, never permission to truncate. Resource exhaustion is unavailable/failure, not success for a prefix. |

Lists representing item sets (shape members, units, bounds, shell faces, styles,
layers and product lists) are nonempty and reject duplicate IDs. Unit list minimum
is two: exactly one length and one plane angle, plus at most one reviewed steradian.
Ordered edge-use lists retain repeated uses and have minimum one; loop validity and
the platform's later topology invariants are NOT established by this field minimum.
Control-point lists/grids and weights preserve duplicates and order. Grids are
rectangular with at least two rows and columns. Unknown members, `$` or `*` cannot
disappear from an aggregate during filtering.

For B-splines, curve degree p must be less than control-point count N. Knot and
multiplicity lists have equal length at least two; finite distinct knots strictly
increase. Multiplicity is positive, at most p+1 at the endpoints and p internally;
checked exact sum is N+p+1. Apply the same rules independently to surface U/V rows
and columns. Rational weights are finite positive and match control-point count or
grid dimensions exactly. No tolerance is introduced into these integer checks.

Analytic scalars intentionally have no new sign/range admission guarantee in this
target-only contract. For example, a finite cone angle or radius is not proof that
the cone can be mapped; a signed torus still needs the existing exporter policy and
semantic checks. This is an explicit review choice: target binding is distinct from
geometric validity, regularity, topology, normals and evaluator coverage. Existing
semantic rejection must never be weakened because a row is classified. Degree,
grid, knot and rational-weight constraints are finite record consistency checks,
not a promise that any curve/surface is regular or usable by a particular evaluator.

Identity policy is deliberately canonical `#[1-9][0-9]*`, with no leading-zero
alias normalization or machine-integer parsing of IDs. The measured corpus used
canonical IDs. Other identifier spellings block this proposed target contract;
this does not change what the public reader currently accepts. Reasons sort source
IDs ordinally, not by unsafe numeric conversion. Every record must be Complete and
every declared reference, including metadata, must resolve uniquely.

## M1: independently versioned binding and import compatibility

| Contract | Revision proposed here | Meaning |
| --- | --- | --- |
| RecordRoleManifest | `1-review.1` pending ratification | Whole signature/field/role catalogue and graph requirements |
| TargetBindingPolicy | `1-proposed` | Typed representation/product link plus supported, semantically equal endpoint units |
| CurrentImportCompatibility | `1-proposed` | Current unnormalized mapping path admits only mm + unprefixed radian |
| ContextUnitPolicy | existing 1, unchanged | Supports direct metre, centimetre, millimetre and radian observations |
| Header / envelope | existing 2 / 1, unchanged | Independent observations; not replaced by target proof |
| Evaluator coverage / normalization | absent | No corresponding permission or attestation |

Bind every revision independently to the owning evidence; unsupported/missing
revision cannot mean latest or v1. Store exact rational length scale to metre
(1/1, 1/100, 1/1000) and angle scale to radian (1/1). Two endpoint definitions are
compared by dimension, exact code and reduced rational scale, not IDs or computed
double coefficients. Different IDs with the same definition may match. There is no
coordinate conversion. Conversion-based, unknown or conflicting units remain
unsupported; no base SI fallback. Exact direct steradian can be retained as
auxiliary metadata, never substituted for plane angle.

| Endpoint scenario | Structural unit equality | Current compatibility | Positive target-only result |
| --- | --- | --- | --- |
| mm/radian at both ends, distinct IDs | Pass | Pass | Possible only if every other target requirement passes |
| metre/radian at both ends | Pass | Blocked | Forbidden |
| centimetre/radian at both ends | Pass | Blocked | Forbidden |
| mm versus metre | Blocked | At least one end blocked | Forbidden; preserve both layer reasons |
| Degree, inch, missing/duplicate angle or 2D target context | Blocked/unavailable as appropriate | Blocked/unavailable | Forbidden |

The existing R-U always-angle rule remains in this first target slice, even on a
planar solid. U1's future cone/angle-absence waiver is not implemented here. No
header/schema decision is inferred from product metadata or from the import status.
A later reviewed normalization revision may revise import compatibility without
redefining structural binding. Until then, a file rejected by ImportUnitPrecheck
MUST NOT obtain a positive trusted target-only result.

## Typed T1, T2, product and whole-file requirements

Use reviewed `StepEvidenceSyntax.Components` and the retained captured inventory.
The tooling splitter and observed-shape fallbacks are forbidden in an issuer.
Construct a complete reverse index for detecting additional paths; typed field
positions, not that generic index, establish the accepted path. The global checks
are at least as strict as ImportUnitPrecheck; faithful copies must run its same
tests and the new producer tests to prevent drift.

- Root: exactly one whole simple MANIFOLD_SOLID_BREP, direct member exactly once
  in one SR/ABSR. Other items are validated axis placements only. No second geometry
  representation, automatic choice, root deduplication or geometry-bearing alias.
- T2: either direct SDR, or one exact simple four-field nontransforming bridge to a
  distinct SR main endpoint with placements only. Either endpoint order. Exactly one
  SDR across both endpoints, targeting the main endpoint. Reject direct-plus-bridge
  double SDR, chain, branch, self-link, cycle, subtype, transform, mapping and assembly.
- Product: fully typed SDR → PDS → PRODUCT_DEFINITION → admitted formation → PRODUCT,
  with the definition/product contexts and APPLICATION_CONTEXT validated. Geometry
  incoming is only direct SDR or the one bridge; main incoming only SDR plus bridge;
  PDS incoming only this SDR; selected product-definition incoming only this PDS.
  This retains R-U checks and strengthens its not-fully-typed product tail. Other
  metadata does not authorize an assembly path.
- T1: validate every STYLED_ITEM, layer and presentation row, including all items
  in presentation lists. The exact finite catalogue is STYLED_ITEM, layer assignment,
  mechanical presentation representation, style assignment, style usage, side style,
  surface fill, fill-area style, fill colour and RGB. Child-role checks, side enum,
  RGB bounds and active-stack cycle detection are mandatory; sharing a valid style
  is allowed. OVER_RIDING_STYLED_ITEM, PMI and unknown style records remain blocked.
- Style/layer geometry targets must belong to the root's typed dependency closure;
  presentation cannot sponsor otherwise unowned geometry. Each styled item only has
  validated incoming presentation/layer references. Presentation contexts stay
  separate from the binding context. Presentation is never a physical occurrence.
- Account for every record. Geometry must belong to the root's typed graph or an
  admitted shape-placement dependency graph. A standalone point blocks; harmless
  metadata is not inferred from a name, suffix, lack of references or clean diagnostics.
  Every context and metadata row is field/reference validated even when not on the
  selected path. Uncertainty remains retained metadata; its length definition must
  agree with its owning context, but no precision adequacy claim is issued.

## Proposed observation, ownership and all-reason behavior

The JSON proposes separate layer outcomes: Observed means the checks completed and
passed; Blocked means known violation; Unavailable means a prerequisite could not
be established. None may be collapsed into successful zero diagnostics. Proposed
aggregate names are TargetBoundOnly, Blocked and NoEvidence. NoEvidence is only
missing owner/session/evidence; foreign ownership or unsupported revision blocks.
Do not inspect a foreign evidence object's data to obtain additional findings.

TargetBoundOnly requires real owner/session/revisions, InventoryClear, full record
and graph accounting, T1/product/binding/compatibility checks and no blocking or
unavailable target reason. Header and envelope keep their separate observations;
the future combined gate must also require them. A positive target result does not
replace either observation or Policy A and grants no permission to map/evaluate.

Follow the accepted C1 pattern: private observation constructors and neutral data,
owner-checked bind/read, no caller-constructed observation as gate input, no
evidence-to-session back-reference or equal-hash owner substitution. No fresh file
read or detached clean inventory/units. Future producer parameters must not accept
neutral/bound observations as authority. Extend the reflection parameter lock to
new types, all Application namespaces, methods and constructors (only the bound
observation's own private constructor is exempt); restrict issuers/return paths too.
Production must never pass `afterSourceCaptured`.

All 20 proposed reason codes and Error severities are explicit in JSON, separate
from existing inventory/header/envelope reason manifests; existing codes are not
renamed or demoted. Retain policy layer, code, source ID, component, field and nested
list indices, related IDs, original stage and stable occurrence index. Collect all
independently observable reasons across the inventory. For an unparseable/unresolved
parent, record the real failure plus dependent unavailability; do not invent child
field outcomes, suppress other independent failures or catch programmer faults.

Sort ordinal layer, code, source ID, component; then numeric field/list index tuple,
ordinal related-ID tuple and original stable occurrence. Deduplicate only an exact
same structured tuple; do not merge distinct fields into a single vague message.
Render display precedence without discarding the underlying full reason set.
Unexpected exceptions retain the existing policy, with no broad catch/new fake
input diagnostic. Existing importer/wire behavior is untouched by this design.

## Explicit deltas from historical T3 and acceptance before producer merge

This contract strengthens the draft's serialization shapes with exact field kinds,
lengths, enum values, full target types, all-row T1/product validation, B-spline
record consistency, identity and ownership/reason rules. It also explicitly permits
`$` axis/ref-direction fields already accepted by R-U and semantic readers, and
supports metre/centimetre only in the separate structural layer as required by M1.
Neither is a relaxation of current positive target/import compatibility. Profile
effects of stricter field rules are NOT yet measured. The old 17/3 count must not
be copied into an implementation report as if these new rules had run.

Before implementation: Senior must accept/amend this exact catalogue and the M1
split. After accepted document merge, build only the consumer-free C1-owned target
producer, with no physical-occurrence/angle-absence proof or Ready result.

Before that producer may merge:

| Required check | Evidence to retain |
| --- | --- |
| Historical 20-file + 12-control comparison | Same input hashes; draft result, ImportUnitPrecheck result and new producer layers/reasons side by side; every stricter difference explained; no R-U-rejected file gets TargetBoundOnly |
| M1 cases | Distinct-ID equal mm passes binding/compatibility; equal m and equal cm pass binding but fail compatibility; mixed units preserve both reasons; conversion and missing/duplicate dimensions fail |
| Field boundaries | For each reusable profile and each conditional field wiring: wrong kind, `$` vs `*`, min−1/min/extra items, duplicates where forbidden, nonfinite/overflow, invalid enum/ref target/unknown complex component; integer versus real; optional directions; B-spline degree/grid/knot/weight consistency |
| T1/product parity | All current precheck cases, style catalogue/target/side/RGB/cycle and product tests; additional all-row malformed/unowned/unknown presentation and product-tail cases; whole-list behavior |
| Determinism and ownership | Multiple simultaneous reasons in stable complete order; unavailable prerequisites; same bytes/different session and foreign hash; immutable data; reflection method/constructor lock; no production hook use |
| Traceability and platform | Exact producer and parser source hashes, catalogue bytes, code/severity/revision pins; two-process repeats; final-head Windows full build/regression/Worker CI; public Windows profile; private corpus scope stated honestly |

Any stricter loss of a previously successful mm/radian real import must be explained
and accepted by the owner before producer merge; do not silently expand the catalogue
to recover a file. Existing imports are not consumer targets in this next slice.
New exhaustive field tests and comparison measurements belong to the producer PR,
not fabricated as completed evidence in this documentation PR.

## Senior review decisions requested

1. Are the 86 exact signatures, 54 conditional field profiles, five explicit
   unsupported and 27 unclassified rows sufficient for the first trusted target
   producer? In particular, accept/amend the finite-only analytic scalar boundary
   and the additional B-spline/identity constraints without claiming geometry validity.
2. Does M1 correctly separate supported/equal binding units from current mm/radian
   compatibility, with distinct revisions and all reasons? Is the prohibition on a
   positive target for every R-U rejection adequately specified?
3. Are all-row T1, full product tail, unknown incoming path and unowned-geometry
   checks a faithful or stricter transfer of the reviewed R-U rules? Any role/field
   allowing a hidden geometry/occurrence or a false metadata exemption?
4. Are result semantics, unavailable dependencies, deterministic full reasons and
   C1/reflection boundaries sufficient for the consumer-free target-only slice?
5. Can this contract be promoted after any amendments and green exact-head CI,
   and can the trusted producer then proceed under the stated comparison/test gates?

Risk trigger trust/fail-closed YES; independent design review required. Wire/shared
numeric enums/IPC/persistence/UI entry NO. No additional physical acceptance is
needed for these documents. Local JSON/exhaustiveness/ref-link checks and
`git diff --check` are documentation consistency checks, not policy acceptance.
Reviewed/Closing and final-head CI pending; no merge before Senior decision.
Manifest remains proposed, M2 OPEN.
