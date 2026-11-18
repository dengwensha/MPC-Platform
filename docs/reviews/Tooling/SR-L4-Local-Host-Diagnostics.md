# SR-L4 local Host diagnostics

Date: 2026-11-18 Europe/Istanbul. Class: HARDEN.
Start: main `6799ab06b1a89d42ae687f532577e937593970dd`.
Goal: retain v1.1 source-face Host launch, timeout, observation, result-read/
validation and transport failures without changing their operation results.

## Operator use

Inspect `%LOCALAPPDATA%\MPC\Diagnostics\host-last-failure.json`.
Match RequestId and UTC time. It records HostStatus, observed Worker exit code
when available, retained failure type/message and exception details/inner causes
when an exception is available at the command boundary. Launch/observer result
objects retain type/message only; this change does not invent missing stacks.
For controlled ImportFailed use the separate Worker log from #104.

Only the latest successfully written Host record is kept. A stale record can
remain after an unsuccessful write or a subsequent success. Details are capped
at 8192 characters and the message at 1024; completed JSON is below 64 KiB even
with default JSON escaping. Truncation of details is explicit and valid surrogate
pairs are not split. Unique staging plus net48 File.Replace for an existing file
(or File.Move for the first file) avoids publishing partial records. Concurrent
first writers may lose one optional record; subsequent successful replacement
wins. Externally terminated processes can leave host-*.tmp files; no cleanup
policy is introduced here.

Exception text can contain customer filenames, local paths and STEP values.
No source/request payload is copied or uploaded. Review before sharing; support
handling of customer identifiers must follow the applicable confidentiality/
data-sharing agreement. No agreement is asserted to exist by this document.

## Isolation and residual scope

The existing ExecuteCore control flow retains validation, outcome/exit checks
and transport cleanup. Execute logs only the five listed Host failure statuses
after that result is computed. The diagnostic-only catch covers file/logging
errors; no additional importer or Host business exceptions are caught.
InvalidRequest, SourceLinkMatched, Blocked and WorkerFailed do not access the
Host diagnostic directory. Logger failure does not replace the returned result.

No DTO, enum, Worker classification, legacy v1.0 or UI message change. Thin Host
references remain unchanged. Worker Unicode/temporary-file follow-ups from
SR02 section 13 and unhandled InvalidOperationException remain outside scope.
No overall SR-L4 closure or physical SOLIDWORKS acceptance is claimed.

Tests cover five failure categories including malformed/correlation/exit replies,
unwritable logging, no logging for other statuses, bounded replacement and
Turkish text round-trip. Exact-head Release/full net48 Host regression and
real-corpus CI plus Start/Reviewed/Closing trace are recorded in the PR.
Persistence and Host failure-path changes require independent Senior review
before merge. If that review requires interactive acceptance, it remains a
separate explicit gate; automated fake-process evidence is not physical evidence.
