---
id: FND-TALK-011
title: What the decoded conversations show about the inn, the priests, the lair variable, the lady variable and item gifts
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x01B99135..0x01B9B8F0
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x01B4BF02..0x01B99135
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x01BC9E4B..0x01BCB212
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x01B9B8F0..0x01BC9E4B
tool: Conqueror.Inspect action-tree and conversation decoders (tools/Conqueror.Inspect)
environment: null
---

## Observation

Following the continuation of each root node in `all.cbf` to the first node with a portrait:

| Root | Node | Portrait | Speaker |
|---:|---:|---|---|
| 1900 | 1999 | `FREDERIC.PCC` | Frederick |
| 1100 | 1152 | `GERARD.PCC` | Earl Gerard |
| 3200 | 3298 | `BARKEEP.PCC` | Bartender |
| 3500 | 3599 | `OTTO.PCC` | Otto |
| 1600 | 1699 | `HUGH.PCC` | Hugh |
| 1400 | 1499 | `GILBERT.PCC` | Gilbert |
| 3000 | 3099 | `BARMAID.PCC` | Nellie |
| 1698 | 16999 | `RICHARD.PCC` | Richard |
| 3300 | 3399 | `IVO.PCC` | Ivo |
| 3600 | 3699 | `ALBERT.PCC` | Albert |
| 3100 | 3101 | `PRIEST.PCC` | Priest |
| 3149 | 3152 | `PRIEST.PCC` | Father Hyacinth |
| 5000 to 5500 | 5001 to 5501 | `PRIEST2.PCC` to `PRIEST7.PCC` | Priest |

Node 3201 has `BLACKSMI.PCC` and speaker Blacksmith. Nodes 3101 and 5001 to 5501 each offer five
responses.

In the action trees, group 2447 adds 1 to variable 2, group 3203 sets it to 1 and group 3202 tests
it for 1; no other group uses it. Variable 42 is set to 1 by group 2045, 2 by 2118, 3 by 2451, 4 by
2541, 5 by 2923 and 0 by 2407, and read by 12 groups. Function 7 gives an item in 24 groups, 23 of
them in the lady ranges 2000 to 2999 and 24000 to 24999: 2051, 2056, 2060, 2063, 2126, 2150, 2152,
2154, 2156, 2159, 2418, 2449, 2548 (twice), 2551, 2931, 2936, 2940, 2944, 2948, 2950, 2954, 24162
and 24421; the other is 1433.

## Interpretation

The ten inn roots are those of Frederick, Gerard, the bartender, Otto, Hugh, Gilbert, Nellie,
Richard, Ivo and Albert. The church roots are the generic priest (3100), Father Hyacinth (3149) and
six regional priests with their own portraits, which the place record selects (FND-UI-005).
Variable 2 records that Anna Lisa has told the player of the lair, and variable 42 which lady's
colours the player wears: 1 Wendessa, 2 Victoria, 3 Anna Lisa, 4 Valletta, 5 Jane. The ladies'
gifts are given by their scripts; the joust settlement gives no item (FND-TALK-009).

## Alternatives

The lady for each group range is taken from the portraits of the nodes that run the groups. Which
routines call groups 3202 and 3203 was not traced.

## How to reproduce

Run `tools/Conqueror.Inspect` with `--conversation-nodes=` for the roots and `--action-groups=`
for the groups, and follow the continuations and function calls.
