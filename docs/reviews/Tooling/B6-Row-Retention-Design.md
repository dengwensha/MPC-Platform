# B6 — parsed-row retention: design decision

Date:2026-11-25 Europe/Istanbul. VERIFY/design; no optimization implemented.
Start:8068a398834428fb9c6c514910b10eb188f12c83 (#144 merge, Senior §42).
Owner:Mehmet İlkay Kınacı. Follow-up:#141. First consumer remains blocked.

## Evidence and current dependency boundary

Accepted R03 controlled after-GC heap:76.8MiB at target entry,283.7 after row
parsing,299.3 after incoming indexing,299.5 after A1. Interval growth is not an
exclusive graph size or natural Worker/resident peak. No extra raw evidence is
needed for this design comparison.

Current sequence:parse/classify all rows → incoming index → field/spline checks →
contexts → Bind (A1 first, then shape/bridge/product) → presentation/ownership.
Each Row keeps full Parts trees plus Record, Signature, Role and cached References.

| Consumer after parsing | Data still needed | Release constraint |
| --- | --- | --- |
| Field/spline validation | Full parameter values, numeric arrays, other row signatures | Complete checks first; preserve forward references and reason order |
| Context/uncertainty checks | Unit/context components and values, `_bad` | Keep these trees or lossless typed projections |
| A1 | SAME original inventory and selected root | Keep direct call and rejection evidence; no copied-policy substitute |
| Shape/bridge/product binding | Members/context, endpoints, SDR/PDS/product fields | Retain metadata parameters |
| Placement admission | `P(target,"AXIS2_PLACEMENT_3D")` presence | Clearing every geometry tree would change this check |
| Presentation/layers | Styled target, layer and representation members | Retain these lists and target checks |
| Closure/ownership | Cached parsed references, role/type, incoming evidence, `_bad` | Preserve order/multiplicity and negative paths |

Incoming indexing intentionally includes ORIGINAL incomplete/unknown records. It
must not switch to references from admitted rows only. All later P()/component
reads must be accounted for before implementing a release policy.

## Three options

| Option | Benefit | Cost/risk | Initial scope |
| --- | --- | --- | --- |
| A: tree only during field/spline checks; compact row stores signature/role/references | Can reduce the simultaneous AST set, including construction peak | Forward target signatures require an index first. Straightforward two-pass parse/validation adds CPU/allocation; deferred constraints are a larger validator redesign | Potential later slice |
| B: release eligible trees before A1 | Shortens overlap of validated geometry trees with A1/later scratch data | Earlier all-row construction peak and GC reservation remain; later metadata/placement reads must survive | Recommended first bounded implementation proposal |
| C: reuse an inventory representation | Could avoid duplicate parsing if a suitable shared representation exists | Current inventory has no retained typed parameter tree; adding one changes reader/model ownership and may extend AST lifetime to session release | Separate architecture work |

### A — ephemeral validation trees

The first row cannot simply be validated and forgotten: reference constraints need
OTHER row signatures, including forward references. Current classification errors
precede field errors with stable Occurrence values. A two-pass design can index
signatures first, then reparse/check/discard one tree at a time from the SAME immutable
inventory, using the existing hardened Components parser and stable ID order.
It must preserve every cause and report extra parsing/allocation. Binding/context/
presentation data still needs selected retained trees or compact projections.

### B — recommended proposal, not implementation approval

After record/field/spline AND context checks, before Bind/A1, release ONLY payloads
proved unnecessary to later checks. Initially retain unit/context, shape/bridge/
product and presentation metadata. Keeping small AXIS2_PLACEMENT_3D trees is safer
than redefining P() presence in the first slice. Unknown/unsupported/partial cases
stay conservative until their downstream reads are mapped.

Retain Record, Signature, Role, parsed References, `_bad`, `_incoming` and all reasons.
References remain ordered, multiplicity-preserving and operation-local. Release
only inside temporary Check; never mutate retained inventory/shared parser objects
or published evidence. Keep the direct same-inventory A1 call, C1 issuers and all
policies; no production forced GC, early exit on other errors or reason deduplication.

This option does NOT promise lower resident peak: all trees were built earlier and
GC segments may remain committed. Measure actual capture before/after and accept a
no-gain result if necessary; do not substitute controlled-GC heap for the budget.

### C — verified inventory limitation

StepDataEntityRecord retains RawStatement, type/component names, reference IDs,
ReadStatus and diagnostics. StepDataEntityReader extracts references structurally;
it does not publish/retain a StepParameter AST. Raw text/IDs cannot replace typed
field positions, numeric/spline checks. A new shared AST/private parse cache needs
reader/model ownership, immutability and same-byte lifetime design. Attaching a
full AST to inventory could raise the ~63MiB retained baseline until session release.
There is no small existing-AST reuse available today; defer this from B.

## §42 stale-profile prerequisite — implemented tooling guard

The builder now archives CURRENT committed HEAD, not fixed9b5f033. Before creating
output it compares normalized SHA256s of InventoryEvidence, TargetProducer and
TargetBinding with that commit, rejects changed/staged/untracked production/build
inputs, and checks source/HEAD stability around archive creation. Extracted starting
files must match before insertion. SourceCommit, checked hashes and explicit
no-approval inference are recorded. Exact-anchor failures still stop profiling;
new algorithms may require reviewed instrumentation updates. Historical #144
snapshots remain unchanged as evidence of their recorded old source.

Disposable Git regression: clean/current accepted; dirty/staged/untracked/other
source changes rejected; a committed semantically neutral future change MUST appear
in the copied source/hash. This closes silent old-copy reuse before optimization.
It does not prove a manually supplied stale DLL is current: CI must still build
canonical and diagnostic DLLs from their recorded sources.

## Implementation acceptance after design approval

- Scope initially TargetProducer and only minimal required binding support; no
  reader/inventory contract expansion. Preserve placement and metadata reads above.
- Exact decisions/ordered reasons on20 corpus files +13 controls; meaningful tests
  for the release/presence boundary, including unknown/partial and A1 negatives.
- Update/review source pins; keep C1/reflection locks, revisions, Error severities,
  same-inventory A1, all independent reasons and reference order/multiplicity.
- Actual-capture B6 before/after plus current-source heap profiles; separately report
  latency, allocation, retained heap and resident peak, including regressions.
- Exact-head Release/full Windows CI and Worker acceptance. Real Worker/private20
  on owner's16GB Windows,5s target/10s proposal, headroom/owner ceiling and external
  enforced failure remain separate #141 gates before first consumer.

Review decisions requested: accept the source guard; choose B versus A/C for the
first bounded proposal; accept retained-data constraints and implementation matrix.
This PR contains guard/design only, not production optimization.
