# TOOL-01B-02 — Sandvik source-field mapping and trust boundary

Status: VERIFY-FIRST source comparison. Design/review ONLY; no import, production code, persistence, conversion or vendor data redistribution.

## Source links and evidence
- Official Sandvik holder: https://www.sandvik.coromant.com/en-us/product-details?c=c8-dclnr-55080-19 — C8-DCLNR-55080-19, material ID 5729638, holder right-hand, master insert identification CNMG 190612, machine connection C8.
- Official Sandvik insert: https://www.sandvik.coromant.com/nl-nl/product-details?c=CNMG+19+06+12-PM++++4425 — CNMG 19 06 12-PM 4425, material ID 7860702, IC 19.05 mm, thickness 6.35 mm, grade 4425, chipbreaker PM, seat size 19, neutral hand.
- Pages checked during TOOL-01B-02 review on 2026-10-25; URLs represent living catalog pages and should be captured and versioned at actual import time.

## Field mapping review
| Catalog field | MPC target | Evidence/decision |
| --- | --- | --- |
| Manufacturer and material ID | ToolProductIdentity.Manufacturer / ManufacturerProductId | Direct after source and product revision recorded |
| ISO/order designation | ToolProductIdentity.OrderCode | Keep source spelling as raw value; normalization auditable |
| EAN | ToolProductIdentity.Ean | Optional, never generated from ID |
| Insert IC and thickness | ToolInterface.InscribedCircleDiameterMillimeters / ThicknessMillimeters | Explicit conversion to mm; reject unsupported/unspecified units |
| Insert grade and substrate | CuttingGrade.ManufacturerDesignation / Substrate | Distinct from insert size and SKU |
| Chipbreaker PM | CuttingInsert.ChipBreakerDesignation | Optional and separate from grade |
| HAND | ToolHand | Requires enumerated source vocabulary mapping |
| Seat SSC_M=19 | ToolInterface.SeatSizeCode | Candidate direct mapping; must be checked against applicable vendor definition |
| Rhombic 80 / CN1906 | ToolInterface.InsertShapeCode / IncludedAngleDegrees | Requires documented mapping; never infer all interface semantics from text |
| Insert fixing hole IFS; holder clamping MTP | ToolInterface.MountingStyleCode | **Unresolved:** vendor strings vs integer domain code. Needs reviewed mapping; do NOT assume equal code or fabricate int |
| Holder CUTINT_MASTER CNMG 190612 | ToolHolder.MasterInsertIdentification | Catalog insert family reference, not exact stocked variant |
| Holder C8 interface and assembly dimensions | Future machine-side / holder fields | Not represented in current bounded insert-holder interface. Defer to separate design |

## Guardrails
1. Catalog values from different regional pages may display different units (inch vs mm). Preserve raw number/unit and explicit conversion provenance; rounding rules must be reviewed before accept.
2. The existing test pair checks a bounded interface and is NOT proof that the physical holder and insert assembled without collision.
3. Use manufacturer + product ID + catalog revision for a candidate identity key. Do not merge grade/chipbreaker variants by ISO designation alone.
4. Unmapped MountingStyleCode, unknown shape/seat vocabulary, missing source revision, or incomplete metadata yield NeedsMapping/Rejected, never a guessed compatibility result.
5. External STEP/GTC files require source hash and rights check. No actual manufacturer files committed or downloaded in this sprint.

## Next single-target gate: TOOL-01B-03
Create a test-only reviewed vendor-field map that accepts known unambiguous raw fields and explicitly marks mount-style/machine-side fields NeedsMapping. Negative coverage for unsupported vendor codes, missing source units and conflicting catalog identities. No production importer/database/toolpath changes.

## Acceptance disposition
Identity, grade and core insert dimensions have independently located vendor descriptions. The mounting-style translation into current numeric `ToolInterface.MountingStyleCode` remains **unproven** and must remain closed. Documentation-only sprint; full CI success needed before merge.
