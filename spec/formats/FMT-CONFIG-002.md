---
id: FMT-CONFIG-002
title: Setting node, one key and value of the loaded CONQUER.INI
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: []
byte_order: little
size: 12
text: false
definition: fmt_config_002.ksy
evidence: [FND-CONFIG-001, FND-CONFIG-002]
conflicting: []
split_with: []
related: [RULE-CONFIG-001, RULE-CONFIG-002]
---

## Layout

A structure the game keeps only in memory, one per line of `CONQUER.INI` that has a key. The nodes
sit in file order in one block, and the key and value strings in one pool beside it; a value the
game changes is moved to its own allocation.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `UINT32LE` | `key` | Address of the key, a `char[]` in CP437. | supported | FND-CONFIG-001 |
| `0x04` | 4 | `UINT32LE` | `value` | Address of the value, a `char[]` in CP437. | supported | FND-CONFIG-001, FND-CONFIG-002 |
| `0x08` | 4 | `UINT32LE` | `next` | Address of the next node, or 0 in the last. | supported | FND-CONFIG-001 |

## Enumerations and flags

None.

## Differences between builds

None known.

## Coverage

Not checked in a running game; the layout comes from the code that fills and reads it.

## Open questions

None.
