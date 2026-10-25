# GE03B-04B-09B — R01 Single-Face Physical Acceptance Checklist

Status: READY FOR PHYSICAL EVIDENCE, NOT EXECUTED. No production change or material certification.

## Inputs
- Original R01 native CAD model (prefer SLDPRT) and R01 STEP; independently record their origin, creation path, SHA-256, units and model transform.
- CI artifact `mpc-full-evidence-40213221138` from GitHub Actions run 40213221138; open `PT04B-R01.json`, including `PhysicalReviewPreparation`.
- Installed CAD tool and exact version; independent reviewer and date; separately record whether the native model is independent of STEP import.

## Perform one bounded R01 face
1. Verify the STEP SHA-256 equals `E518DB3332DC665B16CC31DA960C46B75B8D75128234129922A25146BE1C33AD`; stop if it differs.
2. From `PhysicalReviewPreparation`, select exactly one `StepFaceId` and record `CanonicalFaceIndex` and `SurfaceType`; do not rely on face enumeration order.
3. Locate its corresponding native CAD face using geometry, landmarks and coordinate alignment. Record how the mapping was established; reject ambiguous correspondence.
4. Select a regular point strictly within the trimmed face away from edges/seams/singularities; record point XYZ in mm, model-to-STEP transform, inward/outward probe offsets, clearance and measurement uncertainty. A surface origin is not automatically a valid face sample.
5. Independently query native CAD solid occupancy on both sides of the sample, without relying on MPC orientation, STEP same_sense or canonical winding. Record `PlusOccupancy`, `MinusOccupancy` as Material/Void/Boundary/Ambiguous, probing direction and tool used. Repeat for inverted candidate normal.
6. Capture evidence: native CAD SHA-256, reference geometry provenance, CAD version, screenshot or controlled measurement log, review identity/date and linkage to the STEP face ID.
7. If any identity, transform, boundary clearance, uncertainty, independence or occupancy condition fails, record `Unverified` with reason. Even successful local probing is **point-local evidence only**, not full-face qualification.

## Acceptance evidence record (one face)
| Field | Value before physical session |
|---|---|
| STEP source SHA-256 | pinned above |
| Source STEP face ID | pending selection from JSON |
| Canonical face index/type | pending selection from JSON |
| Native CAD source and SHA-256 | pending |
| Native CAD independent of STEP import? | pending attestation |
| Point XYZ, units, coordinate transform | pending |
| Probe normal, offset, edge clearance, uncertainty | pending |
| Material + / Material - and inverted control | pending |
| Screenshot/log, tool version, reviewer/date | pending |
| Local outcome | Unverified |
| Whole-face outcome | Unverified |

## Gate
R01 material status: 0/11 independently qualified; R02 0/23. `MaterialOrientationVerified=false`; production convexity/concavity remains `Indeterminate`. Full-face acceptance requires independently justified face-domain coverage/invariance, separate from this one-point pilot.
