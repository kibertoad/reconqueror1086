---
id: FMT-TALK-005
title: Action, a record of ALL.TMB
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
files: ["C1086.GOB"]
byte_order: little
size: null
text: false
definition: fmt_talk_005.ksy
evidence: [FND-TALK-004]
conflicting: []
split_with: []
related: [RULE-TALK-002]
---

## Layout

A record of the `ALL.TMB` entry of `C1086.GOB`.

| Offset | Size | Type | Name | Meaning | Status | Evidence |
|---|---|---|---|---|---|---|
| `0x00` | 4 | `INT32LE` | `kind` | See `kind`. | supported | FND-TALK-004 |
| `0x04` | 4 | `INT32LE` | `expression` | Offset in `ALL.TMB` of an FMT-TALK-006 expression. | supported | FND-TALK-004 |
| `0x08` | 4 | `INT32LE` | `branch_count` | Number of branches; must be 1 for kind 1 and 2 for kind 2. | supported | FND-TALK-004 |
| `0x0C` | `4 * branch_count` | `INT32LE[]` | `branches` | Offsets in `ALL.TMB` of FMT-TALK-005 actions. | supported | FND-TALK-004 |
| | | | | Total size `12 + 4 * branch_count` | | |

## Enumerations and flags

### `kind`

| Value | Name | Meaning | Status | Evidence |
|---|---|---|---|---|
| 1 | `ACTION_WHEN` | Runs `branches[0]` when the expression is not 0. | supported | FND-TALK-004 |
| 2 | `ACTION_IF_ELSE` | Runs `branches[0]` when the expression is not 0, otherwise `branches[1]`. | supported | FND-TALK-004 |
| 3 | `ACTION_EVALUATE` | Evaluates the expression for its effects. | supported | FND-TALK-004 |

## Differences between builds

None known.

## Coverage

Read against `0x0006AEAC`.

## Open questions

- What branch count kind 3 needs; the shipped actions of kind 3 have 0.
