# PERF-12 — Single-Pass StepDataInventory Construction

## Classification

`[OPTIMIZE]`

## Objective

Reduce `StepDataInventory` construction time and allocation by
combining defensive copying, null validation, and type counting into a
single enumeration.

## Traceability

- Branch: `main`
- Start Commit: `56d956f0ddc1eabf7c3aa640c6ec268711dee70d`
- Reviewed HEAD: `56d956f0ddc1eabf7c3aa640c6ec268711dee70d`
- Closing Commit: To be resolved from the commit containing this report
- Review Date: 2026-08-16

## Measured Basis

PERF-10 attributed approximately 8% of total parser CPU time to the
`StepDataInventory` constructor for both T001 and T103.

PERF-11 added an isolated benchmark that measures only:

    new StepDataInventory(
        entities);

The baseline confirmed size-dependent constructor cost and allocation.

## Contract Hardening

A dedicated `StepDataInventoryTests` suite was added before production
code was modified.

The 11 test cases cover:

- null enumerable rejection;
- null entity rejection;
- empty inventory behavior;
- original entity order;
- defensive copying of caller-owned collections;
- total entity count;
- entity type aggregation;
- case-insensitive and trimmed `GetCount`;
- zero count for unknown types;
- rejection of null, empty, and whitespace count queries.

The tests passed against the pre-change implementation.

Golden inventory tests also remained green.

## Previous Implementation

The previous constructor performed:

1. `entities.ToList`;
2. a separate `Any` pass for null validation;
3. `AsReadOnly`;
4. `GroupBy` using a case-insensitive comparer;
5. group enumeration using `Count`;
6. `ToDictionary`.

This required multiple passes and temporary grouping structures.

## New Implementation

The constructor now:

1. creates a destination list with known capacity when available;
2. creates a case-insensitive count dictionary;
3. enumerates the source exactly once;
4. rejects null entities during enumeration;
5. appends each entity in original order;
6. increments the corresponding type count;
7. exposes the copied entity list through `AsReadOnly`.

The implementation preserves defensive copying and deterministic entity
order.

No cache or retained external collection reference was introduced.

## Isolated Benchmark Results

| File | Pre Mean | Post Mean | Time Improvement | Pre Allocated | Post Allocated | Allocation Improvement |
|---|---:|---:|---:|---:|---:|---:|
| T001_PRISM | 20.673 us | 13.463 us | 34.88% | 12.16 KB | 3.64 KB | 70.07% |
| T002_SPHERE | 9.915 us | 5.133 us | 48.23% | 8.38 KB | 2.77 KB | 66.95% |
| T003_CYLINDER | 14.472 us | 8.960 us | 38.09% | 12.85 KB | 5.64 KB | 56.11% |
| T101_BASIC_POCKET | 32.395 us | 23.215 us | 28.34% | 19.12 KB | 7.23 KB | 62.19% |
| T103_MULTIPLE_ISLANDS | 69.401 us | 57.553 us | 17.07% | 29.34 KB | 10.56 KB | 64.01% |

## Timing Result

All five benchmark inputs improved.

Observed timing improvements range from:

- 17.07% for T103;
- to 48.23% for T002.

The improvement remains measurable as inventory size increases.

## Allocation Result

All five benchmark inputs reduced managed allocation.

Observed allocation improvements range from:

- 56.11% for T003;
- to 70.07% for T001.

T103 allocation decreased from 29.34 KB to 10.56 KB per inventory
construction.

## Behavior Preservation

The following behavior remains unchanged:

- null enumerable rejection;
- null entity rejection;
- caller collection isolation;
- entity identity preservation;
- entity order preservation;
- total entity count;
- case-insensitive type aggregation;
- trimmed and case-insensitive `GetCount`;
- zero for unknown types;
- deterministic type counts.

## Validation

Before optimization:

- dedicated inventory tests: 11 passed;
- golden inventory tests: 10 passed.

After optimization:

- dedicated inventory tests: 11 passed;
- golden inventory tests: 10 passed.

Final solution build and full regression are required after result files
are added.

## Acceptance Decision

PERF-12 is accepted.

The single-pass implementation:

- improves time on every isolated benchmark input;
- reduces allocation on every input;
- removes grouping and LINQ materialization structures;
- preserves the public inventory contract;
- introduces no architectural expansion.

## Stored Evidence

The following files are included:

- `pre-change.csv`;
- `post-change.csv`;
- `comparison.csv`.

The isolated benchmark remains available as:

`StepDataInventoryConstructionBenchmarks`.

## Out of Scope

PERF-12 does not change:

- STEP parsing;
- inventory validation;
- entity record construction;
- semantic handlers;
- mapping;
- caching;
- parallelization;
- topology construction;
- healing or sewing.
