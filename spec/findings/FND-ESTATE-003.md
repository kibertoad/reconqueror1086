---
id: FND-ESTATE-003
title: The monthly estate pass settles fief 0 once a month and charges company upkeep
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002B060..0x0002B1EB
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002D52C..0x0002D6C0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002DC08..0x0002DD3B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001074C..0x000109AA
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Fief records are reached through the list of pointers at `+0x08` of the record the pointer at
`0x0009ABEC` points to. `0x0002D5F4(f)` and `0x0002D608(f, v)` read and write `+0x04`;
`0x0002D57C` and `0x0002D590` `+0x08`; `0x0002D5A8` and `0x0002D5BC` `+0x0C`; `0x0002D558`,
`0x0002D52C` (add 1) and `0x0002D540` (store 0) `+0x20`. `0x0002D5D4(f)` returns `+0x14`, or 100
when it is above 100. `0x0002D674(f)` returns `+0x1C` after storing 0 there when it is negative, and
`0x0002D6A0(f, v)` stores `v`, or 0 when `v` is negative. `0x0002D620(f)` and `0x0002D654(f, v)` do
the same with the dword at `+0x04` of the list the pointer at `+0x2C` points to.

`0x0002B060` reads the month through `0x0003866C` and fief 0's `+0x08`, and stores attribute 17 of
row 0 in fief 0's `+0x04`. It returns unless the month is above `+0x08`, or the month is 0 and
`+0x08` is 11. It then calls `0x0002DE60`, `0x0002D9E8`, `0x0002DA48`, `0x0002E588`, `0x0002DC08`
and `0x0002E628` with 0, stores the month in `+0x08` and calls `0x0002E150(0)`. In month 6, when
`+0x0C` is 0, it calls `0x0002E3B8`, `0x0002DEAC`, `0x0002DDFC`, `0x0002DD3C` and `0x0002DD94` with
0 and stores 1 in `+0x0C`; in month 6 it then calls `0x0001074C` and returns when that returns 1. In
any other month it stores 0 in `+0x0C` when it is not 0. In month 2 it shows the message at object-2
offset `0x3C48`, a suggestion to plant crops. When `+0x04` is 0 it adds 1 to `+0x20` and calls
`0x0002CCD4(0)` when `+0x20` is then above 2; otherwise it stores 0 in `+0x20`. Last it stores
`+0x04` in attribute 17 of row 0 and calls `0x0005C550` and `0x0005C5CC`.

`0x0002DC08(f)` sums, over four lists of fief `f`, the product of two dwords of each row whose first
dword is not 0: for the list at `+0x28`, 19 rows 32 bytes apart, the dwords at `+0x1C` and `+0x28`;
at `+0x2C`, 15 rows 44 bytes apart, `+0x24` and `+0x2C`; at `+0x34`, 10 rows 40 bytes apart, `+0x18`
and `+0x20`; at `+0x30`, 9 rows 32 bytes apart, `+0x10` and `+0x18`. It adds
`0x00029E58(a, t) * 0x00029EF0(t)` for every army `a` from 0 to 4 and kind `t` from 0 to 5, and
stores `+0x04` minus the sum in `+0x04`, or 0 when the sum is above `+0x04`.

`0x0002CCD4(f)` draws `0x00024C38(17)` as a row of the list at `+0x28` and, when that row's `+0x1C`
is not 0 and the row is 9 to 15, shows the message at object-2 offset `0x3E50`, that lack of money
is causing the castle to deteriorate.

`0x0001074C` returns 0 when attribute 26 of row 0 is 0 or less. Otherwise it takes
`due = b + b / 2` of that value `b`. When `due` is not above fief 0's `+0x04` it asks through
`0x000256A0` whether the player will pay `due`. On yes it stores `+0x04 - due` in `+0x04`, calls
`0x000628AC` with the dword at `0x0009A928`, 6 and 1, stores 0 in attribute 26 and returns 0. On no,
and when `due` is above `+0x04` after the message at `0x0178`, it runs `0x00021E88("MONEY.RES")`
between `0x00059CDC`, `0x0003CED8` and `0x0001070C`. When that returns 1 it shows the message at
`0x00B8` or `0x01D8`, that Drogo is dead and the moneylender will not bother the player again, and
returns 0; only the branch taken when `due` is above `+0x04` also calls `0x000628AC` twice and
stores 0 in attribute 26. Otherwise it shows that Drogo killed the player, stores 1 in `0x0009ADC0`,
calls `0x000106C0` and `0x0001BF54(2)` and returns 1.

## Interpretation

Fief record `+0x04` is the fief's wealth for the length of the pass: the pass loads it from the
player's WEALTH and writes it back. `+0x08` is the month last settled, `+0x0C` marks July's harvest
steps as done, `+0x1C` is the free serfs, `+0x20` counts months with no money, and the dword at `+0x04`
of the `+0x2C` list is the population. Every company costs its kind's monthly cost each month,
wherever the army is. A loan is repaid with half again as interest in July, and killing Drogo after
refusing to pay leaves the debt in place.

## Alternatives

In `0x0002DC08` the second dword of a row in the `+0x28` and `+0x2C` lists lies past the row's own
bytes, at `+0x08` of the next row and `+0x00` of the next row. Either the rows are read that way on
purpose or the offsets are wrong; the layout of those rows has not been recovered.

## How to reproduce

Disassemble `0x0002B060`, `0x0002D52C` to `0x0002D6C0`, `0x0002DC08` and `0x0001074C` to
`0x000109AA`. Read the strings at the object-2 offsets named above.
