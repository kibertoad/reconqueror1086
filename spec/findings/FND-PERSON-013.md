---
id: FND-PERSON-013
title: Generation draws the first dilemma independently of AGE and continues until AGE equals 18
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
    address: 0x000151F8..0x000153B7
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015054..0x000150E9
tool: Bounded Capstone 5.0.9 reading and reconciliation with FND-PERSON-012
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

The first and rerolled dilemma draws use a fixed bound of 4, independently
of current AGE. Continue tests actual AGE for equality with 18 and otherwise
uses that AGE in its group calculation. Those operations do not alone guarantee
six answers or establish AGE at the first answer.

FND-PERSON-011 records the loaded file value of 12; FND-PERSON-012 identifies
the separate March increment before input. EXP-PERSON-001 corroborates that
pass on fabricated state, and EXP-PERSON-002 records first-answer AGE 13 and
five successive one-year answer changes to 18 in an owned new-game traversal.
Its initial dilemma still comes from numbers 0 through 4; subsequent group
starts at observed ages 14, 15, 16 and 17 are 10, 15, 20 and 25. The group
starting at 5 is consequently not selected on that traversal.

Reroll reloads the file and keeps name and colour, rather than guaranteeing
that a later answer sees the file's AGE unchanged. The three shields give
COLOR 0, 3 and 5. This reading replaces the earlier inferred fixed six-answer
count; the observed call arguments and AGE-dependent selection remain.

## Alternatives

The older notes placed the selection at `0x00012616`, `0x00022190`, `0x00011E49` and `0x000127EC`; the code at those addresses is unrelated, and the correct addresses are `0x2AA8` higher.

## How to reproduce

Disassemble the listed ranges and find the callbacks' registrations by searching the code for
their addresses.
