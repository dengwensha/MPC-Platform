# PERF-08 — Numeric STEP Entity ID Parsing Attribution

## Classification

`[PROFILE]`

## Objective

Determine why `TryParseNumericEntityId` became a visible cost after the
PERF-06 entity record materialization optimization.

This sprint performs source-flow analysis and call-volume measurement.
It does not modify production code.

## Traceability

- Branch: `main`
- Start Commit: `9b158f6bfec1575753b94d337e5bf59bdf29572d`
- Reviewed HEAD: `9b158f6bfec1575753b94d337e5bf59bdf29572d`
- Closing Commit: To be resolved from the commit containing this report
- Review Date: 2026-08-16

## Profile Evidence

PERF-07 attributed the following inclusive parser CPU shares to
`StepDataInventoryValidator.TryParseNumericEntityId`:

| File | Inclusive CPU Share |
|---|---:|
| T001_PRISM.step | 6.65% |
| T103_MULTIPLE_ISLANDS.step | 9.47% |

Percentages are relative to `StepDataEntityReader.Read`.

## Call-Flow Analysis

`TryParseNumericEntityId` has two application call sites.

### Source entity IDs

`CreateIndexedEntity` parses each entity's source ID once and stores the
result in `IndexedEntity.NumericSourceEntityId`.

Both duplicate-ID and missing-reference validation reuse the stored
numeric source ID.

Therefore, source entity IDs are not reparsed across validation passes.

### Referenced entity IDs

`AddMissingEntityReferenceDiagnostics` parses each referenced entity ID
occurrence once.

References are inserted into a per-entity `SortedSet<BigInteger>` after
parsing. Repeated references within an entity are therefore deduplicated
after numeric conversion.

## Measured Call Volume

| File | Source ID Calls | Reference ID Calls | Total Calls |
|---|---:|---:|---:|
| T001_PRISM.step | 187 | 216 | 403 |
| T103_MULTIPLE_ISLANDS.step | 755 | 896 | 1651 |

T103 performs approximately 4.10 times as many numeric ID parse calls
as T001.

Machine-readable values are stored in
`numeric-id-call-counts.csv`.

## Current Parsing Path

The current method performs:

1. null or whitespace validation;
2. `string.Trim`;
3. `StartsWith('#')`;
4. substring allocation using `normalized[1..]`;
5. digit validation using LINQ `All`;
6. `BigInteger.TryParse`;
7. non-negative validation.

The measured scaling and profile attribution indicate that the local
parsing path is a valid optimization candidate.

## Cache Decision

No numeric ID cache is justified.

Source IDs are already parsed once and retained in `IndexedEntity`.
Reference IDs are parsed once per occurrence, and no evidence currently
shows that retained cross-entity caching would provide sufficient
benefit to justify additional state.

## Next Optimization Candidate

The next proposed sprint is:

`PERF-09 — Span-Based Numeric STEP Entity ID Parsing`

Classification:

`[OPTIMIZE]`

The intended change is local to `TryParseNumericEntityId`:

- use `ReadOnlySpan<char>`;
- trim without creating a new string;
- validate the leading `#`;
- scan digits with a direct loop;
- pass the numeric span directly to `BigInteger.TryParse`;
- preserve all current accepted and rejected inputs.

## Required Contract Preservation

Any optimization must preserve:

- null, empty, and whitespace rejection;
- surrounding whitespace acceptance;
- mandatory leading `#`;
- rejection of empty numeric parts;
- rejection of non-digit characters;
- support for arbitrarily large non-negative IDs;
- equivalence of IDs with leading zeroes;
- deterministic duplicate and missing-reference diagnostics;
- current diagnostic ordering.

## Out of Scope

PERF-08 does not introduce:

- production changes;
- numeric ID caching;
- changes to `IndexedEntity`;
- changes to diagnostic ordering;
- regex parsing;
- parallelization;
- topology changes;
- healing or sewing.

## Acceptance Decision

PERF-08 is accepted as a successful attribution sprint.

The cost is explained by call volume and the local parsing
implementation. A bounded span-based optimization is justified without
introducing cache state or architectural expansion.
