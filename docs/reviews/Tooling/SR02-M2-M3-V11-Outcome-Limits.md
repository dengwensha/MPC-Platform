# SR02 M2 and M3 v1.1 outcome limits

Date: 2026-10-28 Europe/Istanbul.
Class: HARDEN / documentation only.
Current delivery status reconciled at main `857808225aa9a27e98c51e9a4839d8227a5fddee`.
The code inspection baseline and historical review sequence below are retained.
Verified code baseline: `3ba5f9c034305e3427b3aaa54aab836f57f34a6e`.
This records existing behavior; it does not change DTOs, enum numbers,
validators, exit handling, UI or runtime classification.

The wire meaning clarification required independent Senior review. Section 7
conditionally approved a48f0be8; section 9 explicitly approved the corrected
head 5c8926b4. Its own CI and post-merge main CI succeeded; #99 is merged.
The historical sequence below is retained. No v1.1 freeze or release approval
is inferred from documentation acceptance.

## M2 MissingRegion is not an absent-face diagnosis

For the current Worker, MissingRegion (numeric 2, rejection exit 2) means the
requested supported ManufacturingRegion could not be constructed. It is not
proof that only the source face ID is wrong, that the whole file was parsed
successfully, or that the expected hash was checked.

The verified production path is:
- ManufacturingRegionImportService reads original bytes once, hashes that private
  snapshot, and reads/maps a temporary copy.
- It returns false for absent/failed inventory, a root count other than exactly
  one manifold solid, unsuccessful/null mapping, or failure to create the
  requested mapped planar face.
- ManufacturingRegion.TryCreate returns false when the requested face cannot be
  resolved or is not a Plane3D (and for invalid internal identity inputs).
- ValidatedRegionImportService maps any TryCreate false to gate MissingRegion;
  the Worker maps that to wire MissingRegion.
- This is a list of observed return paths, not an exhaustive guarantee that all
  malformed STEP files return false. Unexpected exceptions may escape (SR-L4).

ImportFailed (7, exit 3) is different: the Worker catches IOException,
UnauthorizedAccessException, ArgumentException and NotSupportedException
around processing/import and returns ImportFailed. Source-file absence can
therefore be ImportFailed, while a reader returning failed inventory can be
MissingRegion. The transport result is not a detailed diagnostic taxonomy.

### Ordering matters

Structural request validation happens before file access. Source import and
region construction happen before RegionSourceGate compares expected hash/face.
If region construction fails, that request returns MissingRegion without
reaching document comparison. A file can have both changed bytes and an
unconstructible region; consumers cannot demand DocumentMismatch in that case.
A changed, otherwise constructible golden file does reach DocumentMismatch.

No physical/material/tool approval is inferred from any of these outcomes.
Do not retry an aggregate failure with weaker requirements or legacy import
in order to obtain source-link approval.

### Operator interpretation

Suggested neutral wording: "The requested supported region could not be
created. Check the STEP input, supported solid/planar geometry and source face
ID. The result does not identify which of these conditions failed."

This is proposed guidance, not a claim that an existing SOLIDWORKS message
was changed. The pending #94 UI wording/physical acceptance must consider
this limitation before release; main has no menu entry for this operation.

### Diagnostic separation decision

Retain the existing v1.1 enum values for this bounded correction and
document the aggregate result; do NOT add SourceUnreadable based only on a
boolean failure. A future diagnostic separation needs a production reason
producer below TryCreate, explicit error precedence, backwards-compatibility/
version assessment and tests for each emitted cause. That future implementation
is not authorized by this document.

### Bounded deferral register

The user directed continuation after receiving the updated Senior conditions
on 2026-10-28. The following records the bounded tracking decision for this
correction work, not a backdated approval, release waiver or production closure.

