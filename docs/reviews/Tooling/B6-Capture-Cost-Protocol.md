# B6 — production capture cost, before consumer integration

Date: 2026-11-24 Europe/Istanbul. Class: VERIFY / measurement only.
Start and current production source: f55bbb6422ef8149503c6fb3d8ef0afe8da1487e.
Baseline production source: 264b16e548d77fcf1d2365670b7f97803fa3450c (before target capture).
Branch: perf/B6-capture-measurement. Owner: Mehmet İlkay Kınacı.
Refs: #141, Senior SR02 section 39. Owner's Windows/SOLIDWORKS machine: 16 GB RAM.

## One goal and scope

Measure the actual CaptureFromFile cost and isolate TargetProducer.Read on prepared
inventory, without the old audit adapter's second independent R-U read or report
serialization inside the timer. Compare the same approved input bytes with the
pre-target capture baseline. No src, production test, C1, parser, contract, reason,
import, wire, Host, Worker or SOLIDWORKS behavior changes. No cache/optimization,
resource guard, consumer or budget enforcement is implemented by these tools.

The previous 21–23-second / 600–700-MB whole-corpus audit is not a production-call
budget: it combined two inventories and JSON output. This protocol supplies the
missing narrower measurements. It does not prove a worst-case execution bound.

## Actual measured units and boundaries

| Unit | Inside timer/allocation counter | Outside |
| --- | --- | --- |
| Baseline CaptureFromFile | Actual approved private capture, including its own IO/hash/provenance, inventory/header/envelope and session binding | Assembly/delegate binding, preflight hash, summary projection and report output |
| Current CaptureFromFile | Same actual entry point, including target data/observation creation and its direct A1 R-U call | Same exclusions; gate sampled after timing only to verify semantics |
| Current isolated TargetProducer.Read | Actual production method, including field/graph checks and obligatory direct R-U | One prepared immutable inventory read from separately captured/hash-checked bytes; this setup remains resident but is not timed |

Target-only output is neutral data, not forged bound authority. The C1 gate is not
fed those neutral results. Capture measurements call the real owner/evidence path.
Reflection/expression binding exists only in the separate BCL-only measurement
executable, with no friend assembly or success-provider seam. The existing test
hook is always null. The output flag excluding independent precheck/JSON means
audit-only second reads and report emission; production's A1 call and embedded
JSON policy work remain inside the production method wherever normally executed.

## Repetition, validity and lifecycle

Two independent process trials per file and unit. Each child records its first
production invocation, two unrecorded warmups and three recorded warm invocations.
First invocation is AFTER assembly/delegate binding and a streaming input hash;
it is not process startup, cold disk cache, full cold JIT or a worst-case claim.
The reverse unit order in trial two reduces simple ordering drift. Calls run
sequentially, not as concurrent workloads.

Explicit full GC occurs before calls and for retained-heap/lifecycle sampling after
calls, outside timing/allocation delta. Real collections inside the production
call remain timed and their generation deltas are recorded. This controls samples,
but does not model arbitrary long-running GC pressure or concurrent CAD use.

Each invocation must preserve semantic fingerprints. Baseline/current common
evidence (hash/status/diagnostics/count) must match; owned and isolated current target
data must match. Normal corpus entries require available nonempty inventory; a
missing observation is not a successful zero-record sample. Missing-file and known
parser-rejection controls separately verify expected unavailable stages and null
counts. Unexpected exceptions or stale output fail the benchmark.

The result/session is kept strongly alive through retained-heap sampling, before
summary projection, then only a weak reference crosses the no-inline call boundary.
After release/full GC every sampled session/data object must be collected. This
checks the measured operation's lifetime, not arbitrary future consumer retention.
Output contains only filenames/hashes/IDs/counts/reasons/metrics; no private CAD,
raw statements, scalar geometry or inner-cause text. Input hash matches the actual
capture or prepared target inventory, and input is checked unchanged after calls.

## What memory numbers mean

- AllocatedBytes is process-wide managed allocation delta during the call, not
  retained memory, working set or a cap. Runtime background work can contribute.
- LiveHeapAfterCall includes uncollected garbage. RetainedHeapDelta is signed
  heap difference after forced GC with the result alive, relative to pre-call heap.
  It can include first-use caches and is not an isolated object-size proof.
- Working set and lifetime peak are sampled before/after the call, before report
  output. They include process/framework/setup. Warm lifetime peaks may include
  earlier samples/projections. Target-only peak also includes prepared inventory.
  Subtracting peaks does NOT establish attributable incremental target memory.
- Paired capture median delta is an empirical before/after difference. It includes
  target capture plus any changed provenance/JIT/GC costs; it is not an exact sum
  or replacement for the separately timed production method.

## Build/platform and reproducibility gates

SDK 8.0.422, Release/warnings-as-errors. Record production source hashes, exact
baseline/current/probe DLL hashes and informational versions, harness source hashes,
commands, input identities, platform/architecture and each child result. The full
approved 20-file corpus runs locally on Linux; Windows CI uses nine public inputs.
Private Windows corpus/real 16-GB workstation acceptance is not inferred from CI.

CI builds the exact pre-target baseline in a second pinned checkout and runs the
same protocol, alongside existing full Release/regression/Worker and semantic
acceptance. No wall-clock or RSS pass threshold is asserted on shared CI runners;
identity/semantic/lifecycle checks are mandatory. A future resource budget requires
owner acceptance and an explicitly reviewed enforcement design before a consumer.

Initial tool smoke: five golden files, 30 fresh processes, 120 timed calls and
60 warmups plus four expected-unavailable controls; identity/semantics/lifecycle
checks passed. The final full-corpus evidence and a measured budget proposal are
retained with the delivery PR. No performance closure or production limit claimed
by an unfinished measurement or by green functional CI alone.

Reviewed/Closing and final-head CI recorded in the delivery PR. B6 measurement,
owner budget acceptance, enforcement and private Windows validation are distinct
completion items; #141 must not be closed by this benchmark alone.
