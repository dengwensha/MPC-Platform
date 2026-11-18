# SR-L4 InvalidOperationException classification

Date: 2026-11-18 Europe/Istanbul. Class: VERIFY -> bounded HARDEN.
Start/inspection baseline: `fa5ea2bf8126f269a5affe216ff7cb5c21e84015`.

## What the static count means

Pinned file reads reproduce the Senior's 76 explicit
`throw new InvalidOperationException` sites: 27 in MPC.Connectors.Step and
49 in MPC.Mapping.Step. The file/line inventory below is from that baseline,
not the follow-up line numbers. Search discovery was followed by reading the
pinned files. This is not an exhaustive census of implicit framework throws,
all other projects, recursion/resource exhaustion or every malformed STEP.

| Group | Count | Evidence and disposition |
| --- | ---: | --- |
| Semantic resolver duplicate-ID constructor guard | 1 | Reachable from real imported bytes; reader keeps duplicates with SuccessWithDiagnostics. Narrow correction below |
| Semantic resolver lookup API empty/missing/wrong-type rejection | 3 | StepReferenceResolver/StepCanonicalReferenceResolver already convert lookup failure to mapping diagnostics; typed production adapter handles type mismatch. No new broad catch |
| Semantic handler wrong-type/family dispatch preconditions | 17 | Reader selects handlers by normalized type and CanRead; direct misuse is a programming/configuration fault. Keep guards |
| Semantic reader duplicate handler registration | 1 | Fixed Application catalog configuration, not a file declaration. Keep guard |
| B-spline semantic source-parameter-name switch default | 1 | Internal diagnostic parameter-name mapping, not a source-file supplied switch key. Keep guard |
| StepDataEntityRecord complex/status consistency | 4 | Reader builds coherent record metadata/diagnostics; invariant guards retained |
| Mapping resolved/success result null-value postconditions | 45 | ReferenceResolutionResult/MappingResult enforce success-with-value. These guards protect internal contracts, not a new malformed-input classification |
| Face identity index lifecycle/missing/duplicate identity | 3 | Factory mapping resets index, reader supplies source IDs, resolver uniqueness protects declaration identity and ClosedShellMapper filters repeated references before face mapping. Keep guards |
| Canonical mapping resolver configuration | 1 | Factory supplies the resolver; missing configuration is a programming fault. Keep guard |
| Total | 76 | Static classification with bounded behavioral evidence, not proof of universal exception containment |

Topology/result/Host invariants outside these two STEP projects are not silently
reclassified. Existing catches in reference/face/edge mapping remain unchanged.
A new reproducible escape from any retained group must be investigated, not
automatically relabelled as invalid input.

## Reproduced input path and minimal correction

Append a second #7 declaration (CARTESIAN_POINT or DIRECTION) to T001_PRISM.
There is still exactly one MANIFOLD_SOLID_BREP, so the importer reaches resolver
construction. StepDataInventoryValidator reports duplicate ID but Reader returns
SuccessWithDiagnostics and preserves both declarations. StepSemanticEntityResolver
constructor rejects the duplicate using InvalidOperationException.

Only construction of that resolver is wrapped in Application. The constructor
currently has one InvalidOperationException producer: duplicate IDs. It is
translated to InvalidDataException with the original exception retained. Semantic
dispatch/enumeration, mapping and region publication are outside this inner catch.
No message matching, broad importer catch or Worker dependency on STEP is added.
The resolver's public exception contract remains unchanged.

The existing Worker catches InvalidDataException, writes its local diagnostic
and returns correlated ImportFailed (7) / exit 3. No region is published.
No new DTO, enum, source identity normalization, hash ordering, v1.0 or UI change.

## Acceptance and limits

Two duplicate declaration variants run through both importer and actual Worker
runner. Importer asserts retained InvalidOperationException and null region;
runner asserts validated correlated v1.1 result and exit 3. Existing direct
resolver duplicate/type/missing guard tests remain unchanged.
Three real-file controls retain MissingRegion/exit 2: absent shell reference,
wrong shell reference type and repeated shell face. Golden success, legacy and
full corpus regressions remain required. Reproduction-first and final exact-head
CI evidence/trace are recorded in the PR; no physical SOLIDWORKS PASS is claimed.