| Field | Recorded decision |
| --- | --- |
| Finding | SR-M2 detailed cause separation; linked SR-L4 unexpected parser exceptions |
| Accountable owner | Repository owner Mehmet İlkay Kınacı; implementation assistant maintains the technical evidence/disposition in the working session |
| Recorded date | 2026-10-28 Europe/Istanbul |
| State | BOUNDED DEFERRAL for diagnostic separation; production gap remains OPEN |
| Reason | Current aggregate failures do not grant approval; boolean TryCreate supplies no reliable detailed cause. Senior section 7 assesses this as diagnosis risk, not a present fail-open finding |
| Reopen deadline | BEFORE catalog intake/importer work starts OR BEFORE an end-user release, whichever happens first |
| Additional release condition | #94 UI must use neutral aggregate wording; no absent-face-only diagnosis. Independent #94 review and physical acceptance remain required |
| Linked SR-L4 risk | Unexpected parser exceptions may escape and leave no result; Host remains fail-closed. Track exception/diagnostic design with M2; no broad catch or guaranteed ImportFailed claim |
| Required follow-up evidence | Reviewed production cause producer, exception/precedence policy including SR-L4, compatibility assessment and meaningful per-cause tests; or a separately explicit owner-approved revised disposition before crossing a trigger |

The deadline is a work/release gate, not a calendar reminder or an automation.
At either trigger this record must be revisited before proceeding; it cannot
be carried forward silently as an indefinite waiver.
#94 wording is a pending dependency, not completed UI work in this documentation PR.
No end-user release, catalog importer or material/machine approval is authorized.

## M3 FaceMismatch is defined but not currently emitted by the Worker

FaceMismatch retains its defined wire value 5. The current Worker passes
request.SourceFaceId both as the face to import and as expectedSourceFaceId to
ValidatedRegionImportService. A successfully created region has that same
source ID. Therefore the current v1.1 Worker input cannot independently express
two different faces to produce FaceMismatch.

This is not the same as asking for an absent face: an unresolved face prevents
region construction and yields MissingRegion. It is also not a reason to fall
back to whole-model import.

The Application API can take distinct imported/expected IDs and does produce
FaceMismatch. Existing test
ValidatedRegionImportServiceTests.WrongExpectedFace_DoesNotPublishImportedRegion
imports golden #8 with expected #30 and asserts FaceMismatch without publishing
a region. This is Application evidence, not proof of a Worker-emitted result.

The result validator currently recognizes defined non-Unknown outcomes,
including FaceMismatch. The Host handles a correlated FaceMismatch with exit 2
as Blocked. Neither validator recognition nor a controlled response test proves
the current Worker emits that code.

Preserve value 5: do not remove, renumber, recycle it or invent an input field
just to make the branch reachable. No separate ExpectedSourceFaceId wire field
is introduced. A future capability requiring two face identities needs an
explicit requirement and reviewed contract change.

InvalidSourceIdentity (3) is likewise a mapped Application-gate reason; malformed
wire identity is rejected by structural validation as RequestRejected before
import. Do not promise that every enum value is a producible Worker outcome.

## Current outcome and exit interpretation

Read this table together with structural validation, correlation and actual
producer reachability. It is not a guarantee of every row being Worker-emitted.

| Value | Outcome | Current interpretation and boundary |
| --- | --- | --- |
| 0 | Unknown | Invalid result; validator rejects it |
| 1 | SourceLinkMatched | Exact source link only; exit 0; Host accepts only for SourceLinkOnly |
| 2 | MissingRegion | Aggregate region construction failure; exit 2; not an absent-face-only diagnosis |
| 3 | InvalidSourceIdentity | Defined but not emitted by current v1.1 Worker; structural validation checks the same 64-hex digest rule and a stricter positive STEP face ID before import |
| 4 | DocumentMismatch | Expected digest differs after successful region construction; exit 2 |
| 5 | FaceMismatch | Defined but not emitted by current v1.1 Worker; Application can produce it |
| 6 | MaterialSideUnverified | After import/source checks, material-dependent evaluation remains blocked; exit 2 |
| 7 | ImportFailed | Caught processing/import exceptions; exit 3; not every parser failure is caught |
| 8 | RequestRejected | Invalid structural request; exit 2; malformed DTO type envelope correction merged in #97 |

