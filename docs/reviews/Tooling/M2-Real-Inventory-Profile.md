# M2 real STEP inventory diagnostic profile

Date: 2026-11-19 Europe/Istanbul. Class: VERIFY / measurement only.
Reader source baseline: `de1ba770b26c3b04e7913bc9b37a807dacb9a6f7` (merged #113).
Policy A is owner-accepted and Senior section 19 approved; no production gate is implemented.

## Inputs and method

Download the existing ci-inputs-v1 p01-real-parts.zip release package and verify
SHA256 `497d4fc9b7df2d2b5ff93b89fa7ad2b78fd53ba5ddd26ddb85898ef2dbc516da`.
The three individual hashes are pinned in the existing CI corpus manifest and
in the linked JSON profiles below. Raw STEP files are not added to the repository.

The non-production M2InventoryProbe reads each original file once, verifies its
expected hash, writes those bytes to a private temporary STEP snapshot and calls
the existing StepDataEntityReader with StepDataInventoryValidator. It measures
raw-inventory diagnostics before semantic mapping or Application outcome translation.
Aggregate result diagnostics already include record and validator diagnostics,
so record diagnostics are not counted a second time. Snapshot cleanup follows.
Reader assembly/informational version and assembly SHA256 are recorded alongside
the supplied source commit. That supplied commit is provenance input, not attestation.
This JSON must never be used as importer-owned Application eligibility evidence.

Local measurement: Linux x64, .NET SDK 8.0.422, fresh Release build, 0 warnings/errors.
The reader informational version identifies the baseline above. The tool's own
code is reviewable in this delivery; it does not alter that production reader.

## Observed sample

| File | Inventory records | Inventory status | Error | Fatal | Diagnostics | Profile |
| --- | ---: | --- | ---: | ---: | ---: | --- |
| R01 Prismatic | 329 | Succeeded | 0 | 0 | 0 | [R01.json](evidence/M2-Real-Inventory-Profile/R01.json) |
| R02 InternalBoundaries | 649 | Succeeded | 0 | 0 | 0 | [R02.json](evidence/M2-Real-Inventory-Profile/R02.json) |
| R03 CurvedSurfaces | 135828 | Succeeded | 0 | 0 | 0 | [R03.json](evidence/M2-Real-Inventory-Profile/R03.json) |

Attempted files: 3; available observations: 3; read failures: 0;
files with Error/Fatal: 0/3 (0% in this fixed sample). Total inventory records:
136806, all with reader-level Complete metadata. Diagnostic code/severity
distribution is empty for each input. These facts concern raw inventory only.

This is a curated three-file validation corpus, not a representative workshop
sample. It does not establish the real-world rejection rate, false-blocking rate,
whole-file semantic completeness, supported geometry, or manufacturing suitability.
In particular R03's successful raw inventory is not proof that every geometric
entity maps or that any future evaluator may accept it. Do not silently weaken A
based on these measurements or infer full-file approval from zero diagnostics.

Workshop calibration files measured: 0; unavailable/not collected in this session.
The real-file follow-up is PARTIALLY COMPLETE for R01/R02/R03 and remains OPEN
for workshop inputs and interpretation before implementation. Accountable owner:
Mehmet İlkay Kınacı; implementation assistant maintains measurements and evidence.
No physical SOLIDWORKS acceptance is claimed.

## Repeatability and measurement controls

Build `tools/ci/M2InventoryProbe/M2InventoryProbe.csproj` in Release. Invoke:

```text
dotnet M2InventoryProbe.dll <input.step> <expected-sha256> <source-commit> <output.json>
```

Exit 0 means a reader observation was obtained, including a possible returned
Failed inventory with Fatal diagnostics; it never means eligibility or no errors.
Exit 1 means a probe/read exception; exit 2 means invalid arguments or hash mismatch.
For unavailable observations diagnostic counts remain null, never synthetic zero.
The previous output is removed before a new valid invocation to prevent stale
success evidence. No error-message matching or production exception translation
is introduced.

`tools/ci/check_m2_inventory_probe.py` runs six process controls: complete record,
one partial-record Error counted exactly once, Fatal/missing DATA inventory,
FormatException unavailable, hash mismatch, and missing input file. Local result:
6/6 PASS. These are tool controls, not the full production policy acceptance suite
or the complete diagnostic-code/severity pinning requested by Senior section 19.

CI builds the probe, executes these controls and profiles the same three pinned
real inputs. Profiles are retained by the existing evidence-artifact step. The
measurement step requires available observations/correct hashes, not zero Errors;
it does not implement policy A or add an inventory eligibility check to production.
The final PR CI must pass independently of local measurements.

## Remaining work and scope

Before implementation, add actual workshop calibration inputs with source hashes,
reader revision, sample origin and separate read-failure accounting. If rejection
incidence is high, retain A; any evaluated-solid-reachable partial-scope alternative
needs separate design/review. Full code-to-severity pinning, importer-only immutable
evidence, pure Application decision design, consumer verification and the separate
supported-geometry/completeness gate remain OPEN. M2 is not closed.

Production source, contracts, enum values, severity assignments, IPC, persistence
and UI are unchanged. Probe code, its controls, CI measurement and observation
records only. Risk triggers for this measurement diff: wire No; enum No;
trust/fail-closed No (no eligibility output or accepted meaning change); IPC No;
persistence No; production UI entry No. Standard review and its own full CI apply.
The future production gate retains its independent review requirement.
