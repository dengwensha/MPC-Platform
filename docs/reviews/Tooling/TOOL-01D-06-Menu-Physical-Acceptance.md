# TOOL-01D-06 — Controlled STEP source-face menu acceptance

Status: physical acceptance accepted under SR02 section 11 for code head
`1bf56d2c2e4b8b2cd5e1bbb53f33fb5a94f8721f`, using the staged evidence below.
Merge readiness requires green CI on the final documentation delivery.
Scope: source-file/STEP-face linkage only, not selected SOLIDWORKS face mapping,
material-side qualification, tooling approval or machining.

## Menu and input

Existing import command/group IDs remain 10001/1001. New command ID is 10002,
labelled "STEP kaynak yüzünü doğrula". Both callback and enable callback are
forwarded by the existing COM-visible callback proxy.

The user selects a STEP file. The Host captures its SHA-256 before the face
prompt, asks for a canonical positive STEP source ID (for example #8), and sends
a SourceLinkOnly v1.1 request. It never reads the active SOLIDWORKS selection.
The displayed hash binds this selected file snapshot; it is not independent
supplier authentication or proof of continuity with another CAD export.

InputBox uses the Microsoft.VisualBasic framework assembly supplied with
.NET Framework; no NuGet package or new CAD/Application reference is introduced.
The existing v1.0 import command is untouched.

The source-face command uses a .NET Framework Windows Forms OpenFileDialog,
owned by the SOLIDWORKS process main window. It returns the selected path on OK
or null on cancellation. RestoreDirectory preserves the process working directory.
Both file-list selection and a pasted full path must reach the face prompt.

## Automated evidence to require

- Input cancellation does not launch Worker or report approval.
- Malformed face ID fails before launch.
- Request uses the pre-prompt hash and explicit source ID.
- A blocked or uncorrelated response cannot display a match.
- Controlled UI interaction -> real net48 Host -> net8 Worker covers golden
  match and changed-file mismatch. This does not exercise a live SOLIDWORKS menu.

## Workstation acceptance procedure

Use an approved Release package with the matching Worker beside the add-in.
Record exact commit, SOLIDWORKS version and results; do not call this checklist PASS
from CI alone.

1. Load the add-in in SOLIDWORKS 2024 SP1. Verify the existing import entry and
   the new source-face entry both appear. Restart/reconnect once to check callbacks.
2. Select T001_PRISM.step and enter #8. Expect that the pre-prompt and analysis
   content digests match and a supported planar STEP face was constructed.
   Repeat by pasting the same full file path in the File name field. Both
   selection methods must reach the face prompt without opening a CAD document.
   The message must not claim continuous file monitoring or continuity with an
   earlier import; physical/tool/material qualification is explicitly excluded.
3. Cancel file selection and then face input in separate runs. Expect no Worker
   launch and no approval/error dialogue caused by cancellation.
4. Enter malformed "8" or "#0". Expect input rejection. Enter "#999999".
   Expect the neutral Turkish aggregate-region failure explanation rather than
   raw MissingRegion or whole-model import fallback. Check that it mentions
   STEP input, supported solid/planar geometry and face identity without selecting
   one as the proven cause.
5. Work on a copy of the golden STEP. While the face input is open, append a
   legal newline to the file and save it, then enter #8. Expect Turkish digest
   mismatch wording. This golden-file test does not promise that an unconstructible
   changed file always reaches digest comparison.
6. Run the original STEP import command and confirm its prior summary behavior.
7. Check that the InputBox opens in front of SOLIDWORKS and does not become hidden
   behind its window; record any focus/ownership problem.
8. On a representative large STEP copy, record file size, approximate hashing
   duration before the face prompt and Worker wait duration. Record how long the
   UI cannot respond and whether it resumes. Calls are synchronous on the UI thread;
   do not claim responsiveness or solve it with a background-COM change here.

## Executed acceptance and trace

Date: 2026-11-18 Europe/Istanbul. Workstation: SOLIDWORKS Premium 2024 SP1.0,
Windows x64, .NET SDK 8.0.422. The user performed the interactive actions and
supplied screenshots and explicit confirmations in the working conversation.
This record is prepared by the implementation assistant.

| Check | Tested code head | Result and evidence |
| --- | --- | --- |
| Menu, restart and callbacks | 699db456634e601d4ce7eb8b7073dbc9241a45ed | PASS. Original import worked after restart; source-face picker and foreground prompt shown in image(20261117-212009).png and image(20261117-212318).png / image(20261117-212337).png |
| File selection and golden #8 | 699db456634e601d4ce7eb8b7073dbc9241a45ed | PASS. Full path entered in the picker led to T001_PRISM.step face prompt and bounded success message, image(20261117-211229).png / image(20261117-211236).png |
| File cancellation and face cancellation | 699db456634e601d4ce7eb8b7073dbc9241a45ed | PASS, user confirmation. The user withdrew an initial ambiguous confirmation, repeated the checks, then explicitly authorized acceptance. Windows closed, SOLIDWORKS remained open, no result/error message. Worker absence is covered by controlled tests, not live process observation |
| Malformed and absent face | 699db456634e601d4ce7eb8b7073dbc9241a45ed | PASS. "8" rejected as malformed; "#999999" produced neutral aggregate-region failure. image(20261117-211325).png / image(20261117-211331).png / image(20261117-211351).png / image(20261117-211401).png |
| Changed source bytes | 699db456634e601d4ce7eb8b7073dbc9241a45ed | PASS after repeat with save before confirmation. Digest mismatch shown in image(20261117-211914).png. Earlier unchanged-digest success in image(20261117-211725).png is excluded from mismatch evidence |
| Existing STEP import | 699db456634e601d4ce7eb8b7073dbc9241a45ed | PASS. Solid 1, shell 1, faces 6, loops 6, edges 12, vertices 8, image(20261117-212009).png |
| Prompt visibility/focus after restart | 699db456634e601d4ce7eb8b7073dbc9241a45ed | PASS, visible prompt and focused input, image(20261117-212337).png |
| Large R03 observation and recovery | 699db456634e601d4ce7eb8b7073dbc9241a45ed | 14,003,088 bytes; approximately 3 seconds from file selection to prompt and 5.44 seconds from #8 confirmation to message, user measured. Menus usable after dismissal. Initial response failure led to the fix below |
| R03 controlled failure retest | 1bf56d2c2e4b8b2cd5e1bbb53f33fb5a94f8721f | PASS. Controlled Turkish import failure, image(20261117-214113).png; user explicitly confirmed menus usable |
| Final golden success regression | 1bf56d2c2e4b8b2cd5e1bbb53f33fb5a94f8721f | PASS. T001_PRISM.step / #8 success, image(20261117-214250).png |

The screenshot files are conversation evidence and are identified by their supplied
filenames. They are not committed repository attachments. Earlier code-head results
are retained under their actual SHAs. The UI checks were not all repeated at
1bf56d2c. SR02 section 11 explicitly accepted their applicability because the
699db456 -> 1bf56d2c delta changes Application/Worker handling and its test, not
Host code, and both R03 recovery and golden success were repeated at the final
code head.

The final R03 duration was not remeasured. The recorded 3 s / 5.44 s values belong
to 699db456 and its failing path. UI blocking during those intervals was not
independently timed. Calls remain synchronous. The observed recovery is accepted
under section 11; continuous responsiveness is not established by these results.

## Findings and corrective delivery

Historical full-path failure at 25e6e088a789276f7906cf5aaea75ee18a267662 was
followed by picker correction 699db456. The internal GetOpenFileName2 cause was
not proved. The successful full-path retest above supersedes the earlier FAILED
acceptance state.

R03 direct execution at 699db456 reproduced an unhandled StepSemanticValueException
wrapping ArgumentOutOfRangeException for majorRadius -178.5, native exit
-532462766 and no result JSON. An earlier probe targeted a directory and is
excluded from diagnosis. Commit 1bf56d2c translates that specific semantic
exception into InvalidDataException at the Application importer boundary, retaining
the inner exception. The v1.1 Worker returns existing ImportFailed (7), exit 3,
which Host maps to its controlled Turkish import failure. Negative and positive
torus radius cases exercise the real importer, runner and correlated serialization.

Other parser exception classes remain tracked under SR-L4 in
[the M2/L4 register](SR02-M2-M3-V11-Outcome-Limits.md). The reproduced semantic
exception case is addressed; broader parser diagnosis and local diagnostic logging
remain open follow-up work.

## Main, independent review and CI

- SR02 section 10 approved code/documents at 144d2b65 subject to updated-main CI
  and physical acceptance.
- Current main a2f0f5ffdfde8b07dc5a44608a10152d8070bbd8 was incorporated at
  25e6e088a789276f7906cf5aaea75ee18a267662, with all eight reviewed PR blobs
  preserved.
- User-supplied SR02 section 11 approved the 699db456 and 1bf56d2c corrections.
  It found no merge-blocking code issue and accepted staged physical evidence.
  Its remaining merge condition was this final factual acceptance update and
  verification of green CI. The Senior did not run build/tests or verify CI.
- The implementation assistant verified CI #209 SUCCESS on
  1bf56d2c2e4b8b2cd5e1bbb53f33fb5a94f8721f:
  [44181299212](https://github.com/dengwensha/MPC-Platform/actions/runs/44181299212).
  Release 0 warnings/errors, 1866/1866 full tests with 0 skipped, standalone Worker
  5/5, targeted 3/3 and all real corpus/whitespace/clean-checkout checks passed.
- This final update changes acceptance and remaining-risk records only. The
  final documentation commit's own CI and merge status are recorded in PR #94.
  Runtime code remains identical to the physically retested 1bf56d2c code tree.