Host still validates version, operation and exact request correlation, and
cross-checks result/exit consistency before consuming outcomes. Known non-success
codes other than ImportFailed require rejection exit 2. A recognized code alone
does not authorize success, tool selection or machining.

## Evidence and acceptance limits

Inspected source at the pinned baseline:
- src/MPC.Application/Manufacturing/ManufacturingRegionImportService.cs
- src/MPC.Application/Manufacturing/ManufacturingRegion.cs
- src/MPC.Application/Manufacturing/ValidatedRegionImportService.cs
- src/MPC.Application/Manufacturing/RegionSourceGate.cs
- src/MPC.Worker/RegionSourceValidationProcessor.cs
- src/MPC.Contracts/RegionSourceValidationResult.cs and result validator
- src/MPC.Host.SolidWorks/Commands/RegionSourceValidationHostCommand.cs

Existing tests inspected: MatchingRealImport_PublishesOnlySourceLinkedRegion,
UnknownSourceFace_ReturnsMissingRegion,
WrongExpectedFace_DoesNotPublishImportedRegion and
ChangedSourceBytes_RejectOldExpectedHashWithoutPublishingRegion in
ValidatedRegionImportServiceTests. Standalone acceptance's absent-face case
demonstrates one MissingRegion cause only. Neither existing CI nor this document
proves a new full malformed-file/root/unsupported-surface diagnostic matrix.

Historical baseline CI #188 succeeded at the pinned code-inspection main.
#99's final head CI #194 succeeded, followed by merged main CI #199 at
857808225aa9a27e98c51e9a4839d8227a5fddee: 1815/1815 full tests, 5/5 standalone
and 3/3 targeted cases; Release 0 warnings/errors and corpus checks PASS.
M3 documentation/review/merge/CI conditions are satisfied. #96 H1/H2 record
reconciliation, #97 M1 and #98 M5 are also merged; their retained historical
limits and residual issues are in the [current disposition](TOOL-01D-01-07-Scope-and-SR02-Disposition.md).
M2 production diagnostic separation remains OPEN under the bounded register.
#94 UI wording/review/physical acceptance remains pending. No overall SR02
closure, deployment or v1.1 freeze is asserted.

## Updated Senior review and changed-head limits

Source: user-supplied updated SR02 DOCX, dated 2026-10-28, section 7.
SHA-256: `592AB490CEDD452BED642BBC08337928343A9FE2174478BC5BB0FA71FAA37C97`.
The report is external conversation evidence, not a repository-resolvable file.
It reviews `a48f0be8698d370c87c0704acb5bbc40b6a0cc39` and conditionally approves
#99 for merge after the InvalidSourceIdentity table clarification and its own
CI. The reviewer statically verified behavior against main 3ba5f9c but did not
independently verify CI or run tests.

This follow-up implements that explicit table correction and records the
recommended M2 owner/date/triggers with linked SR-L4 and pending #94 neutral UI
condition. No enum/runtime change or early-hash comparison is introduced.
The suggested hash-before-parse redesign is outside scope and needs its own
decision and tests.

The original reviewed SHA must not be relabelled as this follow-up's SHA.
At that historical revision, the additional deferral/disposition delta needed
review. Subsequent SR02 revision 3 section 9 explicitly approved
5c8926b417312725bdd908622217556049bff3fe, subject to its own green CI.
Source SHA-256: `4F92343132574FC80905A768A7DA6BF1F6A63538DB5B48364D03BEA02E1745CD`.
This is the completed changed-head review, not an unperformed pending gate.
#99 merged as 857808225aa9a27e98c51e9a4839d8227a5fddee after exact-head CI;
main CI #199 also succeeded. No further review is requested for that same delta.
Section 7 did not cover #94/#96/#97/#98; revision 2 section 8 later reviewed
those separately. It requested changes on #94 and approved the other scopes
as recorded in the current disposition. Earlier section 7 approval is not
retrospectively extended to different PRs.
