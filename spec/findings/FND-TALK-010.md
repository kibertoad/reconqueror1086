---
id: FND-TALK-010
title: The action trees hold 689 groups, one redirect to a missing node and one item test outside the item table
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x01BC9E4B..0x01BCB212
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x01B9B8F0..0x01BC9E4B
  - build: BLD-GOG-EN
    file: C1086.GOB
    offset: 0x01B99135..0x01B9B8F0
tool: Conqueror.Inspect action-tree and conversation decoders (tools/Conqueror.Inspect)
environment: null
---

## Observation

Decoding `all.tmi` (FMT-TALK-003) and every group it indexes in `all.tmb` (FMT-TALK-004 to
FMT-TALK-007), following each reference once, gives 689 groups, 2,267 distinct actions
(302 `When`, 576 `IfElse`, 1,389 `Evaluate`), 10,373 distinct expressions and 13,352 distinct values
(1,476 expressions, 8,538 literals, 3,338 function calls). No reference is out of range and no
chain loops. The function calls are 716 of function 3, 388 of 4, 282 of 5, 1,890 of 6, 25 of 7,
6 of 8 and 31 of 9, each always with the same number of arguments (1, 3, 3, 2, 2, 2 and 2).

Every node that a function-3 call names is in `all.cif` except 5011, which group 5021 names in one
branch of its `IfElse` chain. The item arguments of functions 7, 8 and 9 are 0 to 23, except one
function-9 test of item 161 in group 2143, which also requires variable 104 above 16 and variable
119 equal to 0. No function-7 or function-8 call uses 161.

## Interpretation

The scripts use only functions 3 to 9 of FND-TALK-005. Redirecting to 5011 would ask the node
reader for a node the index does not hold. The item test of 161 reads entry 161 of the item table,
well past its 24 values, so its result depends on whatever lies there; no script ever gives item 161.

## Alternatives

What the node reader does with a missing node, and what the word 161 entries into the item table
holds, were not checked.

## How to reproduce

Run `tools/Conqueror.Inspect` against the installation; the action-tree report gives the counts
and the missing node, and `--action-groups=` lists the groups with their function calls.
