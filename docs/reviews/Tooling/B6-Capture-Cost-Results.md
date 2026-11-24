# B6 — isolated capture measurement and budget proposal

Date: 2026-11-24 Europe/Istanbul. Source/measurement protocol:
[B6-Capture-Cost-Protocol.md](B6-Capture-Cost-Protocol.md).
Owner workstation: Windows + SOLIDWORKS, 16 GB RAM (owner confirmation).
This document proposes an engineering budget; it does not enact one.

## Verified Linux result

Baseline: 264b16e548d77fcf1d2365670b7f97803fa3450c.
Current production: f55bbb6422ef8149503c6fb3d8ef0afe8da1487e.
Harness equivalent-source commit: bfc6d635838d1ce889d939596b0b9af30e06973c.
SDK 8.0.422, Release. No production sources or tests changed.

20 identical approved input hashes / 444,497 records; two process trials for each
of three units: baseline capture, current capture and current isolated target.
120 fresh normal processes, 480 timed calls and 240 warmups. Four separate failure
processes add 24 expected-unavailable calls. Every semantic repeat, baseline/current
common observation and owned/neutral target-data comparison matched. Every released
session/data object was collected. Current target outcomes remain 17 bound/3 blocked.

Complete per-call records, hashes, source/DLL provenance and methods are retained in
[evidence/B6-Capture-Cost-Linux/Validation.json](evidence/B6-Capture-Cost-Linux/Validation.json)
and [Summary.json](evidence/B6-Capture-Cost-Linux/Summary.json). Full-result maxima
below are observed samples, never p95/p99 estimates from only six warm samples.

| Input | Baseline capture warm median | Current capture warm median | Isolated target warm median | Current first-call max | Current all-sample process peak |
| --- | --- | --- | --- | --- | --- |
| R03 | 0.766 s | 5.426 s | 4.586 s | 6.622 s | 461.5 MiB |
| CORE STEEL | 0.422 s | 2.908 s | 2.480 s | 3.904 s | 315.7 MiB |
| CORE BAS | 0.278 s | 1.950 s | 1.703 s | 2.974 s | 226.1 MiB |
| CAVITY BASE | 0.251 s | 1.362 s | 1.199 s | 2.515 s | 185.3 MiB |
| CAVITY STEEL | 0.223 s | 1.426 s | 1.184 s | 2.505 s | 188.5 MiB |
| BACK PLATE original | 0.253 s | 1.305 s | 1.118 s | 2.273 s | 181.6 MiB |
| R01 | 0.0028 s | 0.0196 s | 0.0162 s | 0.150 s | 58.4 MiB |
| R02 | 0.0048 s | 0.0442 s | 0.0413 s | 0.141 s | 62.0 MiB |

R03's current warm allocation is about 1,935.6 MiB per capture, while maximum
retained heap delta with the result alive is about 62.9 MiB. CORE STEEL is about
1,109.6 MiB allocated and 37.0 MiB retained. High temporary allocation/GC work is
therefore material, even though retained sessions do not show a leak in this test.
Allocated volume must not be described as resident RAM required at once.

The empirical R03 added capture median is about 4.66 s; the separately timed target
method is about 4.59 s. That supports prioritizing target work before optional
provenance hash caching, but it is not a profiler-level allocation/CPU attribution
or an optimization result. No cache, lazy execution or policy shortcut is applied.

## Interpretation limits and Windows gate

The timer excludes a second independent audit read and JSON report work. It includes
production's own direct R-U precheck and all production parsing normally reached.
First call follows assembly/delegate binding and a file preflight hash; it excludes
startup and cold disk-cache behavior. Full GC is forced around calls, outside timing.
Peak is whole-process lifetime peak, including setup/warmup/earlier projections;
isolated target peak additionally includes its prepared inventory. It is not valid
to subtract the two peaks and call that attributable target RAM.

