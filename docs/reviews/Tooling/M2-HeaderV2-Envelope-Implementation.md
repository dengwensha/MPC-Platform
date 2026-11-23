# M2 — consumer-free header v2 and captured envelope evidence

Date: 2026-11-23 Europe/Istanbul. Class: NEW BUILD / HARDEN — BUILD JUSTIFIED.
Start: 2a9385161dbb2a6a20f7d435e425bf102106353a.
Implementation: 3e80536939f81dc1a353e07d2060085e8bfbf002.
Branch: feature/M2-header-v2-envelope-evidence. Owner: Mehmet İlkay Kınacı.
State: implemented; independent final-diff review and exact-head Windows CI pending.
M2 OPEN. N1 bounded CLOSED. C1 accepted only within its pre-consumer boundary.

## One goal, justification and scope

Implement the accepted #128/Senior §30 header policy and D1/D2 as independent,
owner-bound observations of the same privately captured bytes, without a consumer
or combined readiness decision. Owner explicitly accepted envelope in the v2 slice.
V1 could not recognize the recorded Festo forms, and the existing inventory gate
could be clear after reading only the first of multiple DATA sections. It cannot
provide envelope proof. This missing capability justifies the narrow new producer.

HeaderSchemaAllowlist=2, ContextUnitPolicy=1, EnvelopePolicy=1. Recognize 1/2;1
and exactly bare AUTOMOTIVE_DESIGN or its seven-arc e3 qualifier. Regex validates
the reviewed ASCII grammar; OID arcs are compared as strings, not numeric values.
Preserve Schema and ImplementationLevel meanings; add RawSchema/RawImplementationLevel
literal text, RecognizedSchema and immutable SchemaOid strings. Unknown qualifiers
are not discarded. The 17 existing Error reasons and all unit logic remain unchanged.
FILE_POPULATION/SECTION_LANGUAGE/SECTION_CONTEXT still produce HeaderMalformed.
Header scanning still stops at HEADER ENDSEC; existing B1/BOM tests are retained.

A separate envelope producer visits structural statements through EOF, respects
strings/doubled quotes/comments/binary, and requires the reviewed single bare DATA,
section order and terminal marker. It rejects parameterized/multiple DATA, forbidden
sections, malformed lexical/closing structure and trailing structural content.
Only a 128-character statement prefix is buffered, not complete DATA/entity strings.
This is a code property, not a measured performance/allocation benchmark.
Five separate Error reasons and the whole producer source are pinned. No full
Part 21 semantic/conformance validation or entity-consumption alignment is claimed.

## Same capture and ownership

InventoryEvidence's private constructor gives header and envelope producers separate
non-writable streams over the SAME private byte array already hashed/read for inventory.
Neither producer reopens the source or creates a temporary STEP. Captured neutral
header/envelope data are private and getter-only; ImportSession exposes them only
after Owns(evidence). Bound HeaderUnitObservation/EnvelopeObservation constructors
are private; binders require evidence AND expected session and use guarded reads.
Stored bound views are private on the owning session. Observations have no owner,
inventory, stream or byte-array back-reference. Equal hashes do not grant ownership.

EnvelopeProducerRevision reuses the existing Application producer assembly revision
(the same assembly supplies both producers); its policy/source/reason manifest is
separate. No third assembly hash is recalculated per capture. N3/B6 remains open.
Neutral producer data remain constructible for tests but cannot mint bound evidence.
The method-parameter lock now includes envelope types. The two §29 expansions
(all Application namespaces and constructors) remain on the combined-gate PR.

## Verification

Before implementation: six focused cases, five failed/one passed. These were the
four level/schema combinations and two actual captures showing InventoryClear
with a missing envelope proof path; no hash-pin failure was counted as reproduction.
Local SDK 8.0.422/Linux .NET 8: Application 400/400; Release build 0 warnings/errors;
diff --check PASS. Windows Host/Worker full CI remains required at the final head.

Tests cover qualifier allowlist/ASCII/overflow-sized string arcs, original literal
preservation, extra header records, section order, multiple/parameterized DATA,
quoted/comment keywords, trailing/unterminated syntax, long entity payload, BOM and
invalid UTF-8 parity, expected IO propagation, capture replacement, ownership,
private construction and independent policy/reason/source manifests. The old v1-only
2;1 rejection row was removed; the quarter-cone fixture now recognizes header only.
Inventory policy A remains unchanged; an InventoryClear result is not eligibility.

