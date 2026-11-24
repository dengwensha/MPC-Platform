# R-U — current import context-unit risk: bounded reproduction

Date: 2026-11-24 Europe/Istanbul. Class: VERIFY. Owner: Mehmet İlkay Kınacı.
Start: e2015d5ce6f7ef649d5b5fce957de692d5b1f1ae. Refs #133, Senior §33.
Scope: observe existing behavior and review a correction boundary; no production fix.
R-U OPEN. M2 OPEN. No new target/coverage capability or geometry eligibility.

## Reproduction and limits

RUUnitProbe derives seven public synthetic inputs from T001_PRISM. Exact text
replacements are checked for unique anchors. It normalizes source CRLF to LF and
writes UTF-8 without BOM, records each input hash and checks file unchanged. No
private workshop/Festo data is needed. Full fixture generation is in the probe.

Actual calls: StepImportApplicationService.Execute; ManufacturingRegionImportService
TryCreate on planar face #30; ValidatedRegionImportService with its exact hash and
SourceLinkOnly. Separately trace the same root mapping using the production semantic
handler catalogue (audit-only reflection) and inspect face #8's mapped cone angle.
The trace does not mint InventoryEvidence or any trusted readiness proof. Legacy
StepUnitResolver is observed separately; the two imports do not call it.

Length variants preserve all geometric coordinates and topology, changing only
#61 target context's unit assignment. The inch unit references 25.4 millimetres;
its SI base is not the assigned context unit. The metre variant intentionally
leaves the separate presentation context millimetric, demonstrating why inventory-
global unit inference is inadequate. Identical numeric origins under different
source length units are not evidence of normalized internal dimensions.

Angular variants replace face #8's PLANE with CONICAL_SURFACE radius 1, semi-angle
0.5, using the existing placement; degree is defined using its radian factor.
The root's other face #30 remains planar and is requested for region publication.
These are deliberately perturbed NEGATIVE geometry fixtures: the original prism
boundary is not reconstructed as a geometrically valid closed cone. This proves
unit-blind acceptance/mapping on these exact records, not valid cone-solid support,
physical CAD correctness or population incidence. A valid independent cone fixture
is required before the future correction's full geometry acceptance; do not silently
claim this probe supplies it. Region itself remains planar, not a published cone.

## Baseline observations

Local .NET SDK 8.0.422/Linux observation (raw summaries committed separately):

| Case | Target context | Observed existing behavior |
| --- | --- | --- |
| C01 control | millimetre / radian | Both imports succeed; SourceLinkMatched, planar region published |
| C02 inch | INCH / radian | Same origin numbers as C01; legacy resolver reports Millimetre; imports/gate still succeed |
| C03 SI contrast | metre / radian; presentation context millimetre | Same origin numbers; global resolver Unknown length; imports/gate still succeed |
| C04 angular control | millimetre / radian, cone field 0.5 | Mapped angle 0.5; planar region from same root published |
| C05 degree | millimetre / DEGREE, cone field 0.5 | Mapped angle 0.5, not expected ~0.00872664626 radians; both imports succeed |
| C06 missing | millimetre, no target angle assignment | Mapped angle 0.5; unrelated radian records do not prove target angle; imports succeed |
| C07 conflicting | radian + DEGREE in target context | Mapped angle 0.5; imports succeed |

All seven inventory results are Succeeded without inventory diagnostics. That
confirms inventory policy A alone cannot establish units; this is not an A change.
SourceLinkMatched still means provenance only and is not itself a false promise
of dimensional or machining eligibility. The defect is absent unit enforcement
before geometry mapping, not broken hash correlation or a cone-region contract.

Successful probe execution means observation was captured, NOT safe import or
expected-bad behavior acceptance. There is no assertion requiring wrong-unit success;
a later fix can change the recorded results without preserving the bug. Missing
results, invocation failures and exceptions remain distinct, never zero diagnostics.
CI stores observations in the validation artifact and logs compact seven-case JSON.
No raw customer data, UUID-bearing topology or temporary paths are committed.

## Proposed correction boundary — not implemented/ratified

Recommend a separate, bounded fail-closed guard BEFORE semantic geometry mapping
in BOTH current imports: unsupported conversion length/angle, absent required units,
conflicting assignments and unresolved context binding must not produce a region
or a successful root import. Do not silently convert degrees or infer millimetres
from a conversion base; do not fix this only inside the unused legacy resolver.
Use units from the exact bytes/inventory actually mapped; a fresh path read cannot
supply trustworthy proof. Preserve existing correlation, failure envelopes and
unrelated exception policy; inspect actual Host/Worker effects in the fix PR.

Context binding is a prerequisite, not already provided by #132. Before coding,
review a small explicit set of admitted representation-to-context paths (T1 style
is not another physical target; unresolved/mapped/ambiguous paths fail closed).
T2 non-transforming bridge support is still undecided. Avoid a global "all SI
candidates look safe" shortcut. If an interim all-context restriction is proposed,
make its stricter rejection boundary explicit, profile the 15 real/5 golden set
and get independent acceptance rather than broadening the manifest covertly.

A guard alone does not prove metre/radian coordinate normalization. Current SI
coordinates are unchanged; do not claim normalized output, enable calculations or
close M2 until its separate normalization/coverage and consumer requirements pass.
The reviewed fix must explicitly preserve or label source-unit coordinate behavior
and test physically equivalent SI length representations; any conversion is a
separate validated contract, not an incidental mapper refactor.

Required future red/green: valid unit-only inch prism, valid angular cone in degree/
radian, missing/conflicting angle context, same/different unit multiple contexts,
foreign/modified source, no publication on rejection, compatible real/SI corpus,
both imports and applicable Worker status/RequestId. Independent final-diff review,
Windows full CI/main CI and scoped physical decision before bounded #133 closure.

## Review request and traceability

Please confirm (1) the bounded risk reproduction and planar-vs-cone distinction,
(2) the narrow reject-before-mapping approach versus conversion, and (3) prerequisites
for exact context binding and SI compatibility. A proposal needing a context manifest
must not bypass the existing manifest/T2/T3 review gates. No production fix or issue
closure in this PR. Work ownership and priority remain #133, independent of M2.

Risk triggers: wire, enum numeric values, runtime trust-rule change, IPC, persistence,
UI entry all No for the actual probe/document/CI-only diff. Proposed future guard is
trust/fail-closed Yes and requires independent review; obtain design feedback here
before implementation. Final PR CI pending. Local probe Release build 0 warnings/
errors; full Windows regression is provided by the PR workflow, not a local claim.
Reviewed/Closing pending; measured source/fixture/DLL hashes and committed-code
identity are recorded in evidence/RU-Unit-Risk/Validation.json.
