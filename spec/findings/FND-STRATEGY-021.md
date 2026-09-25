---
id: FND-STRATEGY-021
title: The encounter routine 0x00039428 fights a player army against a hostile force and settles the result
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00039428..0x000398FC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001146C..0x00011552
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00039428(i, j)` returns at once when the dword at `0x0009ADC0` is not 0, when
`0x00029E58(i, 0) + 0x00029E58(i, 1) + 0x00029E58(i, 2)` is 0, or when record `i`'s `+0x54` is above
0. When the dword at `0x000AA4AC` is 1, the one at `0x000AA4B0` is 0 and hostile record `j`'s `+0x28`
equals the dword at `0x000AA4A4`, it stores 1 in `0x000AA4B0`. It adds 1 to character attribute 6 of
row 0, focuses the map on record `i`, saves the screen, shows a message box, calls `0x0003CED8` and
`e = 0x00035924(i, j)`. It plays sound `0x14A` for `e == 1`, and otherwise `0x14B` unless `i` is the
dword at `0x0009AE6C` and the three pools of `i` are all 0, and restores the screen.

For `e == 1` it adds 1 to attribute 24, calls `0x0003A6A0(j)`, stores 0 in `+0x54`, adds 1 to
attribute 4, and stores `max(1, r - 3)` as the byte `+0x0C` of person `+0x2C` of record `j`, with
`r` its previous value. Otherwise it adds 1 to attribute 25. When a pool of `i` is still above 0 it
takes the point halfway between the truncated position and the anchor of the dwords at `0x000AB0D4`
and `0x000AB0D0`, with signed halving of each sum; when that point's tile kind is `0x16` or 9 it uses
the anchor and that home cell instead. It stores the point as the position and its cell as the grid,
0 in `+0x18`, `+0x30` and `0x0009AE58`, 1 in `+0x0C`, the truncated position in `+0x3C` and `+0x40`,
focuses the map, and stores 20 in `+0x54` when `0x00024C38(10)` is above 5 and 0 otherwise. With all
pools at 0 it stores 1 in `+0x54`; then for `i` equal to `0x0009AE6C` it shows a death message,
calls `0x000106C0` and `0x0001BF54(2)` and stores 1 in `0x0009ADC0`, and otherwise calls
`0x00029D24(i)`.

Last, when the dword at `0x0009AEFC` is not 0 it calls `0x0005B3B0` with it, `0x87B`, 0 and
`0x7FFF`, and calls `0x0001146C` with hostile record `j`'s grid, which calls `0x0003EB4C` for that
cell and eight cells around it.

## Interpretation

Attribute 6 is FAME, 4 INTELLIGENCE, 24 BATTLE_WON and 25 BATTLE_LOST (FND-PERSON-002). A player army
fights at most once per contact: a won battle removes the hostile force and lowers its lord's rating
by 3; a lost battle pulls the army back halfway toward home and, half the time, keeps it from fighting
again for 20 passes. An army left with no troops is removed, and when the player rides with it the
game ends. Beating a force from the property the king ordered attacked carries out the order. The
nine calls through `0x0001146C` redraw the cells around the battle.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00039428..0x000398FC`, `0x0001146C..0x00011552` in `CD:CONQUER.EXE`.
