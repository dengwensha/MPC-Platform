# TOOL-01B — Versioned Manufacturer Catalog Intake Contract

Status: VERIFY-FIRST / DESIGN-ONLY. No importer implementation, vendor data download, persistence, manufacturing behavior or tolerance change.

## Source baseline
Current `MPC.Tooling` models: `ToolProductIdentity` (manufacturer, manufacturer product ID, order code, optional EAN), `CuttingInsert` (interface, grade, chip breaker, hand), `ToolHolder` (required interface, master insert identification, hand), `ToolInterface` (shape/seat, diameter, thickness, included and clearance angles, mounting style). `InsertHolderCompatibilityEvaluator` is a bounded shape/interface checker, not evidence of assembly or machine fit. TOOL-01A covers mismatch behavior.

## Import envelope — required for all records
- `schemaVersion`: explicit version (initial proposal `1`), unknown version is rejected.
- `recordKind`: `CuttingInsert` or `ToolHolder`; other types are deferred, not silently coerced.
- `source`: manufacturer name, catalog/document identifier, revision/version, publication date when supplied, source URL/file identifier, original source SHA-256 and extraction method/reviewer.
- `identity`: manufacturer's product ID and order code. EAN optional; missing EAN is **not** an error and is never synthesized.
- `measurementUnits`: explicit millimeters and degrees, or an explicitly declared validated conversion at intake; unlabeled units fail closed.
- `interface`: insert shape code, seat size code, inscribed-circle diameter, thickness, included angle, clearance angle, mounting style; required fields must be present and valid for the corresponding current model.
- `evidenceStatus`: `Unreviewed` initially; catalog extraction and geometry assertions are not independently verified by the model constructor.

## Kind-specific required fields
- `CuttingInsert`: grade designation and substrate, hand; chip breaker and coating information optional. Distinguish base ISO-style insert designation, chip-breaker designation and grade/manufacturer variant; do not derive missing fields from textual SKU assumptions.
- `ToolHolder`: master insert identification and hand. The master insert identification is a catalog reference/constraint, **not** automatically resolved to a unique stocked insert SKU. Preserve raw text and normalization provenance.

## Identity and duplicates
- Key candidate: normalized manufacturer identity + manufacturer product ID + catalog revision. Order code is an alternate search key, **not** a global unique ID.
- An identical identity with conflicting dimensions, hand, grade, catalog revision or interface MUST be quarantined for review; no last-write-wins.
- The same ISO insert family can have multiple chip-breaker/grade/vendor variants. Do not coalesce distinct purchasable SKUs.
- Reject unknown/invalid numeric values, NaN/infinity, negative critical dimensions, unsupported enumerations and contradictory units. No silent default dimensions, shape, hand, grade or mounting style.
- Keep raw vendor values alongside normalized candidate data and transformation diagnostics so import is reproducible.

## Transaction and trust boundary
1. Parse in a staging/preview process; apply strict schema and provenance validation before constructing domain models.
2. Generate a per-record outcome: `AcceptedForReview`, `Rejected`, or `NeedsMapping`, with stable code and source-row identifier.
3. No persistent catalog commit without explicit reviewed preview acceptance and an idempotent, scoped commit decision.
4. Unknown vendor semantics (mounting style, seat naming, hand) remain `NeedsMapping`; do not infer compatibility from a superficially matching product designation.
5. Vendor PDFs, spreadsheets, ZIPs and STEP files remain untrusted inputs. External downloads, redistribution and vendor licensing need separate authorization.
6. No claim of physical assembly or machine compatibility arises from successful catalog import.

## Proposed test fixtures (future TOOL-01B-implementation, not delivered here)
- Valid minimal insert: `CNMG 19 06 12-PM 4425` and holder `C8-DCLNR-55080-19`, matching the already-pinned test-domain records; retain their distinct manufacturer product IDs.
- Negative: missing dimensions, missing holder master insert identification, duplicate identity with conflicting thickness, nonfinite dimension, incorrect units, unknown mounting-style mapping, modified catalog SHA, unsupported schema version, grade variant incorrectly coalesced.
- Positive: optional EAN absent, case/whitespace normalization with raw value retained, repeat preview producing identical diagnostics.

## Acceptance and scope
TOOL-01B is complete when this versioned intake boundary and fail-closed requirements are peer-reviewed, and full Release/CI succeeds for documentation changes. No production importer, live vendor feed, database schema or UI is in scope. Next: TOOL-01B-01, an isolated **test-only schema acceptance proof** before any runtime ingestion.

## Dependencies
GitHub-only discovery is enough now. Actual supplier catalogs and CAD/STEP files on the other workstation are future optional physical acceptance sources, not grounds for inventing catalog records. CAD material-side qualification stays separate and `Unverified`.
