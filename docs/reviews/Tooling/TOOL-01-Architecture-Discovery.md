# TOOL-01 — CNC Tool Library Verify-First Architecture Discovery

Status: DISCOVERY ONLY. No source mutations, catalog import, database, deployment, or physical acceptance.

## Confirmed source inventory (current main)
- `src/MPC.Tooling/MPC.Tooling.csproj`: .NET 8 class library.
- `ToolProductIdentity`: manufacturer, manufacturer product ID, order code, optional EAN.
- `ToolInterface`: normalized shape and seat codes; inscribed circle diameter, thickness, included and clearance angles, mounting style, validated finite values.
- `ToolProducts`: `CuttingInsert`, `CuttingGrade`, `ToolHolder`, hand information and master insert designation.
- `Compatibility/InsertHolderCompatibilityEvaluator`: deterministic comparison of shape, seat, diameter, thickness, angles, mounting style, hand. Current internal tolerance 0.001 mm / 0.001 deg.
- `tests/MPC.Tests/Tooling/InsertHolderCompatibilityEvaluatorTests.cs`: Sandvik C8-DCLNR-55080-19 holder / CNMG 19 06 12-PM 4425 insert compatible example; shape and thickness mismatch, invalid dimensions.
- The source inventory does not itself establish persistence, import, catalog completeness, or support for assemblies and machines.

## Scope boundary and risk notes
1. Preserve distinct manufacturer catalog identity, product variant, grade, interface geometry, and compatible assembly; do not collapse order code, ISO insert designation and tool assembly into one ID.
2. Current `InsertHolderCompatibilityEvaluator` checks an intentionally narrow interface. It is **not** proof of machine compatibility, interference-free machining, spindle/turret fit, or complete catalog interchangeability.
3. Any catalog import must preserve source vendor, document/version/date, units and provenance; missing critical interface fields must fail closed rather than invent numeric values.
4. Explicitly distinguish toolholder, cutting insert, assembled tool, adapter/extension, insert grade and catalog/document resource before adding database persistence.
5. No new tolerance policy, STEP geometry mapping, CAD simulation or NC toolpath in TOOL-01.

## Recommended bounded implementation sequence
- **TOOL-01A (next):** verify compatibility contract and negative-test coverage; specifically inspect shape/seat, mount/hand, missing fields, numeric tolerances and master-insert identification. Single outcome: evidence-backed gap list; code only if a proven mismatch is found.
- **TOOL-01B:** define versioned external catalog intake schema and provenance, with minimal samples for the existing Sandvik holder/insert pair; no vendor download automation until allowed and licensed.
- **TOOL-01C:** define library persistence and assembly references, without conflating individual catalog SKUs with assembled tools. Require independent review of the schema before coding.
- **TOOL-01D:** integration boundary to CAD/CNC analysis, only after catalog and compatibility acceptance.

## TOOL-01A acceptance
- Baseline source and tests inspected against current main, exact commit tracked.
- One discrepancy/risk matrix covering dimension precision, matching semantics, brand/catalog variants, hand and missing data.
- Out-of-scope documented; if implementation is required, one target PR only, Release/full CI, clean commit chain, no CAD analysis changes.

## Dependencies and schedule
- GitHub-only discovery and tests can be performed without workstation catalog files.
- Evening/local dependency: catalog ZIPs, STEP inserts/toolholders, supplier drawings, CAD fit visualization and external source authorization.
- Main CNC priority stays CAD analysis -> tool library -> cutting CNC analysis. CAD physical evidence 09B remains parked awaiting the other workstation.