The measurements came from a shared Linux environment, not the owner's 16-GB PC.
Windows CI repeats the same paired protocol on nine public inputs. The final-head
CI result and measured Windows table are recorded in the PR; the private20 Windows
repeat remains a first-consumer acceptance item. CI has semantic/identity/lifecycle
assertions, not noisy elapsed/RSS pass thresholds.

## Verified public Windows result

Windows CI #301, head `45f6f7a0ceae3ec5ce3b27c826bb7261f0193bdf`, passed full
2248/2248 regression and Worker5/5. The paired B6 protocol passed on nine public
inputs: 54 fresh normal processes, 216 timed calls and 108 warmups, plus four
expected-unavailable control processes. Identity/semantic/lifecycle checks passed.
Full snapshots and the verified artifact digest are retained in
[Windows Validation.json](evidence/B6-Capture-Cost-Windows/Validation.json).

| Windows input | Baseline warm median | Current warm median | Isolated target warm median | Current first-call max | All-sample process peak |
| --- | --- | --- | --- | --- | --- |
| R03 | 0.702 s | 4.873 s | 4.259 s | 6.100 s | 505.4 MiB |
| R01 | 0.0021 s | 0.0316 s | 0.0303 s | 0.147 s | 43.3 MiB |
| R02 | 0.0036 s | 0.0588 s | 0.0625 s | 0.183 s | 55.6 MiB |

R03 has only about 6.6 MiB observed space below a 512-MiB resident-memory target.
That is NOT robust headroom or an accepted operating envelope. Do not raise the
target automatically: measure the owner's private corpus and actual 16-GB machine,
then review work/allocation improvement and enforceable limits before a consumer.
More installed RAM alone does not establish equivalent GC/OS/process behavior.
The independently timed target median is not required to be less than the paired
capture median for every tiny file: distinct processes/JIT/GC add measurement noise.

## Proposed first-consumer budget — awaiting owner acceptance and enforcement

| Item | Proposal | Meaning / next design requirement |
| --- | --- | --- |
| Concurrency | One capture/target operation per worker at a time | The benchmark was serial; it gives no concurrent CAD workload guarantee. |
| Interactive target | At most 5 s for ordinary admitted files | Observed largest admitted file is below this in Linux first-call samples; show progress rather than promise instant import. |
| Capture acceptance ceiling | 10 s per file for the defined supported input envelope | Proposed phase budget; not a new implemented timer. Existing Host whole-command timeout is 30 s and is unchanged. |
| Resident-memory engineering target | 512 MiB process peak per capture worker | Compared with observed complete process peaks; not allocated bytes or private-commit limit. Do not silently raise it to make a platform pass. |
| Before consumer merge | Owner accepts values; private Windows corpus confirms envelope; reviewed enforcement/failure design exists | Define file/record/work bounds and how excess terminates/rejects without success; a post-call stopwatch alone is not a hard limit. |

These are provisional proposals for a 16-GB development workstation. The current
Windows result is too close to the memory target to approve headroom. Windows results
cannot authorize a larger cap automatically. Worst-case/adversarial inputs, machine pressure, storage
latency and the remaining consumer phases have not been bounded by this corpus.
No source-size limit, OS/job memory cap, cancellation API or new result code is
introduced in this measurement PR. The choice of enforceable resource metric and
mechanism requires a separate consumer-boundary design review.

## Review / completion decisions

1. Is the measurement separation and paired baseline adequate, with explicit GC,
   first-call, lifetime peak, shared host and stage-preparation caveats?
2. Do the 20-input identity/semantic/lifecycle comparisons and unavailable controls
   support the reported cost without claiming geometry/eligibility or worst-case bounds?
3. Is the proposed 16-GB workstation budget a reasonable target, subject to Windows
   confirmation and separately reviewed enforcement before first consumer?
4. Can this measurement-only PR merge once final Windows CI is green, while #141
   remains open for budget acceptance, enforcement and private Windows validation?

No runtime trust boundary, wire, policy or rejection behavior changes. The remaining
nonblocking section39 code suggestions are outside this measurement. No performance
closure or first-consumer authorization is claimed merely by delivering this report.
