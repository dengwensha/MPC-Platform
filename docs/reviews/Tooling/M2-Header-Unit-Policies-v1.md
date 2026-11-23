# M2 schema allowlist v1 and context unit policy v1

Date: 2026-11-23 Europe/Istanbul. Design only, no production implementation.
Start: `17a112446078b9349eac39962561b2adb9d63a8c`.
State: OWNER-ACCEPTED DIRECTION; independent Senior policy review pending.
Depends on amended PR #122 (head b3b18a5), not yet merged at preparation.
M2 OPEN; N1 acceptance remains separate/pending. Owner: Mehmet İlkay Kınacı.

## Scope and separate revisions

Owner accepted the narrow v1 recommendation: verified SI units and radian;
conversion-based inch/degree explicitly blocked, not guessed or silently converted.
This document makes concrete proposals for Senior review; it is not an approved
implementation or an evaluator authorization. Separate revisions:
HeaderSchemaAllowlist=1; ContextUnitPolicy=1. Future root-definition and evaluator
coverage-manifest revisions remain independent and are not minted here.
PR #122's same-session ownership, single composite issuer and Header -> Target ->
target-context units -> Coverage order remain unchanged. Initial evidence production
has no target selection: enumerate and record each representation context separately.

## HeaderSchemaAllowlist 1 — proposed exact rules

| Field | Required v1 behavior |
| --- | --- |
| Source | Parse exactly the privately captured bytes; no reopening original source path |
| Header | Existing required header fields must parse; preserve diagnostics and producer revision |
| FILE_SCHEMA | Exactly one decoded nonempty identifier, AUTOMOTIVE_DESIGN |
| Comparison | Ordinal exact comparison after STEP string decoding and trimming surrounding whitespace; no case folding, substring/prefix matching or alias inference |
| Implementation level | Exactly decoded 1 after surrounding whitespace trim |
| Missing/malformed fields | Block or evidence unavailable with original cause; never synthesize header success |
| Other schemas / multiple entries | Explicit UnsupportedSchema, including otherwise known AP203/AP242 identifiers, qualified aliases and duplicate schema entries |
| Other metadata | Preserve description/exporter metadata; description text such as AP214 does not establish schema or content compatibility |

All five repository golden files declare AUTOMOTIVE_DESIGN and 1. This is
fixture-backed choice of an initial allowlist, not STEP-wide certification or a
claim that every AP214 file passes. Real workshop/header bytes were not freshly
reprofiled for this policy session; inventory measurements are not header evidence.
Do not widen the list for convenience without reviewed revision and fixtures.
Metadata author/timestamp strings are not authenticity or legal permission proof.

## ContextUnitPolicy 1 — proposed exact rules

Enumerate each geometric representation context and its own GLOBAL_UNIT_ASSIGNED_CONTEXT
references using parsed parameters/reference identity, not raw substring matching.
Store context ID, dimension, assigned unit IDs, recognized definitions, unsupported
candidates, reason codes and prospective normalization factor. A geometric context
with missing/unresolvable unit assignment receives an explicit non-ready observation;
it must not disappear from the context set. Non-geometric contexts do not receive
invented dimensional requirements. No inheritance/default unit assumption.

| Dimension / definition | Policy v1 | Prospective scale to internal unit |
| --- | --- | ---: |
| LENGTH_UNIT + SI_UNIT($,.METRE.) | Supported definition | 1 metre |
| LENGTH_UNIT + SI_UNIT(.CENTI.,.METRE.) | Supported definition | 0.01 metre |
| LENGTH_UNIT + SI_UNIT(.MILLI.,.METRE.) | Supported definition | 0.001 metre |
| PLANE_ANGLE_UNIT + SI_UNIT($,.RADIAN.) | Supported definition | 1 radian |
| CONVERSION_BASED_UNIT, including INCH and DEGREE | Explicitly blocked | None |
| Other prefix/unit, including prefixed radian | Explicitly unsupported | None |
| Missing, cyclic, malformed or dimensionally inconsistent definition | Non-ready with distinct reason | None |

