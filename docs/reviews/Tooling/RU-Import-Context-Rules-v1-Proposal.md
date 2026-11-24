# R-U — bounded import context rules v1 (proposal for review)

Date: 2026-11-24 Europe/Istanbul. Class: VERIFY / design preparation.
Start: 9c00253c1ceddf680ac3d3d895eb4d7ccfe43906.
Branch: docs/R-U-import-context-rules-v1. Owner: Mehmet İlkay Kınacı.
State: PROPOSED, not ratified or implemented. Refs #133 / Senior §34.
R-U OPEN; M2 OPEN. No runtime guard or normalization claim in this PR.

## Goal and constraints

Prevent current imports from silently mapping unsupported length/angle units without
waiting for the full M2 manifest. §34 accepts reject-before-mapping and requires
millimetre + radian ONLY because raw coordinates and absolute tolerances currently
lack a proven scale-independent or normalized contract. ContextUnitPolicy 1 remains
unchanged: its ability to describe metre/centimetre evidence is not import approval.
The small reviewed rules below must later be carried verbatim/versioned into manifest
v1; amendments require review, not quiet drift. They do not replace T3, positive
geometry coverage, same-session proofs or a consumer's M2 gates.

Verified code at Start: both imports require one semantic MANIFOLD_SOLID_BREP before
mapping; neither checks target-context units. ManufacturingRegion supports only a
planar face. RegionSourceValidationProcessor already maps InvalidDataException to
ImportFailed; WorkerExitCodes.OperationFailed = 3. #134 observation is merged, main
Windows CI #288 passed 2147/2147, Worker 5/5 and inventory controls 6/6. This is the
baseline, not a correction result. No production code/test is modified here.

## R1 — same inventory, precheck only

Run a shared internal Application precheck after the existing root-count/duplicate-ID
validation and BEFORE StepCanonicalMappingOperation.Map in each import. It accepts
only the exact retained StepDataInventory instance and selected source-root ID used
by that call. If guard placement requires duplicate validation earlier, preserve
existing error identity/order and add regression tests; never resolve duplicates by
first match. Missing inventory or non-unique IDs in required records cannot pass.

No path, fresh file read, temporary copy, CaptureFromFile call, detached HeaderUnitData
or caller-supplied "clean" units. Region still uses its existing captured snapshot;
legacy import's inventory is read once for this check. Its existing toroidal metadata
path is not refactored into a new lifecycle in this slice; any resulting same-input
risk found in the fix must be reported, not mislabeled solved by this inventory check.
A check result is local control data, not InventoryEvidence, a bound observation,
normalization attestation or a Ready decision. Do not change C1/K2 entry points,
construct protected neutral/observation parameters or add friend access. Reuse an
existing parser where possible; any shared decoder extraction needs its own final-diff
review and source/reason pin update without relaxing old evidence behavior.

## R2 — direct root membership and T1 exclusions

Initial admitted simple representation signatures:
SHAPE_REPRESENTATION(name, items, context_of_items) and
ADVANCED_BREP_SHAPE_REPRESENTATION(name, items, context_of_items).
Require a string name, a nonempty aggregate of direct entity references and one
resolved context reference. The selected root must occur exactly ONCE in exactly
ONE such geometry representation's item list. Duplicate membership fails even if
IDs match. Unknown/complex representation extensions cannot inherit approval from a
recognized base component. Other direct shape representation forms require an
explicit reviewed amendment. No name-text, proximity or first-context inference.

The root is not a second occurrence merely because STYLED_ITEM or its reviewed
presentation representation/layer-assignment chain refers to it (T1). Parse exact
roles: STYLED_ITEM item field, presentation representation item list and layer item
list; verify unique targets and the expected style links. An axis-placement item
beside the root is placement metadata, not another solid. These paths cannot supply
the geometry context or introduce a mapping transform. A presentation path with an
unrecognized component/relationship or hidden geometry fails classification rather
than being discarded by suffix/name. A second independent geometry representation
containing the root fails; no equivalence assumption or implicit deduplication.

Typed membership/context fields supply positive binding. A whole-inventory reverse
reference index may only detect additional/unknown incoming paths; a generic reference
scan cannot establish a context. Inventory references alone are not a STEP role proof.
Review the exact presentation type/component signatures as part of implementation,
with T001 #47/#63 versus #48/#144 and layer fixtures. This is a bounded unit binding
check, not general physical-occurrence or all-file geometry coverage proof.

