# TOOL-01C-03 policy decision record

Date: 2026-10-28 Europe/Istanbul.
Reconciled against main `3ba5f9c034305e3427b3aaa54aab836f57f34a6e`.
This record distinguishes observed implementation, proposed policy and reported
conversation decisions. It is not a backdated user sign-off.

## Sources and approval limits

The conversation supplied the policy draft and Senior conditional-GO copy dated
2026-10-26. Their technical recommendation columns explicitly do not constitute
user approval. Their existence is not proof that every G1 requirement was met.
The subsequent implementation/merge directions are reported session context;
their exact original dates and complete decision-specific wording are not
available as repository evidence. Do not invent an approval sentence or date.

The present owner instruction authorizes correcting these records. It does not
by itself convert all P1..P7 proposals into accepted production policy.
Original G1/pre-merge review trace remains unresolved where not evidenced.
A new explicit ratification may establish a present decision, not historical
compliance.

## P1 through P7

| Policy | Observed implementation or recorded direction | Decision status and remaining work |
| --- | --- | --- |
| P1 assembly revision | Design/test-local distinction: candidate listing versus assembly-dependent evaluation | PROPOSAL / ratification not evidenced here; durable historical resolution OPEN (TL-05) |
| P2 unknown units | Proposed dependent-evaluation block, without guessed conversion; raw record may be retained | PROPOSAL / ratification not evidenced here; production intake/unit path OPEN (TL-03/TL-07) |
| P3 EAN/GTIN | #85 preserves exact raw Ean; format/check digit status is separate; invalid text does not throw | IMPLEMENTED bounded behavior; historical explicit approval trace OPEN; format is not source authentication, uniqueness or a product merge key |
| P4 master insert | Current ToolHolder trims MasterInsertIdentification; it does not resolve an exact SKU | PROPOSAL for raw text plus separate versioned normalization; raw preservation is NOT implemented by the trimmed field |
| P5 field identity | Current required identity fields are trimmed; no reviewed vendor-specific alias/normalization pipeline | PARTIAL PRINCIPLES / details and ratification trace OPEN; controlled #86 models do not prove production source conflict handling |
| P6 hand/mounting | #85 rejects Unknown or null mounting before interface approval; InterfaceOnly scope; Neutral behavior preserved | IMPLEMENTED narrow correction; historical explicit approval trace OPEN; Neutral matrix and numeric vendor mounting meanings uncertified |
| P7 composition location | Discovery records user direction for bounded composition in Application, separate project deferred; #89/#90 implement source linkage there | REPORTED LOCATION DECISION; no Application-to-Tooling reference or full bridge authorization; original policy draft's deferral is superseded only for that narrow location |

## P3 exact behavior and equality limits

`ToolProductIdentity.Ean` retains source text exactly. Null yields NotProvided;
empty or whitespace-only text yields NeedsReview. A trimmed candidate passes
ValidFormat only for 8/12/13/14 ASCII digits with a valid check digit; only then
is FormatValidatedGtin populated. No supplier lookup or source/product binding
is performed. Valid format does not prove a real or unique product.

The record's equality includes the raw value and other stored state. Equality
of these objects must not be represented as an approved product deduplication,
source revision or assembly identity policy. P5 requires a separate explicit
field-wise decision. This documents SR-L1, not a new normalization behavior.

## P6 stable values and characterization

ToolHand is Unknown=0, Left=1, Right=2, Neutral=3. Left/Right retain their prior
numbers; default now means Unknown. Neutral's numeric change and the reviewed
baseline's lack of serialized ToolHand consumers must not be generalized to
future consumers. Enum numeric changes trigger independent Senior review.

Null MountingStyleCode means unmapped and cannot approve an interface match.
Existing integers, including controlled fixture value 2, have not acquired a
reviewed manufacturer meaning. Known Neutral matching is characterization of
existing behavior, not certification of hand semantics.
Linear/angular tolerances remain 0.001 mm / 0.001 degree.

## Evidence is not production closure

- #85: actual production-model negative tests and InterfaceOnly scope.
- #86: domain construction tests with controlled data, no source intake mapper.
- #87: test-local revision coexistence, no production persistence/history.
- #88: retained byte snapshot integrity, no supplier authentication or revision association.
- TL-03, TL-05 and TL-07 remain OPEN at production level.
- Missing master text provenance, normalization details and Neutral/vendor code
  semantics remain OPEN; no source correctness is inferred from test names.

See [scope and SR02 disposition](TOOL-01D-01-07-Scope-and-SR02-Disposition.md)
for CI and source review limits. This documentation changes no runtime behavior,
tolerance, enum, importer or persistence.
