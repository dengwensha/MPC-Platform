# A two-pass experiment — paired results, no product acceptance

Date2026-11-25 Europe/Istanbul. SeniorSR02 §44. Baseline/main7aebe6d2de57916d76ea2d55aa88937120f5c6e0
(#146,mainCI314 green). Measured code6e560cd02bc726aa721b582a16bf0dd556454b96.
ExperimentPR147 is DRAFT/DO NOT MERGE; production PR remains separate/unopened.
This reporting branch changes no source or tests relative to the measured code.

## Corrected B disposition

§44 supersedes the original allocation-based interpretation: B is rejected for
NO MEANINGFUL NATURAL PROCESS-PEAK REDUCTION despite43% controlled-heap reduction.
One-time39–64KB table allocation (~0.004%) and noisy medians are informational,
not the deciding rejection. Historical numbers/archives are unchanged.

## Exact semantics and traceability

Linux:20 corpus +13 established controls +5 downstream negatives,2process trials,
all before/after AND repeated complete audit projections equal.17 corpus positives
remain17. Actual-capture protocol:120 fresh processes,480 measured +240 warmup
calls;4 expected-unavailable controls. All input hashes/DLL stability/lifetimes pass.
WindowsCI316:public9 +13+5 only,2trials, all27 audit cases equal;54 fresh cost
processes,216 measured +108 warmups,4 unavailable controls. Full regression2253/2253,
Worker5/5,source guard and12-process CLR profile+3negatives pass. Artifact digest,
explicit review head and producer source pin were independently checked. Private
Windows20 and owner16GB evidence are NOT supplied by this CI run.

## R03 natural process peak — decisive criterion passes in observed scopes

MB below means1,000,000bytes. Bounds evaluated on bytes/ratios, per trial, using
max lifetime process peak across measured calls (includes setup/warmups). These
are capture-probe processes, not a real Worker budget test.

|Platform/trial|Before MB|A MB|Reduction|15% bound|
|---|---:|---:|---:|---|
|Linux1|473.37|356.20|24.75%|Pass|
|Linux2|479.16|342.46|28.53%|Pass|
|Windows public1|531.15|381.02|28.27%|Pass|
|Windows public2|531.80|380.70|28.41%|Pass|

## Time and allocation tradeoff — OWNER DECISION, not automatic rejection

R03 warm allocated bytes rise from≈1.423GB to≈1.926GB, **+35.36%**, exceeding30%.
Linux8/20 files exceed30%:2_S26352_0300_05_2210,BACK PLATE,BACK_PLATE_ORIGINAL,
CAVITY BASE,CAVITY STEEL,CORE BAS,CORE STEEL,R03 (≈32.37–36.75%). Windows public
R03 exceeds30%; the other8 do not. Report all per-file/trial values in Assessment.
This is total allocation over a call, separately from simultaneous resident peak.

All observed warm/first time bounds pass. Linux R03 per-trial warm medians:
4008.59/3732.72ms before vs3797.92/3701.15ms A. Median delta121.11ms is smaller
than275.87ms inter-trial variation: **no measured time change** per§44.
Windows public medians4116.76/3813.68→3620.10/3693.51ms;308.42ms delta is only
slightly larger than303.08ms noise. Do not generalize a guaranteed speedup.
R03 first calls A:Linux5066/4816ms;Windows5111/5095ms, below10s. Retained-heap
comparisons pass±1MiB throughout (R03 about28KB incremental retained metadata).

Startup proxy=max(0,first allocation minus same-process warm median), not exclusive
static initialization. Both absolute and incremental proxies are disclosed.
A's incremental startup proxy max is69,216B on Linux; Windows roughly37–62KB.
Absolute proxy exceeds1MiB in existing Festo/Linux and baseline/updated Windows
cases; whether1MiB means TOTAL or A INCREMENT was explicitly asked and remains
unanswered. No automatic approval is inferred from an assumed interpretation.

## Disposition and next step

Peak gate passes both observed trials/platform scopes; warm allocation exceeds.
§44 therefore requires an OWNER TRADEOFF decision, not rejection/acceptance by this
agent. Recommendation: provisionally accept the≈35.36% total-allocation cost in
exchange for≈25–28% peak reduction, subject to complete owner16GB Windows20
measurements satisfying peak/time/retained bounds and separate product review.
No budget ceiling is raised;512MiB first-consumer prerequisite remains unchanged.

Windows20 local bootstrap kit is privately delivered to the owner. It contains15
private/reference inputs;5 public goldens are restored byte-for-byte from Git to
avoid LF/CRLF identity differences. Isolated baseline/trial worktrees are pinned;
active checkout/COM registration is untouched. Core PowerShell runner passedCI316;
the local bootstrap/owner machine have not been executed here. Share only generated
Windows-A-Evidence.zip, containing technical JSON, not raw CAD.

## Evidence

Linux and Windows-public subdirectories under evidence/B6-A-Experiment contain
Assessment.json,CaptureSummary.json,Validation.json and full hash-indexed JSON
archives. They include audit comparisons, repeats and every cost sample. No CAD
is in either archive. See README for hashes/extraction. No A production PR or
merge approval is requested by this evidence alone.
