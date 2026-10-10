---
id: FND-PERSON-011
title: The shipped character table gives row zero AGE 12
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x021B5D42..0x021B5D76
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x00C292DC..0x00C29B3F
tool: Independent bounded Python decoding from FMT-RES-001/002/003 and RULE-RES-002
environment: null
---

## Observation

The fingerprinted BLD-GOG-EN C1086.GOB directory record at the first location
names `charactr.dat`, storage kind 1, expanded size 5700 and the stored extent
at the second location. Bounded independent block decoding produces 5700
bytes with SHA-256
5b8de2a6467ba1e7583eeb2006ac6d9f00c4e50ce0efad83ffd2e8389ca2e79b.
The first character block has 30 numeric values. Its zero-based field 18 is
12. FND-PERSON-002 identifies that field as AGE and FND-PERSON-003 identifies
the first row as the player's loaded row.

Two existing local guest-extracted copies from independently started native
probes have the same size and hash and the same row-zero field value. No
character name, prose or source bytes are retained in this finding.

## Interpretation

The shipped table supplies an initial row-zero AGE of 12. The startup shift
described by FND-PERSON-003 covers fields 0 through 14, so this data value is
outside that shift. This supports a concrete initial-data check independently
of the manual's stated starting age.

## Alternatives

This does not establish AGE at a later screen boundary or exclude writes by
other callers. It does not explain EXP-UI-001's five prescribed answer/Continue
pairs. A later value must be observed or traced through its own producers;
the decoded file alone cannot establish it.

## How to reproduce

Verify the manifest's container identity. Read the located directory record
with FMT-RES-002 and bound its stored bytes before decoding with RULE-RES-002.
Require exactly the declared expanded size and the recorded output hash.
Follow FMT-PERSON-001's first character block and numeric value line and read
zero-based field 18. Compare with the field order and startup shift in the
cited findings. Keep decoded content local.
