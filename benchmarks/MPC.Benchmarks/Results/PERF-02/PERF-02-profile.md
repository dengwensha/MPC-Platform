# PERF-02 — STEP DATA Parsing Cost Attribution

Classification: `[PROFILE]`

## Traceability

- Start Commit: `3c7bbfd792c1d108f8edcb6105b330205baf7460`
- Reviewed HEAD: `3c7bbfd792c1d108f8edcb6105b330205baf7460`
- Closing Commit: Pending
- Branch: `main`
- Review Date: `2026-08-16`
- Working Tree at Start: Clean

## Objective

Attribute the CPU cost inside `StepDataEntityReader.Read()` without
optimizing or changing production code.

## Method

- BenchmarkDotNet 0.15.8
- EventPipe CPU-sampling profiler
- Release configuration
- .NET 8.0.28
- Inputs:
  - `T001_PRISM.step`
  - `T103_MULTIPLE_ISLANDS.step`
- Trace formats:
  - `.nettrace`
  - `.speedscope.json`
- Attribution reference:
  - Inclusive duration of `StepDataEntityReader.Read()` = 100%

Private production methods were resolved directly from EventPipe symbols.
No reflection, copied parser algorithm, public API expansion, or production
measurement seam was used.

Profiler runs are attribution runs and do not replace the PERF-01 timing
baseline because profiling adds execution overhead.

## CPU Attribution

| Stage | T001 | T103 |
|---|---:|---:|
| Entity record creation | 32.35% | 39.73% |
| Inventory validation | 30.63% | 39.25% |
| Missing-reference validation | 24.01% | 30.09% |
| Select/ToArray materialization | 21.55% | 26.87% |
| Entity-record constructor | 9.08% | 11.18% |
| File ReadAllText | 15.08% | 6.85% |
| File.Exists | 7.68% | 2.68% |
| Statement splitting | 7.37% | 5.26% |
| Inventory/type-count construction | 4.17% | 4.16% |
| Regex matching | 2.30% | 1.93% |
| Duplicate-ID validation | 1.44% | 1.75% |
| Referenced-ID extraction | 0.52% | 0.89% |
| DATA-section extraction | 0.29% | 0.29% |

Inclusive percentages represent nested call-tree costs and must not be
summed.

## Findings

Two co-dominant parser cost centers were reproduced across both inputs:

1. Entity-record creation
2. Inventory validation

The strongest individual internal cost is missing-reference validation,
particularly its per-entity LINQ `Select(...).ToArray()` materialization
chain.

As input size increases, record creation and validation take a larger
relative share, while file I/O takes a smaller relative share.

The evidence does not support prioritizing regex matching, statement
splitting, DATA-section extraction, duplicate-ID validation, or inventory
type-count construction.

## PERF-01 Baseline Preservation

The accepted PERF-01 values remain unchanged:

- T001 DATA parsing: 821.6 us, 700.61 KB
- T103 DATA parsing: 2.963 ms, 2,750.42 KB

PERF-02 profiled means are not replacement baselines.

## Scope Compliance

- Production optimization: None
- Production source changes: None
- Parallelization: None
- Cache or pooling: None
- Parser API changes: None
- Reflection: None
- Algorithm duplication: None

## Recommended Next Step

Before optimization, define a narrowly scoped measurement-backed change
around missing-reference validation materialization and entity-record
normalization. Any change must preserve deterministic diagnostics,
reference semantics, entity ordering, and full regression behavior, then
be re-measured against the PERF-01 baseline.
