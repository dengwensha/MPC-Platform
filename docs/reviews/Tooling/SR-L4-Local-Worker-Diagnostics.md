# SR-L4 local Worker diagnostics

Date: 2026-11-18 Europe/Istanbul. Class: HARDEN.
Start: main `15aaac60e38a483c380e86171ce4f6451f38da7a`.
Goal: retain the technical cause of a caught v1.1 region import exception
without changing its result, correlation or exit handling.

## Operator use and limits

After a controlled STEP import failure, inspect
`%LOCALAPPDATA%\MPC\Diagnostics\worker-last-failure.json`.
Match its RequestId and UTC time to the failing request; the record can be
older if a later write failed. It is local diagnostic evidence, never approval
or a replacement for the validated result JSON.

The record holds timestamp, RequestId, exception type and Exception.ToString()
(including inner causes and available stack traces). Details are limited to
8192 characters and truncation is explicit. Only the latest successfully
written record is retained; each completed record is below 64 KiB. Concurrent
Workers stage separately and the last successful replacement wins. Staging
files are removed on normal completion/failure; an externally terminated
process can leave a small worker-*.tmp file. No background cleanup is added.

Exception details can contain local paths and STEP values. No source file or
request payload is copied and no network service receives the log. Review the
record before sharing it. A success does not clear the previous failure.

## Scope and isolation

Only exceptions already caught by RegionSourceValidationProcessor are logged.
Structural rejection and success do not access the diagnostic directory.
The existing ImportFailed result/exit path is unchanged even if logging fails.
The broad diagnostic-only catch prevents optional record creation/cleanup
from replacing the business result; it does not catch additional importer
exceptions. Worker dependencies and wire contracts are unchanged.

Host launch/timeout/transport failures, legacy v1.0, request/result file errors,
unhandled InvalidOperationException and other unexpected exceptions are outside
this slice. The Host diagnostic gap and SR-L4 residuals remain OPEN. This is
not a claim that every Worker failure is recorded or that logging is durable.

## Acceptance and review

Tests cover UTC/correlation/inner cause, bounded replacement, real missing-file
import failure, blocked log directory, no diagnostic I/O on structural rejection
and golden success. Existing runner tests retain result/exit and valid-input
regressions. Exact-head Release/full regression/real-corpus CI evidence and
Start/Reviewed/Closing trace are recorded in the PR.

Persistence is a written risk trigger: independent Senior review is required
before merge. No new physical SOLIDWORKS acceptance is claimed; Host is unchanged.
