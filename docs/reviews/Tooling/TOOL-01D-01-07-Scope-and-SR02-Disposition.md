# TOOL-01D-01..07 scope and SR02 disposition

Date: 2026-10-28 Europe/Istanbul.
Class: HARDEN / documentation only.
Verified baseline: `3ba5f9c034305e3427b3aaa54aab836f57f34a6e`.
Owner: repository owner / user. Record prepared by the implementation assistant.

## Purpose and authority

This is a present-day reconciliation of delivered scope and reported user
instructions, not a backdated approval. The user directed continuation of the
bounded source-link work and explicitly approved the #91..#93 and #95 merges in
the working conversation. That conversation is external to this repository;
this record is not independent proof of the original approval times.
PR/commit links below establish delivered code, not the identity or date of a
policy approver. No missing historical approval is inferred from a merge.

On 2026-10-28 the user supplied the SR02 review, requested a technical response,
accepted written risk triggers through the subsequent conversation, and directed
continuation of the correction work. That instruction permits preparing this
documentation PR, not silently ratifying every historical policy or merging it.

The discovery document alone never authorized implementation. Its original
NO-GO list is reconciled here only for the already delivered source-link slice.
This is not a new permission for general CAD/tooling integration.

## Delivered scope and separate UI branch

| Work | Evidence | Permitted result and limits |
| --- | --- | --- |
| 01D-01 source gate | [#89](https://github.com/dengwensha/MPC-Platform/pull/89) | Application checks exact document hash and source face; no Tooling reference |
| 01D-02 import composition | [#90](https://github.com/dengwensha/MPC-Platform/pull/90) | Same-snapshot STEP import and source gate; material-dependent approval blocked |
| 01D-03 wire DTO | [#91](https://github.com/dengwensha/MPC-Platform/pull/91), head 28d6a1ab60ce75385a75b42ec20e3adb13a3ce4c | Separate v1.1 source validation request/result; legacy v1.0 remains separate |
| 01D-04 Worker | [#92](https://github.com/dengwensha/MPC-Platform/pull/92), head 8da51781754838f0bd5987f64ef50fc195ded82f | Route source requests without legacy fallback; no physical or tooling approval |
| 01D-05 Host | [#93](https://github.com/dengwensha/MPC-Platform/pull/93), head de571245190d1d6f0fc6e7a72c6f132721ba6dd1 | Process invocation, response correlation and exit consistency |
| 01D-06 UI | [#94](https://github.com/dengwensha/MPC-Platform/pull/94), head 5f718bae4cf36ee1ae724b4c2d54a15730f628a0 | OPEN / NOT MERGED at this baseline; explicit STEP face entry only; independent diff review and physical acceptance pending |
| 01D-07 standalone acceptance | [#95](https://github.com/dengwensha/MPC-Platform/pull/95), head 2a44c4874790945a5c4149214ec3f42bd499ea5c | Five real Worker cases without SOLIDWORKS; no additional product capability |

There is no UI entry point for this operation on the verified main baseline.
#94 is not covered by the merged main range or by SR02's main-only code review.
Its controlled interaction tests do not establish live SOLIDWORKS menu acceptance.

## Remaining scope restrictions

No catalog importer, persistence, Application-to-Tooling reference, automatic
product selection, selected-SOLIDWORKS-face mapping, material-side approval,
machine qualification, toolpath, NC output or deployment is opened by this
record. Unknown geometry evidence remains blocking for dependent evaluations.
A source-link match is not physical suitability or supplier authentication.

The bounded Application source gate is not a complete CAD/tooling bridge.
The Application location direction does not authorize a new Tooling reference.
UI is confined to the already prepared, unmerged #94 scope; further UI scope
requires its own decision and trigger review.

## SR02 source and disposition

Source: user-supplied `MPC SR02 — TOOL-01C-04..07 ve TOOL-01D-01..07 Senior Review.docx`,
dated 2026-10-28; DOCX SHA-256
`7B6C1952F8A8CC0CFFB334EB5A2EA99C64BF0F742255FB4A7D05F069F37F26EB`.
The source is external to the repository. Its hash identifies bytes, not the
truth of all conclusions. SR02 is static review of f0281cc through 3ba5f9c;
the reviewer did not execute build/tests or independently verify CI.

| Finding | Disposition in this PR | Closure still required |
| --- | --- | --- |
| H1 scope/NO-GO conflict | Reconciliation and discovery links prepared | Owner acceptance of this record and historical trace limits; final PR CI |
| H2 policy record missing | [Policy record](TOOL-01C-03-Policy-Decision-Record.md) prepared; proposals and code facts separated | Outstanding policy ratification is explicit; final PR CI; no backdated G1 or pre-merge Senior claim |
| M1 v1.1 malformed types yield legacy error | OPEN / next isolated code correction | v1.1 rejection and correlation tests, full CI, trigger-based independent Senior review before merge |
| M2 MissingRegion aggregates causes | OPEN / diagnosis limitation | Decide/document current meaning versus production diagnostic separation before v1.1 freeze; do not equate it only with absent face |
| M3 FaceMismatch unreachable from Worker input | OPEN / documentation correction queued | State it is not emitted by current v1.1 Worker; preserve enum numbering |
| M4 production proof | OPEN | TL-03/TL-07 intake mapping and TL-05 durable history are not closed by #86/#87 |
| M5 vendor identity in synthetic tests | OPEN / isolated fixture cleanup queued | Synthetic identity values, preserved interface expectations and full CI |
| L1 raw/whitespace EAN equality | Documented in policy record | No product-merge identity claim; downstream identity policy remains open |
| L2 numeric wire/exit codes | OPEN | Check existing coverage and pin relevant values without new dependency layer |
| L3 Unknown pair coverage | OPEN | Add meaningful missing matrix cases when changing Tooling tests |
| L4 unexpected parser exceptions | OPEN risk | Evidence-led exception/diagnostic decision; do not mask programming failures with broad catch |
| L5 snapshot memory | DEFERRED risk | Real importer size/memory limits; no speculative streaming rewrite |

Policy acceptance, documentation repair, code correction and production finding
closure are separate. This PR does not declare all SR02 findings CLOSED.
The historical pre-merge review gap cannot be repaired by relabelling SR02 as
a review performed before those merges.

## Verification evidence and review rule

- Historical main CI #169: [40500846940](https://github.com/dengwensha/MPC-Platform/actions/runs/40500846940),
  commit 53e87d33dbd4fdcbeacee45d886d7994266352e9; successful, not evidence for later v1.1 code.
- Current baseline main CI #188: [40648818847](https://github.com/dengwensha/MPC-Platform/actions/runs/40648818847),
  commit 3ba5f9c034305e3427b3aaa54aab836f57f34a6e; Release 0 warnings/errors,
  1797/1797 full tests, 5/5 standalone cases, 3/3 targeted cases and corpus checks.
  This total excludes #94's 21 additional cases.
- These are repository CI results, not a claim the Senior independently ran them.
  Full-suite totals do not supply a separate production/specification taxonomy.
- The final documentation commit requires its own CI; baseline CI is not reused
  as validation of changed documentation.
- [Risk-based Senior review](../Risk-Based-Senior-Review.md) replaces a blanket
  every-PR requirement from the user decision dated 2026-10-28. It does not waive
  any currently triggered independent review.
