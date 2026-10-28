# TOOL-01D-07 — Standalone Worker acceptance
[VERIFY / HARDEN] Reuse the existing v1.1 Worker process without SOLIDWORKS.
No new DTO, UI, geometry policy or product approval is introduced.

Prerequisites: PowerShell 7, repository-pinned .NET 8 SDK (global.json), git,
and restore access. No SOLIDWORKS, interop assembly, registration or administrator
privilege is needed. Run from the repository root:

~~~powershell
pwsh -File tools/acceptance/Invoke-RegionSourceAcceptance.ps1
~~~

The script builds only Worker and its non-COM project dependencies in Release.
It copies the golden STEP to a unique temporary evidence directory, including a
path containing spaces. It never edits the original fixture. Evidence includes
each JSON request/result and a summary with repository HEAD and source SHA-256.
An explicit -EvidenceDirectory must name a nonexistent directory.

Five real Worker processes check: exact file/face match; changed bytes against
the original digest; absent face; malformed face; material-dependent request
without independent evidence. Each must return the expected exit code, outcome,
v1.1 operation and exact request correlation. Timeout or missing/malformed result
fails the script; no summary PASS is emitted on failure. Evidence is retained
for diagnosis, including copies. Remove the temporary directory when no longer
needed.

CI executes the same script after full regression and retains its evidence.
This is additional process acceptance, not a substitute for full regression.
The SOLIDWORKS menu/callback acceptance in TOOL-01D-06 remains PENDING.
No live workstation execution, merge, deployment or physical qualification is
claimed by this document.
