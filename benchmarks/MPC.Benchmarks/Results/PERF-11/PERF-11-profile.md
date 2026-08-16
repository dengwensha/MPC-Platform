# PERF-11 — StepDataInventory Construction Attribution

## Classification

`[PROFILE]`

## Objective

Isolate and measure the construction cost of `StepDataInventory`
without including STEP file reading, parsing, or validation.

No production code is changed in this sprint.

## Traceability

- Branch: `main`
- Start Commit: `f9d50593d969a20cd6733c514af881fe804ee7cf`
- Reviewed HEAD: `f9d50593d969a20cd6733c514af881fe804ee7cf`
- Closing Commit: To be resolved from the commit containing this report
- Review Date: 2026-08-16

## Motivation

PERF-10 attributed approximately 8% of parser CPU time to the
`StepDataInventory` constructor for both T001 and T103.

The stable percentage across both files justified an isolated
constructor benchmark.

## Benchmark Harness

The following benchmark was added:

`StepDataInventoryConstructionBenchmarks.ConstructInventory`

Global setup:

1. locates the selected golden STEP file;
2. reads and validates the inventory once;
3. retains the resulting entity list.

The measured method performs only:

    return new StepDataInventory(
        _entities);

File reading, statement parsing, entity record construction, and
inventory validation are outside the measured operation.

## Benchmark Environment

- BenchmarkDotNet: 0.15.8
- Runtime: .NET 8.0.28
- SDK: .NET 8.0.422
- Configuration: Release
- Diagnoser: MemoryDiagnoser

## Results

| File | Mean | Allocated |
|---|---:|---:|
| T001_PRISM.step | 20.673 us | 12.16 KB |
| T002_SPHERE.step | 9.915 us | 8.38 KB |
| T003_CYLINDER.step | 14.472 us | 12.85 KB |
| T101_BASIC_POCKET.step | 32.395 us | 19.12 KB |
| T103_MULTIPLE_ISLANDS.step | 69.401 us | 29.34 KB |

Machine-readable evidence is stored in:

- `inventory-construction-baseline.csv`;
- `inventory-construction-summary.csv`.

## Source Attribution

The constructor currently performs these operations:

1. materializes `entities` with `ToList`;
2. scans the materialized list with `Any` to reject null entries;
3. wraps the list with `AsReadOnly`;
4. groups entities by source type using case-insensitive `GroupBy`;
5. enumerates every group to calculate its count;
6. materializes the groups using `ToDictionary`.

This creates multiple enumeration and grouping structures.

## Scaling Observation

The constructor cost increases with inventory size.

The largest profiled input, T103, requires:

- 69.401 us per construction;
- 29.34 KB managed allocation per construction.

The result confirms that the approximately 8% end-to-end profiler
share represents measurable constructor work rather than attribution
noise.

## Optimization Boundary

The next proposed sprint is:

`PERF-12 — Single-Pass StepDataInventory Construction`

Classification:

`[OPTIMIZE]`

The bounded candidate implementation should:

- enumerate the supplied entities once;
- validate null entries during enumeration;
- copy entities defensively;
- increment case-insensitive type counts during the same pass;
- preserve entity order;
- preserve case-insensitive `GetCount`;
- preserve read-only public exposure.

## Contract Requirements

Any optimization must preserve:

- null enumerable rejection;
- null entity rejection;
- defensive immutability;
- original entity order;
- total entity count;
- case-insensitive type aggregation;
- trimmed, case-insensitive `GetCount`;
- zero for unknown entity types;
- argument validation for invalid `GetCount` input.

## Decision

PERF-11 is accepted as a successful isolated attribution sprint.

The constructor has a measurable, size-dependent allocation and timing
cost. A single-pass implementation is justified without caching,
parallelization, or architectural expansion.

## Out of Scope

PERF-11 does not change:

- production code;
- inventory validation;
- entity parsing;
- semantic handlers;
- mapping;
- caching;
- parallelization;
- topology construction;
- healing or sewing.
