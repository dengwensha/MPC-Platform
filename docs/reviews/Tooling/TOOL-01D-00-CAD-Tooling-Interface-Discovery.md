# TOOL-01D-00 — CAD / Tool Library Interface Pre-Discovery

Status: VERIFY-FIRST / DOCUMENTATION ONLY. Independent of TOOL-01C-SR01 senior review. No runtime integration, code, database, merge, or deployment.

## Verified references
- `src/MPC.sln`: distinct `MPC.Geometry`, `MPC.Topology`, `MPC.Mapping`, `MPC.Application` and `MPC.Tooling` projects.
- `src/MPC.Tooling/Compatibility/InsertHolderCompatibilityEvaluator.cs`: bounded shape/seat/dimension/angle/mount/hand compatibility; 0.001 mm and 0.001 degree internal comparison tolerance.
- `docs/reviews/CAD-GE03/CAD-GE03B-09B-R01-Physical-Acceptance-Checklist.md`: R01 0/11 and R02 0/23 qualified face orientations; `MaterialOrientationVerified=false` and convexity/concavity `Indeterminate`.
- `docs/reviews/Tooling/TOOL-01C-Persistence-Assembly-Identity-Contract.md`: persistence and versioned assemblies proposed but production implementation gated by independent senior review.

## Interface architecture recommendation (not implementation)
1. CAD should emit an **AnalysisRequirement** with stable source STEP hash, identified face/feature references, units, coordinate transform provenance and per-claim verification state. All fields are prospective: current working DTO availability is **not verified**.
2. Tooling must produce **ToolCandidate** referencing immutable supplier product revision, catalog provenance, reviewed vendor-field mappings and optional assembly revision; current production candidate API is **not verified**.
3. Bridge (future Application-layer composition) should join requirements and candidates explicitly with diagnostics. Neither geometry layers nor `MPC.Tooling` should depend on SolidWorks host.
4. An input with unverified material orientation or ambiguous face mapping must not request a cutting strategy dependent on inside/outside / concavity. Return `BlockedByUnverifiedGeometry` or equivalent diagnostic as a **proposal**, not a current API.
5. A candidate based on unreviewed vendor mounting-style mapping, missing assembly revision, or insufficient machine envelope is `NeedsMapping`/`NeedsVerification`, never `PhysicallyCompatible`.
6. Interface-level insert/holder compatibility is independent of machine fit, reach, holder collision, cutting data and toolpath feasibility. No NC output gate opened here.

## Minimum test scenarios for a later gate
- Same stable source face under different list ordering retains identity and provenance.
- Unverified material side blocks dependent machining choice rather than silently choosing a tool.
- Reviewed catalog product revision does not imply physical assembly certification.
- Revision update cannot silently change a previously selected tool candidate.
- Unsupported units, missing transforms and unresolved vendor codes fail closed.
- No SolidWorks COM dependency enters the `MPC.Tooling` model.

## Dependencies and decision
**GO** for documentation of requirement/candidate trust boundaries only.
**NO-GO** for contracts, implementation, persistence, selection algorithm, UI or toolpath until: independent TOOL-01C-SR01 findings dispositioned; CAD material/face evidence accepted where the requested feature requires it; actual application/DTO dependencies inspected in a dedicated implementation discovery.

This review does not alter or preempt the running TOOL-01C senior review. No claims of independently certified CAD/tool compatibility are made.
