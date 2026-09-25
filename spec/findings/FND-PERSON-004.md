---
id: FND-PERSON-004
title: The generation screen draws the first dilemma, sets COLOR from three shields, rerolls and continues by age
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000142A4..0x000142AF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000145C3..0x0001464A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000148C0..0x00014902
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015228..0x000152A5
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015054..0x000150E9
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The generation screen's entry callback, registered at `0x00014731`, stores 1 in the dword at
`0x000A7550` and calls `0x00024C38(4)` (`0x000148F1`), then loads the dilemma with that number
through `0x00018B60`. The options screen stores 0 in field 19 (COLOR) of row 0 when it opens
(`0x000142AA`). Its click handler reads the clicked region through `0x00059D50`: region 3 stores
0 in field 19, region 4 stores 3 and region 5 stores 5 (`0x00014645`).

The path at `0x0001523D` saves row 0's name and field 19, frees the table (`0x00015AA0`), loads
`CHARACTR.DAT` again through `0x00015920`, restores the name and field 19, calls `0x00018F54` and
loads the dilemma numbered `0x00024C38(4)` (`0x00015294`).

The Continue callback `0x00015054`, registered at `0x000147AD`, reads field 18 (AGE) of row 0.
When it is 18 it calls `0x0005B2C0`, stores -1 in the dword at `0x0009AAC4` and calls
`0x000596C0(6, 0, 1)`. Otherwise it calls `0x00018F54` and loads the dilemma numbered
`(age - 12) * 5 + 0x00024C38(4)` (`0x000150BE` to `0x000150DE`).

Each dilemma load calls `0x00018B60(attribute_count, attribute_names, 3, 3, 2, 5, number)`,
passing the table's attribute count and name list from `0x00015FD0` and `0x00015FC4`. The
executable's data holds the names `DILEM0.DAT` to `DILEM29.DAT` in order, 12 bytes apart, from
object-2 offset `0x185C`.

## Interpretation

The generation screen offers six dilemmas, one for each age from 12 to 17, choosing among the five
of that age at random. The manual's Reroll is the path at `0x0001523D`: it rolls row 0 again and
starts over at age 12 while keeping the name and colour. The three shields give COLOR 0, 3 and 5.

## Alternatives

The older notes placed the selection at `0x00012616`, `0x00022190`, `0x00011E49` and `0x000127EC`; the code at those addresses is unrelated, and the correct addresses are `0x2AA8` higher.

## How to reproduce

Disassemble the listed ranges and find the callbacks' registrations by searching the code for
their addresses.
