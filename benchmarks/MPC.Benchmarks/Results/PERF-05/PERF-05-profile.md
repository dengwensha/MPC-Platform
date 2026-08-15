# PERF-05 — Entity Record Creation Cost Attribution

## Classification

`[PROFILE]`

## Objective

Attribute the CPU cost inside
`StepDataEntityReader.CreateEntityRecord` after the
missing-reference validation optimization.

This sprint performs profiling and documents the next measured
optimization target. It does not modify production code.

## Traceability

- Branch: `main`
- Start Commit: `8c1e51836f086d092e8005be727e2f8e6d71134a`
- Reviewed HEAD: `8c1e51836f086d092e8005be727e2f8e6d71134a`
- Closing Commit: To be recorded after commit
- Review Date: 2026-08-16

## Benchmark Scope

The following STEP golden files were profiled:

- `T001_PRISM.step`
- `T103_MULTIPLE_ISLANDS.step`

The profiled benchmark was:

`MPC.Benchmarks.StepDataParsingBenchmarks.ParseDataInventory`

Profiling was performed with the BenchmarkDotNet EventPipe profiler
using Release binaries.

## Profiling Command

    dotnet run `
        --project .\benchmarks\MPC.Benchmarks\MPC.Benchmarks.csproj `
        -c Release `
        --no-build `
        -- `
        --filter "*T001_PRISM*" "*T103_MULTIPLE_ISLANDS*" `
        --profiler EP

## Contextual CPU Attribution

All values are inclusive percentages relative to
`StepDataEntityReader.CreateEntityRecord`.

| Method | T001_PRISM | T103_MULTIPLE_ISLANDS |
|---|---:|---:|
| `CreateEntityRecord` | 100.00% | 100.00% |
| `StepDataEntityRecord` constructor | 22.40% | 19.42% |
| Regex `RunSingleMatch` | 6.20% | 4.32% |
| `ExtractReferencedEntityIds` | 2.02% | 1.56% |
| `ParseComplexComponentTypes` | 0.72% | 0.29% |
| String case conversion | 0.43% | 0.45% |

The machine-readable values are stored in
`entity-record-attribution.csv`.

## Interpretation

The largest reliable application-level cost inside
`CreateEntityRecord` is the `StepDataEntityRecord` constructor.

The constructor currently materializes the supplied collections and
normalizes their values. The reader also materializes referenced entity
IDs before passing them to the constructor. This indicates possible
duplicate collection materialization and avoidable empty-collection
allocations.

Regex processing is a secondary cost. Reference extraction and complex
entity component parsing are comparatively small and are not selected
as the next optimization targets.

## EventPipe Attribution Limitation

The contextual trace also attributed approximately 80–86% to
`List<T>(IEnumerable<T>)` and `List<T>.CopyTo`.

Those percentages exceed the inclusive percentage attributed to the
`StepDataEntityRecord` constructor that is expected to contain the
relevant materialization calls. This violates the expected parent-child
relationship for inclusive timings.

The trace contains synthetic `CPU_TIME` frames. Therefore, the
`List<T>` percentages are treated as an EventPipe contextual
aggregation artifact and are not used as standalone optimization
evidence.

The constructor-level attribution is consistent across both files and
is supported by direct source inspection, so it remains a valid
optimization candidate.

## Bottleneck Decision

The next optimization target is:

`StepDataEntityRecord` collection materialization.

The intended follow-up is:

`PERF-06 — StepDataEntityRecord Collection Materialization`

Classification:

`[OPTIMIZE]`

The optimization must preserve:

- public constructor validation behavior;
- defensive immutability;
- source entity type normalization;
- component type normalization;
- referenced entity ID normalization;
- diagnostic/status consistency;
- complex entity consistency;
- deterministic observable results.

## Out of Scope

The following changes are excluded from PERF-05:

- production code changes;
- regex replacement;
- parser redesign;
- reference extraction changes;
- caching;
- parallelization;
- topology mutation;
- healing or sewing.

## Completion Assessment

PERF-05 identified and documented the post-validation bottleneck inside
entity record creation.

The minimum evidence required to open PERF-06 is present:

- two representative golden STEP files were profiled;
- constructor cost was reproduced in both traces;
- secondary costs were separated;
- unreliable low-level attribution was explicitly excluded;
- the next optimization boundary was defined.
