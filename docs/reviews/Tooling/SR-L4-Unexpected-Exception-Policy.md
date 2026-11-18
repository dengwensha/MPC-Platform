# SR-L4 unexpected exception policy and callback evidence

Date: 2026-11-18 Europe/Istanbul. Class: HARDEN / policy disposition.
Start: `9cfca948fbd98119c9c84df00f5cbb4f4f74d395`.

## Decision proposed for independent acceptance

Known malformed-input causes are translated only at evidenced narrow boundaries.
Unexpected Worker exceptions are not silently converted to ImportFailed, retried
with weaker requirements or downgraded to legacy import. A process may terminate
without valid result JSON. The Host must continue to reject absent, malformed,
uncorrelated or exit-inconsistent replies and record available process evidence.
A diagnostic record is not a validated operation result and never grants approval.

This retains the existing Worker policy rather than adding a catch-all runtime
handler. New evidence of a specific input-caused escape gets a scoped regression
and classification review, like #107; programming/configuration invariant guards
remain intact. The 76 explicit STEP throw-site classification is bounded static
evidence, not a universal containment claim.

| Boundary / event | Required/current behavior |
| --- | --- |
| Structural invalid request | Reject before source access; no fallback |
| Known caught v1.1 processing failures | Existing ImportFailed (7), exit 3 and request correlation; Worker diagnostic best effort |
| Resolver-constructor duplicate semantic IDs | #107's narrow InvalidDataException translation with original cause |
| Duplicate solid roots | Existing aggregate MissingRegion before resolver construction; differs from other duplicate IDs and remains documented under M2 |
| Unexpected Worker exception / missing result | No invented result or input-error classification; Host ResultReadFailed and observed exit evidence when available |
| Unexpected source-validation callback exception | Existing generic Turkish failure/COM containment; now local original exception evidence |
| Expected pre-prompt read/hash errors | Existing workflow error message/filter retained; this change does not claim all expected UI errors are logged |
| Optional diagnostic/cleanup failure | Suppress inside diagnostic boundary; never replace operation result |
| Forced kill, CLR/native/resource-fatal failure | No reliable logging or UI-containment guarantee; no successful operation may be inferred |

## Callback implementation

OnValidateStepSource delegates its existing general try/catch and guarded error
display to RegionSourceValidationCallback. The menu/interaction/command work is
the action. When the workflow constructs a request, the callback tracks that
original RequestId immediately before executing the command. Before that point
RequestId is null. Each Run resets context; no generated or stale ID is used.

HostFailureLog adds local Boundary: HostCommand or SourceValidationCallback.
For callback exceptions HostStatus, WorkerExitCode and received reply fields
are null; no synthetic TransportFailed or wire outcome is invented.
The original exception, inner causes, UTC and available request identity go to
the same bounded host-last-failure.json. Error-display failure is still
contained and does not overwrite the original exception evidence.
Existing generic message text, Host command status/exit validation and
transport cleanup are unchanged.

No v1.0 callback/lifecycle expansion, new DTO/enum, Worker handler, source retry,
exception message matching or CAD-independent layer dependency is introduced.
Current local confidentiality, size, staging and stale-record rules still apply.
#106 exact-extension and cleanup-branch test follow-ups are outside this scope.

## Verification and closure boundary

Ten new cases cover before/after request correlation, logger-initialization/logging/display failure
containment, original evidence retention, normal return, context reset, actual
workflow-to-callback unexpected failure, and absent replies at CLR-style/other
unexpected exit codes. Last two tests are controlled process observations, not
a claim of reproducing every real Worker crash or native COM failure.
Release/full net48 Host, regression, corpus CI and commit trace are in the PR.
No new physical SOLIDWORKS PASS is claimed.

This implementation and policy are submitted for independent Senior acceptance,
not self-declared overall closure. A bounded SR-L4 closure decision must explicitly
accept the retained process-termination policy and its limits. M2 detailed cause
separation, future defects and non-blocking follow-ups are not thereby closed.
The previously recommended missing-Worker physical smoke check remains pending
for the next physical session unless the reviewer requires it as a gate.
