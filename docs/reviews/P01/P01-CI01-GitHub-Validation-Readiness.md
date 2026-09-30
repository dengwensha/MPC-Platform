# P01-CI01 — GitHub validation readiness and partial .NET 8 CI

Classification: HARDEN, following current-source VERIFY.
Status: draft delivery; CI execution and full-environment prerequisites are separate gates.

## Traceability
- Date: 2026-09-30 (Europe/Istanbul)
- Branch: ci/p01-ci01-github-validation
- Start / reviewed product baseline: 1de9960f2f46efecc5fb576aff0da211d22212a5
- Reviewed delivery: this commit's workflow and decision record, added to that baseline.
- Closing: pending; no complete project-validation closure is claimed.

## Goal and verified need
Enable reviewable GitHub-only build/test evidence without changing product behavior.
At Start, the recursive repository tree contained no .github workflow and no AGENTS.md.
The Actions runs query for Start returned zero runs.
B15 records 1273 passing tests executed locally on its implementation; this is historical
evidence, not a new GitHub run.

The project owner requested continuation using GitHub only. This delivery changes only
automation and this record; it does not authorize new STEP capabilities or waive gates.

## Delivery
A Windows-hosted workflow uses the SDK pinned by global.json (8.0.422), explicit Release
builds without incremental reuse, warnings-as-errors, and the existing four .NET 8 test
projects. Worker tests retain x64. Each test invocation reuses its immediately preceding
build with the same platform. TRX validation rejects zero tests, failures and unexecuted
tests, and retains observed counts and commit/run provenance as artifacts.
GitHub Actions dependencies are pinned to verified commit IDs.

The workflow is explicitly named partial. It neither builds the full solution nor runs
the Host project. The B15 historical .NET 8 subtotal is 1204; actual CI counts must be
observed, not inferred or hardcoded as proof of success.

## Open prerequisites and scope boundaries
- MPC.Host.SolidWorks targets net48 x64 and references three assemblies under
  SolidWorksApiRedistPath: SolidWorks.Interop.sldworks.dll, swconst.dll and swpublished.dll.
  No matching DLLs are tracked and no release assets were available at review.
- No source change, substitute Interop package, fake Host or weakened analyzer rule is
  introduced to bypass this dependency. An authorized compatible assembly source or
  suitable GitHub runner is required before full solution build/Host regression.
- CL01 explicitly keeps proprietary R01/R02/R03 STEP inputs and raw results outside
  the repository. The tracked golden fixtures are not substitutes for that corpus.
- No private corpus upload, new physical execution, or new R03 diagnostic distribution
  is claimed. Corpus compatibility remains last-observed PARTIAL 2/3; R03 remains OPEN.
- Developer workstation cleanliness cannot be established through GitHub APIs.
  The workflow verifies only its own ephemeral checkout.

## Acceptance and completion
1. Workflow plus record only; no production/test-source changes.
2. Four .NET 8 Release builds and TRX-verified test runs succeed.
3. Evidence identifies tested commit, actual counts and exclusions.
4. Full solution/Host and private-corpus prerequisites remain explicitly open.

A green partial job is useful CI evidence, not permission to close full regression or
physical validation. Keep the PR draft while environment disposition remains unresolved.
No independent senior review is claimed.
