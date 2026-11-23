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
| Comparison | Schema raw string content must contain only ASCII [A-Z0-9_], then ordinal exact comparison; no schema escape decoding, case folding, substring/prefix matching or alias inference |
| Implementation level | Exactly decoded 1 after surrounding whitespace trim |
| Missing/malformed fields | Block or evidence unavailable with original cause; never synthesize header success |
| Other schemas / multiple entries | Explicit UnsupportedSchema, including otherwise known AP203/AP242 identifiers, qualified aliases and duplicate schema entries |
| Other metadata | Preserve description/exporter metadata; description text such as AP214 does not establish schema or content compatibility |

H1: v1 accepts only implementation level '1'. Levels '2;1' and '3;1' are
explicitly blocked with the separate conceptual reason UnsupportedImplementationLevel,
not misreported as an unsupported schema. Repository fixture
GE03B01-QuarterConePole.step declares '2;1' and is intentionally outside this v1
header scope; its existing mapping tests remain valid and unchanged. This policy
does not imply that the fixture is malformed or that all repository files pass.

H2: the new captured-header producer must structurally identify HEADER records
outside quoted strings/comments, not use the first textual name match. Require
FILE_DESCRIPTION, FILE_NAME and FILE_SCHEMA each exactly once, in valid structure;
duplicate records are malformed even if identical. A name in a description string
must not create or substitute a header record. A missing, dollar or empty required
record/schema entry is malformed and must not be silently filtered from a list.
Empty optional author/organization strings remain optional metadata: this rule
does not invent a nonempty-author requirement. Schema strings containing STEP
escapes are unsupported; qualified schema forms and all other characters outside
[A-Z0-9_] are unsupported, never normalized into an accepted alias. Preserve
distinct malformed-record, unsupported-schema-form and unsupported-level reasons.
These are new producer requirements, not claims about existing StepHeaderReader;
its path API, first-match parsing, dollar filtering and lack of escape decoding
must not be silently altered under this isolated evidence slice.

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
fabricated: retain the missing-angle reason. U1: geometry requirements and evaluator
requirements jointly determine whether angle proof is mandatory. If target geometry
contains an angle-valued field (at least CONICAL_SURFACE.SemiAngle), validated
context angle units and normalization are mandatory before mapping/using that
field; no evaluator-only exemption. Existing cone mapping uses Math.Tan(SemiAngle)
assuming radians. A later no-angle exemption requires positive coverage proof that
the target has no angle-valued fields AND an evaluator contract needing no angles.
Unknown/incomplete coverage cannot establish that exemption. No exemption makes
the underlying incomplete per-context observation complete.
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

U2 adopted as an implementation requirement: store policy-bound exact scale
definitions as rational constants (metre 1/1, centimetre 1/100, millimetre 1/1000,
radian 1/1), not authoritative precomputed double values. Any later floating-point
conversion is derived at the normalization boundary and tested for finite result
and expected numerical error; it cannot revise the policy scale or enable inch/degree.
U3: contexts are emitted in deterministic ordinal SourceEntityId order; preserve
that order in evidence/tests, independent of dictionary enumeration. Missing or
duplicate context IDs produce explicit non-ready observations, no unstable tie-break
or overwrite. Ordering is not target selection or geometric precedence.

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
| Implementation levels '2;1' (including GE03B01) and '3;1' | UnsupportedImplementationLevel, distinct from schema rejection |
| Duplicate FILE_SCHEMA records, including identical records | Malformed header; never first-match success |
| FILE_SCHEMA text only inside description / text plus real record | Text is not a record; missing real record fails, one valid real record parses normally |
| Dollar or empty required schema entry, even alongside valid schema | Malformed; no silent filtering |
| Escaped schema identifier / qualified schema form | Explicit unsupported form; no decoding into accepted identifier |
| Cone in degree context with base radian reference | Blocked conversion unit before angle-dependent mapping; never feed degrees to radian formula |
| Cone with missing angle unit / evaluator says no angles | Non-ready; evaluator cannot waive geometric angle requirement |
| Positive no-angle coverage and evaluator requiring no angles | May waive angle requirement only; all other gates still apply |
| Exact scale definitions and shuffled context input order | Rational constants preserved; stable ordinal context evidence order |

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

## Senior section 24 — conditional acceptance and implementation gates

Owner supplied independent review of `022941a8a6bed48fa2c54b3b37c7b5bc1556f849`
on 2026-11-23 in this working conversation. Reviewer accepted both policies
conditionally on H1/H2/U1; all three are incorporated above. U2/U3 recommendations
are adopted as explicit requirements. No fabricated GitHub review or reviewer CI
verification is claimed. 25 acceptance rows now specify future fixtures, not runs.

Before implementation: #122 at b3b18a5 or later must merge green; amended #123 must
merge. Before implementation merge: N1 must close; new header producer must meet H2
and path/captured encoding parity without silently changing StepHeaderReader;
pin sources/codes/severities; no production afterSourceCaptured or consumer.
Profile header and units of workshop, Festo and R01-R03 inputs, reporting v1
blocking incidence separately from inventory diagnostics. Inputs not supplied or
not measurable are unavailable, never zero blocking; obtain missing Festo STEP
inputs before claiming that required profiling complete. Existing inventory
measurements do not satisfy this header/unit profile requirement.

Owner of deferred work: Mehmet İlkay Kınacı. Review level '2;1'/AP242 in v2 before
allowlist expansion; inch/degree validated support before conversion enablement;
GLOBAL_UNCERTAINTY before precision-dependent evaluation; multi-body design and
coverage manifest before relevant consumer readiness. M2 OPEN; N1 remains pending.
