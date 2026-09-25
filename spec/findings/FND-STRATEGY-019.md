---
id: FND-STRATEGY-019
title: The player pass at 0x00013168 moves each player record, then tests it against the hostile records and the dragon cells
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00013168..0x0001351E
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

For each record `i` from 0 to 5 at `0x000AA4B8 + 0x118 * i` whose dword `+0x00` is 1, `0x00013168`
subtracts 1 from `+0x54` when it is above 0, calls `0x00011554(i)`, then visits the hostile records
`j` from 0 to 4. When `j`'s `+0x00` is not 0 and both `abs` of the float differences of `+0x5C` and
of `+0x60` are below the double at `0x0009065A` (30.0), it stores 0 in `+0x04` of the record the
dword at `0x0009AE64` names, `i` in `0x0009AE64` and 1 in the record's `+0x04`. Then, for `i` other
than 5, when the dword at `0x0009ADC0` is 0 it calls `0x00039428(i, j)`; for `i == 5`, when `+0x54`
is 0, it focuses the map on the record through `0x000130E4`, shows a message box and stores 120 in
`+0x54`.

After the hostile scan, when `i` equals the dword at `0x0009AE6C` and the record's grid `+0x44`,
`+0x48` is `(63, 116)`, `(62, 114)`, `(64, 115)`, `(63, 114)` or `(63, 113)`, it saves the screen
through `0x00059CDC`, calls `0x0003CED8` and `0x0005B2C0`, stores -1 in the dword at `0x0009AAC4`,
calls `0x0001BB5C`, restores the screen through `0x0001070C`, shows a victory message and media when
that returned 1 and a death message otherwise, calls `0x0001BF54` with 1 for a return of 1 and 0
otherwise, calls `0x00024CA0`, stores 1 in `0x0009ADC0` and returns.

## Interpretation

Records 0 to 4 are the player's field armies and record 5 is the player's own figure on the map.
`0x0009AE64` is the selected record and `0x0009AE6C` the record the player rides with. An army that
touches a hostile force fights it; the player's figure alone is only warned, at most once every 120
passes. The five cells are the dragon's lair: the record the player rides with starts the dragon run
(RULE-JOUST-003) there, and the campaign's map session ends either way.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00013168..0x0001351E` in `CD:CONQUER.EXE`.
