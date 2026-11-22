# M2 workshop inventory measurement and N1 disposition

Date: 2026-11-22 Europe/Istanbul. Class: VERIFY, factual evidence only.
Start / measured source: `098c2ee3323676e3b8245ff5e28c87b6f7a6531b`.
Branch: `docs/M2-workshop-measurement-N1`. Closing: pending delivery.
Owner: Mehmet İlkay Kınacı; implementation assistant maintains evidence.
State: N1 MEASURED, ACCEPTANCE PENDING after independent measurement review; M2 OPEN.

## Sample and provenance

Owner confirmed local development/test permission, workshop origin and conversion
using SOLIDWORKS 2024 / AP214. Five S26352 files belong to one machining job;
five mold parts belong to one mold. BACK PLATE was manually corrected in geometry
before export; nine other parts had no correction/simplification. The additional
BACK_PLATE_ORIGINAL is the unmodified comparison version of the same part.
Thus 10 parts, 11 file versions and two families, not 10 independent jobs.
No raw STEP files or raw entity statements are published with this record.
Specific export options/service pack and detailed manual edits were not supplied.

## Method and trace

External observation harness, Linux x64, .NET SDK 8.0.422, Release. It captures
each input into a byte array, hashes it, calls production Read(path), then
ReadCaptured on a non-writable MemoryStream over those bytes, and repeats the
captured read once. It rechecks the source hash after reads. Serialized complete
result objects are compared by SHA-256, including status, inventory records,
raw statements, record metadata/references/diagnostics, type counts and result
diagnostics. Order is preserved; no filtering or diagnostic-message normalization.
This is observation only, not importer-owned Application evidence or eligibility.
The historical harness source is included as text, not added to production or CI.
Its SourceCommit literal is the measured commit, not a reusable attestation.
Input capture outside its catch can abort a run: JSON absence is not clean evidence.
Here all 14 expected observations exist and are available; none were excluded.

Reader source SHA-256 (UTF-8 BOM removed, LF):
`22A2E16F54C6D418B288D169C185D0102632BD598237D2A993907E9D230BCC45`.
Validator source SHA-256:
`FB5869D3EB0FE8B5507F4041A221CBC2DB88FE16917C5BC7DA7472B6771B7301`.
Reader assembly SHA-256:
`F8B9530C0F554FA32CC9315927BD44482D48FC15C583DB650A629F5E15CAAACE`.
Per-input hashes, complete-record counts, type counts, diagnostics, three result
digests and parity/repeat flags are in [evidence](evidence/M2-Workshop-20261122/).
Source/assembly pins identify this local build; no Windows binary identity is implied.

## Observations

All 14 files: Succeeded, all records Complete, zero diagnostics, unchanged input
hash, equal path/captured results and equal captured repeats.

| Input | Records | Role |
| --- | ---: | --- |
| 0_S26352_0700_01_9001 | 9167 | Machining job |
| 1_S26352_0300_05_2201 | 879 | Machining job |
| 2_S26352_0300_05_2210 | 5519 | Machining job |
| 3_S26352_0300_05_2118 | 935 | Machining job |
| 4_S26352_0300_05_2114 | 935 | Machining job |
| BACK_PLATE_ORIGINAL | 37844 | Mold, unmodified baseline |
| BACK PLATE | 26124 | Same part, manually corrected comparison |
| CAVITY BASE | 40333 | Mold |
| CAVITY STEEL | 41350 | Mold |
| CORE BAS | 58568 | Mold |
| CORE STEEL | 82444 | Mold |
| R01 | 329 | Fixed reference |
| R02 | 649 | Fixed reference |
| R03 | 135828 | Fixed reference |

Observed Error/Fatal incidence: machining 0/5, mold unmodified baseline 0/5,
corrected comparison 0/1. Baseline 0/10 parts; all supplied versions 0/11.
Two correlated families and one converter do not estimate general workshop or
false-blocking incidence. No policy relaxation follows. Both BACK PLATE versions
have no inventory diagnostics despite a 11720-record difference: no geometric
equivalence, validity, repair effectiveness or machining suitability is inferred.
R03 raw inventory success does not supersede its historical semantic-layer limit.

