# B6 — target work/allocation reduction, first slice

Date: 2026-11-24 Europe/Istanbul. Class: HARDEN / measured optimization.
Start: 927bed567cbb223350be37ad88bc0c728a3c4bff (#142 merged, main CI303 green).
Source-equivalent implementation: 139efdf227cc9ed3d79644961f9a41eb0f3dc414.
Branch: perf/B6-target-allocation. Owner workstation: Windows/SOLIDWORKS,16GB.
Refs: #141; Senior SR02 sections39–40.

## One goal and unchanged boundaries

Reduce repeated target work and managed allocation while retaining exact target
binding decisions and ordered reasons. Only TargetProducer production code changes.
No consumer, new evidence issuer, parser/import/wire/Host/Worker change, policy
relaxation, reason aggregation, normalization or resource enforcement.
The hash-checked approved contract, revision tuple,20 Error reasons, direct same-
inventory ImportUnitPrecheck call and C1 ownership boundary remain unchanged.

Field/profile metadata is indexed once from the fixed embedded contract. This is
bounded contract preparation, not cross-file caching: no source-file values,
inventory, session, observation or decision is stored in static metadata. Matching
uses decoded JSON ValueEquals and equivalent short-circuit loops, avoiding repeated
string decoding/predicate iterators. Every valid parsed row records references
once in the same depth-first order with duplicates retained; each Check owns these
arrays and releases them with its other temporary structures. The reverse index
still includes original incomplete/unknown records as negative path evidence.

An exploratory, temporary stage-instrumented R03 build identified parsing and field
validation as large allocation sources. Instrumentation was removed before the
actual paired experiment; it is not production source or benchmark evidence.
This slice leaves the shared STEP parameter parser unchanged. It does not promise
lower resident peak merely because allocation volume drops.

## Paired protocol

Reuse #142 B6 protocol: same20 historical SHA256 inputs, fresh process per file/unit,
two trials, first call, two untimed warmups and three timed warm calls; explicit GC
outside timer, reverse stage order in trial2, fixed DLLs through the experiment.
Immediate-before approved source927bed5 replaces the earlier pre-target baseline.
The new optional --require-target-equality additionally requires before/after owned
capture semantic projections to be identical; repeated owned/neutral target data,
identity, unavailable stages and released-object collection remain mandatory.
Historical #142 source/evidence stays intact. Prepared-inventory target timing and
process-lifetime memory caveats still apply. This is not a cold OS-cache test,
worst-case bound, real Worker peak, workstation acceptance or physical CAD test.

The candidate DLL was built before the source-equivalent delivery commit; Validation
records normalized source hashes instead of claiming that later commit was embedded
in the measured binary. SDK8.0.422 Release; no rebuild during paired measurement.

## Verification and merge gate

Local Application526, core1366, Contracts94 and portable Worker90 tests PASS.
One existing Worker test explicitly requires MPC.Worker.exe; Linux emits a native
executable without that extension, so that test was not counted as a Linux pass.
Full Windows2248 regression including that test and net48 Host, Worker5/5 acceptance,
public9 paired measurement and all existing real/control checks are merge gates.
No tests or assertions are weakened; TargetProducer source pin changes for review.

The companion audit comparator retains every projected decision field, ordered
reason, severity, index, identity and revision. Only Application/reader DLL hashes
are treated as provenance (retained separately); same reader source is verified
against the pinned baseline. Existing audit policy omits inner cause text to avoid
private STEP tokens; that omission is unchanged. A corrupted-severity self-check
is rejected. CI compares public9 and13 controls with the actual baseline audit.

Final20-file/two-trial measurements,33-input audit comparison, Windows evidence
and exact review head CI are recorded below/on the PR when complete. No result is
inferred from a partial run.

## Remaining resource acceptance

Serial operation;10s ceiling remains a proposal. The512MiB engineering target is
not raised or declared met by this slice. The suggested roughly410MiB R03 peak
(20% headroom), or a reasoned explicit owner ceiling, must be evaluated in the real
Worker with the private corpus on the owner's16GB Windows machine. External
process enforcement and non-success on limit breach require a separate reviewed
design. First consumer remains blocked; #141 is not closed by this PR.

## Completed paired measurements

Linux:20 files/444497 records;120 fresh normal processes,480 measured calls,
240 warmups; four additional unavailable controls/24 calls. All input identities,
repeated semantics, before/after target projections and released-object checks
passed. Target outcomes remain17 TargetBoundOnly /3 Blocked. All20 warm medians
and allocated-byte medians decreased in this experiment; no worst-case claim.
The separate33-input audit (20 corpus +13 controls) exactly matches the approved
historical target projection after excluding only DLL provenance hashes. This
includes ordered reasons, severity, indices, messages, IDs and projected revisions;
no historical candidate loss or compatibility relaxation occurred.

Values below are actual CaptureFromFile, not the isolated target stage. Warm is
median of six calls; peak is process-lifetime high-water, not Worker memory.

| Platform/input | Warm before → after (s) | Allocated before → after (MiB/call) | Peak before → after (MiB) |
| --- | ---: | ---: | ---: |
| Linux R03 |5.310 →3.624|1935.6 →1357.2|461.53 →458.66|
| Linux CORE STEEL |2.958 →2.073|1109.6 →799.2|314.79 →318.60|
| Linux CORE BAS |1.976 →1.477|733.6 →528.9|226.08 →230.07|
| Linux CAVITY BASE |1.397 →0.982|541.4 →389.3|185.38 →185.04|
| Linux CAVITY STEEL |1.418 →1.026|552.9 →397.2|188.77 →186.89|
| Windows CI304 R03 |6.146 →4.269|1935.4 →1357.2|506.75 →489.77|

R03 paired warm median fell31.7% on Linux and30.5% in Windows CI; allocation
fell29.9%. Isolated current target warm median is3.244s Linux /3.385s Windows.
First-invocation maximum R03 capture:7.016 →5.283s Linux;7.518 →5.538s Windows.
First invocation still exceeds the aspirational5s ordinary-file target. R03 retained
capture heap remains about63MiB; every released sample was collected.

Memory reduction is NOT general: CORE BAS and CORE STEEL process peaks increase
by about4MiB, despite substantially lower allocation. Per-capture reference arrays
trade temporary lifetime storage for fewer repeated traversals. Linux R03 first-
call peak also increased although all-sample lifetime peak decreased. Peak values
must not be subtracted as attributable target RAM. No robust410MiB/512MiB acceptance
is established; actual Worker/private Windows validation remains outstanding.

Windows CI304 on exact source commit139efdf227cc9ed3d79644961f9a41eb0f3dc414:
https://github.com/dengwensha/MPC-Platform/actions/runs/45445275044
Release/regression2248/2248, Worker acceptance5/5, public9 paired measurement
54 fresh normal processes/216 timed calls/108 warmups, plus four unavailable
controls PASS. Outcomes7 bound/2 blocked; every before/after target projection and
released-object check passed. Full artifact downloaded and ZIP digest verified;
Windows JSON stored with CRLF→LF normalization only, original/stored hashes retained.

Evidence folders: B6-Target-Allocation-Linux and B6-Target-Allocation-Windows under
Tooling/evidence; Validation.json records source/DLL/tool hashes and scope. The final
review head adds this evidence and a stricter33/public-control audit comparator
without changing measured production code. Its exact-head CI is recorded in PR143.
This is a work/allocation improvement slice; #141 acceptance/enforcement is not closed.
