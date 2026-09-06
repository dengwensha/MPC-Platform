# P01-CL01 - Final Real-Part Revalidation and Closure Assessment

## Classification and decision

Classification: VERIFY.

P01 validation assessment: COMPLETE WITH FINDINGS.
Fixed-corpus compatibility: PARTIAL - 2/3.
R03 full compatibility: OPEN.

This completes the final planned validation assessment authorized by
P01-SC01. It does not establish full STEP compatibility, authorize B14,
or approve unrelated product scope.

Documentation publication remains pending remote verification.

## Traceability

| Field | Value |
|---|---|
| Branch | main |
| Start Commit | 4f0095636092fad2eee8038ee2564e67a4cf154e |
| Reviewed HEAD | 4f0095636092fad2eee8038ee2564e67a4cf154e |
| Verified remote main before physical execution | Same as Reviewed HEAD |
| Physical execution date, local | 2026-09-07 |
| Evidence session | P01-CL01-20260907-003909-baf8604c |
| Initial and post-execution working tree | Clean |
| Closing Commit | The commit introducing this document and its two companion evidence files |
| Production, test and build-configuration mutation | NONE |

Build, tests and physical processes were executed locally by the project
owner; their outputs were supplied in the session. This assessment is
not a new independent senior review. Prior SR02 evidence retains its
original static-review limits.

## Governing scope

P01-SC01-Scope-Freeze-and-Final-Planned-Work-Decision.md governs this
assessment. B13 remains the last delivered B-series capability.

Historical reports remain unchanged. Their conclusions apply to their
own reviewed baselines.

## Build and full regression

Commands executed at Reviewed HEAD:

- dotnet build src/MPC.sln -c Release
- dotnet test src/MPC.sln -c Release --no-build --no-restore

Release build: PASS, 0 warnings, 0 errors; elapsed 00:00:11.08.

| Test project | Passed | Failed | Skipped |
|---|---:|---:|---:|
| MPC.Tests | 1112 | 0 | 0 |
| MPC.Contracts.Tests | 35 | 0 | 0 |
| MPC.Worker.Tests | 16 | 0 | 0 |
| MPC.Application.Tests | 11 | 0 | 0 |
| MPC.Host.SolidWorks.Tests | 69 | 0 | 0 |
| Total | 1243 | 0 | 0 |

Counts match SC01. Host tests ran from bin/x64/Release/net48.

## Corpus identity

Inputs remained outside the repository. Byte sizes and SHA256 values
were checked before execution and rechecked afterward.

| Part | Bytes | SHA256 |
|---|---:|---|
| R01 | 23222 | E518DB3332DC665B16CC31DA960C46B75B8D75128234129922A25146BE1C33AD |
| R02 | 46323 | 0E9E2D6E15E1328C068E30703AD621AE883B6FE94A5DB51353A0D26F82E1578F |
| R03 | 14003088 | 319DD894747CAF95AC1100C6283B55C014D40AEAC4ECCC00EF54648ADF9AEA12 |

## Physical execution method

Nine separate dotnet processes executed the Release worker:

src/MPC.Worker/bin/x64/Release/net8.0/MPC.Worker.dll

Invocation: dotnet "<worker-dll>" "<request-json-path>" "<result-json-path>".

Each request used ContractVersion "1.0", Operation 0 (AnalyzeModel),
a distinct GUID RequestId and the corresponding InputArtifactPath.
The observation timeout was 120000 ms per process.

The local evidence session retains session metadata, deployed-file hash
manifest, per-run invocation, request, result, stdout, stderr, execution
metadata and ordered payload files. Files included in the pre-run
worker-output manifest were hash-checked again after execution.

Duration is process wall time including startup; these measurements
are validation observations, not a performance benchmark.

The ordered fingerprint is SHA256 of the PowerShell-generated UTF-8
JSON representation of the entire parsed result excluding RequestId.
Diagnostic array order is retained. It is a session comparison method,
not a cross-serializer canonical JSON specification.

## Nine-run results

| Part | Run | Exit | Status | Duration ms | Diagnostics |
|---|---:|---:|---|---:|---:|
| R01 | 1 | 0 | Succeeded | 271 | 0 |
| R01 | 2 | 0 | Succeeded | 264 | 0 |
| R01 | 3 | 0 | Succeeded | 267 | 0 |
| R02 | 1 | 0 | Succeeded | 274 | 0 |
| R02 | 2 | 0 | Succeeded | 280 | 0 |
| R02 | 3 | 0 | Succeeded | 275 | 0 |
| R03 | 1 | 3 | Failed | 2813 | 2772 |
| R03 | 2 | 3 | Failed | 2750 | 2772 |
| R03 | 3 | 3 | Failed | 2776 | 2772 |

