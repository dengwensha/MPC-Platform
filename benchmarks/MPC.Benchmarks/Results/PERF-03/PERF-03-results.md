# PERF-03 — Missing-Reference Validation Optimization

Classification: `[OPTIMIZE]`

## Traceability

- Start Commit: `37141596735457d1749c91844107691bc05357db`
- Reviewed HEAD: `37141596735457d1749c91844107691bc05357db`
- Closing Commit: Pending
- Branch: `main`
- Review Date: `2026-08-16`

## Evidence Basis

PERF-02 identified missing-reference validation as the strongest individual
STEP DATA parsing cost. Its per-entity LINQ materialization chain accounted
for 21.55% of T001 and 26.87% of T103 inclusive parser CPU attribution.

## Change

The following chain was removed from
`AddMissingEntityReferenceDiagnostics`:

`Select → Where → GroupBy → Select First → OrderBy → ToArray`

It was replaced with a lazily created `SortedSet<BigInteger>`.

The replacement preserves:

- Numeric reference parsing
- Numeric-equivalent reference deduplication
- Numeric target ordering
- Missing-reference diagnostic content
- Source identity and entity type
- Final deterministic diagnostic ordering
- References to later entities
- Invalid-reference behavior

The unused `ParsedReference` private record was removed.

## Benchmark Results

| Input | Before | After | Time improvement |
|---|---:|---:|---:|
| T001_PRISM.step | 805.794 us | 702.323 us | 12.84% |
| T103_MULTIPLE_ISLANDS.step | 2.8133 ms | 2.4966 ms | 11.26% |

| Input | Before allocation | After allocation | Allocation improvement |
|---|---:|---:|---:|
| T001_PRISM.step | 700.61 KB | 575.94 KB | 17.79% |
| T103_MULTIPLE_ISLANDS.step | 2,750.43 KB | 2,243.82 KB | 18.42% |

The before/after confidence intervals do not overlap. The improvement is
reproduced on both the small canonical prism and the largest golden input.

## Validation

- Targeted validator tests: 19/19 passed
- Release build: Passed with 0 warnings and 0 errors
- Public API changes: None
- Diagnostic semantic changes: None
- Production files changed: One
- Benchmark source changes: None

## Decision

The optimization is accepted because it produces a repeatable improvement
in both execution time and managed allocation while preserving the locked
validator contracts.
