# CAD-GE03B-01 — Independent STEP pole reference acceptance contract

**Frozen starting point:** `main` `4886c5acfaf0371d359fbc3be63d1d80aae02e7a`.  
**Disposition:** GE03B-01 remains OPEN; this document is an evidence specification, not a geometry result or release approval.

## Verify-First findings

- R03 B16SurfaceProbe maps 276 rational faces and audits 1,104 parametric boundary samples.
- Existing R03 evidence reports 1,104 accepted and zero rejected boundary normals; subsequent control-net proximity audit reported **zero pole candidates** under the current relative `1e-6` rule.
- R03 therefore does not establish behavior at a **known** geometric pole. A control-net candidate is not independent proof of a geometric pole; the absence of candidates is not proof that the STEP file contains none.
- Existing test `RationalBSplineNormalQueryTests.ExactQuarterConeApex_WithExportNoise_IsRejected` supplies a synthetic analytic pole oracle, not an imported real STEP fixture.
- `RationalSurfaceApplicationTests` exercises complex STEP rational surface semantic parsing but its fixture deliberately lacks a resolved face bound; it is **not** a valid production-end-to-end pole acceptance source.

## Reference fixture requirements (do not claim fulfilled before measured)

1. Obtain or construct a valid ISO-10303-21 STEP file with a known rational B-spline representation of a quarter-cone-to-apex (or sphere pole), exported independently from the tested normal evaluator. Capture CAD generator/version or mathematical source, parameter orientation, control net, weights, knot vectors, and SHA256.
2. Specify the pole as a **geometric reference** (XYZ apex or known degenerate parametric edge), not simply by inferred tiny control-net spread. State whether the STEP advanced face has a valid trim, bounds, shell and orientation. Distinguish surface-normal evidence from whole-solid import acceptance.
3. Record at least one regular interior UV witness whose expected normal is independently derived. For a unit quarter-cone with base `(cos theta, sin theta, 0)` and apex `(0,0,1)`, an outward-oriented regular patch has a normal proportional to `(cos theta,sin theta,1)`; orientation convention must be explicitly fixed.
4. Record at least four U locations and both pole and near-pole V samples (1, 2, 10 and 100 ulps inside), together with the full `1e-16`–`1e-6` control-point perturbation sweep and explicit expected **reject or independently accurate** behavior.
5. Capture both non-rational and rational behavior when representations are available; never assume equal-weight coverage implies unequal-weight coverage.
6. Run parser → semantic entity → canonical geometry mapping → normal query, writing a JSON evidence record with source SHA, mapped surface count, pole UV/XYZ provenance, accepted/rejected, rejection reason when available, interior analytic error, and unresolved cases.
7. Enforce a CI failure if the **independently identified pole** produces an accepted, unstable normal, or if a regular analytic interior witness is incorrectly returned with angular error above the approved threshold. Set error threshold from oracle uncertainty and numerical stability, not merely from past green outcomes.
8. Compare release regression and R03 evidence before/after fixture onboarding. Preserve the explicit warning that thin but mathematically regular boundary edges may be conservatively rejected with the current `1e-6` policy; characterize this usability limitation separately.
9. Reference the actual GitHub run URL, head SHA and uploaded evidence JSON. Do not close GE03B-01 solely on synthetic tests or unrelated R03 green CI.

## Acceptance decision

- **PASS (finding closure candidate)** only after independent source/geometry provenance, reproduction via real STEP import/mapping, normal oracle comparison, pole rejection or proven correctness, full regression SUCCESS, and reviewer acknowledgement of thin-edge false rejection.
- **BLOCKED** if no independently documented pole STEP is available; leave GE03B-01 open instead of manufacturing a proxy as proof.
- **FAIL** for accepted incorrect pole normal, inconsistent evidence, or new regular-surface regression.

## Scope boundary

No numeric tolerance changes, no manufacturing-side classification enablement, no geometry API refactor. GE03B-02 and all subsequent findings remain separate. Local SOLIDWORKS physical verification and final production acceptance remain independent gates.
