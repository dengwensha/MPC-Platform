# PERF-07 — Post-Materialization Bottleneck Verification

## Classification

`[RE-MEASURE]`

## Objective

Verify the parser bottleneck distribution after the PERF-06
`StepDataEntityRecord` collection materialization optimization.

This sprint performs profiling and records the bottleneck shift. It
does not modify production code.

## Traceability

- Branch: `main`
- Start Commit: `35d8b8f62031f624197b5ff307406b871346027e`
- Reviewed HEAD: `35d8b8f62031f624197b5ff307406b871346027e`
- Closing Commit: To be resolved from the commit containing this report
- Review Date: 2026-08-16

## Profile Scope

The following golden STEP files were profiled:

- `T001_PRISM.step`
- `T103_MULTIPLE_ISLANDS.step`

The profiled benchmark was:

`MPC.Benchmarks.StepDataParsingBenchmarks.ParseDataInventory`

Profiling used:

- BenchmarkDotNet 0.15.8;
- EventPipe profiler;
- Release configuration;
- .NET 8.0.28;
- .NET SDK 8.0.422.

## Profiling Command

    dotnet run `
        --project .\benchmarks\MPC.Benchmarks\MPC.Benchmarks.csproj `
        -c Release `
        --no-build `
        -- `
        --filter "*T001_PRISM*" "*T103_MULTIPLE_ISLANDS*" `
        --profiler EP

## Current Attribution

All percentages are inclusive values relative to
`StepDataEntityReader.Read`.

| Method | T001_PRISM | T103_MULTIPLE_ISLANDS |
|---|---:|---:|
| `StepDataInventoryValidator.Validate` | 22.99% | 28.36% |
| `StepDataEntityReader.CreateEntityRecord` | 21.95% | 28.26% |
| `StepDataEntityReader.SplitStatements` | 10.63% | 9.33% |
| `AddMissingEntityReferenceDiagnostics` | 9.30% | 11.96% |
| `StepDataInventory` constructor | 8.38% | 8.13% |
| `TryParseNumericEntityId` | 6.65% | 9.47% |
| `ExtractReferencedEntityIds` | 4.78% | 7.72% |
| `AddDuplicateSourceEntityIdDiagnostics` | 1.57% | 2.48% |
| `StepDataEntityRecord.MaterializeStrings` | 1.28% | 1.17% |
| `StepDataEntityRecord` constructor | 1.21% | 0.98% |

Machine-readable attribution is stored in
`post-change-attribution.csv`.

## Bottleneck Shift

| Method | T001 Before | T001 After | T103 Before | T103 After |
|---|---:|---:|---:|---:|
| `CreateEntityRecord` | 40.61% | 21.95% | 50.28% | 28.26% |
| `StepDataEntityRecord` constructor | 8.65% | 1.21% | 10.55% | 0.98% |
| `Validate` | 15.24% | 22.99% | 17.86% | 28.36% |
| `AddMissingEntityReferenceDiagnostics` | 9.00% | 9.30% | 9.36% | 11.96% |

The relative constructor share decreased by:

- approximately 86% for `T001_PRISM.step`;
- approximately 91% for `T103_MULTIPLE_ISLANDS.step`.

The broader `CreateEntityRecord` share decreased by:

- approximately 46% for `T001_PRISM.step`;
- approximately 44% for `T103_MULTIPLE_ISLANDS.step`.

## Interpretation

PERF-06 successfully removed
`StepDataEntityRecord` construction as a parser bottleneck.

The constructor now represents approximately 1% of total parser CPU
attribution for both profiled files. Its remaining
`MaterializeStrings` cost is also close to 1%.

The increased percentage attributed to validation does not by itself
indicate a validation regression. PERF-06 reduced total parser time,
which increases the relative share of unchanged work.

The post-change parser has two co-dominant regions:

- inventory validation;
- entity record creation outside the record constructor.

Within validation, `TryParseNumericEntityId` is now a visible repeated
cost. Missing-reference validation remains measurable but is no longer
the dominant allocation-heavy path observed before PERF-03.

## Optimization Decision

No additional production optimization is made in PERF-07.

The next candidate is a focused measurement of numeric STEP entity ID
parsing inside `StepDataInventoryValidator`.

Any follow-up optimization must first determine:

- how many times each source entity ID is parsed;
- how many times each referenced entity ID is parsed;
- whether numeric IDs are reparsed across validation passes;
- whether a single indexed representation can safely serve duplicate
  and missing-reference checks;
- whether the expected gain justifies additional retained state.

Caching or architectural expansion is not authorized by this result
alone.

## EventPipe Limitation

EventPipe attribution contains synthetic `CPU_TIME` and unmanaged
frames. Inclusive percentages are used only for bottleneck comparison
and do not sum to 100%.

Low-level framework frames are not treated as standalone optimization
evidence unless their application-level parent relationship is
consistent.

## Acceptance Decision

PERF-07 is accepted as a successful re-measurement.

Evidence confirms:

- the targeted constructor bottleneck was removed;
- the result is reproduced on both small and larger golden STEP files;
- the next candidate is identified without speculative implementation;
- no production code was changed in this sprint.

## Out of Scope

PERF-07 does not introduce:

- production code changes;
- numeric ID caching;
- parser redesign;
- regex replacement;
- parallelization;
- topology changes;
- healing or sewing.
