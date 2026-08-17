# G010-A03 — Host/Worker Completion and Failure Observation

## Review Record

- Review Date: 2026-08-18
- Branch: main
- Start Commit: 5a0495c0563bc497e0024456c0419c4d56d83f6f
- Reviewed HEAD: 5a0495c0563bc497e0024456c0419c4d56d83f6f
- Classification: [VERIFY] → [NEW BUILD]
- Intervention Rule: Minimum Intervention

## Verified Starting Condition

The existing host boundary launched the worker and returned only its
process identifier. The operating-system Process instance was disposed
immediately after launch.

The host therefore could not observe:

- process completion;
- process exit code;
- observation timeout;
- wait failure;
- exit-code read failure;
- deterministic process-handle disposal.

The confirmed gap opened the narrow New Build Gate for completion and
failure observation only.

## Delivered Scope

- retained the existing worker launch validation and invocation behavior;
- transferred live process ownership through WorkerProcessHandle;
- introduced a testable IProcessExecution boundary;
- added synchronous, bounded process completion observation;
- preserved zero and non-zero worker exit codes without interpretation;
- distinguished Completed, TimedOut, and ObservationFailed outcomes;
- prevented repeated observation of the same process handle;
- made process-handle disposal deterministic and idempotent;
- preserved the existing file-based request/result transport;
- added a real Windows process-boundary integration test.

## Result Semantics

### Completed

The worker exited within the configured timeout. The exact exit code is
preserved. Exit code zero is exposed as successful completion; non-zero
exit codes remain completed process observations with unsuccessful
worker outcomes.

### TimedOut

The worker did not exit within the configured observation period. No exit
code is reported. Timeout does not introduce kill or retry behavior.

### ObservationFailed

Waiting for completion or reading the terminal exit code failed. The
failure type and message are preserved without propagating the expected
process-observation exception into the SOLIDWORKS host.

## Process Ownership

SystemProcessStarter no longer disposes the Process immediately after
launch. Ownership is transferred as:

SystemProcessStarter → WorkerProcessHandle → WorkerProcessCompletionObserver

The completion observer consumes the handle and disposes the host-side
operating-system process resource on every observation result path.
Disposal does not terminate a still-running worker.

## Validation Evidence

- Host project Debug net48 x64 build: passed with zero warnings and zero errors.
- Host unit and integration tests: 57/57 passed.
- Real process exit-code observations:
  - exit code 0 preserved;
  - exit code 7 preserved.
- Pre-integration full regression:
  - MPC.Contracts.Tests: 32/32;
  - MPC.Worker.Tests: 14/14;
  - MPC.Host.SolidWorks.Tests: 55/55;
  - MPC.Tests: 969/969;
  - total: 1070/1070.
- Final regression is required after staging this report and the physical
  process-boundary test.

## Explicitly Out of Scope

- named-pipe transport;
- long-running worker infrastructure;
- retry policy;
- automatic worker termination;
- worker pooling or parallel request execution;
- stdout or stderr transport;
- result JSON interpretation;
- application-service composition;
- SOLIDWORKS user-interface expansion;
- installer and deployment packaging.

## Architecture Assessment

The SOLIDWORKS host remains thin and net48 x64. It does not reference the
net8 worker project or interpret worker-specific exit-code constants.
Process mechanics are isolated behind host-local interfaces, while
operation-result interpretation remains assigned to the later application
boundary.

## Completion Decision

G010-A03 completion and failure observation is complete when the final
staged-scope build, targeted host tests, full regression, diff checks, and
repository-state checks pass.

Closing Commit: to be recorded after the completion gate.
