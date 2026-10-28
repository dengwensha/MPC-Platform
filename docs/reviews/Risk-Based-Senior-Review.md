# Risk based independent Senior review

Effective decision: 2026-10-28 Europe/Istanbul, supplied by the user in the
working conversation and accepted for continuation. This is a current rule,
not a claim it was applied before historical merges.

## Required triggers

Every PR records Yes/No with a short rationale for each trigger against its
actual diff, including semantic specification changes implemented indirectly.

| Trigger | Examples |
| --- | --- |
| Wire contract or serialization | DTO shape, response envelope, JSON type/options, version/error representation |
| Enum numeric values | Addition, reassignment, removal or changed numeric interpretation on a shared boundary |
| Trust boundary or fail-closed rules | Evidence sufficiency, rejection/fallback, validation/correlation, approval scope |
| Interprocess communication | Routing, launch/timeout, transport, exit/result interpretation |
| Persistence | Durable storage, schema, revision/history, transaction or retention behavior |
| UI entry into production code | New/changed callbacks or user flow invoking production services/processes |

If any trigger is Yes, an independent Senior review of the relevant final diff
is required BEFORE merge. The implementation author's self-review and green CI
do not substitute for independence. Record reviewer, reviewed commit, findings
and disposition in the PR. If the reviewed content changes, assess the new diff
and renew review for affected triggered scope; do not silently carry stale sign-off.

If all triggers are No, standard code review, relevant build/tests and required
CI suffice. Do not create an independent Senior ceremony for every PR.
Documentation-only status is not an exemption when it changes boundary meaning;
a purely factual reconciliation with no new boundary behavior may record No.

If trigger applicability is uncertain, inspect the diff and record the concrete
reason; unresolved material uncertainty is not an automatic No.
Review scope does not grant merge/deploy authorization or replace physical
acceptance. Existing blockers remain blockers.

## Current applications

- SR-M1: Yes for wire error representation, trust boundary and process response;
  independent review required before its future correction merges.
- #94: Yes for UI entry and process invocation; main-only SR02 is not review
  evidence for this unmerged menu diff; physical SOLIDWORKS acceptance remains pending.
- SR02 record reconciliation: No runtime/wire/enum/persistence/UI behavior change;
  it records existing delivered limits and this owner-directed process rule.
  This does not grant new product scope or close unproven historical approvals.

The [PR template](../../.github/pull_request_template.md) makes this assessment
visible. It is a required recorded process check, not an automated GitHub
branch-protection rule. No automated enforcement or reviewer assignment is claimed.
