---
id: FND-TALK-009
title: The joust settlement writes variable 3 with 2 for a win and 1 for a loss
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000406E4..0x0004084C
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

Inside `0x0003FCA8` a result of 1 adds the stake to the dword `0x0002C20C` returns through
`0x0002C218`, adds it to field 17 of row 0, adds 1 to field 20 of row 0 and to field 21 of the
opponent, and writes 2 to variable 3 through `0x000628AC`. A result of 0 subtracts the stake in
the same two places, adds 1 to field 21 of row 0 and field 20 of the opponent, and writes 1 to
variable 3. A scan for pushes of 42 or 3 before calls to `0x00062828` and `0x000628AC` finds no other
use of variables 3 and 42.

## Interpretation

Variable 3 tells the lady conversations how the last joust ended. Variable 42 is set and read only
by the scripts.

## Alternatives

A variable number built at run time would escape the scan.

## How to reproduce

Run `tools/Conqueror.Inspect` against the installation with `--disassemble=ADDR --executable-only` for each address listed, and read the report.