RequestId was preserved in 9/9 results. Each part had identical ordered
payload fingerprints across its three runs. No worker crash, timeout
or APP-STEP-UNEXPECTED-001 was observed.

R03 had no successful summary in any failed result.

| Part | Solid | Outer shell | Inner shell | Faces | Loops | Edges | Vertices |
|---|---:|---:|---:|---:|---:|---:|---:|
| R01 | 1 | 1 | 0 | 11 | 12 | 24 | 16 |
| R02 | 1 | 1 | 0 | 23 | 28 | 48 | 32 |

Both signatures match SC01 acceptance criteria.

## R03 diagnostic reconciliation

All 2772 diagnostics were attributed to source entities in the
SHA256-verified R03 input. The inventory indexed 135828 records.
There were zero unclassified diagnostics.

| Diagnostic code | Source family | Count per run |
|---|---|---:|
| MAP-STEP-FACE-001 | Standalone B_SPLINE_SURFACE_WITH_KNOTS | 696 |
| STEP_REFERENCE_RESOLUTION_FAILED | Standalone B_SPLINE_CURVE_WITH_KNOTS | 1796 |
| STEP_REFERENCE_RESOLUTION_FAILED | Complex rational B-spline surface | 276 |
| STEP_REFERENCE_RESOLUTION_FAILED | Complex rational B-spline curve | 4 |
| Total | | 2772 |

For the 696 mapping diagnostics, both the ADVANCED_FACE source and its
reference to the reported surface were checked.

These are observed diagnostic counts, not complete entity inventory
counts. In particular, 1796 observed standalone curve diagnostics must
not replace B11's inventory of 1910 standalone curves. Surface-first
traversal limits downstream visibility; no one-to-one inventory coverage
claim is made.

B12 semantics are visible in the physical path: 696 standalone surfaces
are recognized as StepBSplineSurfaceWithKnotsEntity but lack advanced-face
mapping support. B13's geometry contract alone does not compose that path.
Rational/complex surfaces and B-spline curves remain unresolved.

The total happens to equal B10's historical 2772. It was measured anew;
the current split is 2076 reference failures plus 696 mapping failures.

## Signed-toroidal boundary

Current source inventory: 120 TOROIDAL_SURFACE records, including 12
negative-major records. Corrected diagnostic reconciliation found zero
toroidal-family diagnostics and zero diagnostics targeting those signed IDs.

Current Application composition still supplies the scoped signed-toroidal
policy; the semantic catalog and advanced-face mapper retain their toroidal
and degenerate-toroidal paths.

Together with the passing full regression, the observed execution shows
no recurrence of the accepted signed-toroidal blocker. Absence of these
diagnostics is not proof of complete geometric correctness or R03 success.

## Evidence-helper correction

The initial Part 5 helper misclassified standalone entities and reported
zero toroidal entities. Part 5A established the actual source record format.
Part 5B reproduced a tr-TR difference between case-insensitive and
case-sensitive regex matching, then used case-sensitive CultureInvariant
Regex for corrected classification.

Part 5B supersedes the initial derived source classification. Original
worker requests/results and the nine physical runs were unchanged.
This was an evidence-helper correction, not a production-code fix.

## Remaining compatibility work

Separately authorized capability delivery is still required for:

- standalone B-spline surface mapping and advanced-face composition;
- rational/complex surface semantics and mapping;
- non-rational and rational B-spline curve support and composition;
- subsequent end-to-end physical revalidation of R03.

B14 remains subject to its own NEW BUILD gate and is insufficient by
itself for full R03 compatibility. SC01's resolver design qualification
and other scope exclusions remain in force.

## Evidence and delivery

Repository companions:

- P01-CL01-Physical-Worker-Runs.csv
- P01-CL01-R03-Source-Reconciliation.json

Local detailed evidence:
C:\projects\MPC-P01-Evidence\P01-CL01-20260907-003909-baf8604c

The repository CSV preserves all nine RequestIds, result hashes, ordered
fingerprints, timings, status codes and topology signatures. The JSON
preserves corrected family counts and the twelve signed source IDs.
Detailed raw evidence remains local; the repository companions do not
embed proprietary STEP inputs or substitute for those raw files.

This delivery contains exactly three documentation/evidence files.
Final delivery requires verifying the closing commit's parent, exact
changed paths, remote publication and clean Git state.
