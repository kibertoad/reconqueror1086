---
id: FND-TALK-005
title: Script functions 3 to 9 redirect, assign, add, read and handle items
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00022590..0x0002267B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000222B0..0x00022369
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002236C..0x00022461
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00022464..0x0002250B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0002250C..0x00022563
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00043100..0x00043131
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x000225AC(fn, args, &result)` jumps through the table at `0x00022590` for `fn` 3 to 9, stores the
handler's result and returns 1; for any other `fn` it stores 0 and returns 0. 3 calls `0x0001A140(args[0])`
with a result of 1. 4 calls `0x000222D4(args[0], args[1], args[2])`: scope 0 writes variable
`args[1]` through `0x000628AC` unless the value is `0x80000000`, giving the writer's result; scope
1 maps the selector through the table at `0x000222B0` and calls `0x00015F0C(0, field, value)`,
giving 0; selectors 1 and 4 and other scopes give 0 and write nothing. 5 calls `0x00022390`:
scope 0 reads the variable, gives 0 if the read fails, adds `args[2]` and writes the sum unless it
is `0x80000000`; scope 1 writes `attr(0, field) + args[2]` and gives 1. 6 calls `0x00022488`:
scope 0 gives the variable, or 0 when the read fails; scope 1 gives `attr(0, field)`. The three
selector tables send selector 0 to field 17, 2 to 5, 3 to 6, 5 to 2, 6 to 0, 7 to 4 and 8 to 3.
7, 8 and 9 take the signed 16-bit value at `0x0009A938 + 2 * args[1]` as an item and call
`0x00043100`, which adds 1 to the dword at `0x0009C6C8 + 8 * item` (7, result 1), `0x00043124`,
which stores 0 there (8, result 1), or `0x0004310C`, which gives 1 when it is not 0 (9).

## Interpretation

Scope 0 is the conversation variables and scope 1 the player's attributes. Selector 7 is
INTELLIGENCE and selector 8 STAMINA.

## Alternatives

Functions 7 to 9 ignore `args[0]`; what the scripts pass there was not checked.

## How to reproduce

Run `tools/Conqueror.Inspect` against the installation with `--disassemble=ADDR --executable-only` for each address listed, and read the report.
