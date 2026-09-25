---
id: FMT-TALK-007
title: Value, a record of ALL.TMB
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: null
text: false
definition: fmt_talk_007.ksy
evidence: [FND-TALK-004, FND-TALK-005]
conflicting: []
split_with: []
related: [RULE-TALK-002, RULE-TALK-003]
---

## Layout

A record of the `ALL.TMB` entry of `C1086.GOB`.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `kind` | See `kind`. | supported | FND-TALK-004 |
| `0x04` | 4 | `INT32LE` | `value` | For kind 1 the offset of an FMT-TALK-006 expression, for kind 2 the value, for kind 3 the function number of RULE-TALK-003. | supported | FND-TALK-004, FND-TALK-005 |
| `0x08` | 4 | `INT32LE` | `flag` | -1 or less leaves the result, 1 negates it logically; any other value fails the action. | supported | FND-TALK-004 |
| `0x0C` | 4 | `INT32LE` | `argument_count` | Number of arguments; 0 for kinds 1 and 2 in the GOG archive. | supported | FND-TALK-004 |
| `0x10` | `4 * argument_count` | `INT32LE[]` | `arguments` | Offsets in `ALL.TMB` of FMT-TALK-006 expressions. | supported | FND-TALK-004 |
| | | | | Total size `16 + 4 * argument_count` | | |

## Enumerations and flags

### `kind`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 1 | `VALUE_EXPRESSION` | The result of the nested expression. | supported | FND-TALK-004 |
| 2 | `VALUE_LITERAL` | `value` itself. | supported | FND-TALK-004 |
| 3 | `VALUE_FUNCTION` | The result of script function `value` called with the arguments. | supported | FND-TALK-004 |

## Differences between builds

None known.

## Coverage

Read against `0x0006A9E0`, which reads the record into a `0x90`-byte buffer.

## Open questions

- The largest argument count the `0x90`-byte buffer allows is 32; whether a longer list is checked was not read.
