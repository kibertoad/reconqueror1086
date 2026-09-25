---
id: FND-STRATEGY-001
title: The strategic pass at 0x0003C290 runs brigands, the spy, the player forces and then the hostile pass
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003C290..0x0003C414
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0003C290` calls, in this order: `0x0003AFB4`; `0x0003B0E4` with 0; `0x00038C84`; `0x00013168`.
When the dword at `0x0009AEF8` is 0 it then calls `0x0003C088`, and returns at once when the dword
at `0x0009ADC0` is then 1. It goes on with `0x00011EE4`, `0x00012F28` and `0x0003A63C`. When the
dword at `0x0009A92C` is 1 it stores 0 there and calls `0x0003B97C`; when the dword at
`0x0009A930` is 1 it stores 0 there and calls `0x0003B9B4`. When the dword at `0x0009A560` is 1 it
shows a message box through `0x000256A0` and stores 0 there.

It then calls `0x00038684`. When the result is at least the dword at `0x0009AE4C`, the byte at
`0x0009B8EC + 15 * p` is not 0 for `p` the dword at `0x000AB0CC`, and the byte at
`0x0009B8EC + 15 * 7 + 0x0D` is 0, it calls `0x0003B9F4(1, m, y, p, 0)` with `m` the result of
`0x00024C38(3)` plus 7 and `y` the dword at `0x0009AE4C`, and adds 1 to that dword.

Last, when `0x00038684` returns at least the dword at `0x0009AE50`, `0x0003866C` returns at least 5
and the same byte of record 7 is 0, it draws `r = 0x00024C38(13)` until the byte at
`0x0009B8EC + 15 * r` is neither 0 nor 7 and `r` differs from the dword at `0x000AB0CC`. It then
calls `0x0003B9F4(2, 0x00024C38(4), y + 1, 7, r)` with `y` the dword at `0x0009AE50`, and adds 1 to
that dword.

## Interpretation

This is the order of one pass of the strategic map. `0x0003B0E4` with a null argument advances the
brigand forces, `0x00038C84` is the spy report, `0x00013168` the player forces and `0x0003C088` the
hostile forces. `0x0009AEF8` is a busy flag that holds back only the hostile pass, and `0x0009ADC0`
is set when a battle has ended the campaign's current map session. `0x0003866C` returns the
zero-based month and `0x00038684` the year (FND-STRATEGY-002). The last two blocks issue a yearly
brigand order and a yearly order from the king.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0003C290..0x0003C414` in `CD:CONQUER.EXE`.
