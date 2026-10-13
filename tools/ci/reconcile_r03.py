import collections
import hashlib
import json
import pathlib
import re
import os

root = pathlib.Path(os.environ["RUNNER_TEMP"]) / "mpc-full-validation" / "corpus"
source = list(root.rglob("P01-R03-CurvedSurfaces.STEP"))
assert len(source) == 1, "Expected one R03 source"
raw = source[0].read_bytes()
digest = hashlib.sha256(raw).hexdigest()
assert digest == "319dd894747caf95ac1100c6283b55c014d40aeac4eccc00ef54648adf9aea12"
# STEP statement boundaries respect escaped apostrophes inside strings.
statements, current = [], []
quoted = False
# One-to-one byte decoding for ASCII STEP syntax; no header text interpretation.
text = raw.decode("latin-1")
i = 0
while i < len(text):
    c = text[i]
    current.append(c)
    if c == "'":
        if quoted and i + 1 < len(text) and text[i + 1] == "'":
            current.append(text[i + 1])
            i += 1
        else:
            quoted = not quoted
    if c == ";" and not quoted:
        statements.append("".join(current).strip())
        current = []
    i += 1
assert not quoted, "Unclosed STEP string"
records = {}
for statement in statements:
    match = re.match(r"^(#\d+)\s*=\s*(.*);$", statement, re.S)
    if match:
        entity_id, body = match.groups()
        assert entity_id not in records, "Duplicate source ID"
        records[entity_id] = body
assert len(records) == 135828, len(records)

def family(body):
    if body.startswith("("):
        # Ignore quoted text when extracting component names.
        clean = re.sub(r"'(?:''|[^'])*'", "''", body)
        names = re.findall(r"\b([A-Z][A-Z0-9_]*)\s*\(", clean)
        return "COMPLEX[" + "+".join(names) + "]"
    match = re.match(r"([A-Z][A-Z0-9_]*)\s*\(", body)
    assert match, "Unclassified source"
    return match.group(1)

families = {key: family(body) for key, body in records.items()}
results = sorted((root.parent / "evidence").rglob("R03-*-result.json"))
assert len(results) == 3, "Expected three R03 result files"
runs = []
fingerprints = []
for path in results:
    payload = json.loads(path.read_text(encoding="utf-8-sig"))
    diagnostics = payload["Diagnostics"]
    groups = collections.Counter()
    ids = []
    for diagnostic in diagnostics:
        assert diagnostic["Code"] == "STEP_REFERENCE_RESOLUTION_FAILED", diagnostic
        match = re.fullmatch(r"STEP semantic entity '(#\d+)' could not be resolved\.", diagnostic["Message"])
        assert match, diagnostic
        entity_id = match.group(1)
        assert entity_id in records, entity_id
        ids.append(entity_id)
        groups[families[entity_id]] += 1
    fingerprint = hashlib.sha256(json.dumps(diagnostics, sort_keys=True).encode()).hexdigest()
    fingerprints.append(fingerprint)
    runs.append({"File": path.name, "Count": len(diagnostics), "UniqueIds": len(set(ids)), "Groups": dict(groups), "DiagnosticFingerprint": fingerprint,
                 "ToroidalDiagnosticCount": sum("TOROIDAL" in families[x] for x in ids)})
assert len(set(fingerprints)) == 1, "Diagnostic runs differ"
report = {"SourceSHA256": digest, "SourceEntities": len(records),
          "EvidenceRun": os.environ["GITHUB_RUN_ID"], "ReviewedCommit": os.environ["GITHUB_SHA"],
          "Runs": runs, "BSplineInventory": dict(collections.Counter(v for v in families.values() if "B_SPLINE" in v))}
(root.parent / "evidence" / "b17-reconciliation.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report, indent=2))
with open(os.environ["GITHUB_STEP_SUMMARY"], "a", encoding="utf-8") as output:
    output.write("## R03 source reconciliation\n\n")
    output.write("Source hash verified; three physical results from this build reconciled.\n\n")
    for name, count in runs[0]["Groups"].items():
        output.write(f"- {name}: {count}\n")
