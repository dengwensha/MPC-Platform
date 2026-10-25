# CAD-GE03B-04B-05 — Real R01/R02 independent occupancy reference discovery

Status: VERIFY-FIRST discovery; no production certification and no classifier activation.

## Current established facts
- Main includes GE03B-04B-04 T001-only STEP face identity + coordinate-pinned box occupancy tests.
- The R01 and R02 actual corpora are distributed to CI from private `ci-inputs-v1` release package `p01-real-parts.zip`, checked by SHA-256 in `Invoke-MpcFullValidation.ps1`.
- R01 SHA-256: `E518DB3332DC665B16CC31DA960C46B75B8D75128234129922A25146BE1C33AD`, 11 mapped faces.
- R02 SHA-256: `0E9E2D6E15E1328C068E30703AD621AE883B6FE94A5DB51353A0D26F82E1578F`, 23 mapped faces.
- Existing CI `PT04BProbe` establishes mapping-time face identities, source orientation trace and bounded dihedral evidence. Its material ledger keeps all 34 face entries `Unverified`; `MaterialOrientationVerified=false`.
- Historical P01 validation reports dated 2026-08-25 describe earlier R02 circle-resolution failure. They are **historical**, not evidence that the current R02 CI mapping fails.

## Independence problem and decision
A reference reconstructed using the same STEP face normals, `same_sense`, canonical winding or dihedral code cannot be used as an independent material-side ground truth. The private source STEP bytes are available to CI, but their presence does not by itself provide an independent material occupancy reference.

### Ranked options
1. Independently authored CAD solid reference: obtain an external neutral solid/mesh representation, with provenance, unit, coordinate alignment, orientation, face associations, file hash, and recorded reference-tool version. Use a separately validated inside/outside query on both sides of face-local samples. **Preferred**, conditional on fixture availability and independent review.
2. Human-reviewed per-face outward vector witness: separately identify each source face with an independent drawing/CAD view, known local point and outward direction. Suitable for bounded selected faces only; record reviewer and provenance. Can pilot R01 before R02.
3. Synthetic proxy reconstruction inferred from R01/R02's STEP data: **not acceptable as independent real-part proof**, even if it produces plausible occupancy answers.

## Acceptance contract for candidate real-part oracle
- Tie reference SHA-256, original STEP SHA-256, units/transform and attributable provenance to each claim.
- Map identity to real `ADVANCED_FACE` IDs uniquely, never by enumeration order.
- Report face-local sample point, inward/outward probe offsets, clearance, occupancy results and uncertainty; reject boundary/ambiguous results.
- Test deliberate normal inversion and concave/pocket cases; reverse must give the opposite accepted conclusion.
- Any unqualified face remains `Unverified`, with explicit reason, and must not contribute to a global material certification.
- Full-face proof needs coverage or a mathematical argument for sign invariance across the face; one local point is insufficient.
- Independent review must explicitly evaluate the reference's independence rather than only successful consistency with the current mapping.

## Bounded next micro-sprint GE03B-04B-06
Single goal: build a **test-only intake validator** for externally supplied material oracle manifest (source/reference hashes, face IDs, sample coordinates, independent occupancy labels and uncertainty), with adversarial missing/duplicate/mismatched cases. It must reject absent reference and certify **zero** real faces by default. Do not invent a reference from STEP input. Do not change production classifier, `MaterialOrientationVerified`, public API, geometry tolerances or SOLIDWORKS host.

## Gate status
R01: 0/11 independently verified. R02: 0/23 independently verified. No certified occupancy fixture identified by this discovery. No claim of physical acceptance; maintain fail-closed behavior. A real reference acquisition/approval is an external dependency before any nonzero real-face verification.
