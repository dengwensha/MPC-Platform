# TOOL-01C-02 — Independent Senior Review Package

**Status:** Ready for independent review, not approved. Documentation-only Verify-First gate. Reviewers must record an explicit verdict and evidence; CI SUCCESS is not a substitute for architecture approval.

## Scope and reviewed baseline
- `docs/reviews/Tooling/TOOL-01-Architecture-Discovery.md` — initial domain inventory and staged plan.
- `docs/reviews/Tooling/TOOL-01B-Versioned-Catalog-Intake-Contract.md` — catalog source, provenance and rejection boundary.
- `docs/reviews/Tooling/TOOL-01B-02-Sandvik-Field-Mapping-Review.md` — vendor field mapping and unresolved mounting style.
- `docs/reviews/Tooling/TOOL-01C-Persistence-Assembly-Identity-Contract.md` — identities, versions and transactions.
- `src/MPC.Tooling/ToolProductIdentity.cs`, `ToolProducts.cs`, `ToolInterface.cs`, `Compatibility/InsertHolderCompatibilityEvaluator.cs` — existing implementation, not a persistence model.
- `tests/MPC.Tests/Tooling/ManufacturerCatalogIntakeContractTests.cs`, `VendorFieldMappingContractTests.cs`, `ToolAssemblyRevisionContractTests.cs` — **test-local proof fixtures**, not production importer or assembly logic.

## Reviewer questions — mandatory independent disposition
| ID | Decision requested | Acceptance evidence / failure mode |
| --- | --- | --- |
| SR-01 | Is manufacturer+product ID+source revision a safe catalog revision key? | Show collision/renaming/merger cases; specify normalization and explicit source revision rules |
| SR-02 | Can raw source SHA, extraction method, unit and review lineage be retained immutably? | Trace one sample row source -> raw -> mapped value and reject changed hash |
| SR-03 | Can a product revision remain stable while different assemblies reference it? | Verify no implicit last-write-wins and no historical assembly mutation on catalog refresh |
| SR-04 | Is assembly revision identity distinct from catalog SKU, physical stock instance and master insert designation? | Check uniqueness, missing references, duplicate holder/insert role and source revision ambiguity |
| SR-05 | Is vendor vocabulary conversion fail-closed? | Unreviewed numeric `MountingStyleCode`, HAND, seat vocabulary and unit conversion must be NeedsMapping, not inferred |
| SR-06 | Are preview, explicit reviewed commit, quarantine, idempotency and rollback coherently specified? | Identify atomic boundaries, authorization and conflict outcome; test-only fixtures do not yet prove transactions |
| SR-07 | Does a compatible interface remain distinct from physical/machine safety approval? | Explicitly keep machine connection, collision, holder stack length, adapter fit, CAD verification and cutting clearance outside early acceptance |
| SR-08 | Are persistence provider, schema migrations and indexing choices still appropriately deferred? | Recommend minimum provider-neutral model and migration gate; do not silently assume relational DB or introduce tables now |
| SR-09 | Are adapter and multi-insert extensions safely deferred without breaking identity design? | State whether role/slot cardinality needs an extensibility seam; no implementation requested |
| SR-10 | What is the smallest safe implementation slice after approval? | Specify tests, rollback, acceptance and out-of-scope for one prospective PR only |

## Critical risks to challenge
1. **Source identity:** `SourceHash` placeholders and test-local `Reviewed` strings are fixtures, not supplier-issued provenance or authenticated approvals.
2. **Non-production validation:** `ToolAssemblyRevisionContractTests` checks static in-memory references only; it does not verify durable storage, concurrency, duplicate transactions, cryptographic hashes or actual insert/holder compatibility.
3. **Numeric mapping gap:** supplier fixing/clamping vocabularies have no proven translation to integer `MountingStyleCode`; unresolved status blocks auto-assembly.
4. **Hidden identity collision:** order code, EAN and master insert designation are not safe global primary keys.
5. **Physical qualification:** catalog suitability/shape matching cannot establish physical assembly, machine installation or toolpath clearance.
6. **Revision history:** changing external catalog source must never rewrite an assembly's pinned product revisions.

## Requested review outcome
Reviewer to return:
- Verdict: **APPROVE / APPROVE WITH CONDITIONS / REWORK / REJECT**.
- Per-item SR-01..SR-10: **PASS / GAP / NOT REVIEWED**, evidence file/line and rationale.
- Severity-ranked findings (Critical/High/Medium/Low), remediation owner and proposed minimal PR boundary.
- Distinguish architectural blockers from deferred follow-up; list conditions before a production persistence implementation.
- Verify proposed sample cases against original supplier documents independently where available; otherwise mark **Unverified**.

## Project decision gate
**NO-GO** for implementing database, migrations, catalog importer, permanent assembly store, adapter graph or CAD/CNC integration until review verdict and explicit acceptance of blocking findings.

A subsequent TOOL-01C-03 decision record may disposition review findings; avoid assuming all reviewer suggestions require code changes. Release build/full CI and reviewable clean Git history remain mandatory for any approved changes.

## Independence and delivery note
This file is the **review brief**, not a completed independent Senior Review. No independent reviewer has signed off through this PR. CAD-GE03B physical R01 verification is a separate pending workstation acceptance.
