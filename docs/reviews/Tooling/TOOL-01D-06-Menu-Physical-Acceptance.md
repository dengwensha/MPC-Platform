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
2. Select T001_PRISM.step and enter #8. Expect only a file/face match and an
   explicit statement that no physical/tool/material qualification is granted.
3. Cancel file selection and then face input in separate runs. Expect no Worker
   launch and no approval/error dialogue caused by cancellation.
4. Enter malformed "8" or "#0". Expect input rejection. Enter "#999999".
   Expect MissingRegion rather than whole-model import fallback.
5. Work on a copy of the golden STEP. While the face input is open, append a
   legal newline to the file and save it, then enter #8. Expect DocumentMismatch.
6. Run the original STEP import command and confirm its prior summary behavior.

Interactive acceptance, add-in deployment/registration, merge and deploy are
separate actions. No local workstation test or production suitability is claimed.
