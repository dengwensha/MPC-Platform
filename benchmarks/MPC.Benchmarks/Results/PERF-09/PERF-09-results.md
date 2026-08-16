# PERF-09 — Span-Based Numeric STEP Entity ID Parsing

## Classification

`[OPTIMIZE]`

## Objective

Reduce numeric STEP entity ID parsing allocations and CPU cost without
introducing caching or changing validation behavior.

## Traceability

- Branch: `main`
- Start Commit: `416229f603be75f23865d7fcc304394d41f37d96`
- Reviewed HEAD: `416229f603be75f23865d7fcc304394d41f37d96`
- Closing Commit: To be resolved from the commit containing this report
- Review Date: 2026-08-16

## Measured Basis

PERF-08 identified the following numeric ID parse volume:

| File | Source Calls | Reference Calls | Total Calls |
|---|---:|---:|---:|
| T001_PRISM.step | 187 | 216 | 403 |
| T103_MULTIPLE_ISLANDS.step | 755 | 896 | 1651 |

PERF-07 attributed 6.65% and 9.47% of parser CPU time to
`TryParseNumericEntityId` for T001 and T103 respectively.

## Contract Hardening

Six focused numeric-ID tests were added before modifying production
code.

The tests cover:

- arbitrarily large equivalent source IDs;
- very large declared references;
- canonical diagnostics for very large missing references;
- malformed source ID rejection;
- malformed reference rejection;
- equivalence of leading-zero references.

All six tests passed against the pre-change implementation.

The existing 19 `StepDataInventoryValidatorTests` also remained green.

## Implementation

The optimization is local to
`StepDataInventoryValidator.TryParseNumericEntityId`.

The previous implementation used:

- `string.Trim`;
- `StartsWith`;
- substring allocation with `[1..]`;
- LINQ `All(char.IsDigit)`;
- string-based `BigInteger.TryParse`.

The new implementation uses:

- `ReadOnlySpan<char>`;
- span-based trimming;
- direct leading-character validation;
- direct ASCII digit scanning;
- span-based `BigInteger.TryParse`;
- invariant parsing with `NumberStyles.None`.

No cache, new retained state, or validation architecture was added.

## Primary A/B Results

| File | Pre Mean | Post Mean | Time Change | Pre Allocated | Post Allocated | Allocation Improvement |
|---|---:|---:|---:|---:|---:|---:|
| T001_PRISM | 695.9 us | 873.7 us | -25.55% | 511.53 KB | 486.45 KB | 4.90% |
| T002_SPHERE | 484.9 us | 412.1 us | 15.01% | 224.13 KB | 214.61 KB | 4.25% |
| T003_CYLINDER | 536.8 us | 502.6 us | 6.37% | 363.20 KB | 346.77 KB | 4.52% |
| T101_BASIC_POCKET | 1091.1 us | 999.5 us | 8.39% | 884.01 KB | 839.48 KB | 5.04% |
| T103_MULTIPLE_ISLANDS | 2523.0 us | 2363.4 us | 6.33% | 1983.26 KB | 1880.23 KB | 5.20% |

The first T001 timing result conflicted with the other four files while
its allocation reduction remained consistent. A focused repeat was
therefore required before acceptance.

## T001 Repeat Measurement

| Metric | Pre | Post | Decision |
|---|---:|---:|---|
| Mean | 613.8 us | 614.5 us | Equivalent |
| Error | 11.81 us | 12.09 us | Overlapping |
| StdDev | 16.17 us | 14.85 us | Comparable |
| Allocated | 511.53 KB | 486.45 KB | 4.90% improvement |

The 0.11% timing difference is substantially smaller than both
benchmark error intervals and is treated as no measurable timing
change.

The initial T001 timing regression is therefore classified as
environmental measurement variation rather than a reproduced code
regression.

## Allocation Result

All five benchmark inputs reduced managed allocation.

Observed allocation improvements range from:

- 4.25% for T002;
- to 5.20% for T103.

The allocation result is deterministic across both T001 runs.

## Timing Result

Four of five inputs improved in the primary run:

- T002: 15.01%;
- T003: 6.37%;
- T101: 8.39%;
- T103: 6.33%.

T001 produced equivalent timing in the focused repeat.

No reproduced timing regression remains.

## Validation

Before optimization:

- focused numeric-ID tests: 6 passed;
- existing validator tests: 19 passed.

After optimization:

- focused numeric-ID tests: 6 passed;
- existing validator tests: 19 passed.

Final solution build and full regression are required after result files
are added.

## Acceptance Decision

PERF-09 is accepted.

The implementation:

- removes per-call substring allocation;
- removes LINQ digit-scanning overhead;
- reduces managed allocation on every benchmark input;
- improves or preserves timing across the verified measurements;
- preserves arbitrarily large `BigInteger` IDs;
- preserves diagnostic semantics and ordering;
- introduces no cache or architectural expansion.

## Stored Evidence

The following machine-readable evidence is included:

- `pre-change.csv`;
- `post-change.csv`;
- `pre-change-t001-repeat.csv`;
- `post-change-t001-repeat.csv`;
- `comparison.csv`.

## Out of Scope

PERF-09 does not change:

- source ID indexing;
- reference deduplication;
- duplicate detection;
- missing-reference detection;
- diagnostic ordering;
- entity parsing;
- regex handling;
- caching;
- parallelization;
- topology construction;
- healing or sewing.