Reproduction-first CI #221 at `30c358c194aace11a639722f932436fe86fb93e8`
failed exactly the four duplicate cases with original
InvalidOperationException: Duplicate STEP semantic entity id '#7'.
The remaining 76 Worker cases passed, including the three reference controls.
[Baseline reproduction](https://github.com/dengwensha/MPC-Platform/actions/runs/44326661906).
This was an intentional test-before-fix run; final green evidence is separate.

Independent Senior review must confirm the constructor-only classification and
remaining invariant dispositions. This delivery does not close overall SR-L4:
the add-in general-catch diagnostic gap and universal unexpected-exception policy
still need disposition. #106's exact-extension and missing cleanup branch tests
are independent non-blocking follow-ups, not part of this input-error correction.

## Pinned explicit throw-site inventory

| Baseline source file | Sites | Lines |
| --- | ---: | --- |
| `src/MPC.Mapping.Step/StepFaceBoundMapper.cs` | 2 | 58, 75 |
| `src/MPC.Mapping.Step/StepFaceIdentityIndex.cs` | 3 | 21, 23, 25 |
| `src/MPC.Mapping.Step/StepEdgeLoopMapper.cs` | 2 | 67, 85 |
| `src/MPC.Mapping.Step/StepAdvancedFaceMapper.cs` | 3 | 77, 109, 127 |
| `src/MPC.Mapping.Step/StepPlaneGeometryMapper.cs` | 4 | 56, 75, 116, 159 |
| `src/MPC.Mapping.Step/StepCircleGeometryMapper.cs` | 3 | 53, 72, 113 |
| `src/MPC.Mapping.Step/StepLineGeometryMapper.cs` | 3 | 50, 70, 90 |
| `src/MPC.Connectors.Step/Models/StepDataEntityRecord.cs` | 4 | 218, 226, 254, 263 |
| `src/MPC.Connectors.Step/Semantics/StepLineEntityHandler.cs` | 1 | 30 |
| `src/MPC.Connectors.Step/Semantics/StepPlaneEntityHandler.cs` | 1 | 26 |
| `src/MPC.Mapping.Step/StepToroidalSurfaceGeometryMapper.cs` | 4 | 58, 77, 119, 162 |
| `src/MPC.Mapping.Step/StepSphericalSurfaceGeometryMapper.cs` | 4 | 56, 75, 116, 159 |
| `src/MPC.Mapping.Step/StepManifoldSolidBrepMapper.cs` | 1 | 58 |
| `src/MPC.Connectors.Step/Semantics/StepSemanticEntityResolver.cs` | 4 | 36, 48, 56, 73 |
| `src/MPC.Mapping.Step/StepCylindricalSurfaceGeometryMapper.cs` | 4 | 56, 75, 116, 159 |
| `src/MPC.Connectors.Step/Semantics/StepCircleEntityHandler.cs` | 1 | 26 |
| `src/MPC.Mapping.Step/StepConicalSurfaceGeometryMapper.cs` | 4 | 56, 75, 116, 159 |
| `src/MPC.Mapping.Step/StepClosedShellMapper.cs` | 2 | 108, 126 |
| `src/MPC.Connectors.Step/Semantics/StepComplexBSplineEntityHandler.cs` | 1 | 24 |
| `src/MPC.Connectors.Step/Semantics/StepVectorEntityHandler.cs` | 1 | 32 |
| `src/MPC.Mapping.Step/StepVertexPointMapper.cs` | 1 | 48 |
| `src/MPC.Mapping.Step/StepDegenerateToroidalSurfaceGeometryMapper.cs` | 4 | 58, 77, 119, 162 |
| `src/MPC.Connectors.Step/Semantics/StepSphericalSurfaceEntityHandler.cs` | 1 | 26 |
| `src/MPC.Mapping.Step/StepBSplineCurveWithKnotsGeometryMapper.cs` | 1 | 24 |
| `src/MPC.Connectors.Step/Semantics/StepDirectionEntityHandler.cs` | 1 | 32 |
| `src/MPC.Connectors.Step/Semantics/StepCylindricalSurfaceEntityHandler.cs` | 1 | 26 |
| `src/MPC.Connectors.Step/Semantics/StepConicalSurfaceEntityHandler.cs` | 1 | 26 |
| `src/MPC.Connectors.Step/Semantics/StepCartesianPointEntityHandler.cs` | 1 | 32 |
| `src/MPC.Mapping.Step/StepAxis2Placement3DMapper.cs` | 1 | 182 |
| `src/MPC.Mapping.Step/StepCanonicalMappingOperation.cs` | 1 | 188 |
| `src/MPC.Connectors.Step/Semantics/StepAxis2Placement3DEntityHandler.cs` | 1 | 32 |
| `src/MPC.Connectors.Step/Semantics/StepRationalBSplineCurveEntityHandler.cs` | 1 | 33 |
| `src/MPC.Mapping.Step/StepAdvancedFaceGeometryMapper.cs` | 1 | 176 |
| `src/MPC.Connectors.Step/Semantics/StepToroidalSurfaceEntityHandler.cs` | 1 | 41 |
| `src/MPC.Mapping.Step/StepBSplineSurfaceWithKnotsGeometryMapper.cs` | 1 | 72 |
| `src/MPC.Connectors.Step/Semantics/StepRationalBSplineSurfaceEntityHandler.cs` | 1 | 33 |
| `src/MPC.Connectors.Step/Semantics/StepBSplineCurveWithKnotsEntityHandler.cs` | 1 | 14 |
| `src/MPC.Connectors.Step/Semantics/StepSemanticEntityReader.cs` | 1 | 56 |
| `src/MPC.Connectors.Step/Semantics/StepDegenerateToroidalSurfaceEntityHandler.cs` | 1 | 27 |
| `src/MPC.Connectors.Step/Semantics/StepBSplineSurfaceWithKnotsEntityHandler.cs` | 2 | 26, 363 |
