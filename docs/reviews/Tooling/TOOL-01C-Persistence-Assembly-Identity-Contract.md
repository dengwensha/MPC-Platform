# TOOL-01C — Tool Library Persistence and Assembly Identity Contract

Status: VERIFY-FIRST architecture discovery; no database, production model, migration, API, deploy or import changes.

## Confirmed source baseline
- `MPC.Tooling` is a .NET 8 domain library with `ToolProductIdentity`, `CuttingInsert`, `ToolHolder`, `ToolInterface`, `CuttingGrade`, and `InsertHolderCompatibilityEvaluator`.
- Product identity currently contains Manufacturer, ManufacturerProductId, OrderCode, optional EAN. This is a catalog product identity, **not** an installed assembly or inventory item identity.
- ToolHolder stores required insert interface and MasterInsertIdentification (insert family designation, not a resolvable exact SKU).
- No production persistence or assembly graph is established by the inspected source. TOOL-01B catalog intake contract is design/test evidence, not a runtime importer.

## Explicit identities and cardinalities
1. **CatalogSourceVersion**: manufacturer, document/revision, immutable source SHA-256, unit/provenance record. One source version can support many catalog revisions.
2. **CatalogProductRevision**: stable internal record ID + manufacturer/product ID + source revision, kind (insert, holder; adapter in later phase), raw order code and normalized fields. A product can have multiple revisions, and grade/chipbreaker alternatives must retain different purchasable identities.
3. **PhysicalToolInstance** (deferred): a serialized, stocked or issued instance with asset/location/lifecycle data. Never infer from catalog SKU count.
4. **AssemblyRevision**: internal immutable assembly ID, revision and review status. Multiple assemblies may use the same catalog product; an assembly can be revised without mutating catalog facts.
5. **AssemblyComponent**: parent assembly revision, role (holder/insert/adapter), explicit referenced product revision, position/quantity and, where needed later, verified transform and slot. Reference only identified revisions; no string-only implicit matching.

## Initial assembly restrictions
- First implementation candidate: exactly one holder and one cutting insert, with no adapters, machines or transforms. Expanded graph is deferred.
- Before creating a review-eligible assembly, resolve each component revision uniquely, preserve source provenance and check `InsertHolderCompatibilityEvaluator`. Compatibility result is a *bounded interface check* only, not proof of safe physical fit.
- `MasterInsertIdentification` is a declared family constraint; matching that text is not authority to choose any stocked insert product or bypass geometric checks.
- Unknown unit conversion, vendor MountingStyleCode mapping, part version, missing provenance, or conflicting SKU revision => `NeedsMapping`/`Rejected`, never auto-assembled.

## Persistence/transaction guarantees
- Immutable supplier source record and materialized normalized candidate must be linked; keep original source values, raw units and normalized mapping diagnostics.
- Use distinct unique keys: source (manufacturer, document ID, revision, SHA), catalog revision (manufacturer identity, manufacturer product ID, source revision), assembly (assembly ID, revision). No global uniqueness claim for order code, ISO family, EAN or toolholder master insert text.
- Duplicate exact source ingestion should be idempotent; conflicting payload under same identity must be quarantined rather than last-write-wins.
- Catalog revision changes must not silently rewrite historical assemblies; an explicit new assembly revision is needed.
- Preview/commit authorization is separate; failed import must not leave partial accepted records; keep change audit and provenance.
- Never delete catalog records referenced by historical assemblies without explicit retention/migration policy.

## Boundary of the first implementation (TOOL-01C-01)
A **test-only** in-memory identity and assembly contract fixture:
- Accept two uniquely identified candidate catalog revisions (holder/insert), create an independent assembly revision referring to both.
- Reject duplicate revision IDs with conflicting payload, missing component revision, same role twice, unknown vendor mapping and silent replacement of a referenced revision.
- Prove same insert SKU may be referred to by multiple assembly revisions without merging those assemblies.
- Mark outcome `EligibleForReview`, not `PhysicallyCompatible`; production code and database unchanged.
- CI Release/full regression required. Physical fitting and supplier CAD geometry deferred.

## Decision, risks, future gates
- **GO** for small test-only identity/assembly invariants; **NO-GO** for DB schema/migrations and production assembly features until reference model and adapter scope have independent design review.
- ToolLibrary -> CAD/CNC integration is later TOOL-01D; CNC toolpath, machine capability, cutting data and simulation remain out of scope.
- CAD-GE03B R01 physical acceptance remains a separate pending external workstation task. Do not conflate a catalog assembly with a verified machining setup.
