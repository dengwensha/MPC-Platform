# PERF-13 — Post-Inventory Bottleneck Verification

## Classification

`[RE-MEASURE]`

## Objective

Verify the parser bottleneck distribution after the PERF-12
single-pass `StepDataInventory` construction optimization.

This sprint also evaluates whether the current STEP parsing performance
optimization phase should continue.

No production code is changed.

## Traceability

- Branch: `main`
- Start Commit: `e37c15ff15a24bf55ed66fb8287fc6bcd1500483`
- Reviewed HEAD: `e37c15ff15a24bf55ed66fb8287fc6bcd1500483`
- Closing Commit: To be resolved from the commit containing this report
- Review Date: 2026-08-16

## Profile Scope

The following golden STEP files were profiled:

- `T001_PRISM.step`
- `T103_MULTIPLE_ISLANDS.step`

The profiled benchmark was:

`MPC.Benchmarks.StepDataParsingBenchmarks.ParseDataInventory`

Profiling used BenchmarkDotNet EventPipe with Release binaries.

## Inventory Constructor Shift

| File | Before | After | Relative Share Change |
|---|---:|---:|---:|
| T001_PRISM.step | 8.27% | 6.86% | -17.05% |
| T103_MULTIPLE_ISLANDS.step | 8.41% | 7.04% | -16.29% |

Percentages are inclusive values relative to
`StepDataEntityReader.Read`.

The constructor share decreased for both profiled files.

This result is consistent with the isolated PERF-12 benchmark, where
all five inventory inputs improved in time and managed allocation.

## Current Parser Attribution

| Method | T001_PRISM | T103_MULTIPLE_ISLANDS |
|---|---:|---:|
| `StepDataInventoryValidator.Validate` | 21.67% | 27.77% |
| `StepDataEntityReader.CreateEntityRecord` | 19.44% | 30.43% |
| `AddMissingEntityReferenceDiagnostics` | 9.90% | 13.40% |
| `StepDataEntityReader.SplitStatements` | 9.52% | 9.01% |
| `TryParseNumericEntityId` | 5.50% | 8.34% |
| `ExtractReferencedEntityIds` | 4.36% | 8.22% |
| `StepDataInventory` constructor | 6.86% | 7.04% |
| `AddDuplicateSourceEntityIdDiagnostics` | 1.30% | 2.04% |
| `StepDataEntityRecord.MaterializeStrings` | 1.15% | 1.34% |
| `StepDataEntityRecord` constructor | 1.08% | 0.92% |

Machine-readable evidence is stored in:

- `post-change-attribution.csv`;
- `inventory-bottleneck-shift.csv`.

## PERF-12 Confirmation

PERF-12 isolated benchmark improvements were:

- timing: 17.07% to 48.23%;
- managed allocation: 56.11% to 70.07%.

PERF-13 confirms that the optimized constructor also occupies a smaller
share of the full parsing pipeline.

## Performance Series Outcomes

The completed performance series established:

1. a repeatable BenchmarkDotNet harness;
2. baseline measurements for five golden STEP inputs;
3. EventPipe bottleneck attribution;
4. reduced missing-reference validation materialization;
5. reduced entity record materialization;
6. span-based numeric entity ID parsing;
7. single-pass inventory construction;
8. dedicated behavioral tests for each optimized contract;
9. post-change profiling after every optimization.

The optimizations were selected from measured evidence rather than
speculative architecture changes.

## Current Bottleneck Shape

The parser no longer has one dominant local allocation hotspot.

The remaining cost is distributed across:

- entity record parsing;
- inventory validation;
- missing-reference validation;
- statement splitting;
- reference extraction;
- inventory construction.

`CreateEntityRecord` and `Validate` are broad orchestration regions.
Their remaining costs are composed of several smaller operations.

Further local optimization based only on the current profile would have
a lower expected return and a higher risk of increasing parser
complexity.

## Stop Decision

The current STEP parsing optimization phase should stop after PERF-13.

This decision follows the governing sequence:

`Minimum Working System → Measure → Identify Bottleneck → Optimize → Re-measure`

The phase has completed multiple full cycles and reached a distributed
cost profile.

No additional optimization is justified until one of the following
occurs:

- larger representative STEP files expose a new dominant bottleneck;
- production workloads exceed an agreed latency or allocation budget;
- a newly added parser feature changes the profile;
- regression benchmarks show a material loss;
- a stable application-level method becomes dominant across repeated
  profiles.

## Explicit Non-Decisions

The current evidence does not justify:

- caching parsed IDs;
- cross-file retained state;
- parallel parsing;
- spatial indexing;
- parser-wide rewriting;
- regex replacement;
- mandatory healing or sewing;
- topology mutation during import.

## Validation State

At the start of PERF-13:

- solution build was successful;
- full regression contained 959 passing tests;
- PERF-12 contract and golden inventory tests were green;
- remote and local `main` were synchronized.

Final build and full regression are required after the PERF-13 result
files are added.

## Acceptance Decision

PERF-13 is accepted as:

- a successful post-optimization re-measurement;
- confirmation of the PERF-12 bottleneck reduction;
- the formal stop point for the current parsing optimization phase.

Future performance work should begin with a new measurement sprint and
fresh representative workload evidence.

## Out of Scope

PERF-13 introduces no changes to:

- production code;
- benchmark implementation;
- validation behavior;
- parsing behavior;
- semantic handlers;
- mapping;
- topology;
- caching;
- parallelization;
- healing or sewing.
