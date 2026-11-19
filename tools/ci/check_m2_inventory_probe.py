"""Measurement-tool controls; no production eligibility decision is tested here."""
import hashlib
import json
from pathlib import Path
import subprocess
import sys
import tempfile

probe = Path(sys.argv[1]).resolve()
dotnet = sys.argv[2] if len(sys.argv) > 2 else "dotnet"
source_commit = "0" * 40  # An explicitly supplied smoke-fixture label, not production provenance.
with tempfile.TemporaryDirectory(prefix="mpc-m2-probe-checks-") as folder:
    root = Path(folder)
    output = root / "observation.json"

    def run(source, expected_hash, expected_exit):
        process = subprocess.run(
            [dotnet, str(probe), str(source), expected_hash, source_commit, str(output)],
            capture_output=True, text=True, timeout=30,
        )
        if process.returncode != expected_exit:
            raise AssertionError((process.returncode, expected_exit, process.stderr))
        return json.loads(output.read_text(encoding="utf-8"))

    def measure(name, content, expected_exit=0):
        source = root / (name + ".step")
        source.write_bytes(content)
        return run(source, hashlib.sha256(content).hexdigest(), expected_exit)

    clean = measure("clean", b"ISO-10303-21; DATA; #1 = UNSUPPORTED_ENTITY(); ENDSEC; END-ISO-10303-21;")
    assert clean["InputHashVerified"] and clean["ObservationAvailable"] and not clean["ReadFailed"]
    assert clean["ErrorCount"] == clean["FatalCount"] == 0

    partial = measure("partial", b"ISO-10303-21; DATA; #1 = ( ); ENDSEC; END-ISO-10303-21;")
    assert partial["DiagnosticCount"] == partial["ErrorCount"] == 1
    assert partial["FatalCount"] == 0 and partial["RecordStatusCounts"] == [{"Status": "Partial", "Count": 1}]
    assert partial["DiagnosticGroups"] == [{"Code": "STEP_COMPLEX_ENTITY_COMPONENTS_NOT_FOUND", "Severity": "Error", "Count": 1, "SourceEntityCount": 1}]

    fatal = measure("fatal", b"not a STEP DATA section")
    assert fatal["ObservationAvailable"] and fatal["ReadFailed"] and fatal["EntityCount"] is None
    assert fatal["ErrorCount"] == 0 and fatal["FatalCount"] == 1

    exception = measure("exception", b"ISO-10303-21; DATA; #1 = COMPLEX_ENTITY(); ENDSEC; END-ISO-10303-21;", 1)
    assert not exception["ObservationAvailable"] and exception["ErrorCount"] is None
    assert exception["ExceptionType"] == "FormatException"

    mismatch = run(root / "clean.step", "A" * 64, 2)
    assert not mismatch["InputHashVerified"] and not mismatch["ObservationAvailable"]
    assert mismatch["ErrorCount"] is None and mismatch["DiagnosticCount"] is None
    assert mismatch["ExceptionType"] == "InputHashMismatch"

    missing = run(root / "missing.step", "A" * 64, 1)
    assert not missing["ObservationAvailable"] and missing["ErrorCount"] is None
    assert missing["ActualSha256"] is None and missing["ExceptionType"] == "FileNotFoundException"

print("PASS: 6/6 inventory measurement controls (not production eligibility tests).")
