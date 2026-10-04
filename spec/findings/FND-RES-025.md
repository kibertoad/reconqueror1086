---
id: FND-RES-025
title: SETUP relocations target selectors rather than the SIERRA filename offsets
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    kind: file-data
    offset: 0x00004BDE..0x00004DD8
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    kind: file-data
    offset: 0x00003F01..0x00003F03
  - build: BLD-GOG-EN
    file: CD:SETUP.EXE
    kind: file-data
    offset: 0x00003FB4..0x00003FB6
tool: Python complete bounded NE relocation-table and selector-chain traversal
environment: null
---

## Observation

The complete source matches BLD-GOG-EN's length and canonical fingerprint.
FND-RES-024 supplies its NE segment mapping. Segment 1 has relocation metadata;
segment 2's relocation-present flag is clear. At file offset 19422, following
segment 1's stored bytes, a two-byte count is 63. All 63 eight-byte records fit
through file offset 19928. No table or record-count cap was reached.

Using SRC-NE-REFERENCE's target-kind constants, 61 records are imported-ordinal
references and two are internal references. Both internal records have source
type 2, flags 0 and target offset zero: one names segment 1 and the other segment
2. Type 2 is selector-only, not a segment-and-offset pointer. No internal record
names segment 2 with target offset 818 or 842, the filename offsets recorded in
FND-RES-024. This absence covers the complete declared table only.

The segment-2 selector record begins at file offset 19480. Its source-chain head
is segment-1 offset 14868. Reading the stored next-link word gives 14689, whose
next link is 65535, ending the chain. Both two-byte source extents lie inside
segment 1, and a visited-offset set found no cycle. The corresponding file-data
word locations are 0x00003FB4 and 0x00003F01. These words were read as relocation
chain data, not decoded as instructions. The additive flag is clear.

The loader metadata therefore names two selector patch sites; it does not reveal
which instructions surround them, what register eventually holds the selector,
or how filename offsets are supplied. No original program was run, no code
range was established, and no function inventory or code reading is claimed.

## Interpretation

Searching for direct filename target relocations cannot locate these consumers
in the declared table. Instruction analysis must retain the selector provenance
and independently trace fixed offsets or constructed pointers. The data-selector
chain supplies concrete sites for that next reading without proving DS identity
or a reachable file open.

## Alternatives

- Direct internal relocation records to either SIERRA filename offset are ruled
  out within the declared table.
- A selector-only record fixing a complete far filename pointer is inconsistent
  with its source type; offset provenance is still required separately.
- Near data-offset references, constructed pointers and a different consumer
  route remain possible. The relocation absence proves none of them absent.

## How to reproduce

Verify SETUP source identity and FND-RES-024's NE segment mapping. Read each
segment's relocation-present flag. Bound the complete declared record table and
classify every record by source type and target kind under SRC-NE-REFERENCE.
Check internal target offsets against both filename offsets. Follow the
non-additive data-selector source chain by two-byte links, bounding every link,
rejecting cycles and requiring its terminal word. Keep source words as loader
metadata; export no bytes, instructions or broad relocation listing. Obtain a
correctly mapped function inventory before starting code analysis.