Within a context, require exactly one assigned length unit and one assigned plane
angle unit for a complete v1 unit observation. A missing angle unit cannot be
fabricated: retain the missing-angle reason; a later evaluator requirement may
declare that angle evidence is not needed without making the observation complete.
Other dimensional units (e.g. solid angle) are recorded separately, never mistaken
for length/plane angle, and confer no evaluator readiness. Duplicate assignments
for the same required dimension are ambiguous even when numerically equivalent.
Multiple distinct contexts with identical units are valid and remain separate.
Different contexts may have different supported units: preserve their own factors;
cross-context evaluation requires later validated binding/transforms.

Visit all assigned unit definitions, including unsupported ones. Do not filter
unknown candidates and accept the one remaining recognized SI record. An inch
conversion's referenced base millimetre is not the context length unit; a degree
conversion's base radian is not its angle unit. Conversion factor parsing must not
quietly enable these blocked forms. Legacy StepUnitResolver is unchanged.

Evidence of a supported unit definition/scale does not prove coordinates were
normalized. First slice emits unit observations only, never normalized geometry
or ReadyForDeclaredEvaluation. Later mapped geometry must carry separately verified
normalization using the selected context, with positive finite scales, consistent
point/vector/radius/angle treatment and dimensional scaling for area/volume.
CadUnitInfo.InternalLengthUnit=Metre alone is insufficient.

## Acceptance fixtures required before implementation merge

| Case | Expected |
| --- | --- |
| Valid golden AUTOMOTIVE_DESIGN, 1 header | Header definition accepted only |
| Missing/empty/malformed header or FILE_SCHEMA | Non-ready, retained cause |
| Unknown schema, AP203/AP242 name, lowercase alias, multiple/duplicate schemas | Unsupported schema; no fallback |
| Wrong implementation level | Explicit non-ready result |
| Captured input then original replaced/deleted | Header/inventory still derive from original captured bytes |
| UTF-8, supported BOM variants, invalid UTF-8 fallback | Path/captured header parity and encoding behavior aligned with inventory reader |
| Metre/centimetre/millimetre and unprefixed radian | Correct per-context scale, no consumer decision |
| INCH with base millimetre reference | BlockedConversionUnit, never millimetre |
| DEGREE with base radian reference | BlockedConversionUnit, never radian |
| Two contexts same units | Two valid separate observations |
| Two contexts different supported units | Separate correct factors; no global unit |
| Unitless context / unresolved ID / cyclic reference | Explicit non-ready observation, never omitted |
| Unsupported prefix plus recognized base SI elsewhere | Unsupported; no recognized-candidate filtering |
| Duplicate or conflicting required dimension assignments | Ambiguous, no arbitrary selection |
| Foreign equal-hash session proof / policy revision mismatch | Rejected by ownership/revision check |
| Unit metadata without normalization proof | No geometry-dependent evaluation |

Fixture expectations are proposed acceptance criteria, not executed test results.
Pin complete header/unit producer sources and diagnostic-code/severity mappings;
test exact reason/severity classification without changing the eight-code inventory
manifest's meaning. Unknown diagnostics/severities/revisions cannot imply readiness.
All failures retain stage/cause; no synthetic zero counts for unavailable evidence.

## Preconditions, next slice and review decisions

Before implementation begins: PR #122 merged green, independent review of each v1
policy above. Before implementation merges: N1 closed; header captured entry and
BOM/encoding parity tests; unit/header producer source/code/severity pins; all
required synthetic fixtures; independent final-diff review and Release/full CI.
No target selection, consumer, wire/DTO/shared enum, Host/Worker/IPC, temp snapshot,
production afterSourceCaptured use or silent StepUnitResolver behavior change.

Senior decisions requested separately: (1) accept/amend HeaderSchemaAllowlist 1,
especially exact schema/implementation-level scope; (2) accept/amend ContextUnitPolicy
1, especially conversion-unit blocking, duplicate assignments and angle-observation
requirements; (3) authorize the isolated per-context evidence implementation after
recorded preconditions. Any accepted amendment must be explicit, versioned and tested.
N1 and M2 do not close through policy review. GLOBAL_UNCERTAINTY, multi-body scope,
conversion support and evaluator coverage manifest remain later reviewed work.

Trust/fail-closed trigger YES: independent Senior review before merge. Other five
triggers No for this design-only diff. git diff --check required; PR CI pending.
Reviewed/Closing: pending final-diff review and delivery; no runtime PASS claimed.
