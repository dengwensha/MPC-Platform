# A two-pass throwaway experiment — Senior §44

Date2026-11-25 Europe/Istanbul. Start7aebe6d2de57916d76ea2d55aa88937120f5c6e0.
Branch experiment/B6-A-two-pass. NEVER MERGE THIS EXPERIMENT AS PRODUCT.
A production PR requires separate review after evidence and any owner tradeoff.

## Implementation boundary

Pass1 parses each record with hardened Components, records complete signature,
role and ordered references, emits existing classification findings, then retains
only the explicit B component-name list. Every name survives; unknown nonempty
payloads become empty name-only markers too. There is no retained full geometry
AST set. Original inventory and incoming negative evidence stay unchanged.
Pass2 emits cached unresolved-reference findings in the original order. Admitted
rows are reparsed from SAME immutable RawStatement one at a time. A temporary Row
serves existing field and P()/spline checks, restored in finally before the next
row. A1, context/product/style binding, ownership, C1, revisions/codes/severities
remain unchanged. No source file reread, AST added to inventory, deferred checks,
parallelism or forced product GC. Temporary rows never become observations.

## Accepted bounds and reporting

20 exact input hashes (InputManifest.json),13 established +5 downstream negatives,
two process trials. Full ordered semantics equal. Linux and Windows SEPARATE.
Each R03 trial natural process peak must fall≥15%; less means rejection.
R03 warm time≤20% increase; anybaseline>1s file≤25%; firstcall≤10s.
Warm allocation≤30% increase;retained±1MiB;one-time initialization≤1MiB.
Time delta smaller than inter-trial variation is no measured change. If peak
passes but time/allocation does not, PRESENT owner tradeoff, never auto-reject or
approve. Explicit512MiB first-consumer prerequisite is unchanged.
First-minus-warm allocation is only an initialization proxy; baseline/current and
incremental values are reported separately, not called exclusive static-table size.
The1MiB interpretation (total versus A increment) is awaiting owner clarification.

B's corrected rejection reason is NO NATURAL PROCESS-PEAK GAIN; small one-time
allocation and noisy median differences do not decide rejection (§44).

## Platform execution

Linux:20 private/reference/golden inputs,38 semantic cases,2trials and120 fresh
cost processes. WindowsCI:9 public inputs +13+5 negatives only; public evidence
must say ScopeIncomplete for full20. Raw atölye/Festo CAD is never uploaded.
Windows20 on the owner's16GB machine uses the runner below with manifest hash
checks. It is independently pending until the resulting evidence is returned.

Prepare a clean baseline worktree at7aebe6d and a clean checkout of the experiment
review head. Build each MPC.Application Release with SDK8.0.422 before measurement.
Then from experiment checkout PowerShell:

```powershell
./tools/ci/B6CaptureProbe/Invoke-AExperiment.ps1 -BaselineApp 'C:\path\baseline\src\MPC.Application\bin\Release\net8.0' -CurrentApp 'C:\path\experiment\src\MPC.Application\bin\Release\net8.0' -Inputs 'C:\path\exact20' -Output 'C:\path\fresh-evidence'
```

The runner checks the20-file manifest before reading, includes38 exact audit
comparisons and2cost trials, and deletes only its own generated fixture copies
when done. Reports contain names/hashes/technical results, not raw geometry.
PublicSubset is CI-only scope disclosure, never private20 acceptance.

Evidence and exact source/DLL/CI identities are appended after execution. No
conclusion or product acceptance is claimed while measurements are pending.