## Disposition and next boundary

N1 real-input path/capture comparison is MEASURED, ACCEPTANCE PENDING.
The supplied independent measurement review requires same-build faulty controls,
explicit record diagnostic counts and session/archive provenance before acceptance. Workshop collection/measurement is complete for this
bounded set; broader diversity and any high-incidence interpretation remain future
measurement work. Original historical profiles are retained unchanged.

M2 remains OPEN: separate supported-geometry/completeness and header/unit gates,
detailed failure-cause propagation (A4), first consumer enforcement and end-to-end
consumer validation are still required. N2 checkpoint omission is checked in the
first consumer PR; N3/B6 assembly-hash performance remains open for measured work.
No new header/unit policy, consumer, wire meaning or scope approval is introduced.
Next micro-sprint is coverage/header/unit gate design, before first consumer.

No SOLIDWORKS physical acceptance was performed or required for this factual
record. Existing separate diagnostic physical follow-up remains open. The revised
Excel's previously reported validation defects are not fixed by this measurement.

## Validation and review scope

The external Release harness completed 42 reads across 14 inputs. Evidence was
checked for expected file count, hashes, common assembly pin, all-record counts,
empty diagnostics, parity/repeat flags and matching digests. Required PR CI remains
pending; no fresh full-regression result is claimed here. Production sources are
unchanged; measured source's historical CI #252 is not this delivery's CI.

All six risk triggers No: no wire/serialization boundary, shared enums, trust or
fail-closed policy, IPC, runtime persistence lifecycle or production UI changes.
JSON is historical observation format only. Standard factual review and required
CI apply; no independent approval of a new boundary is claimed.

## Independent measurement review received 2026-11-22

The owner supplied MPC STEP Atölye Ölçümü — Test Sonuçları İncelemesi.
The review confirms the 14 JSON summaries but requests stronger measurement
controls. PR stays draft; CI success alone cannot accept N1. No numerical
observation is retracted, but no closure or product suitability is inferred.

Verified omitted structural distinction: BACK_PLATE_ORIGINAL has 1
MANIFOLD_SOLID_BREP / 1 CLOSED_SHELL / 979 ADVANCED_FACE; corrected BACK PLATE
has 4 / 4 / 760. These are inventory entity counts, not validated bodies.
Whether the split was intentional is pending owner clarification and CAD check.

Follow-ups before acceptance: same-build known Error/Fatal controls; separate
record-level severity counts; new reader per read and process-independent repeat;
a complete R01 serialized result or equivalent reproducible digest audit;
archive-to-entry hashes and UTC/session/OS/build provenance; expected input
manifest, .stp support and controlled unavailable output. Do not publish raw
R01 geometry statements without checking corpus redistribution scope.

Reader source lines 145-146 append record.Diagnostics into result diagnostics,
so aggregate Diagnostics already includes record-level warnings/errors. Explicit
record totals improve auditability; they must not be added again to aggregate
totals. The harness actually reads path, capture, then capture repeat; any earlier
prose suggesting another order is superseded by this method and harness source.

Two S26352 inputs have equal type totals; they are not independent jobs and
matching counts do not prove identical geometry. A binomial independent-sample
95% upper bound is not justified for this correlated two-family sample.
No population rejection-rate confidence claim is made.

## Same-build control follow-up

See [review follow-up](M2-Workshop-Measurement-Review-Followup.md): six controls
passed against the original DLL; 14 inputs repeated with new readers and separate
processes; explicit record-level counts and archive/session provenance added.
N1 remains MEASURED, ACCEPTANCE PENDING. Owner confirmed the BACK PLATE
multi-body result was not intentional; original is baseline, corrected version
is a suspected editing issue pending CAD inspection. M2 remains OPEN.
