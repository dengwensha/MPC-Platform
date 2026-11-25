# B6 component-scoped parameter release trial

Date: 2026-11-25 Europe/Istanbul. Owner: Mehmet İlkay Kınacı.
Start: f204c031236c0e4ce596954874f912718346f7c2 (#145 merge).
Senior: SR02 §43. HARDEN; draft experiment, not budget/consumer acceptance.

## Locked acceptance

1. Explicit component-name retain list, derived from every later P() call. Preserve
   every component key, including unclassified/partial incoming rows. Never release
   by role. Keep AXIS2_PLACEMENT_3D presence and all shape representation members.
2. Negative tests: absent placement; unclassified styled/layer incoming sources;
   second shape representation; partial incoming root reference. Exact ordered
   decisions/reasons on 20 corpus files and 13 established controls.
3. A meaningful controlled pre-A1 heap reduction AND no actual-capture latency,
   allocation or process peak regression. Controlled GC is not the budget. If no
   acceptable gain, do not ship the production trial; preserve the negative finding
   and evaluate A separately. No threshold relaxation inferred from this PR.

## Candidate boundary

After field/spline and context checks, before Bind/A1, replace non-retained known
component payloads with immutable empty contract-name-only placeholders. Keys,
signatures, roles, original inventory, ordered references, _incoming, _bad and all
reasons survive. Placeholders contain no file data and are bounded by the pinned
contract; unrecognized component names retain their payloads conservatively.
The small immutable placeholder table avoids per-row and per-capture allocations.
All later-used context/unit, shape/bridge/product, placement, styled/layer and
presentation-representation components retain full parameters, regardless of role.
No reader change, normalization, new consumer, policy/revision/severity change,
A1 bypass, forced product GC or C1 issuer change.

## Provenance and validation status

The diagnostic report records the PR review head separately from actual checkout
SourceCommit (Actions may check out a synthetic merge). The current-source guard
and same-semantics-source regression remain mandatory.

Local production/test Release builds pass. Target assertions are invoked directly
where VSTest IPC is unavailable; this is not a substitute for full Windows CI.
Final before/after semantic and cost evidence is pending; this trial is not merge
ready while that evidence or independent review is pending. Raw private CAD stays
outside GitHub. Final results and exact measured source/DLL identities will be
recorded before requesting review. #141 retains actual Worker/private Windows
16GB/headroom/enforced-limit requirements before the first consumer.
