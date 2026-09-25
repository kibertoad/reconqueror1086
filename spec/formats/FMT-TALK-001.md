---
id: FMT-TALK-001
title: Conversation index record, ALL.CIF
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: 8
text: false
definition: fmt_talk_001.ksy
evidence: [FND-TALK-001]
conflicting: []
split_with: []
related: [RULE-TALK-001]
---

## Layout

The `ALL.CIF` entry of `C1086.GOB` is a list of these records with no header, sorted by `node` from lowest to highest, since RULE-TALK-001 finds a node by binary search. The record count is the file length divided by 8.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `node` | The node number. | supported | FND-TALK-001 |
| `0x04` | 4 | `INT32LE` | `offset` | Offset of the node's FMT-TALK-002 record in `ALL.CBF`. | supported | FND-TALK-001 |
| `0x08` | | | | Total size 8 | | |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Read against the lookup at `0x000165EC`; the GOG archive holds 1,311 records [FND-TALK-001].

## Open questions

- Whether two records with the same node number ever occur; the search would return either.
