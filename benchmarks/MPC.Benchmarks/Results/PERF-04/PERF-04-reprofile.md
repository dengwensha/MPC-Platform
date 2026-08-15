# PERF-04 — Post-Optimization Re-profile

Classification: `[RE-MEASURE]`

## Traceability

- Start Commit: `415c06377e9633a365bcbe6eea288fceca5d601b`
- Reviewed HEAD: `415c06377e9633a365bcbe6eea288fceca5d601b`
- Closing Commit: Pending
- Branch: `main`
- Review Date: `2026-08-16`

## Objective

Verify that PERF-03 reduced the profiled missing-reference validation cost
and identify the new STEP DATA parsing bottleneck without applying another
optimization.

## Method

- BenchmarkDotNet 0.15.8
- EventPipe CPU-sampling profiler
- Release configuration
- T001_PRISM.step
- T103_MULTIPLE_ISLANDS.step
- `StepDataEntityReader.Read()` inclusive CPU attribution = 100%

Profiler run duration is not a replacement for the accepted PERF-03
non-profiled timing baseline.

## CPU Redistribution

| Stage | T001 before | T001 after | T103 before | T103 after |
|---|---:|---:|---:|---:|
| Entity record creation | 32.35% | 40.61% | 39.73% | 50.28% |
| Inventory validation | 30.63% | 15.24% | 39.25% | 17.86% |
| Missing-reference validation | 24.01% | 9.00% | 30.09% | 9.36% |
| Entity-record constructor | 9.08% | 8.65% | 11.18% | 10.55% |
| File ReadAllText | 15.08% | 15.96% | 6.85% | 10.72% |
| File.Exists | 7.68% | 8.61% | 2.68% | 4.30% |
| Statement splitting | 7.37% | 9.06% | 5.26% | 6.73% |
| Inventory construction | 4.17% | 7.72% | 4.16% | 7.26% |
| SelectEnumerableIterator.ToArray | 21.55% | Not observed | 26.87% | Not observed |

Inclusive percentages are nested and must not be summed.

## Verification Result

PERF-03 reduced inventory validation by approximately:

- 50.2% relative on T001
- 54.5% relative on T103

Missing-reference validation was reduced by approximately:

- 62.5% relative on T001
- 68.9% relative on T103

The targeted per-reference `Select(...).ToArray()` frame is no longer
observed. Remaining `LargeArrayBuilder` frames originate from other LINQ
paths and are not evidence that the removed chain remains active.

## New Bottleneck Candidate

`StepDataEntityReader.CreateEntityRecord()` is now the dominant profiled
STEP DATA parsing stage:

- T001: 40.61%
- T103: 50.28%

The next performance investigation must profile this method internally
before any further optimization. The current evidence does not justify
optimizing regex matching alone because regex remains a small share of the
overall parser cost.

## Scope Compliance

- New production optimization: None
- Production source changes: None
- Benchmark source changes: None
- Public API changes: None
- Trace files committed: No
- PERF-03 semantic behavior changed: No

## Decision

PERF-03 is confirmed effective. The parsing bottleneck has moved from
missing-reference validation to entity-record creation.
