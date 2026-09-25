---
id: FND-STRATEGY-033
title: The brigand pass fights player armies on contact, rewards or punishes, and ends orders by date
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003B2A0..0x0003B978
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

With a null descriptor, `0x0003B0E4` visits the blocks `d` from 0 to 2 whose `+0x14` is not 0. For
each player record `i` from 0 to 5 whose `+0x00` is not 0 and whose float position is within the
double at `0x00095654` (30.0) of brigand `d` on both axes, record 5 is warned as in the player pass
(FND-STRATEGY-019) and the scan goes on. An army instead is focused and selected, gets 1 added to
attribute 5, and a message; the pass calls `0x0003CED8`, reads the three pools, stores 0 in the
army's `+0x14` and calls `e = 0x000258FC` with the six counts by reference, 0 and 0. For `d == 1` it
calls `0x000628AC(g, 0x2C, &1)` and for `d == 2` `0x000628AC(g, 0x61, &1)`, with `g` the dword at
`0x0009A928`. It sets negative counts to 0, zeroes the brigand counts for `e == 1`, then stores
`e = 1` when the brigand counts are all 0 and `e = 0` when the player's are. For `e == 1` it calls
`0x00029DE0(i, pools)`. It stores the brigand counts back, plays a sound, restores the screen and
stops the army as the encounter routine does (FND-STRATEGY-021).

When both sides still have troops it returns 1. For `e == 1` it draws `0x00024C38(100) + 50` and
uses 40 instead for `d == 1`, adds the amount to attribute 17 with a message, adds 1 to attribute 24,
stores 1 in `+0x18` and 0 in `+0x14`, frees the route and stores 0 in the brigand's `+0x00`, and
returns 1. Otherwise it adds 1 to attribute 25 and stores 1 in `+0x18`; for `i` equal to
`0x0009AE6C` it shows a death message, calls `0x000106C0` and `0x0001BF54(2)` and stores 1 in
`0x0009ADC0`, and otherwise calls `0x00029D24(i)`, and returns 1.

After the player scan it calls `0x0004A61C` for the brigand. When `0x0003866C` returns at least the
block's `+0x04` and `0x00038684` at least its `+0x08`, it stores 0 in `+0x14` and the brigand's
`+0x00`, frees the route and, when `+0x18` is 0, the origin's state is not 0 and `d` is 0, subtracts
1 from attribute 5, calls `0x000435E8(origin)` and shows a message; it returns 0. Otherwise it draws
the brigand's marker (FND-STRATEGY-030) and goes on with the next block.

## Interpretation

Attribute 5 is HONOR, 17 WEALTH, 24 BATTLE_WON and 25 BATTLE_LOST. Beating a brigand force pays a
bounty, 40 for the Scottish raid, and ends the order. Losing marks the order as fought but leaves the
brigands on their route. A timed brigand order that nobody fought before it ended costs honour and
alerts the property. The pass returns at the first fight or the first ended order, so later blocks
wait for the next pass. The two conversation writes record that the Scottish or Welsh raid was met.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0003B2A0..0x0003B978` in `CD:CONQUER.EXE`.