## R3 — context and import-compatible units

Resolve context ONLY from the unique geometry representation's context_of_items.
Require a unique valid 3D GEOMETRIC_REPRESENTATION_CONTEXT and its
GLOBAL_UNIT_ASSIGNED_CONTEXT; parse structural components and parameter roles.
Missing/2D/unknown/partial context, missing assignments, cycles or unresolved/duplicate
references fail. Inspect every assigned candidate: no filtering of unrecognized ones.

Require exactly one length assignment AND exactly one plane-angle assignment:
- Length: direct LENGTH_UNIT() + NAMED_UNIT(*) + SI_UNIT(.MILLI.,.METRE.).
- Angle: direct PLANE_ANGLE_UNIT() + NAMED_UNIT(*) + SI_UNIT($,.RADIAN.).
No additional/unknown components or duplicate dimensions. Metre, centimetre, inch,
degree, prefixed radian and every CONVERSION_BASED_UNIT fail; a conversion base SI
record elsewhere cannot substitute. This initial guard ALWAYS requires angle units,
even for planar/no-angle input; M2's future positive-coverage U1 waiver is not applied.
A recognized direct steradian SOLID_ANGLE_UNIT may coexist as non-target dimensional
metadata only after validating its complete known form; it is not a second angle
assignment. Other assigned dimensions or unknown forms fail this initial rule set.
Different presentation contexts do not overwrite the target context's unit choice.

No coordinate/tolerance conversion. Passing the precheck means compatibility with
this existing millimetric/radian path, not proof that values were normalized to SI
metres or that geometry is correct. Any future non-mm enablement needs separately
validated normalization or scale-independent tolerances and real acceptance.

## R4 — narrow T2 bridge proposal and forbidden paths

PROPOSE support for ONE non-transforming SHAPE_REPRESENTATION_RELATIONSHIP bridge
between the unique root-containing representation and a product-associated main
SHAPE_REPRESENTATION. The root remains a direct item of only the geometry endpoint;
the main endpoint must not itself repeat the root or contain another geometry item.
Only reviewed placement metadata may appear there. Validate exactly four fields
(name string, description string or $, rep_1 reference, rep_2 reference), distinct
resolved endpoints and either declared endpoint order. Do not infer transformation
semantics from relationship name text. This is a proposed local admission rule,
not automatic support for every subtype in the STEP specification.

Require both linked 3D contexts to satisfy R3 independently. Compare normalized
semantic unit definitions (mm and unprefixed radian), not unit/context IDs: different
IDs with identical definitions may pass; metre-versus-mm, missing/conflicting units
or unknown definitions fail. Validate unique SHAPE_DEFINITION_REPRESENTATION binding
of the main representation; follow its typed product-definition-shape linkage to
check for assembly usage, not a blanket product traversal or first-reference guess.

Reject chained/branching/multiple/duplicate bridges, self-links, cycles, unknown
relationship subtypes, complex transformation extensions,
REPRESENTATION_RELATIONSHIP_WITH_TRANSFORMATION, MAPPED_ITEM, representation maps,
context-dependent assembly and occurrence/assembly placement on the root's binding
path. Inspect incoming paths to both endpoints and the product-binding chain;
unknown structural relationships cannot be silently skipped. Define the exact
accepted product-link signatures and forbidden forms with negative fixtures in the
implementation diff; unresolved classification is rejection, not implicit support.
Apply the same typed product/assembly-path checks to direct representations, not
only bridge cases. Missing/ambiguous required product linkage cannot be assumed
to mean "not assembly". This finite admitted-path set is deliberate. Known unrelated presentation metadata
is T1, not a second physical path. No mapping/assembly support is introduced.

The bounded bridge proposal is the R-U-specific T2 decision requested here; full M2
manifest adopts it only after review. Profile rejection effects; do not expand paths
inside a correction merely to turn supplier files green. Full M2 T3 remains separate.

## R5 — failures and existing Worker envelopes (proposed)

