# M2 C1 — header/unit evidence ownership

Date: 2026-11-23 Europe/Istanbul. Class: HARDEN.
Start: 998896dc79bab409031b85e75852980fd07b58c3.
Branch: fix/M2-header-unit-evidence-ownership. Owner: Mehmet İlkay Kınacı.
Senior section 28 reviewed bbb2c809b8639685102169797bd5bf0433157c54:
merge approved; C1 PARTIALLY accepted, with K1/K2 due before combined gate or first consumer.
Senior section 29 accepted K1/K2 and C1 fully within this consumer-free boundary
at 7dcbfb96dfb407482bd0b47440f563db5af8db43. #127 merged as
b6a80ef5719a670bc501fa0ad7736d83badd330b. Exact-head CI #274 and main CI #275
passed: 2048/2048, Worker 5/5, Release 0 warnings/errors. M2 OPEN; N1 bounded CLOSED.
This is a factual reconciliation of the acceptance supplied in the working
conversation; no further runtime or boundary behavior is changed here.

## One goal and boundary

Separate neutral parser data from the restricted bound observation, and require
exact-reference session ownership on every typed path to captured header/unit
content. Neutral HeaderUnitData/ContextUnitData/UnitDefinitionData and ExactUnitScale
remain constructible for parser/control tests. They cannot issue importer evidence,
bound observations or decisions.

InventoryEvidence's private constructor produces HeaderUnitData from the same
captured bytes and inventory. That getter is private. Only the nested ImportSession
can read it, through ReadHeaderUnitData(evidence), after Owns(evidence).

HeaderUnitObservation has a private constructor. Its nested EvidenceBinding.Bind
requires BOTH evidence and expected session, delegates to that ownership-checked
data access, and returns null for missing/unowned inputs. It accepts no caller
metadata, stream or inventory. An equal hash does not grant ownership. The session
creates its stored bound observation only after its Evidence has been assigned;
ReadHeaderUnits(evidence) also checks Owns before returning that view. The private
bound getter is on the session, not the evidence. Observations retain no owner,
inventory, source bytes or stream.

No combined decision gate or product consumer is introduced. Future combined-gate
APIs must accept evidence/session, never a standalone bound or neutral observation.
Null access is unavailable evidence, never clean evidence or eligibility. The
existing inventory policy/decision/status remains unchanged.

## Senior section 28 findings

K1: the previous one-argument Bind and internal neutral getter allowed a caller to
read another operation's real content without ownership. This was not fabricated
content, but could mix sessions. Both paths now require the expected owner. Tests
exercise every exposed route with owned, null, different-hash and same-hash foreign
inputs; a valid owner still preserves the blocked observation reasons.

K2: reflection now scans parameters as well as issuer return types on all declared
Inventory methods (public/nonpublic, instance/static). HeaderUnitObservation and
HeaderUnitData parameters are forbidden, including generic/array/by-ref wrappers.
Constructors are deliberately checked separately: the bound constructor must be
private; neutral-data constructors do not issue evidence. This preserves the private
binding constructor without allowing an independently supplied observation API.

## Acceptance and scope

Private bound issuer, private captured neutral getter, ownership-checked issuance
and reads, immutable output, no owner back-reference; source replacement after
capture preserves observed data. Reflection locks typed paths and issuer signatures.
The future consumer's use of this boundary must still be checked in its own PR.
No header v2, target/coverage decision, cone rule, normalization, CAD dependency,
wire/enum/IPC/Host change or physical acceptance claim. U1 cone rule, header-policy
v2 decision, B2/A4/N2/N3-B6 and Windows repeat remain open.

Risk: trust/fail-closed YES; wire/shared enum/IPC/persistence/UI NO. Section 28's
review covered bbb2c809; the K1/K2 incremental final diff received independent §29 acceptance. C1 acceptance
is confined to the consumer-free boundary. Before the combined gate merges, extend
parameter scanning to all MPC.Application types and to constructors (except only
the bound observation's own private constructor), and verify actual owned access.
Owner: Mehmet İlkay Kınacı; trigger: combined-gate PR before merge.

## Verification and trace

The original three boundary tests failed on Start, then eight C1 tests and
Application 326/326 passed at bbb2c809. Exact-head Windows CI #273 succeeded there:
2043/2043, Worker 5/5, Release 0 warnings/errors. These are historical results, not
validation of this incremental revision.

The [15-file comparison](evidence/M2-C1-Ownership/Comparison.json) likewise belongs
to the original pending-diff producer pinned there, reviewed in section 28. It is
preserved, not relabelled as a rerun of these new source pins. The historical
reflection-only profiler used the then-private evidence getter; current consumers
and future measurements must use the owning session accessor.

Current incremental local SDK 8.0.422/Linux .NET 8: Release Application build
0 warnings/errors; Application 331/331, including 13 C1 tests (five additional
section 28 tests). Before the fix, the two new K1 structural checks failed and
the new K2 parameter lock passed: 2 failed/1 passed as expected. The five new
tests cover the private neutral getter, two-argument binder, independent-observation
parameter prohibition, cross-session neutral/bound reads and missing-owner inputs.
The existing tests were adjusted to the owner-required issuer; no bypass is whitelisted.

Current source pins:
- HeaderUnitObservation.cs: CBC136F2215BF2C5C1D88957C7305C554F511B9A6F512E8BB44625B5577FE1CB
- InventoryEvidence.cs: 27897C8DD22F417D3D7A0DD25A4669C2437AA02CDE6D0454A0DFC121753ECD45
- StepParameterParser.cs unchanged: 69AA2488ECBAF60C869F83EE8DE3797B3DFA04ACBC26CB0E216CBAB27CD4AF97
No parsing branch, reason code/severity, schema/unit revision or SI constant changed.
Diff --check PASS. Exact-head Windows CI #274 and merge/main CI #275 passed (2048/2048, Worker 5/5,
0 warnings/errors). Reviewed: 7dcbfb96dfb407482bd0b47440f563db5af8db43 (Senior §29).
Closing: b6a80ef5719a670bc501fa0ad7736d83badd330b. Branch at delivery:
fix/M2-header-unit-evidence-ownership. Date: 2026-11-23 Europe/Istanbul.
Authoritative delivery and remaining follow-ups: [PR #127](https://github.com/dengwensha/MPC-Platform/pull/127).
