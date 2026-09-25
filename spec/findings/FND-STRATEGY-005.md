---
id: FND-STRATEGY-005
title: The reactive finder 0x0003BB4C scans properties, then player forces, and marks alerted and approached properties
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003BB4C..0x0003BDBF
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

For each property `p` from 0 to 13 whose state byte is not 0, and each player record `i` from 0 to
4 whose dword `+0x00` is 1, `0x0003BB4C` reads `v = 0x0003E708` for the record's grid `+0x44` and
`+0x48`, truncates the record's floats `+0x5C` and `+0x60` to `(x, y)` and reads the property's
words `+0x05` and `+0x07` through `0x000438F8` as `(px, py)`. For `p == 7` it tests whether `(x, y)`
lies in the rectangle at `(700, 600)` of width 11940 and height 3960 through `0x00064164`, uses
the limits 40 and 200, and marks a lord match when `0x0004377C(p) == v`. For other properties the
limits are 30 and 250 and nothing is marked.

When `abs(x - px)` and `abs(y - py)` are both below the first limit, or the lord matched, or
`0x0004377C(p) == v`, it calls `0x000435E8(p)`. When `0x00043640(p)` is then 0 or less it goes on
to the next property; otherwise it stores `i` and `p` through its pointers and returns 1. Otherwise,
when both distances are below the second limit, or the lord matches, or the rectangle held, and
`0x00042F4C(p)` returns 0, it focuses the map on the record through `0x000130E4`, shows the warning
at object 2 `+0x570C` through `0x000256A0`, stores 1 in the record's `+0x0C` and 0 in its `+0x14`
and `+0x18` and in `0x0009AE58`, and calls `0x00042F38(p)`. With no hit it returns 0.

`0x000435E8(p)` stores 1 in the byte at `0x0009B8F9 + 15 * p`; `0x00043628(p)` reads it.
`0x00043640(p)` reads the byte at `0x0009B8F8 + 15 * p` and `0x00043658` writes it.

## Interpretation

The finder is the property's reaction to a player force. Close contact, or a force standing on a
cell of the lord's own people, alerts the property (`+0x0D`) and hands its garrison to a pursuit.
A property with no garrison is still alerted but does not pursue. A force in the wider zone, or
anywhere in London's rectangle, is stopped once with a warning that it approaches a castle armed,
and the property remembers that it gave the warning (`+0x0E`, through `0x00042F38` and
`0x00042F4C`).

## Alternatives

None known.

## How to reproduce

Disassemble `0x0003BB4C..0x0003BDBF` in `CD:CONQUER.EXE`.
