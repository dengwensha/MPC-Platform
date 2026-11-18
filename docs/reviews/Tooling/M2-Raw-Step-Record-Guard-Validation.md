# M2 raw STEP record-guard validation

Date: 2026-11-18 Europe/Istanbul. Class: HARDEN.
Start: `c9c80a87df4b149d5de2903d6e10b9f60d7d2122`.
Test-first commit: `98925829baae0b6adffaf27366d4be47b9ce35cf`.
PR: [#110](https://github.com/dengwensha/MPC-Platform/pull/110).

## Verified gap and narrow correction

Senior #107 finding 3 requested behavioral evidence for StepDataEntityRecord's
four InvalidOperationException guards (baseline lines 218/226/254/263).
The original static classification was incomplete: raw simple declarations
`#999999 = COMPLEX_ENTITY();` and lowercase `complex_entity ('raw input')`
reach line 218 through the reader's simple-entity regex. The type name collides
with the model's internal discriminator while component types are empty.
This exception escapes the Application importer and real v1.1 runner.

Before runtime changes, local .NET 8.0.422 Release tests reproduced exactly
six failing cases: two reader, two importer, two real WorkerProcessRunner.
Six controls passed: four raw-reader metadata branches and two empty-complex
Worker cases. Reader stacks identify StepDataEntityRecord line 218. Tests use
actual file text; no record mocks or replacement importers are used.
[RED CI #229](https://github.com/dengwensha/MPC-Platform/actions/runs/44357374536)
was independently checked: Release 0 warnings/errors; exactly six failures
(reader 2, Worker/importer 4), with the other 1930 cases passing. This is the
expected RED result, not an unexplained CI failure.

The correction rejects only the simple header's normalized COMPLEX_ENTITY name
with FormatException before record construction. The existing Application
FormatException translation retains the original cause as InvalidDataException;
existing v1.1 processing emits ImportFailed (7), exit 3 and original RequestId.
No region is published. The model's four invariant guards remain unchanged.
No broad InvalidOperationException catch or message-based classification is added.

The reader is shared: legacy v1.0 still uses its generic failed-import boundary,
but the exception type named in its diagnostic for this malformed declaration
changes from InvalidOperationException to FormatException. No legacy success,
DTO, enum, transport or UI behavior is broadened. This is a reviewable effect,
not a claim of zero legacy-path change.

## Guard reachability assessment

| Baseline guard | Evidence / disposition |
| --- | --- |
| 218: COMPLEX_ENTITY without components | Input-reachable through the simple header collision above; now rejected before constructing the record. Empty `( )` already takes UNKNOWN/Partial with an Error diagnostic. |
| 226: non-complex record with components | Static reader path supplies components only when the type is COMPLEX_ENTITY and count is positive. Raw valid-complex, simple and UNKNOWN controls exercise the branches. No raw trigger found; not an exhaustive malformed-input proof. |
| 254: Complete with error/fatal diagnostics | Complete reader branches supply no diagnostics; controls check coherent metadata. Guard retained. |
| 263: Partial/Failed without error/fatal diagnostics | Both current Partial branches construct an Error diagnostic; controls cover empty complex and unrecognized header. Reader does not construct Failed records. Guard retained. |

## Existing aggregate limits preserved

`#999999 = ( );` alone yields MissingRegion / exit 2 through the real runner.
The same unreferenced malformed record added to golden T001_PRISM is preserved
as UNKNOWN/Partial by the reader yet still yields SourceLinkMatched / exit 0
for the supported #8 region and current hash. These controls deliberately record
existing behavior: source-region matching does NOT certify complete-file syntax
validity. This work does not silently make all inventory diagnostics fatal or
add detailed M2 outcomes. The Senior must assess this explicit limit.

Normal complex syntax with a string containing COMPLEX_ENTITY remains readable;
ordinary unsupported simple names keep existing complete-inventory behavior.
Existing golden source-match, duplicate-ID and malformed-torus cases remain
regression controls in the Worker suite.

## Acceptance and scope

Twelve added cases: reader 6, Worker/importer 6. Full Windows Release/net48,
standalone Worker and real corpus results belong to the final PR CI.
Local core suite passes 1366/1366; the portable Worker subset passes 85/85. The Linux local full Worker run has one
platform-only failure: existing WorkerExecutableIntegrationTests requires
MPC.Worker.exe, absent from a Linux build; it is not counted as a full PASS.
Windows CI must verify that unchanged executable-boundary test.
No physical SOLIDWORKS test is claimed or substituted by runner tests.

Independent Senior review of the final diff is required: trust/error
classification trigger Yes. Wire shape No; enum values No; IPC mechanics No;
persistence No; production UI entry No. The changed shared-reader legacy
exception text and preserved UNKNOWN behavior are explicitly in review scope.
The prior #108 approval does not cover this difference.

M2 remains OPEN. The raw-input follow-up is reproduced and a correction is
submitted, not self-declared closed. Its existing gate remains BEFORE catalog
intake/importer starts OR end-user release, whichever first. Accountable owner:
repository owner Mehmet İlkay Kınacı; implementation assistant maintains evidence.
SR-L4 remains bounded as previously recorded; new defect evidence is tracked here,
not represented as a universal containment guarantee or a release waiver.

## Senior section 18 bounded closure

The user supplied the independent Senior section 18 decision in the working
conversation: exact #110 head `c5c2ca228dd9f239f3fc2a252a8fb9f434484455`
approved for merge, no blocking finding, no physical acceptance required.
The reviewer read code/tests but did not execute build/tests or verify CI.
This records the supplied review, not a fabricated GitHub review submission;
reviewer identity/report bytes were not independently verified in this session.

Implementation-session verification separately confirmed the unchanged reviewed
head and successful [CI #230](https://github.com/dengwensha/MPC-Platform/actions/runs/44358126197):
Release 0 warnings/errors, 1936/1936 full tests (1366 core, 94 Contracts,
219 Application, 86 Worker, 171 net48 Host), Worker acceptance 5/5, targeted
3/3 and real-corpus compatibility/reconciliation. #110 merged as
`bc51815ad76584d2996db9c41ad5994f496e67d8`.
[Post-merge main CI #231](https://github.com/dengwensha/MPC-Platform/actions/runs/44362559076)
must succeed before this closure record is merged/effective. The documentation
PR must also pass its own CI.

Disposition after those gates: CLOSED WITH DOCUMENTED LIMITS for the raw STEP
negative-test follow-up concerning ONLY StepDataEntityRecord's four baseline
guards at 218/226/254/263. Line 218 was reproduced and corrected; the other
three were independently checked by static reader-branch inspection and
representative control tests. No claim that all malformed STEP inputs, parser
exceptions or inventory errors are covered. The earlier pending statements
above are historical and superseded for this four-guard follow-up only.

M2 remains OPEN. The newly explicit UNKNOWN/Partial inventory-error policy
item is recorded in the [M2 outcome register](SR02-M2-M3-V11-Outcome-Limits.md#m2-inventory-error-policy-open-senior-section-18).
Current source-link matching may retain an unreferenced malformed record.
Before whole-file feature recognition, time or cost evaluation, decide whether
inventory errors block evaluation or are shown to the user, and define evidence,
propagation and tests for the chosen policy. Do not infer a policy choice from
this closure. The M2 catalog/importer/end-user-release gate still applies.

The Senior's non-blocking contract note is implemented as XML documentation on
both IStepDataEntityReader.Read and StepDataEntityReader.Read: a simple reserved
COMPLEX_ENTITY declaration can throw FormatException and abort this read, while
other malformed statements can remain diagnostic-bearing partial records.
This documents #110 behavior; no new runtime or public signature change.

This factual acceptance/closure and XML-comment update adds no wire, enum,
trust/fail-closed, IPC, persistence or UI behavior. All six new-diff risk-review
triggers are No; it records the supplied independently accepted scope and
creates no additional independent-review ceremony. No physical PASS or end-user
release is claimed. All other SR-L4 follow-ups retain their separate owners/gates.
