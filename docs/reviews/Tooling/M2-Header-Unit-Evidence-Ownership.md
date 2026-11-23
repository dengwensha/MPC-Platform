# M2 C1 — header/unit evidence ownership

Date: 2026-11-23 Europe/Istanbul. Class: HARDEN.
Start: 998896dc79bab409031b85e75852980fd07b58c3.
Branch: fix/M2-header-unit-evidence-ownership. Owner: Mehmet İlkay Kınacı.
State: implementation/review pending. M2 OPEN; N1 bounded CLOSED.

## One goal

Senior sections 26/27 require restricted observation construction and an ownership
check before the future combined gate reads header/unit evidence. Existing
HeaderUnitProducer.ReadCaptured accepts an arbitrary stream/inventory pair and
returns the same observation type as InventoryEvidence. Its neutral parser output
must not be mistaken for an importer-bound observation.

Separate neutral parser data from the restricted bound observation. Only a real
InventoryEvidence instance can be supplied to the observation binder; caller
metadata, a stream or an inventory cannot issue the bound type. The binder reads
immutable data already produced in that evidence's private constructor from the
same captured bytes and inventory. A session exposes its bound observation only
when it owns the exact evidence instance, including when two sessions have the
same document hash. No evidence/session back-reference is added to the observation.

This introduces no combined decision gate or product consumer. The future combined
gate must use the session's ownership-checked accessor, never a neutral producer
result. Null/unowned access is unavailable evidence, never a clean observation.
The current inventory decision/status and its ownership checks remain unchanged.

## Scope and acceptance

- Private bound-observation constructor; no metadata-based bound factory.
- Parser results explicitly named neutral data, independently testable.
- Bound observation unavailable through InventoryEvidence's ordinary typed API;
  owning session returns it, other/null evidence returns no observation.
- Reflection locks constructors, issuer signatures, getter visibility, immutability
  and absence of a producer method returning bound observations.
- Actual captures cover same-hash/different-session rejection, blocked-source
  isolation and source replacement after capture.
- Same parsing policy, 17 Error reasons, policy revisions, source decoding and
  existing inventory behavior. Source pins are reviewed and updated explicitly.
- Release build/full CI, clean diff/git and independent final-diff review required.

No header v2, context/target/coverage decision, cone rule, Windows integration,
wire/enum/IPC change, normalization, CAD dependency or physical acceptance claim.
C1 is pending Senior acceptance, not closed by implementation alone. Before first
consumer: U1 cone angle rule and header-policy v2 decision remain mandatory.
B2/A4/N2/N3-B6 and Windows repeat remain open.

Risk: trust/fail-closed YES; wire/shared enum/IPC/persistence/UI NO. This PR must
receive independent review before merge. Existing #125/#126 approval does not
cover this changed evidence boundary.

## Verification and trace

Reviewed: pending diff at Start. Closing: none; independent review pending.
Local SDK 8.0.422/Linux .NET 8: Release Application build 0 warnings/errors;
Application 326/326 (eight new C1 tests), core 1366/1366. In an isolated worktree
at Start, the first three structural boundary tests fail 3/3 as expected. No
runtime reproduction beyond this structural/ownership boundary is claimed.

All 14 real-file profiles plus the single unique Festo example were compared
with the prior pinned profiles: all input hashes and complete HeaderUnits
observations match, 15/15. Local supplemental assembly/source hashes and the
explicit pending-diff build identity are in [Comparison.json](evidence/M2-C1-Ownership/Comparison.json).
The existing reflection-only measurement harness can inspect the private bound
property; it is not a production consumer or an alternative typed issuance path.
The exact delivery commit must receive Windows full CI separately.

The neutral graph is explicitly HeaderUnitData/ContextUnitData/UnitDefinitionData
and ExactUnitScale. Its constructors remain available for parser/control tests;
it cannot issue a bound HeaderUnitObservation. EvidenceBinding.Bind accepts only
real InventoryEvidence, reads its getter-only neutral result and retains no owner
reference. It can make an equivalent view of real evidence, but cannot accept a
caller-created clean graph. Only ImportSession.ReadHeaderUnits exposes the stored
bound view through the typed evidence API after exact-reference ownership.
The future combined gate must take evidence/session, not a supplied observation.
No combined gate is delivered, so review must assess this as a pre-consumer C1
boundary, not end-to-end consumer acceptance.

Source pins were updated only after inspecting the neutral/bound split and
ownership accessor. Policy revisions, the 17 codes/Error severity, SI scales,
reader and shared parameter parser are unchanged. Diff --check PASS.
Windows full CI and independent final-diff Senior acceptance remain pending.
