# PERF-10 — Post-Numeric-ID Bottleneck Verification

## Classification

`[RE-MEASURE]`

## Objective

Verify the parser bottleneck distribution after the PERF-09 span-based
numeric STEP entity ID parsing optimization.

This sprint records profiling evidence and does not modify production
code.

## Traceability

- Branch: `main`
- Start Commit: `74ae648b92130ff63f680713081701588ae032bc`
- Reviewed HEAD: `74ae648b92130ff63f680713081701588ae032bc`
- Closing Commit: To be resolved from the commit containing this report
- Review Date: 2026-08-16

## Profile Scope

The following golden STEP files were profiled:

- `T001_PRISM.step`
- `T103_MULTIPLE_ISLANDS.step`

The benchmark was:

`MPC.Benchmarks.StepDataParsingBenchmarks.ParseDataInventory`

Profiling used BenchmarkDotNet EventPipe with Release binaries.

## Profiling Command

    dotnet run `
        --project .\benchmarks\MPC.Benchmarks\MPC.Benchmarks.csproj `
        -c Release `
        --no-build `
        -- `
        --filter "*T001_PRISM*" "*T103_MULTIPLE_ISLANDS*" `
        --profiler EP

## Current Attribution

All values are inclusive percentages relative to
`StepDataEntityReader.Read`.

| Method | T001_PRISM | T103_MULTIPLE_ISLANDS |
|---|---:|---:|
| `CreateEntityRecord` | 21.30% | 30.01% |
| `StepDataInventoryValidator.Validate` | 18.98% | 27.42% |
| `AddMissingEntityReferenceDiagnostics` | 8.79% | 13.00% |
| `StepDataInventory` constructor | 8.27% | 8.41% |
| `SplitStatements` | 7.69% | 8.15% |
| `TryParseNumericEntityId` | 5.48% | 8.68% |
| `ExtractReferencedEntityIds` | 3.86% | 7.81% |
| `AddDuplicateSourceEntityIdDiagnostics` | 1.37% | 2.24% |
| `StepDataEntityRecord.MaterializeStrings` | 1.30% | 1.52% |
| `StepDataEntityRecord` constructor | 0.92% | 0.40% |

Machine-readable values are stored in
`post-change-attribution.csv`.

## Numeric ID Bottleneck Shift

| File | Before | After | Relative Share Change |
|---|---:|---:|---:|
| T001_PRISM.step | 6.65% | 5.48% | -17.59% |
| T103_MULTIPLE_ISLANDS.step | 9.47% | 8.68% | -8.34% |

The numeric ID parsing share decreased for both profiled files.

This result is consistent with the PERF-09 benchmark evidence, which
showed managed allocation reductions on all five benchmark inputs.

## Related Validation Shift

| Method | T001 Before | T001 After | T103 Before | T103 After |
|---|---:|---:|---:|---:|
| `Validate` | 22.99% | 18.98% | 28.36% | 27.42% |
| `AddMissingEntityReferenceDiagnostics` | 9.30% | 8.79% | 11.96% | 13.00% |
| `CreateEntityRecord` | 21.95% | 21.30% | 28.26% | 30.01% |

The total validation share decreased on both files.

The T103 missing-reference and entity-creation percentages increased
slightly as relative shares. These changes are not treated as
regressions because EventPipe percentages are relative to total parser
time and include sampling variation.

## Interpretation

PERF-09 successfully reduced the measured contribution of
`TryParseNumericEntityId`.

Numeric ID parsing remains visible on T103 because that file performs
1651 numeric ID parse calls. However, its allocation-heavy substring
and LINQ paths were removed, and its relative CPU share decreased.

The parser now has several distributed costs rather than one dominant
constructor or numeric-ID bottleneck:

- entity record creation;
- inventory validation;
- missing-reference validation;
- inventory construction;
- statement splitting;
- reference extraction.

## Optimization Decision

No production change is made in PERF-10.

The next optimization must not be selected solely from small
differences between EventPipe percentages.

A subsequent profile sprint should isolate one of the stable
application-level costs:

- `StepDataInventory` construction, approximately 8% on both files; or
- the remaining work inside `CreateEntityRecord`.

The next candidate should be chosen using allocation evidence and a
bounded source inspection before implementation.

## EventPipe Limitations

EventPipe profiles contain synthetic CPU and unmanaged frames.

Inclusive percentages:

- do not sum to 100%;
- are suitable for identifying broad bottleneck shifts;
- are not precise absolute timing measurements;
- may vary between runs as total parser cost changes.

Only reproduced application-level shifts are used for optimization
decisions.

## Acceptance Decision

PERF-10 is accepted as a successful re-measurement.

The evidence confirms:

- numeric ID parsing share decreased for both files;
- validation behavior remains covered by 948 tests;
- PERF-09 produced no reproduced timing regression;
- managed allocation decreased on every benchmark input;
- no further speculative optimization is introduced.

## Stored Evidence

The following files are included:

- `post-change-attribution.csv`;
- `numeric-id-shift.csv`.

## Out of Scope

PERF-10 does not introduce:

- production changes;
- caching;
- parser redesign;
- inventory redesign;
- regex replacement;
- parallelization;
- topology construction changes;
- healing or sewing.
