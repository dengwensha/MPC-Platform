# R-U — same-inventory import unit/context precheck

Date: 2026-11-24 Europe/Istanbul. Class: HARDEN.
Start: aecccb557c8806731010a4c608d1469277d6ffff (#135, Senior §35).
Measured code: 6b59550b7370eab5116a6923327b29439f45bda5.
Branch: fix/R-U-import-unit-precheck. Owner: Mehmet İlkay Kınacı.
State: implemented, NOT MERGED; independent final-diff review and owner compatibility
approval pending. R-U/#133 OPEN; M2 OPEN.

## One goal / scope

Reject incompatible units and unresolved/unsupported binding BEFORE mapping in both
existing imports. Only direct millimetre + unprefixed radian accepted, including for
planar input. No conversion, normalized-output claim, new evidence/Ready object,
C1/K2 factory, wire enum/DTO or Host/IPC change. ContextUnitPolicy 1 is unchanged.
StepUnitResolver is unchanged. The eight-code inventory manifest is unchanged.

ImportUnitPrecheck.EnsureCompatible receives the same StepDataInventory instance
and selected source-root ID used by each call. The two call sites are after root
and semantic identity checks and before Map. Guard has no path/stream parameter;
retained-inventory replacement-source test confirms it does not reopen the input.
No production afterSourceCaptured hook. Legacy toroidal metadata still rereads its
path, explicitly unresolved; no blanket same-file-lifecycle claim is made.

Typed SR/ABSR direct membership is unique; other items must be validated axis
placements. T1 styles/layers use finite explicit signatures, not suffix exemptions.
Unit context comes only from the representation's context field. Every assigned
unit is inspected; length and angle are exactly single, direct mm/radian forms;
known steradian may coexist. Conversion/base-unit fallback, metre, centimetre,
unknown/partial/duplicate/cyclic references and 2D binding reject.

The admitted bridge is one simple non-transforming SHAPE_REPRESENTATION_RELATIONSHIP
between root geometry and main SR. Either endpoint order works; both contexts must
independently be mm/radian even if IDs differ. Single product/PDS/SDR linkage is
validated; direct + bridged double-SDR, additional geometry representations,
unknown incoming paths, transformations and assembly/product usage reject.

T1 style catalogue in this first slice: STYLED_ITEM, PRESENTATION_LAYER_ASSIGNMENT,
MECHANICAL_DESIGN_GEOMETRIC_PRESENTATION_REPRESENTATION; PRESENTATION_STYLE_ASSIGNMENT,
SURFACE_STYLE_USAGE, SURFACE_SIDE_STYLE, SURFACE_STYLE_FILL_AREA, FILL_AREA_STYLE,
FILL_AREA_STYLE_COLOUR and COLOUR_RGB. All have explicit parameter/reference roles.
Other styles/PMI/validation properties remain rejected on the binding path; no
unreviewed expansion to improve corpus acceptance. Reachable-geometry membership
used to constrain styled targets is NOT positive M2 coverage. Missing geometric
references retain the existing downstream mapper diagnostics; the guard does not
replace geometric validation or claim full Part 21 conformance.

## Existing failures, codes and diagnostics

Legacy APP-STEP-SEMANTIC-001 now also means bounded unit/context input rejection,
beyond its previous semantic-value use. Messages have the fixed tested prefix
`STEP unit/context precheck: `, deterministic reason, selected root and source IDs.
Legacy returns Failed/null summary. Region throws InvalidDataException before Map
and publishes no region. Current Worker handler preserves ImportFailed/RequestId/
exit 3 and failure logging. No broad catch, new wire value or unexpected-exception
reclassification. Malformed binding FormatException is retained as inner cause.
The local result is rejection control only, never a permission or evidence object.

## Verification and independent cone

Initial public-path red: 19 failures / two passing compatible controls. Final new
Application cases: 31 (28 behavior/geometry + three full-source pins); Worker +5.
Tests cover unit forms, missing/duplicate assignments, unknown path/style, 2D,
mapping/transformed/assembly paths, direct/bridge/dual SDR, reversed bridge endpoints,
equal units with different IDs, source replacement and existing error envelopes/log.
Guard and BOTH caller sources are pinned; shared header parser/source pins unchanged.
Three older B-spline mapper-boundary fixtures now supply valid mm/radian/product
metadata. Their original missing-geometry assertions remain; no assertion is relaxed.

RU-QuarterCone.step is a new analytic quarter-frustum, not #134's altered prism.
Six vertices, nine edges and five faces; paired opposite edge uses, Euler V-E+F=2,
closed canonical shell. Radii 1 and 2 at z=1 and 2, angle pi/4; analytic normal at
(1.5/sqrt(2),1.5/sqrt(2),1.5) is (.5,.5,-sqrt(.5)). Both import paths pass the radian
control; equivalent 45-degree declaration is rejected with the fixed prefix. Fixture,
regeneration script and actual mapped normal/closed-shell assertions are retained.
This is analytic automated evidence, not a SOLIDWORKS physical acceptance claim.

Local SDK 8.0.422/Linux: Application 461/461, Core 1366/1366; Worker 90/90 runnable
cases. The additional .exe integration case is Windows-specific and was not passed
on Linux; full Windows CI must run all 91 Worker tests. Committed Application Release
build 0 warnings/errors. Final-head full Windows CI remains required.

## Before/after compatibility and explicit owner gate

[Validation.json](evidence/RU-Import-Unit-Precheck/Validation.json) and 60 per-file
summaries retain baseline and two separate current processes. 15 real inputs were
hash-checked against the earlier manifest, plus five goldens. Each route is called
for every input, with selected planar face where one exists, otherwise a recorded
fallback. Unavailable exceptions are recorded, not zero diagnostics. All 20 rows
repeat identically; inputs unchanged. Assembly/source/fixture hashes, SDK and build
commands recorded. The final evidence-only descendant has identical producer sources;
its CI assembly bytes need not equal the measured DLL. Public reflection in the standalone profiler avoids dependency
on the internal semantic handler catalogue. Raw private CAD/geometry is excluded.

One NEW previously successful real legacy import is rejected:
**R03 P01-R03-CurvedSurfaces.STEP**, input SHA-256
319DD894747CAF95AC1100C6283B55C014D40AEAC4ECCC00EF54648ADF9AEA12.
Root #39941, representation #6306, complex relationship #130526 includes
REPRESENTATION_RELATIONSHIP_WITH_TRANSFORMATION. Target units pass mm/radian, but
this transformation path is explicitly outside the reviewed bounded rule set.
Before: legacy Succeeded. After: Failed, APP-STEP-SEMANTIC-001 with exact reason
TransformedRepresentationPath. Region already failed before; no new region success
loss. **Mehmet İlkay Kınacı's explicit approval of THIS R03 loss is REQUIRED before
merge under §35; pending, not inferred from policy acceptance or green CI.**

Festo already failed before; it now rejects earlier at unreviewed PROPERTY_DEFINITION
#14 incoming to PDS #2122. This shifts failure reason, not successful-file support.
All other 18 files retain both routes' results, including five goldens and R01/R02.
The workshop sample is 11 correlated file versions, not independent jobs. Full 20-
file profile is Linux; Windows CI repeats three public real files, five goldens and
the new cone. No Windows 11-workshop/Festo claim or automatic N1 scope expansion.

CI corpus acceptance now checks R01/R02 success and the EXACT R03 policy rejection
(code/message/IDs, null summary, exit/status, three deterministic processes), not a
generic allowance for any failure. R03 source reconciliation verifies the actual
transformation record. Existing direct R03 B-spline/surface/curve evidence remains
required and is not removed. This verifies the proposed restricted behavior, NOT
owner approval of compatibility loss. The report labels that approval PENDING.

The mandatory reflective RUUnitProbe step is replaced by public-path regression and
compatibility measurements; historical probe source and original JSON remain.
New profiler uses only public members (runtime reflection solely to load separate
baseline/current assemblies), no internal producer/catalogue access.

## Merge gates and Senior questions

Risk triggers: trust/fail-closed YES; serialized error meaning YES (existing code's
explicitly expanded use; no DTO/enum change). Shared numeric enums, IPC, runtime
persistence and UI entry NO. Independent final-diff Senior review required.

Please assess guard placement, same inventory/no C1 bypass, finite T1/T2/product
paths, strict units, double-SDR and message-prefix tests, analytic cone evidence,
unchanged wire/status/log behavior and profile completeness. Can this fix merge
AFTER explicit R03 owner approval and exact-head Windows CI? Is any new physical
acceptance needed from the actual diff? R-U bounded closure would additionally need
merge/main CI and a separately recorded decision; M2 remains OPEN regardless.
