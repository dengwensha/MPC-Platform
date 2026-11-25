# B6 — CLR allocation sampling and controlled live-heap profiling

Date: 2026-11-25 Europe/Istanbul. Class: VERIFY / profiling only.
Start/approved production:9b5f0334001e9d0558e95865bb9001b9ba12cb49 (#143 merge).
Branch:perf/B6-live-heap-profile. Source: Senior SR02 §41 Ö1; follow-up #141.
Owner workstation:Windows/SOLIDWORKS16GB; this is NOT workstation acceptance.

## Goal and boundary

Identify inventory versus target temporary memory contributors before another
optimization. No production src/ or tests/ change; no consumer, policy relaxation,
new authority issuer, lazy production path, resource guard or optimization.
Previous ~500MiB process-lifetime peaks cannot attribute memory to components.

## Three fresh-process observations

1. Inventory: concrete unchanged reader ReadCaptured over private file bytes whose
   SHA256 is checked INSIDE the operation. The result is retained through forced
   GC, then released. This includes its result/diagnostics, first-use runtime caches
   and observer effects; the heap delta is not an exclusive inventory graph size.
2. Canonical: real unchanged CaptureFromFile, existing test hook always null.
   In-process CLR GCAllocationTick events and5ms managed-heap/working-set samples
   observe that operation. The actual captured document hash must match the input.
   The observer window also includes the post-call forced-GC retention sample and
   tooling semantic projection; the final Return segment includes that overhead.
   This is deliberately not the isolated timing/allocation protocol from #142.
3. Diagnostic: a disposable source copy archived from exact9b5f033, with ten
   exact-anchor insertions in InventoryEvidence/TargetProducer/TargetBinding.
   It adds eleven scalar observation checkpoints, including before/after the direct
   A1 precheck. No repository src file is modified. The generator records the full
   replacement text and normalized before/after hashes. The final generator exactly
   reproduced the copy used for measurement. The DLL directory must carry the
   diagnostic marker; modes cannot silently exchange canonical/diagnostic binaries.

The callback exists ONLY in the disposable copy. It observes counts/scalars and
never stores roots, supplies source data or invokes consumers. Its mutable AppDomain
slot is tooling instrumentation, not a production/C1 seam. Generated DLLs and their
objects are not acceptable product evidence; only metadata profiles are emitted.
Original and diagnostic projected document/target decisions, complete ordered reason
fields (including severity, indices and hashed private cause text), IDs and counts
must match across two independent trials. This is projected semantic equivalence,
not assembly-provenance equality or a new eligibility proof.

At each diagnostic checkpoint, forced GC runs while the Check or inventory root is
explicitly kept alive. Counts show rows, incoming-index keys and reasons. The after-
GC heap is a CONTROLLED reachable-heap observation. It is not natural production
liveness, exclusive retained-size/dominator analysis or a reconstruction of actual
Worker peak. The profile changes GC schedule and timing; do not use its duration or
peaks for acceptance. Shared references, captured source bytes, runtime caches and
observer buffers remain in these totals. In particular, subtracting canonical and
inventory heap deltas does NOT measure TargetData size.

Runtime allocation events are SAMPLED. Interval bytes attributed to the triggering
type are weighted estimates, not exact bytes allocated by that type. Trigger-object
size totals cover only sampled objects, not live type populations. Observer/runtime
allocations and delivery delay can affect distributions. The observer cap must not
truncate events; self-test allocates byte arrays and requires native CLR events.
5ms memory sampling can miss peaks; windows with no sample report null, not zero.
GC committed segments, managed heap estimates and resident process memory differ.

## Corpus and checks

Linux: R03 (largest prior memory case), CORE STEEL/CORE BAS (the two documented
small peak regressions), and T001 (small positive control), same historical hashes.
Two independent trials, reverse mode order in trial2, three modes/file:24 fresh
normal processes. Three parser-rejection controls must fail without a zero-memory
success report. A canonical/diagnostic mode mix-up is also rejected. All normal
results require nonempty inventory, same recorded count, positive allocation-event
availability, no observer-cap drops, repeated/canonical-diagnostic semantic equality
and collection of released results. No raw CAD, object addresses, strings from model
values, stacks with values or heap dumps are uploaded. Cause text is hashed only.

Windows CI repeats the exact protocol on public R03 and T001 (12 normal processes
plus3 unavailable controls), alongside existing full regression/Worker/B6 checks.
The private four-file Linux set does not imply private Windows corpus acceptance.
SDK8.0.422 Release/warnings-as-errors; BCL-only profiler, no extra package dependency.
Build/run commands, actual source/tool/DLL/input hashes and snapshots are retained in
Validation.json. Tooling built before its delivery commit is identified by equivalent
source hashes; no later commit is claimed as embedded in measured binaries.

## Interpretation and next gate

A larger post-GC heap following row construction narrows the investigation to the
retained parsed-row structures and associated work in that interval; it is NOT a
precise exclusive object-graph size. Source inspection and sampled allocation types
must corroborate it. Temporary growth during A1 is not a leak if it disappears at
its return/GC boundary, and A1 must not be bypassed to reduce cost.

Any subsequent optimization needs a separate bounded change and identical decision/
reason comparisons. No memory budget,5s/10s acceptance or first-consumer permission
is established. Real Worker/private20 measurements on the owner's16GB Windows
machine, headroom/owner decision and externally enforced failure design remain #141.


## Completed Linux result

Four files, two independent trials,24 fresh normal processes,3 rejected parser
controls. All hashes/counts, repeated and canonical/diagnostic projected semantics,
11-checkpoint order and release/lifecycle checks PASS. Runtime self-test observed65
allocation ticks including byte arrays. Source/test trees remain unchanged.
Retained26 metadata JSON snapshots (24 profiles +summary +overlay) and Validation;
no source geometry or dump published.

Approximate controlled after-GC managed heaps (MiB, NOT resident process peaks).
Two trials agree at the displayed0.1MiB precision except CORE STEEL incoming172.6–172.7.
Inventory column is the isolated reader's rooted delta; other columns are ABSOLUTE
managed heap totals from the different diagnostic process. They are not additive.

| Input | Inventory rooted delta | Target entry | Rows parsed | Incoming built | Before A1 | After A1/GC |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| R03 |63.0|76.8|283.7|299.3|299.4|299.5|
| CORE STEEL |37.0|44.5|161.5|172.6–172.7|172.7|172.8|
| CORE BAS |25.6|30.5|108.2|115.2|115.3|115.3|
| T001 |0.1|0.4|1.0|1.0|1.0|1.0|

R03 row construction corresponds to about207MiB more post-GC heap; incoming-index
construction adds about16MiB. These are interval/controlled-root heap differences,
not precise exclusive graph sizes. The retained Check contains135828 rows and
135777 incoming keys. Its Parts fields hold freshly parsed parameter trees, row
signatures and cached reference arrays; the existing inventory remains reachable.
Sampled row-stage allocation types include strings, parameter arrays/lists and
parser iterator objects. They corroborate the source-level retention investigation,
but do not supply live type populations or exclusive bytes per type.

R03 A1 performs about82MiB main-thread allocation in the instrumented interval. Its
before-forced-GC heap rises to roughly352MiB, then returns to299.5MiB after GC.
This identifies temporary compatibility-check work, not a retained82MiB leak. A1
still calls the original precheck on the same inventory; it must remain mandatory.
Inventory/canonical rooted delta differs by observer buffers/first-use caches as
well as returned data (R03 canonical about64.9MiB versus inventory63.0MiB); do not
label that difference as exact TargetData size.

Conclusion for the NEXT separate change: prioritize the parsed-row graph's lifetime/
representation and overlap with later checks, rather than assuming that the small
reference-cache regression or A1 alone explains the process peak. Any proposal to
release/reduce parameter structures must first establish which later binding,
placement, presentation and ownership checks still need them; preserve forward
references, unknown/partial records and every ordered cause. No such change is
implemented or approved here. Lower post-GC heap does not guarantee lower reserved
GC segments or resident peak; the actual Worker budget remains a later gate.
