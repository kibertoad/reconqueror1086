---
id: FMT-TALK-004
title: Action group, a record of ALL.TMB
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: null
text: false
definition: fmt_talk_004.ksy
evidence: [FND-TALK-004]
conflicting: []
split_with: []
related: [RULE-TALK-002]
---

## Layout

A record of the `ALL.TMB` entry of `C1086.GOB` at the offset an FMT-TALK-003 record gives.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `count` | Number of actions. | supported | FND-TALK-004 |
| `0x04` | `4 * count` | `INT32LE[]` | `actions` | Offsets in `ALL.TMB` of the FMT-TALK-005 action records, run in order. | supported | FND-TALK-004 |
| | | | | Total size `4 + 4 * count` | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read against `0x0006B28C`.

## Open questions

None known.
