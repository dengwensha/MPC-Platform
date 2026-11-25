# B6 option A — bounded design after rejected B trial

Date: 2026-11-25 Europe/Istanbul. DESIGN ONLY; no implementation authority or
resource-budget acceptance inferred. Senior SR02 §43 requires rejection of B if
its complete cost acceptance is not met. B has a meaningful pre-A1 heap gain but
adds first-capture allocation; do not silently relax that condition.

## Proposed smallest A experiment

Use the same immutable captured inventory and the same hardened Components parser.
Pass 1 visits records in the current stable order, preserving identity/classification
findings, exact signatures, roles, component names and ordered/multiplicity-preserving
parsed references. Parse only one full record tree at a time; retain only the
component payloads required by later context/binding/presentation reads. Do not
publish a decision or observation from this neutral index.

Pass 2 reparses each admitted row from its SAME RawStatement, performs existing
field/reference/spline checks against the complete pass-1 signature index, then
releases its transient tree immediately. It must validate forward references,
retain all classification reasons before field reasons, and preserve Occurrence,
indices, multiplicity and final deterministic order. The temporary current row
must be the one read by P() during spline validation; do not read an empty marker
or change component-presence semantics. No source file reread.

After pass 2, use existing contexts, direct same-inventory A1, shape/product/style
binding and ownership logic. Every later P() read still needs full retained values
regardless of row role. Preserve incomplete/unknown incoming evidence and all keys;
private issuers and C1/reflection locks remain unchanged.

## Costs and limits needing review

A straightforward two-pass implementation parses parameters twice. It should
reduce simultaneous full-tree liveness (including the row-construction peak B
cannot remove), but can INCREASE CPU and total allocation. This is not presumed
acceptable. B's no-regression acceptance is not silently replaced by a looser A
budget. Before production implementation, review the two-pass tradeoff and set
explicit cost acceptance against actual capture and the separate #141 resource
limits. No 512MiB/latency ceiling is raised by this document.

An alternative single-pass validator with deferred cross-reference checks avoids
reparsing, but adds a pending-constraint model and reason ordering machinery.
Keep it separate: first evaluate whether a tiny throwaway two-pass experiment is
worth measuring. Do not expand the production reader or attach ASTs to inventory
(option C), add input caches, parallelize, skip A1, or force production GC.

## Proposed experiment acceptance (not approved)

- Exact full ordered target semantics on 20 corpus +13 controls +5 downstream
  negatives, including forward references and unsupported/partial rows.
- Demonstrably lower row-construction live heap and natural capture process peak;
  report latency, first/warm allocation, retained heap and initial metadata cost
  separately, including regressions. No metric substitutes for another.
- Explicitly agreed timing/allocation tradeoff before accepting production code;
  a throwaway probe is not a merge candidate. Independent review before merge.
- Current-checkout/review-head provenance, producer pins and exact-head Windows
  full regression/Worker tests; private16GB Windows/real Worker acceptance remains
  a separate first-consumer gate.

Requested design decision: is this bounded TWO-PASS experiment appropriate, or
should single-pass/deferred validation be designed first? No A production change
is included in this PR.
