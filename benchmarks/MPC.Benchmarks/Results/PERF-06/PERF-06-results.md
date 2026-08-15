# PERF-06 — StepDataEntityRecord Collection Materialization

## Classification

`[OPTIMIZE]`

## Objective

Reduce collection materialization cost in
`StepDataEntityRecord` while preserving constructor validation,
defensive immutability, normalization, and deterministic behavior.

## Traceability

- Branch: `main`
- Start Commit: `c6a208005d8cdbb4b4b76846872498e150acbdc3`
- Reviewed HEAD: `c6a208005d8cdbb4b4b76846872498e150acbdc3`
- Closing Commit: To be resolved from the commit containing this report
- Review Date: 2026-08-16

## Measured Bottleneck

PERF-05 attributed approximately 19–22% of
`StepDataEntityReader.CreateEntityRecord` CPU time to the
`StepDataEntityRecord` constructor.

Source inspection identified repeated collection processing:

- optional collections created new empty lists;
- component types used `ToList`, `Any`, `Select`, and another `ToList`;
- referenced IDs used the same multi-pass pattern;
- diagnostics were copied and then scanned separately;
- source entity type normalization was repeated.

## Contract Hardening

A dedicated `StepDataEntityRecordTests` suite was added before the
production optimization.

The 15 tests cover:

- scalar normalization;
- empty optional collections;
- component type normalization;
- referenced entity ID normalization;
- defensive copying of all input collections;
- invalid component entries;
- invalid reference entries;
- null diagnostic rejection;
- diagnostic/status consistency;
- complex entity consistency;
- undefined read status rejection.

The tests passed against the pre-change implementation before the
optimization was applied.

## Implementation

The constructor now:

- uses shared empty read-only collections for absent or empty values;
- validates and normalizes strings in one enumeration;
- materializes diagnostics in one enumeration;
- preallocates list capacity when the input exposes a collection count;
- avoids LINQ iterator and repeated `ToList` pipelines;
- normalizes `SourceEntityType` once;
- scans diagnostics without LINQ allocation;
- preserves read-only wrappers for non-empty materialized collections.

## Benchmark Method

Both benchmark runs used:

- BenchmarkDotNet 0.15.8;
- .NET 8.0.28;
- .NET SDK 8.0.422;
- Release configuration;
- identical machine and benchmark project;
- the five existing STEP parsing benchmark inputs.

The working changes were stashed for the pre-change run and restored
for the post-change run.

## Results

| File | Pre Mean | Post Mean | Time Improvement | Pre Allocated | Post Allocated | Allocation Improvement |
|---|---:|---:|---:|---:|---:|---:|
| T001_PRISM | 686.9 us | 561.5 us | 18.26% | 575.94 KB | 511.53 KB | 11.18% |
| T002_SPHERE | 343.1 us | 298.2 us | 13.09% | 249.91 KB | 224.13 KB | 10.32% |
| T003_CYLINDER | 497.4 us | 427.6 us | 14.03% | 406.24 KB | 363.20 KB | 10.59% |
| T101_BASIC_POCKET | 1082.8 us | 894.7 us | 17.37% | 997.49 KB | 884.00 KB | 11.38% |
| T103_MULTIPLE_ISLANDS | 2407.3 us | 2010.1 us | 16.50% | 2243.81 KB | 1983.29 KB | 11.61% |

Machine-readable data is stored in:

- `pre-change.csv`;
- `post-change.csv`;
- `comparison.csv`.

## Validation

- Targeted constructor tests: 15 passed
- Full regression: 942 passed
- Build: 0 warnings, 0 errors
- `git diff --check`: clean

Final validation must be repeated after the result files are added.

## Acceptance Decision

The optimization is accepted.

All five representative STEP files improved in both execution time and
managed allocation. No benchmark input regressed.

Observed improvements:

- execution time: 13.09% to 18.26%;
- managed allocation: 10.32% to 11.61%.

The result confirms that collection materialization was a meaningful
parser cost and that the optimization addressed it without expanding
the parser architecture.

## Preserved Behavior

The following behavior remains unchanged:

- defensive copying of caller-owned collections;
- read-only public collection exposure;
- source entity type trim and uppercase normalization;
- component type trim and uppercase normalization;
- referenced entity ID trim normalization;
- diagnostic/status consistency;
- complex entity consistency;
- exception types for invalid inputs;
- deterministic collection order.

## Out of Scope

PERF-06 does not change:

- regex parsing;
- statement splitting;
- reference extraction rules;
- semantic handlers;
- inventory validation;
- caching;
- parallelization;
- topology construction;
- healing or sewing.
