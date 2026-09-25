---
id: FND-PERSON-005
title: A dilemma choice scores one attribute against ordered breakpoints and applies its outcome through 0x00015F0C
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00014B38..0x00014C5F
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00014D1C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00014EA4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00018B60..0x00018C8A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000191EC..0x00019301
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00019302..0x00019450
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00018B60` allocates the current dilemma record, `0x84` bytes, and stores its pointer at
`0x000A95F0`. The accessors read it: `0x000191EC` returns the dword at `+0x00`, the dilemma
number; `0x00019228(choice)` returns entry `choice` of the list at `+0x5C`, the scoring field;
`0x000192C0(choice, outcome)` returns the modifier count from the lists at `+0x68`;
`0x000192DC(choice, outcome, i)` returns the field of modifier `i`, the first dword of 8-byte
pairs in the lists at `+0x64`; `0x0001920C(choice, outcome)` returns an entry of the lists at
`+0x58`.

`0x00019284(score, choice)` walks the breakpoints of the choice in the list at `+0x60`, `+0x7C`
of them, and returns the index of the first whose value is at most `score`, or the count when
none is.

The handler for the first choice, `0x00014B38`, reads field `0x00019228(0)` of row 0, resolves it
with `0x00019284`, and for each modifier of that choice and outcome reads the field
`0x000192DC(...)`, gets a delta from `0x00019238(choice, outcome, field)` and writes
`attr + delta` through `0x00015F0C`. `0x00019238` returns the delta of the first pair in the list
whose field matches, or 0. It then switches on the dilemma number and, for some numbers and
outcomes, calls `0x00043100` with a constant. The handlers of the other two choices call the
resolver at `0x00014D1C` and `0x00014EA4` in the same way. The file parser `0x00019302` reads
lines through `0x000199F4`, which skips lines as `0x00024C4C` does, and requires markers `!`, `&`,
`%`, `*` and `?` in that order of calls, ending the program when one is missing.

## Interpretation

A choice scores win (0) when the attribute reaches the first breakpoint, draw (1) when it reaches
only the second, and lose (2) below both. Outcome changes go through `set_attr`, so the first 15
fields stay within 0 to 20. A field listed twice in one outcome would get its first delta twice;
none of the 30 files lists a field twice.

## Alternatives

The older notes placed the resolver at `0x000167DC`; the code there is unrelated, and the resolver is at `0x00019284`.

## How to reproduce

Disassemble the listed ranges; the callers of `0x00019284` are found by scanning for `E8`
calls.