## Exact committed-code profile comparison

[Validation and summaries](evidence/M2-HeaderV2-Envelope/Validation.json) include
15 unchanged input hashes, owner/capture hashes, SDK/build command, normalized
source pins, profiler source and both Application assembly hashes. Baseline was
built at clean committed Start; current profiles at committed Implementation above.
The subsequent evidence/document commit changes no runtime source. Profile DLL
hashes belong to those measured commits, not the later CI assembly build.

Inputs: R01/R02/R03, 11 workshop file versions and one unique unchanged Festo file.
Workshop versions are correlated (two families); four identical Festo ZIPs count
as one. Existing local-use/reporting permission records apply. No raw private CAD
or full serialized inventory geometry is committed. Full inventory and diagnostic
JSON are hashed in-process; only hashes/counts and header/unit/envelope metadata
are saved. Both measurements use actual CaptureFromFile and owned accessors.

15/15 inventory JSON hashes, diagnostic hashes/status/counts and context/unit
observations are identical. Original declared schema/level are unchanged. Only
Festo intentionally loses UnsupportedImplementationLevel/UnsupportedSchemaForm;
14/15 files have no header reasons, R03 remains UnsupportedSchema. All 15 have
one bare DATA and no envelope reasons. These are narrow observations, not 15
successful geometry/evaluation cases. BACK PLATE remains four bodies/five contexts;
no automatic target selection or repair is introduced.

## E1 — independently reproduced pre-existing DATA extraction alignment gap

The synthetic [control](evidence/M2-HeaderV2-Envelope/controls/C01-CommentData.step)
places a fake DATA/#99/ENDSEC sequence in a HEADER comment, followed by a real DATA
with two points (#1/#2). The unchanged reader's substring search consumes the
comment's point: BOTH baseline and implementation return one record, zero diagnostics,
InventoryClear and the same inventory hash. Baseline/current summaries are retained.
The new envelope correctly ignores the comment and observes one actual DATA section.
This shows why envelope shape is NOT proof that inventory consumed that section.
Header recognition plus clear inventory/envelope still cannot authorize evaluation.

This is not a new regression, not a passing acceptance control, and not hidden by
changing or weakening the envelope's lexical rule. Existing StepDataEntityReader
and shared parser/validator remain unchanged as required by this isolated scope.
Owner: Mehmet İlkay Kınacı. Priority: mandatory before combined-gate/first-consumer
merge or any readiness result. Next correction must align inventory extraction
with the actual structural DATA span, preserve legacy behavior deliberately, and
add a red/green behavioral test. It needs its own scoped independent review.
Senior must explicitly assess whether this consumer-free PR can merge with E1
tracked as a pre-consumer blocker; no self-approved deferral is claimed here.

## Remaining gates and requested Senior decisions

1. Does header v2 implement the accepted grammar, literal preservation and unchanged
   reason/unit semantics without permissive alias/qualifier fallback?
2. Does the separate envelope observation satisfy D1/D2, same-capture identity,
   revision/source pinning and C1 owner-only issuance/access?
3. Are the tests and exact-commit 14+1 profiles sufficient for this consumer-free
   slice, including the intentional Festo-only header delta?
4. Can this PR merge with E1 explicitly mandatory before any combined gate/consumer,
   or should inventory DATA alignment be resolved before this PR merges?

No combined gate, evaluator, target/root selection, geometry normalization, U1 cone
rule, AP203/AP242, conversion-unit support, wire/DTO/shared enum, Host/Worker/IPC or
production afterSourceCaptured use. No physical SOLIDWORKS acceptance claimed.
U1, B2, A4, N2, N3/B6, Windows repeat and §29 reflection expansions remain open.
M2 OPEN. Risk: trust/fail-closed YES; all other five review triggers NO. Independent
final-diff Senior acceptance required BEFORE merge; §30 policy approval alone does
not review this code. Reviewed: implementation plus pending metadata diff; Closing:
none. Exact final-head Windows CI pending.
