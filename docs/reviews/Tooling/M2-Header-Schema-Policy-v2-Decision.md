# M2 — HeaderSchemaAllowlist 2 decision proposal

Date: 2026-11-23 Europe/Istanbul. Class: VERIFY / semantic policy design.
Start: b6a80ef5719a670bc501fa0ad7736d83badd330b.
Branch: docs/M2-header-policy-v2-decision. Owner: Mehmet İlkay Kınacı.
State: Senior section 30 conditionally accepts the policy at 185155e4, subject
to D1/D2 below. Amendments incorporated; owner accepted D1/D2 and envelope-in-v2 grouping
in the working conversation on 2026-11-23. No runtime change.
Runtime still emits HeaderSchemaAllowlist=1 and ContextUnitPolicy=1. M2 OPEN.
N1 bounded CLOSED; C1 accepted within its consumer-free boundary (Senior §29/#127).

## Problem and smallest decision

One unique unchanged Festo file is blocked by v1 solely on its header forms:
implementation level 2;1 and AUTOMOTIVE_DESIGN {1 0 10303 214 3 1 1}. Its context
unit observations have no reasons. Four identical uploaded archives are ONE
sample, not evidence of supplier-wide prevalence. The repository GE03B01 fixture
also declares 2;1, with bare AUTOMOTIVE_DESIGN; it is a surface fixture, not a
closed-solid acceptance case. R03 declares CONFIG_CONTROL_DESIGN and remains
outside this proposed schema allowlist.

Proposal: recognize ONLY two implementation-level strings and two schema forms
below, while preserving strict structural header checks and all independent
inventory, target, unit/normalization and positive coverage gates. Do not rewrite
inputs or pretend v1 accepted these forms. No AP203/AP242 or conversion-unit
support is bundled into this change.

## Technical basis and limits of inference

Primary implementer documentation checked 2026-11-23:

1. [ISO 10303-21 third-edition final text, clause 8.2.2](https://steptools.com/stds/step/IS_final_p21e3.html):
   2;1 denotes edition-1 conformance class 1, not edition 2. Its encoding is
   restricted to a single, unparameterized DATA section and excludes the additional
   FILE_POPULATION, SECTION_LANGUAGE and SECTION_CONTEXT header records. The
   exchange-encoding declaration and the AP214 schema edition are separate notions.
2. STEP Tools, [AP214 usage notes](https://downloads.steptools.com/docs/stp_aim/notes_ap214.html):
   lists {1 0 10303 214 3 1 1} as the AP214 third-edition ASN.1 identifier.
3. STEP Tools, [schema-name writer](https://www.steptools.com/docs/stp_aim/stplib.html):
   gives the complete qualified AUTOMOTIVE_DESIGN third-edition string. That
   library ignores the qualifier when resolving a schema; MPC must NOT copy that
   permissive behavior. The full qualifier must match the reviewed allowlist.

These sources establish a basis for a narrow header-recognition proposal. They
are not an ISO conformance audit, validation of MPC's entire Part 21 parser, proof
of geometry support, or authority to accept other OIDs/editions. Existing local
fixtures and the recorded Festo header corroborate these two concrete forms.

## Proposed exact policy

Reserved HeaderSchemaAllowlist revision: 2. ContextUnitPolicy stays 1; inventory
policy, root definition and future coverage manifests keep separate revisions.
V2 must be emitted only after reviewed implementation; unknown/mismatched policy
revisions cannot make a combined decision ready.

| Item | Allowed in proposed v2 | Remains blocked |
| --- | --- | --- |
| Implementation level | 1 or 2;1 after the existing surrounding-whitespace trim | 3;1, 3;2, 4;1, 2;2, 2, internal spacing or other strings; no prefix matching |
| Bare schema | Exactly AUTOMOTIVE_DESIGN, ordinal ASCII | AP203/CONFIG_CONTROL_DESIGN, AP242, lowercase, legacy aliases and unknown names |
| Qualified schema | AUTOMOTIVE_DESIGN plus exactly OID arcs 1 0 10303 214 3 1 1 | AP214 e1/e2 qualifiers, other OIDs, prefixes, missing/extra arcs, unknown qualifiers |
| Schema count | One nonempty entry | Multiple entries, even two identical values; dollar/empty entries never filtered |
| Header structure | FILE_DESCRIPTION, FILE_NAME and FILE_SCHEMA exactly once | Missing, duplicated or structurally malformed required records |
| Escaped schema content | No STEP escapes | No escape-decoding into an allowed identifier |

The four combinations of the two allowed levels and two allowed schema forms are
proposed for header recognition; neither schema qualification nor level implies
geometry compatibility. V1's optional metadata handling and implementation-level
trim remain unchanged. No broader normalization is introduced for schema strings.

For qualified strings, define a narrow ASCII lexical grammar: a nonempty NAME
using [A-Z0-9_], one or more U+0020 spaces, opening brace, optional U+0020 spaces,
exactly seven unsigned canonical decimal arcs separated by one or more U+0020
spaces, optional U+0020 spaces, closing brace, then end of string. Lexically valid
NAME/OID pairs are recognized ONLY if NAME is exactly AUTOMOTIVE_DESIGN and the
arc vector matches 1/0/10303/214/3/1/1. Compare the canonical decimal arc tokens
as ordinal strings; do not parse them into bounded numeric types or discard
qualifiers on overflow. Other lexically valid pairs are unlisted, not aliases. No leading/trailing spaces outside this grammar,
leading zeroes in multi-digit arcs, signs, dots, commas, symbolic labels, tabs,
line breaks, non-ASCII whitespace or suffix text. Both observed forms
AUTOMOTIVE_DESIGN {1 0 10303 214 3 1 1} and
AUTOMOTIVE_DESIGN { 1 0 10303 214 3 1 1 } are recognized.
This intentionally narrow grammar is a proposed MPC limit, not a claim that other
representations are invalid STEP. It requires explicit Senior acceptance.

Preserve the original declared schema string in evidence, including its original
spacing. Store recognized canonical schema and optional OID arcs separately; never
replace the declared string with a bare alias or drop a mismatched qualifier.
Retain declared implementation-level text alongside the recognized value if the
implementation adds a normalized field. This is internal captured evidence only;
no wire/shared enum or serialization/persistence contract is proposed here.

## Reasons and fail-closed invariants

- Structural required-record failures, dollar/empty required schema entries:
  HeaderMalformed, unchanged.
- Malformed/escaped/non-ASCII schema form: UnsupportedSchemaForm.
- Well-formed but unlisted schema name or qualifier: UnsupportedSchema. A valid
  but different OID is never ignored or classified as its bare identifier.
- Unlisted level: UnsupportedImplementationLevel, independently retained.
- Multiple schema entries: UnsupportedSchema; malformed entries still retain
  HeaderMalformed. Test reason combinations and ordinal deterministic ordering.
- No new reason code or severity downgrade is intended. The same 17 Error reasons
  remain pinned; if implementation needs another reason, revise/review this decision
  explicitly rather than silently extending it.

The header scan must still stop at the first real HEADER ENDSEC (B1). It must not
scan/split/copy DATA to decide a header field or misattribute DATA errors to HeaderMalformed.
Accepting a declared 2;1 header does not validate the rest of the exchange envelope.

### D1 — separate captured envelope observation (Senior section 30)

The existing StepDataEntityReader.ExtractDataSection finds the first DATA and its
following ENDSEC and returns only that substring. Later DATA sections are not
inventoried. Consequently, inventory-clear evidence alone cannot prove whole-file
coverage. This pre-existing v1 limitation is not created by v2; do not trust the
file's own declared level as proof of compliance. No general broken-file safety
or full Part 21 conformance is claimed here.

Require a separate immutable envelope observation from the SAME private captured
bytes as hash/header/inventory. It belongs to neither the bounded header scanner
nor the geometry coverage manifest. Bind it to the encompassing importer evidence
and read it only through the C1 exact-owner session path. Give its policy/producer
revision independent identity; no caller-created clean envelope metadata may issue
bound evidence. Envelope failure must remain visible even if inventory is clear.

The accepted narrow envelope shape must establish all of these structurally:
- Exactly one unparameterized DATA section (DATA; not DATA(...);).
- Its closing ENDSEC is followed only by END-ISO-10303-21; and end of input,
  allowing only ordinary lexical separators such as whitespace/comments.
- No ANCHOR, REFERENCE or SIGNATURE section, no second DATA and no unrecognized
  section or trailing structural content. Header/section opening/closing order
  and the exchange terminator must be complete, not inferred from substring hits.
- Keywords inside quoted strings, binary literals and comments do not count as
  structural sections or terminators. Reuse tested lexical behavior where sound;
  do not make header scanning consume or copy DATA (B1 remains mandatory).

This is a narrow envelope-scope observation, not validation of entity semantics or
all Part 21 encoding constraints. An unavailable/unknown/wrong-owner/revision or
rejected envelope cannot support a ready combined decision. Unit/geometry gates
remain independent; envelope success cannot override their failures.

Timing: REQUIRED before the combined gate/first consumer. Implementation proposal:
include this separate producer in the consumer-free v2 slice, while keeping the
header scan bounded. Owner accepted this delivery grouping on 2026-11-23. If
split out for implementation reasons, it must arrive in a separately reviewed
slice BEFORE the combined gate can merge/issue readiness. Owner: Mehmet İlkay
Kınacı. This timing requirement cannot be deferred beyond the first consumer.

### D2 — intentional strict three-record header limit

FILE_POPULATION, SECTION_LANGUAGE and SECTION_CONTEXT deliberately remain rejected
as HeaderMalformed by the existing exactly-three-required-records rule. Do not
silently accept, ignore or relabel them under v2. The same strict rule also rejects
other extra header records. Header recognition and envelope observation are
separate: a header can be recognized while the file envelope is rejected.

Inventory policy A remains mandatory. A recognized header cannot override an Error,
Fatal, unknown diagnostic severity, missing evidence, ownership mismatch, wrong
hash/revision, unsupported target/context or absent geometry coverage. Unit policy
1 still blocks inch/degree. U1 cone-angle evidence/normalization is mandatory at
the angle-dependent geometry boundary. No header observation is ReadyForDeclaredEvaluation.

## Implementation acceptance matrix (NOT executed results)

| Fixture/control | Expected under proposed v2 |
| --- | --- |
| Existing five golden headers | Remain recognized; inventory/unit observations unchanged |
| Bare/qualified allowed schema crossed with 1/2;1 | Four recognized header combinations |
| Festo exact qualified string and spaced-brace variant | Header recognized; original declared string retained |
| Same schema name, e1/e2 or unknown well-formed OID | UnsupportedSchema; never discard qualifier |
| Missing/extra arc, leading-zero arc, punctuation, tab/NBSP, suffix | UnsupportedSchemaForm; no acceptance |
| AP203/R03, AP242, unknown bare name | UnsupportedSchema, unchanged |
| 3;1, 3;2, 4;1, 2;2, 2 or 2 ;1 | UnsupportedImplementationLevel |
| Duplicate FILE_SCHEMA, names only in strings/comments, missing record | Existing structural rejection preserved |
| Dollar/empty/multiple schema entries | Existing reason rules preserved; no filtering |
| Escaped allowed-looking name or digit | UnsupportedSchemaForm |
| Malformed DATA after a valid HEADER | Header cause unaffected; inventory/coverage causes retained independently |
| Level 2;1 with two DATA sections | Header recognized; separate envelope rejected; combined decision cannot be ready |
| Level 2;1 with DATA(parameters) | Header recognized; separate envelope rejected; no second-section or first-section shortcut |
| Additional FILE_POPULATION / SECTION_LANGUAGE / SECTION_CONTEXT record | HeaderMalformed intentionally retained |
| Real ANCHOR / REFERENCE / SIGNATURE section, trailing statement or missing terminator | Envelope rejected/unavailable; no ready decision |
| Section keywords only inside strings/comments/binary | Not structural section counts; no spurious extra-section failure |
| Captured source replaced/deleted | Declared/canonical fields derive only from original captured bytes |
| Same-hash and different-hash foreign session evidence | No header data or bound observation exposed; C1 remains enforced |
| Wrong policy/producer revision or missing observation | Non-ready/unavailable; no fallback to v1 or bare-schema inference |
| GE03B01 2;1 surface fixture | Header recognized only; no single-solid/target eligibility implied |
| Original Festo vs prior pinned profile | Two header-form reasons intentionally removed only; context/unit/inventory observations and input hash unchanged |

Fixture classification is exact: valid grammar with an unknown NAME or arc vector
is UnsupportedSchema; invalid grammar is UnsupportedSchemaForm. These are proposed tests, not current v1 test results.
The Festo input is the recorded unchanged single example with SHA-256
A1B72CD5A3035C2F8DD8A9FCDED1B32407F1DCE38A308D109E8CA10FD359CD61;
local-use permission/reporting limits are in [the profile record](M2-Festo-Header-Unit-Profile.md).
No raw supplier/workshop CAD is added to GitHub.

## Delivery sequence and requested decisions

1. Review/accept or amend this policy-only diff. No runtime policy is enabled by
   merging the decision document. Explicit owner and independent Senior acceptance
   are required before implementing the new allowlist behavior.
2. Separate consumer-free Application implementation: same private capture and C1
   ownership path; preserve declared fields; version 2 only for the header policy;
   update producer/source/reason pins and all relevant fixtures, BOM/encoding tests
   and exact-head Release/full CI. Leave legacy readers/unit resolver unchanged.
3. Rerun the 14-file profile plus the single unique Festo sample on that pinned
   implementation and record the intentional header-only deltas. Local permission
   and original bytes remain unchanged; unavailable readings are not zero reasons.
4. Deliver D1 as a separate same-capture/owner-bound envelope observation, proposed
   within the consumer-free v2 implementation. It is mandatory before combined-gate
   merge/readiness if separately scheduled. Target/root definition, U1 cone enforcement,
   normalization and evaluator coverage remain separate. Apply the
   two §29 reflection expansions (all Application namespaces and constructors with
   only the bound observation's own private constructor excluded). Verify the
   actual consumer reads only owned evidence; C1 acceptance is not consumer approval.

Senior decisions requested separately:
- Accept the two implementation levels and exact AP214 e3 qualified form, with the
  narrow ASCII spacing/arc grammar and preserved declared metadata?
- Accept unchanged fail-closed/reason semantics and the envelope-proof obligation
  before eligibility, with no implied AP203/AP242/unit/geometry support?
- Authorize the isolated v2 implementation after owner/policy acceptance, with the
  fixture/profile/revision/CI gates above, including D1 in that slice?

Risk triggers: trust/fail-closed YES (semantic acceptance expansion, despite docs
only); wire/shared enum/IPC/persistence/UI NO. Independent final-diff Senior review
required before merge. Start as above; Reviewed: pending diff at Start; Closing:
none. Validation: source/profile/document cross-check and git diff --check; no new
runtime tests claimed. Unchanged runtime baseline main CI #275 at Start: Release
0 warnings/errors, full 2048/2048, Worker 5/5. Exact PR CI pending.


## Senior section 30 disposition and deferred item

Review supplied by owner via the attached working-conversation text on 2026-11-23.
Reviewed: 185155e4d232ceca8ba1a1bc04ebe5a165b44785. Policy/grammar accepted;
merge conditional on D1/D2 documentation, incorporated above. Independent reviewer
did not verify CI #276; the implementation session verified that exact-head run
successful (2048/2048, Worker 5/5, 0 warnings/errors). This is not the amended-head
CI result. No GitHub review or owner acceptance is fabricated.

D1/D2 are the Senior-required disposition. Owner explicitly accepted the amended policy and including the envelope producer
in the v2 implementation in the working conversation on 2026-11-23 (answer:
"Kabul; zarf v2 diliminde"). Senior section 30 conditions D1/D2 are incorporated.
The conditional policy review is satisfied; final-head CI and merge remain required
before the isolated consumer-free v2/envelope implementation. No runtime readiness
is claimed by this document.

Deferred v3 candidate: consider explicit AP214 e1/e2 qualified names when concrete
samples arrive. Bare AUTOMOTIVE_DESIGN remains accepted without an edition, while
v2 intentionally accepts only the sampled explicit e3 qualifier. This is a narrow
MPC policy limit, not a claim that AP214 editions use different EXPRESS schemas.
Owner: Mehmet İlkay Kınacı; trigger: first genuine e1/e2 qualified input, followed
by fixtures and explicit reviewed allowlist revision. No silent expansion.

Current amended delivery: documentation only; source/tests unchanged from Start.
Diff --check required; exact amended-head CI pending. M2 OPEN. §29 reflection
expansions remain assigned to the combined-gate PR, not silently closed here.
