# TOOL-01D-06 — Controlled STEP source-face menu acceptance

Status: implementation prepared; interactive SOLIDWORKS acceptance PENDING.
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

## Automated evidence to require

- Input cancellation does not launch Worker or report approval.
- Malformed face ID fails before launch.
- Request uses the pre-prompt hash and explicit source ID.
- A blocked or uncorrelated response cannot display a match.
- Controlled UI interaction -> real net48 Host -> net8 Worker covers golden
  match and changed-file mismatch. This does not exercise a live SOLIDWORKS menu.

## Workstation acceptance (not yet executed)

Use an approved Release package with the matching Worker beside the add-in.
Record exact commit, SOLIDWORKS version and results; do not call this checklist PASS
from CI alone.

1. Load the add-in in SOLIDWORKS 2024 SP1. Verify the existing import entry and
   the new source-face entry both appear. Restart/reconnect once to check callbacks.
2. Select T001_PRISM.step and enter #8. Expect that the pre-prompt and analysis
   content digests match and a supported planar STEP face was constructed.
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

Interactive acceptance, add-in deployment/registration, merge and deploy are
separate actions. No local workstation test or production suitability is claimed.

## Senior rework and exact build

SR02 section 8 requested changes on head 5f718bae: accurate match scope,
neutral Turkish outcomes, incorporation of current main and physical focus/
responsiveness observations. This revision preserves the original fail-closed
approval checks and adds validated blocked-response/transport-precedence message
tests. No enum/DTO, asynchronous infrastructure or selected-face mapping is added.

Main incorporated: 857808225aa9a27e98c51e9a4839d8227a5fddee, including merged
#95/#97. Parent linkage is retained; no history rewrite or main merge is performed.
The PR records the final head and full Release/CI evidence.
Changed-diff independent Senior review is REQUIRED before merging #94.
Physical acceptance is a pre-merge gate for this PR; it remains PENDING until
the exact final head is tested on SOLIDWORKS 2024 SP1.

The user supplied a screenshot confirming SOLIDWORKS Premium 2024 SP1.0 on
2026-10-29. This establishes workstation version only, not add-in installation,
physical results, dialog focus or performance acceptance.
