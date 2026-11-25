# B6 option B — negative acceptance result

Date: 2026-11-25 Europe/Istanbul. Owner: Mehmet İlkay Kınacı. Senior: SR02 §43.
Start/main: f204c031236c0e4ce596954874f912718346f7c2 (#145, main CI311 green).
Measured B trial: 159b50c0243a03840147fa26517a688da4c1056a (historical PR146 commit).
Disposition: REJECT B because natural process peak did not meaningfully fall (Senior §44). Final src/ is identical to start;
no parameter-release optimization ships. Five contract-negative tests remain.

## Locked gate and measured candidate

§43 required component-name retention/all keys, downstream negatives, exact20+13
semantics and a complete real-capture cost check. §44 corrects the interpretation:
the decisive rejection is unchanged natural process peak, not small startup allocation.
The original zero-tolerance reading was withdrawn by the reviewer. Historical candidate retained an
explicit later-read component list irrespective of role, used bounded immutable
empty contract-name payloads, and preserved original inventory, references, reasons,
A1 and C1. It did not retain input data across captures.

## §44 correction precedence

The historical archive/Validation.json preserves the original §43 zero-tolerance
interpretation for traceability; its allocation-based disposition is superseded by
this §44 correction. Numeric observations and hashes are unchanged.

## Final evidence

SDK8.0.422, Linux x64. Two separate-process trials on20 corpus files +13 established
controls +5 new negatives. Every before/after AND repeated full audit projection is
identical (existing adapter excludes inner-cause text; no new omission). All17
corpus positives remain positives. The five new cases cover absent placement,
unclassified styled/layer root inputs, second shape representation, and partial
incoming root reference. Windows trial CI312 is green:2253/2253,Worker5/5,
public target9+13,profile12 processes+3 negatives. This is trial correctness, not
resource acceptance or a private Windows before/after cost claim.

| R03 metric | Before | B trial | Interpretation |
| --- | --- | --- | --- |
| Controlled pre-A1 rooted heap |299.41/299.42MiB|170.85/170.85MiB|42.94% decrease, not natural peak |
| Warm actual-capture median |3836.42ms|3889.47ms|+1.38%; no statistical/no-regression acceptance inferred |
| Warm allocated bytes (median) |1,423,134,000|1,423,134,168|Reported exactly; do not round a regression into a pass |
| All-sample lifetime process peak |480,002,048B|479,784,960B|Essentially unchanged; includes setup/warmups |

Actual-capture experiment:120 fresh processes,480 measured calls +240 warmups,
4 expected-unavailable controls; all identities/semantics/lifetimes pass. In ALL40
paired first-capture observations (20files×2trials), trial allocation INCREASES by
39,632–63,704 bytes (38.70–62.21KiB). The new contract-name placeholder initialization
adds startup work; do not infer it is the sole cause of every byte. Allocation is
reported separately for first/warm calls. The allocation increase is one-time placeholder initialization (about0.004% of
R03 capture allocation), reported for information, NOT the deciding rejection.
The real process peak stays essentially unchanged despite the43% controlled-heap
gain, consistent with the existing peak occurring during row construction.
Latency differences are smaller than observed noise and are not called a regression.
B is removed because it does not reduce that natural peak.

## Reproducibility and limits

[evidence](evidence/B6-Component-Release/README.md) includes all298 JSON files in a
hash-indexed archive: full audit reports, both overlays, phase samples, every cost
sample, comparisons, summaries and commands. Raw CAD is NOT included. Before/trial
source pins and DLL identities are preserved; final source reversion does not
rewrite history. An earlier mixed-DLL run was rejected by its identity guard; a
superseded per-capture-placeholder run is also excluded. Only final159b50c trial
outputs are in this archive. Diagnostic and canonical trial projections agree;
forced-GC phase data never substitutes for natural capture/real Worker cost.

## Next bounded step

[Option A design](B6-Option-A-Design.md) proposes a two-pass throwaway experiment,
with exact forward-reference/reason ordering and retained later-read metadata.
A may trade lower row-construction liveness for MORE CPU/total allocation. That
tradeoff needs explicit design review and acceptance bounds before product code;
no A implementation, consumer, budget increase or approval is inferred here.

Review requested: accept B's negative disposition and production reversion;
accept the five negative tests +separate review-head metadata; decide whether
A's two-pass experiment is the appropriate next step and what costs are acceptable.
