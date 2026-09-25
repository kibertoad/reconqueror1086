---
id: FND-UI-014
title: The forge hides the smith without a tournament, and the store buys at the listed price and sells at three quarters
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00061038..0x000610EE
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00061154..0x00061189
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006133C..0x0006137A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006118C..0x0006133A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00061AFC..0x00061F56
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0004310C..0x00043131
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00061F70..0x00062130
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The entry routine of screen 13, `0x00061038`, tests `0x0009DED4`. When it is 0 it shows picture
`0x13A` through `0x00024DB8(1, 0x13A, 0)`, disables region 0 and gives region 2 the rectangle (54, 2,
280, 240) through `0x00059C70`; otherwise it gives region 1 the rectangle (1, 371, 632, 106). It then
allocates 800 bytes for the store entries at `0x000B0504`, stopping with the text at `0x8EE0` when
that fails, and loads sound `0x165`. Region 0 (`0x00061154`) stores 7 in `0x000A9B20` and calls
`0x000596C0(24, 0, 1)`. Region 1 (`0x0006133C`) shows pointer frame 0 and calls `0x000596C0(11, 1,
1)`.

The store routines `0x00061AFC` (regions 3, 4, 7 and 8) and `0x00061F70` (regions 5 and 6) do nothing
while the dword at `0x0009DF34` is 0; `0x0006137C` sets it when it opens the store. Both take the
entry `e` at index `0x0009DF30` of the array at `0x000B0504`. Region 3: when `0x0004310C(e.item)`
returns 0, it reads field 17 of row 0; when that is at least `e.price` it stores the difference
through `0x00015F0C(0, 17, ...)` and calls `0x00043100(e.item)`, and otherwise shows the text at
`0x8F74` under the title `0x8F6C` through `0x00025004`. When `0x0004310C(e.item)` returns 1 it calls
`0x00043124(e.item)` and stores `field 17 + e.price - e.price / 4`, the division truncating toward
zero. `0x0004310C(i)` returns 1 when the dword at `0x0009C6C8 + 8 * i` is not 0 and 0 otherwise, and
`0x00043124(i)` stores 0 there. Either way it redraws the entry through `0x0006118C`. Region 7: when the entry has a movie, it
joins the `CD_PATH` value from `0x00062EA0` (or an empty string) and the movie name with `"%s%s"`,
cuts the result at the first LF and plays it through `0x0002FCB0` and `0x0002FCF0` at (28, 27).
Region 4: it frees the two sprite sets, the entries' movie and description copies and three text
windows, enables regions 0 to 2, disables regions 3 to 6, repeats the entry routine's test of
`0x0009DED4` (picture `0x13A` or `0x139`, with the same region changes) and stores 0 in `0x0009DF34`.
Region 8 falls through all cases and does nothing. Region 5 decrements `0x0009DF30`, wrapping from 0
to the last entry `0x000B0500 - 1`; region 6 increments it, wrapping to 0. Both then draw the
entry's `SWORDS.CSF` frame `e.frame` at (28, 27) when it has a movie and at (32, 27) otherwise, redraw
it through `0x0006118C`, and put its description in a text window using `FFONTA2.FNT`.

`0x0006118C(i)` draws, for an owned item, `BUYSELL.CSF` frame 2 at (426, 425), the word at `0x8F04`
at (409, 364) and `e.price - e.price / 4` at (409, 384); otherwise frame 3 at (426, 425), the word at
`0x8F0C` at (521, 364) and `e.price` at (521, 384). It draws frame 1 at (347, 407) when the entry has
a movie and frame 0 otherwise, the word at `0x8F10` at (285, 364) and field 17 of row 0 at (285,
384).

## Interpretation

The smith stands in the forge only while this month's tournament is in town; otherwise the forge
picture has no smith and his region is off. In the store, region 3 buys or sells the shown item,
regions 5 and 6 browse, region 7 plays the item's movie and region 4 closes the store. An item sells
for its price less a quarter, and a sale clears every copy the player holds.

## Alternatives

None.

## How to reproduce

Disassemble the listed routines and read the strings at the object-2 offsets pushed.
