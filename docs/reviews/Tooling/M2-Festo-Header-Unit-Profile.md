# Festo header/unit profile — #125 acceptance follow-up

Date: 2026-11-23 Europe/Istanbul. Observation only; M2 OPEN.
Measured #125 source: `7c8f9952fe85a12b7249c5cf62df6f78bdebb87c`.
No production source, policy, pin or severity change in this follow-up.

## Inputs and permission record

Four owner-uploaded FESTO_5831CAU14 ZIP names have identical SHA-256
`F5682D78530B81ACE4874E8C31162D829DB71E210A4EA8AAE2DBAE09D4030ACE`.
Each has the same 88220-byte `5831 CAU-1_4.stp` plus a download readme.
Count ONE unique STEP, not four independent observations. Input SHA-256:
`A1B72CD5A3035C2F8DD8A9FCDED1B32407F1DCE38A308D109E8CA10FD359CD61`.
Archive-entry hashes and original names are in the linked manifest. Raw STEP and
readme files are not added to GitHub. Do not call manufacturer download open source.

Permission: owner explicitly confirmed local MPC development/test use on 2026-11-23
in the working conversation, including report product name/hash/technical results.
This is the recorded usage basis; no external raw-CAD redistribution permission
or independent legal/licensing determination is claimed. Download readme points
to CADENAS terms; header author metadata declares License CC BY-ND 4.0. Preserve
source/attribution information; metadata alone is not a new license grant.
Input was measured unchanged, not converted or repaired by the implementation session.

## Method and observation

Clean detached worktree at the measured head, Linux/.NET SDK 8.0.422, Release build
0 warnings/errors. Reused historical profile harness calls actual CaptureFromFile,
which privately hashes/captures bytes and issues the encompassing evidence.
Producer source hashes match reviewed #125: HeaderUnitObservation 8F6FA88A...,
InventoryEvidence 0DD4C78C..., StepParameterParser 69AA2488.... Full hashes,
build command/time, SDK, archive/input hashes and assembly hash recorded in
[evidence](evidence/M2-Festo-HeaderUnit/). Profile is not Application eligibility.

| Property | Observation |
| --- | --- |
| Actual available observation | 1/1 unique file |
| Header implementation level | 2;1 |
| Declared schema | AUTOMOTIVE_DESIGN {1 0 10303 214 3 1 1} |
| Header reasons | UnsupportedImplementationLevel; UnsupportedSchemaForm |
| Geometric representation contexts | One, #2117, dimension 3 |
| Length | #2121: millimetre, exact scale 1/1000 metre |
| Plane angle | #2120: radian, exact scale 1/1 |
| Other dimension | #2119: solid angle metadata, OtherDimension, no length/angle scale |
| Context and context-set reasons | None |
| B2 2D-context condition | Not observed in this file; no general B2 closure |

Header non-ready incidence 1/1 unique manufacturer sample. Units have no reasons;
that does not override either header reason or establish geometry suitability.
The existing policy intentionally excludes both level 2;1 and qualified schema
forms. No policy, file header or source was changed to pass this sample. Broader
Festo diversity is not claimed; other placeholder samples remain unmeasured.
No physical SOLIDWORKS or Windows profile execution claimed.

Senior section 26 Festo profile/permission/reporting evidence is now supplied.
Final documentation-head CI still must pass before merge; no new independent
review of profile conclusions is fabricated. B1/B3 are addressed separately by
dependent #126 (which needs its own review). C1/U1 and other pre-consumer follow-ups
stay open. Historical absent-Festo wording in earlier records is superseded here,
not a retroactive claim that those old measurements contained this input.
