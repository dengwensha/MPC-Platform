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
