# CAD-GE03B-04B-02 — Independent material-side reference qualification

Status: design contract only. No material-side classification is enabled.

## Verified starting evidence
- Main contains GE03B-04B-01 per-face ledger in `tools/ci/PT04BProbe/Program.cs`.
- R01: 11 canonical/STEP faces, 24 adjacent pairs; R02: 23 faces, 44 pairs.
- Source IDs and `same_sense` propagation are not an independent solid-interior oracle.
- Every face currently remains `Unverified`; `MaterialOrientationVerified=false`.
- `StepManifoldSolidBrepMapper` maps a `CLOSED_SHELL` into `Solid`, without independent geometric point-in-solid classification.
- T001's known strictly interior point works only for its pinned rectangular prism, and does not establish an arbitrary real-part interior witness.

## Qualification methods and decision
| Candidate | Can establish | Limitation | 04B-02 decision |
| --- | --- | --- | --- |
| Source STEP `same_sense` / canonical winding | Traceability and mapping consistency | Same upstream claim; not independent material-side proof | Reject as independent oracle |
| One chosen interior point with plane dot product | Convex supporting face only when interior lies strictly on material side of that plane | Concave, pocketed or disconnected regions violate global half-space assumption | T001 only; not generalized |
| Independent solid point-in-material test, face-local +/- offset | Interior/exterior on both sides of a regular face sample | Needs trusted solid occupancy oracle, sample clearance, uncertainty policy and geometry coverage | Preferred future candidate, not yet implemented |
| Independently reviewed CAD/B-rep reference with explicit outward-face directions | Per-face ground truth, if provenance, identity and orientation are independently established | Human/exporter dependency and face matching risk | Alternative external acceptance fixture |

## Required future per-face evidence
1. Pin exact input STEP SHA-256 and independent reference provenance/hash; reference must not be derived solely from the same canonical face normal, winding or dihedral calculation.
2. Link every qualified result to an existing unique STEP `ADVANCED_FACE` identity through mapping-time `FaceIdentities`; reject missing and duplicate references.
3. For a sample strictly away from edges and singularities, prove one side solid material and the other side void using an independent occupancy reference; record offset magnitude, boundary clearance and method uncertainty.
4. Test both candidate normal directions (including deliberate inversion); accept outward only if reversing it yields the opposite independent result. For ambiguous offsets, touching surfaces, holes, internal voids, invalid shell or unsupported surfaces, output `Unverified` with reason.
5. Do not infer whole-face certification from one sample unless the chosen reference method demonstrates that the face's material-side relationship is invariant over the entire eligible face domain.
6. Keep global `MaterialOrientationVerified=false` until all required eligible faces have attributable independent proof; do not use the flag for partial successes.
7. Keep production convexity/concavity `Indeterminate` and do not change tolerances, topology, `SignedLocalDihedralQuery` or public manufacturing behavior under this contract.

## Acceptance for next implementation micro-sprint
- Independent reference fixture or implementation can be obtained and its independence reviewed.
- Positive/outward, inverted/inward, pocket/concave and indeterminate boundary examples are independently labelled.
- A bounded test-only probe is specified first; release enablement is out of scope.
- Full solution Release build, complete test suite, real R01/R02 corpus probe, clean diff and PR CI SUCCESS are required for any subsequent code changes.

## Current disposition
R01: 0/11 independently verified; R02: 0/23 independently verified. These are qualification states, not claims of wrong normals. The next go/no-go decision is the availability of an independent material-occupancy reference. No production feature may be enabled from this document.
