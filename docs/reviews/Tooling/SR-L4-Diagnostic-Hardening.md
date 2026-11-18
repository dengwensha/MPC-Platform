# SR-L4 diagnostic hardening

Date: 2026-11-18 Europe/Istanbul. Class: HARDEN.
Start: main `409c88c7207112f8d85b3ce100b56cac1c7eb2ab`.

## Bounded goal

Address the non-blocking diagnostic notes from Senior sections 13 and 14:
readable Turkish, safe Worker truncation, orphan staging retention and received
Host reply fields. Existing outcomes, validation, correlation, exit codes and
log locations stay unchanged. No new parser catch or overall SR-L4 closure.

Both local log serializers now allow Basic Latin, Latin-1 Supplement and Latin
Extended-A, keeping Turkish characters readable without disabling HTML-sensitive
escaping globally. These options do not affect wire serializers. Worker now
avoids cutting valid surrogate pairs, as Host already did. Per-field limits
remain unchanged and completed records remain below 64 KiB.

Host adds ReceivedRequestId (64 chars), ReceivedContractVersion (32 chars),
ReceivedOperation and ReceivedOutcome when a deserialized WorkerResult exists.
These are untrusted diagnostic fields. Original RequestId stays the trusted
request correlation; the existing validator still decides rejection. Raw or
unparseable JSON is not copied; received fields are null when no result exists.

## Staging retention on Windows

Each logger examines at most 64 matching staging entries on a failure write.
Only its own host- or worker- prefix plus exactly a 32-hex GUID and .tmp is
eligible, strictly older than 24 hours. Reparse-point candidates are skipped.
Other prefixes, unknown names, recent files and completed JSON records are
untouched. This is best-effort maintenance, not a guaranteed disk quota or
complete sweep; a directory with many candidates can retain old entries.

Writers create unique staging files with FileShare.None. Cleanup must open an
old candidate with read/write access and FileShare.Delete before deleting it,
keeping that claim open through deletion. Windows sharing rules exclude live
writers and prevent a new writer entering during the claim. The age is rechecked
under the claim. Busy/inaccessible candidates and enumeration/cleanup errors are
ignored independently so record creation is still attempted.

The supported Host environment is Windows/net48; these sharing/deletion tests
run on the Windows CI worker. No equivalent active-file guarantee on Unix is
asserted. The short interval after a completed writer closes and before its
move has a fresh last-write timestamp and is not eligible for age cleanup.
No service, timer or user-directory-wide deletion is added.

## Data handling and remaining work

Existing local-only confidentiality and stale-record guidance remains in force.
No request/source payload copy or upload. Read diagnostic contents before sharing
customer filenames and follow the applicable confidentiality/data-sharing terms.
Unexpected InvalidOperationException classification and add-in general-catch
diagnostics remain separate follow-ups. Physical launch-failure smoke check
suggested in section 14 remains recommended for the next physical session.

Regressions cover locked old staging versus old orphan/recent/unrelated files in
both runtimes, readable non-ASCII size limits, both sides of the surrogate cut,
and wrong reply ID/version/outcome while preserving original correlation.
Exact-head Release/full regression/corpus CI and trace are recorded in the PR.
Persistence cleanup and untrusted diagnostic fields require independent Senior
review before merge. No physical SOLIDWORKS PASS is claimed.
