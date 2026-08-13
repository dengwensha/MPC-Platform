# PERF-01 — Baseline Measurement & Benchmark Harness

using BenchmarkDotNet.Environments;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using System;
using System.Runtime.InteropServices;

Classification: `[MEASURE]`

## Traceability

-Start Commit: `0a6e688948ebc93afeed6d586befc42ff07da60d`
- Reviewed HEAD: `0a6e688948ebc93afeed6d586befc42ff07da60d`
- Closing Commit: Pending
- Branch: `main`
- Review Date: `2026 - 08 - 14`
-Working Tree at Start: Clean
- Starting Regression: `927 / 927`
-Closing Regression: `927 / 927`

## Measurement Principle

Minimum Working System → Measure → Identify Bottleneck → Optimize → Re-measure.

PERF-01 contains no production optimization, parallelization, caching,
spatial indexing, healing, sewing, coincidence search, or topology mutation
outside the existing production pipeline.

## Environment

- OS: Windows 10 22H2
- CPU: Intel Core i5 - 7200U, 2 physical / 4 logical cores
- .NET SDK: 8.0.422
- Runtime: .NET 8.0.28
- JIT: X64 RyuJIT x86 - 64 - v3
- GC: Concurrent Workstation
-BenchmarkDotNet: 0.15.8
- Build Configuration: Release

## Canonical Pipeline Baseline

Input: `T001_PRISM.step`, 13,446 bytes, 187 DATA entities,
154 semantic entities.

| Stage | Mean | Error | Allocation |
|---|---:| ---:| ---:|
| Semantic entity reading | 254.20 us | 4.991 us | 235.98 KB |
| Canonical solid mapping | 371.50 us | 38.174 us | 44.87 KB |
| Canonical topology assembly | 94.21 us | 4.678 us | 33.51 KB |
| End-to-end import | 1.353 ms | 24.219 us | 1,028.34 KB |

The end-to-end result and semantic result are accepted as reliable baseline
measurements. Mapping and assembly are isolated diagnostic measurements.
They use one invocation per iteration to prevent canonical-cache reuse and
topology-state reuse.

The mapping result has a 10.28% confidence margin and multimodal distribution.
It is directional evidence and must not independently justify optimization.

## DATA Parsing Baseline

| Input | Bytes | Mean | Error | Allocation |
|---|---:| ---:| ---:| ---:|
| T002_SPHERE.step | 5,890 | 410.7 us | 8.18 us | 299.01 KB |
| T003_CYLINDER.step | 9,412 | 587.2 us | 11.46 us | 488.60 KB |
| T001_PRISM.step | 13,446 | 821.6 us | 14.66 us | 700.61 KB |
| T101_BASIC_POCKET.step | 23,188 | 1.308 ms | 25.92 us | 1,217.65 KB |
| T103_MULTIPLE_ISLANDS.step | 52,511 | 2.963 ms | 58.51 us | 2,750.42 KB |

Parsing time and managed allocation increase approximately with input size.
No super-linear growth is demonstrated by the current golden dataset.

## Bottleneck Evidence

For `T001_PRISM.step`:

-DATA parsing: 821.6 us
- End - to - end import: 1.353 ms
- Parsing allocation: 700.61 KB
- End - to - end allocation: 1,028.34 KB

DATA parsing represents approximately 61% of the independently measured
end-to-end duration and approximately 68% of managed allocation.

`StepDataEntityReader.Read()` is therefore the first evidence-backed
bottleneck candidate for further profiling. This does not yet identify the
internal cause.File reading, statement parsing, record construction, and
inventory validation must be separated by measurement before optimization.

## Validation

- Benchmark project Release build: Passed, 0 warnings, 0 errors
- Benchmark smoke test: Passed
- Canonical benchmark run: Passed
- Five - file parsing benchmark run: Passed
- Full solution Release build: Passed, 0 warnings, 0 errors
- Full regression: Passed, 927/927
- Production source changes: None