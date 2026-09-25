---
id: FND-STRATEGY-008
title: Force sizes come from the household count and the lord rating, or from the garrison for a pursuit
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00038A8C..0x00038B3C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00038B40..0x00038C64
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043558..0x0004358E
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00038A8C(s)` reads `r = 0x00043794(0x0004377C(o))` for the origin `o` of hostile record `s`,
takes `q = r / 4` rounded toward zero, and takes `h = 176 - 0x00043558()` when `o` is 7 and
`h = 0x00043004(o)` otherwise. It stores `h + q / 3` in `+0x1C`, `+0x20` and `+0x24`, and then
stores 1 in `+0x24` when three times that value is below 1.

`0x00038B40(s, t)` reads `g = 0x00043640(o)` and `n = 0x00029E58(t, 0) + 0x00029E58(t, 1) +
0x00029E58(t, 2) + 3`, and takes `d = min(g, n)`. With `d == 0` the support is 1; otherwise it is
the same `h` as above, limited to 30. For `c = d + support`, when `c` is 3 or less it stores 3 in
`+0x1C` only; otherwise it stores `c / 3` in all three. Last it stores `g - d` through
`0x00043658(o, ...)`.

`0x00043558` follows the byte links at `0x0009BA5D + 18 * n` from the dword at `0x0009B8C4` until it
reads `0xFF`, and returns the number of steps.

## Interpretation

An ordinary force is the lord's household plus a twelfth of the lord's rating in each troop type,
with a single knight when that comes to nothing. London's household is everyone not on the person
list. A pursuit takes up to the hunted player force's size plus three from the garrison. In the
small case only the halberdiers are written, so the other two counts keep the values the slot held
before.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00038A8C..0x00038B3C`, `0x00038B40..0x00038C64`, `0x00043558..0x0004358E` in `CD:CONQUER.EXE`.
