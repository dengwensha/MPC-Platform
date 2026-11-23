# M2 header boundary hardening — Senior section 26 B1/B3

Date: 2026-11-23 Europe/Istanbul. Class: HARDEN.
Start: `7c8f9952fe85a12b7249c5cf62df6f78bdebb87c` (unmerged #125).
Branch: fix/M2-header-boundary-hardening. State: implementation ready for review,
NOT merged or independently accepted. M2 OPEN. Owner: Mehmet İlkay Kınacı.

## One goal and proven failure

Separate HEADER parsing from DATA syntax and keep truncated complex component
bodies inside the controlled FormatException boundary. No schema/unit expansion,
2D-context decision, construction restriction or consumer is introduced.

Before fix: seven new boundary cases were run against the original producer;
six failed, one passed. Three malformed DATA tails were misattributed as
HeaderMalformed, two single-opening-parenthesis component bodies threw
ArgumentOutOfRangeException rather than FormatException, and an actual source
capture with a referenced truncated unit could not retain normal observation.
This is behavioral reproduction, not merely a changed hash-pin failure.

## Narrow implementation

B1: structural scanner now consumes a TextReader and returns immediately after
the first structural ENDSEC. ReadHeader still verifies ISO/header positioning,
all required records, schema and implementation policy. Quoted/doubled-quoted
strings, comments and binary literals still protect separators. It no longer
calls ReadToEnd or splits/copies DATA statements for header validation. Normal
StreamReader buffering can read ahead; no byte-offset/no-prefetch guarantee is
claimed. Shared parameter parsing and BOM/UTF8 fallback remain unchanged.
Data inventory parsing still runs independently and its errors remain visible;
a valid header is not a valid inventory or evaluation approval.

B3: before removing complex-body parentheses, require at least two characters
and a closing parenthesis. Malformed bodies become FormatException, which existing
context/unit catches convert to existing observation reasons. No broad catch or
inventory-error downgrade; no fake clean evidence.

Producer source pin updated after reviewing unchanged 17 reasons, Error severity,
header/unit policy revisions, SI definitions and source binding:
`7706678B27D55A59BA0BF684BEA65F19282D595A170C1DA558415F869FC701BA`.
InventoryEvidence and StepParameterParser source pins unchanged. The pin update
is reviewable code-change evidence, not permission to ignore source changes.

## Tests and dependency

Ten new tests: three malformed DATA tails, three truncated/invalid component
forms, real CaptureFromFile with referenced truncated record, a TextReader that
throws on any request after HEADER ENDSEC, and two malformed HEADER controls.
Focused final result 10/10 PASS. Existing BOM/golden/header/unit tests are retained.
Local Release Application regression 318/318, core1366/1366, Contracts94/94;
Application Release build 0 warnings/errors; diff --check PASS. Exact-head Windows
full CI including Host/Worker is required before merge and remains pending.
No performance benchmark, physical PASS or population input coverage is claimed.

This is a separate dependent branch, leaving #125's reviewed source hashes and
pending Festo profiling unchanged. PR targets main for the existing Windows CI;
until #125 merges its full cumulative diff includes #125. Review the incremental
B1/B3 change against Start above. Do not merge this PR ahead of #125 or treat its
green CI as completion of #125's Festo/permission/B2 conditions. If #125 changes,
refresh the dependency, inspect conflicts and rerun exact-head validation.

The #125 real-file profiles remain historical observations of their pinned
producer, not reruns of this modified producer. They are deliberately not
rewritten or claimed as new-build evidence. Profile refresh requirements must be
assessed at final delivery; Festo remains absent and must not become zero blockers.

Senior section 26 required B1/B3 before the first consumer, not as blockers for
the original consumer-free #125. This follow-up fixes them early but does not
claim their independent acceptance. C1 construction/ownership restriction,
U1 coverage/cone rule, B2 Festo classification, A4 and N3/B6 remain open.

Risk triggers: trust/fail-closed YES (producer parsing/cause attribution and source
pin); wire NO; shared enum values NO; IPC NO; persistence NO; UI NO. Independent
final-diff review required; original #125 acceptance does not cover this change.
Reviewed/Closing: pending final-diff review and delivery. No product consumer.

## Festo unchanged-input comparison, 2026-11-23

One unique unchanged Festo STEP (four byte-identical ZIP uploads) was profiled
on both #125 source 7c8f995 and this producer source fc6b9a7. The full HeaderUnits
observations match: UnsupportedImplementationLevel and UnsupportedSchemaForm;
one 3D context with supported millimetre/radian and no context reasons. B2 not
observed. See [profile and permission](M2-Festo-Header-Unit-Profile.md);
F01-Hardened.json records this producer Application assembly SHA256 separately.
This is not a refresh of the historical 14-file profiles. No policy widening,
source modification or new eligibility decision. Festo is now supplied; exact
final-head CI and independent B1/B3 review remain required before merge.

## Senior section 27 acceptance and dependency refresh

Senior independently accepted B1/B3 and the Festo conditions with no additional
blocking finding. #125 merged first as adbdf85fa156bee5d8f5a7244950ceef137fc510
after exact-head CI #268 succeeded. #126 incorporates this main commit without
conflicts or additional production/test changes. Its final exact-head CI must
succeed before merge; CI #269 alone does not cover this dependency refresh.

The historical 14-file profile was rerun on reviewed producer commit 74ae703,
SDK 8.0.422, Linux .NET 8, Release build 0 warnings/errors. All 14 input hashes
and all 14 complete HeaderUnits observations equal the original profiles.
R03 remains blocked by UnsupportedSchema; no policy was widened. Evidence and
assembly/source hashes: [refreshed profile](evidence/M2-HeaderUnit-Refreshed/Validation.json).
Historical profiles are preserved. This comparison does not establish geometry
eligibility or Windows acceptance. B1/B3 are accepted within this scope.

Before the first consumer: C1 construction/ownership restriction, U1 cone angle
evidence enforcement, and an explicit header-policy v2 decision for implementation
level 2;1 and qualified schema forms. Owner: MPC maintainer (Mehmet İlkay Kınacı).
B2 remains open (not observed in this single Festo sample); A4, N2, N3/B6 and
Windows repeat remain tracked. One supplier sample does not establish prevalence.
M2 OPEN. N1 CLOSED only within section 25's bounded Linux reader scope.
