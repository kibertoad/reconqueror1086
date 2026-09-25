---
id: FMT-TALK-006
title: Expression, a record of ALL.TMB
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: null
text: false
definition: fmt_talk_006.ksy
evidence: [FND-TALK-004]
conflicting: []
split_with: []
related: [RULE-TALK-002]
---

## Layout

A record of the `ALL.TMB` entry of `C1086.GOB`.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `value_count` | Number of values. | supported | FND-TALK-004 |
| `0x04` | 4 | `INT32LE` | `operator_count` | Number of operators; anything but `value_count - 1` fails the expression. | supported | FND-TALK-004 |
| `0x08` | `4 * value_count` | `INT32LE[]` | `values` | Offsets in `ALL.TMB` of FMT-TALK-007 values. | supported | FND-TALK-004 |
| after `values` | `4 * operator_count` | `INT32LE[]` | `operators` | See `operators`; operator `i` joins the result so far with value `i + 1`. | supported | FND-TALK-004 |
| | | | | Total size `8 + 4 * (value_count + operator_count)` | | |

## Enumerations and flags

### `operators`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 1 | `OP_NE` | 1 when the two differ. | supported | FND-TALK-004 |
| 2 | `OP_LE` | 1 when the left is at most the right. | supported | FND-TALK-004 |
| 3 | `OP_GE` | 1 when the left is at least the right. | supported | FND-TALK-004 |
| 4 | `OP_AND` | 1 when both are not 0. | supported | FND-TALK-004 |
| 5 | `OP_OR` | 1 when either is not 0. | supported | FND-TALK-004 |
| 6 | `OP_GT` | 1 when the left is greater. | supported | FND-TALK-004 |
| 7 | `OP_LT` | 1 when the left is smaller. | supported | FND-TALK-004 |
| 8 | `OP_EQ` | 1 when the two are equal. | supported | FND-TALK-004 |

## Differences between builds

None known.

## Coverage

Read against the expression evaluator and the table at `0x0006ABBC`. An operator outside 1 to 8 fails the expression.

## Open questions

None known.