| Boundary | Proposed behavior |
| --- | --- |
| Legacy StepImportApplicationService | Failed, null summary; use existing SemanticFailureCode with explicit bounded unit/context reason and relevant source IDs |
| ManufacturingRegionImportService | Throw InvalidDataException on known precheck rejection before mapping; region stays null |
| ValidatedRegionImportService | Propagate that input failure, never return SourceLinkMatched/publish a region |
| v1.1 region Worker | Existing ImportFailed, original RequestId, exit 3 through current handler; original failure log path retained |
| v1.0 import Worker | Existing unsuccessful import envelope and operation-failed exit; verify exact current mapping in the fix |

Unit rejection must not be disguised as MissingRegion, unexpected programmer failure
or successful zero counts. Do not add broad catches or new shared enum/wire values.
Preserve parser/IO original cause and stage; internal precheck reason vocabulary is
separate from the unchanged eight-code inventory manifest. Existing semantic code's
wider input-rejection use must be explicitly reviewed; no new code is silently added.
Unknown/unexpected exceptions continue under the existing policy.

## Fix acceptance and before/after evidence

| Fixture / scenario | Required result after fix |
| --- | --- |
| T001-like mm+radian with styles, layers and axis placement | Existing behavior preserved in both imports |
| Metre or centimetre, including other context still mm | Rejected before mapping; no published region |
| Inch with base mm / degree with base radian | Rejected, never base-unit fallback |
| Missing/conflicting/duplicate target length or angle, including planar input | Rejected; unrelated context cannot rescue it |
| Valid independent closed cone, radian versus degree | Radian control works; degree rejected before mapping; explicit geometry/oracle provenance |
| Same-root second geometry representation / unknown representation form | Binding rejection |
| One exact T2 bridge, distinct IDs but same mm+radian definitions | Allowed unit binding only, other import checks still required |
| T2 different units, chain, branch, cycle, unknown or transformed extension | Rejected |
| Mapping or assembly/product occurrence on binding path | Rejected |
| Unknown/dangling path or source replaced after inventory read | No borrowed evidence or reread; mapped inventory and check identity match |
| Expected unit failure through Worker | Existing outcome, original RequestId and exit 3, diagnostic log preserved |
| Unexpected internal exception | No new catch/reclassification |

Fix PR must run ALL 15 real + five golden files through BOTH imports before/after,
using pinned identical inputs and appropriate source-face IDs per file. Record root/
representation/context, unit definitions, origin/status/diagnostic outcome, exception
stage and each new rejection's reason. An unsupported/no-planar-face baseline is a
recorded result, not an omitted sample. Never count unreadable files as clean. Record
build/source/input hashes and bounded Linux/Windows scope, correlated workshop families
and file versions; do not publish raw private CAD. Windows full regression and Worker
acceptance are mandatory. Physical acceptance is determined from the actual fix diff.

The §34 valid-cone requirement is not supplied by #134's altered-prism negative
fixtures. Provide an independently justified cone fixture/oracle before correction
acceptance; do not claim their inherited prism topology is physically valid.

Retire the reflection-based RUUnitProbe workflow step once public-path regression
coverage and final before/after evidence replace it, retaining historical JSON/source.
This is the proposed retention decision: do not maintain an indefinite mandatory CI
dependency on an internal handler catalogue. Removing it requires showing equivalent
public-path unit rejection tests; do not remove the reproduction evidence beforehand.

## Review decisions and delivery

Ask Senior separately: (1) accept R1–R3 and mm+radian-only import compatibility;
(2) accept/amend the finite T2 bridge and path/product classification;
(3) accept R5 existing error mapping, valid-cone/profile requirements and probe
retirement condition; (4) permit implementation only after this rule set merges green.
No guard implementation in this PR. Later carry the ratified rules unchanged into
manifest v1; if full M2 wants broader paths, review a versioned extension first.

Trust/fail-closed trigger YES: future rejection/compatibility semantics. Wire shape
and shared numeric enums NO; IPC NO; runtime persistence NO; UI entry NO. Existing
error-code usage is a review topic, no serialization implementation change here.
Independent final-diff review before merge. git diff --check required; final PR CI
pending. No new runtime tests for this documentation-only preparation. Reviewed/
Closing pending. Green CI cannot ratify this policy. R-U/#133 and M2 remain OPEN.

Technical reference for inherited relationship endpoint fields (not a blanket
support policy): STEP Tools, [shape_representation_relationship](https://steptools.com/docs/stp_aim/html/t_shape_representation_relationship.html)
and [representation_relationship](https://steptools.com/docs/stp_aim/html/t_representation_relationship.html), consulted 2026-11-24.
