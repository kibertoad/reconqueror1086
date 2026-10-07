---
id: FMT-RES-124
title: MIDI instrument bank, a BNK entry
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: null
text: false
definition: fmt_res_124.ksy
evidence: [FND-SOUND-004]
conflicting: []
split_with: []
related: []
---

## Layout

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | variable | `BYTE[]` | `body` | Read whole into DOS memory and handed to the MIDI driver through `0x0007AB6C`; the game reads none of it. | supported | FND-SOUND-004 |
| | | | | Total size variable | | |

## Enumerations and flags

None known.

## Differences between builds

None known.

## Coverage

The two entries, `melodic.bnk` (index `0x17E`) and `drum.bnk` (`0x17F`) of
C1086.GOB, are 5,404 bytes each [FND-SOUND-004].

## Open questions

- The bank's layout inside the HMI driver is not described; the rebuild plays
  music without the driver. (Q-RES-211)
