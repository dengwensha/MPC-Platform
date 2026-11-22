# M2 measurement review follow-up — same-build controls

Date: 2026-11-22 Europe/Istanbul. VERIFY only; no runtime/policy changes.
N1: MEASURED, ACCEPTANCE PENDING. M2: OPEN. PR #121 remains draft.
Measured production source: `098c2ee3323676e3b8245ff5e28c87b6f7a6531b`.
Follow-up starts at `35e5fb9d5ead037bd1081f076d11e50b2a4443e1`.
Session: `131bdab3-392e-4c32-a299-463025656b70`.

## Same-build controls and repeated real observations

The follow-up harness references the existing compiled DLLs, without rebuilding
production sources at the documentation HEAD. It asserts the original reader
assembly SHA-256 `F8B9530C0F554FA32CC9315927BD44482D48FC15C583DB650A629F5E15CAAACE`.
External harness Release build: 0 warnings / 0 errors, SDK 8.0.422.
Every call creates a new reader and validator. Every input is observed in two
separate processes; each process reads path then captured bytes. PID, UTC times,
session ID, OS, runtime, source commit and DLL hash are recorded per observation.
No Windows or SOLIDWORKS execution is claimed: both processes run on Linux.

| Synthetic control | Expected and observed result, both entries/processes |
| --- | --- |
| Clean unsupported entity, .stp | Succeeded, zero aggregate and record diagnostics; not coverage proof |
| Empty complex entity | SucceededWithDiagnostics, one record Error and one aggregate Error, code STEP_COMPLEX_ENTITY_COMPONENTS_NOT_FOUND |
| No DATA section | Failed, one aggregate Fatal STEP_DATA_SECTION_NOT_FOUND; no inventory, record counts null |
| Simple COMPLEX_ENTITY discriminator | FormatException, observation unavailable, diagnostic counts null |
| Missing input | Capture unavailable, nonzero exit; no fabricated clean counts |
| Expected hash mismatch | Capture unavailable, nonzero exit; no parse-based observation |

All six controls passed using the original reader DLL. All 14 real inputs passed
again: available Succeeded observations, all Complete records, zero aggregate AND
record Info/Warning/Error/Fatal, unchanged source hashes, matching result digests
between path/captured and across the independent processes. Repeats do not add
independent sample files. There are 72 successful harness calls to Observe across
18 inputs (including the intentionally unavailable parse exception observations),
plus two pre-parse capture controls. This count is not a product acceptance count.

## Trace, digest audit and review dispositions

[Evidence](evidence/M2-Workshop-20261122-v2/) includes manifest linking original
archive hash -> archive entry -> STEP hash, all 36 per-input/process JSON outputs,
two capture-failure outputs and Validation.json. Manifest checks exactly 11 workshop,
3 reference and 4 synthetic inputs, unique names, and .step/.stp extensions.
The six fixture expectations are asserted by the historical run script. File bytes
from both archives match the actual measurement inputs. Reader and validator source
pins remain those in the [original record](M2-Workshop-Inventory-Measurement.md).
Git HEAD/status recorded in Validation.json identify the clean documentation
checkout at measurement time, not the independently pinned DLL build source.

Full R01 result serializations were saved locally for both modes/processes and
their SHA-256 recomputed from actual output bytes. Validation.json records byte
counts, exact top-level/record property lists and matching digests. The full outputs
contain raw geometry statements and are deliberately not committed to GitHub.
Local digest audit is complete; the reviewer's requested full-output package has
not been distributed and is not claimed independently reviewed. It can be inspected
in a controlled review without adding raw CAD to this PR. This is explicit pending
review acceptance, not a silent replacement of the requested deliverable.

Aggregate diagnostics already include record diagnostics in production reader
lines 145-146. The error control proves record Error is preserved, counted once
in each separate view, never added twice to the aggregate. No claim is made that
a Warning-specific fixture was exercised in this follow-up.

BACK PLATE: owner says the four-body situation was not intentional and may have
been overlooked while editing. The unmodified original is baseline; corrected
version is a suspected unintended multi-body outcome, not successful repair.
Actual CAD body structure/intent awaits SOLIDWORKS inspection; inventory BREP
counts alone are not validated topology. No healing or file modification performed.

Two S26352 files have equal type totals, but the inputs differ by SHA-256 and
full result digest. Same-job correlation was already acknowledged; geometric
independence is not established. Independent-binomial population bounds do not
apply to this small correlated sample; no false precision or population estimate.

Remaining before N1 acceptance: review these controls/session/record counts and
the digest-audit disposition; Windows repeat was recommended, not performed.
CAD inspection of BACK PLATE remains separate and does not turn inventory parity
into geometric suitability. N2/A4 consumer obligations and N3/B6 remain open.
All risk triggers No for this historical measurement/review record. PR CI is
required for the final documentation head; old-head green does not approve it.
