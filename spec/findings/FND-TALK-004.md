---
id: FND-TALK-004
title: The action-tree interpreter runs groups, actions, expressions and values from .TMI and .TMB
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006B0B8..0x0006B24C
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006A950..0x0006A9DF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006A9E0..0x0006ABB4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006ABBC..0x0006AEA4
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006AEAC
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006B28C..0x0006B3AD
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0006B0B8(name)` opens `"%s.%s"` with `tmi` and with `tmb`, `"rb"`, keeping the `.TMB` file at
`+0x04` and the `.TMI` file at `+0x08` of a 12-byte context. `0x0006A950(ctx, id)` fails for a
negative `id`, seeks the `.TMI` file to offset 4 and reads 8-byte records one after another until
the first dword equals `id`, returning the second dword, or -1 at the end of the file.
`0x0006B28C(ctx, id)` fails for `id` below 1, looks the group up, reads the group's count and that
many action offsets from the `.TMB` file, and runs each through `0x0006AEAC`, carrying on after a
failed action. An action record is a kind, an expression offset, a branch count and the branch
offsets. Kind 1 needs a count of 1 and runs branch 0 when the expression is not 0; kind 2 needs a
count of 2 and runs branch 0 when it is not 0 and branch 1 otherwise; kind 3 only evaluates it; a
count that does not match, or another kind, fails. The expression reader reads a value count, an
operator count, the value offsets and the operators, evaluates every value in order and folds them
left to right, `acc = op(acc, value)`, with the operators of the table at `0x0006ABBC`: 1 `!=`,
2 `<=`, 3 `>=`, 4 logical AND, 5 logical OR, 6 `>`, 7 `<`, 8 `==`. An operator count other than the value count minus 1, or an operator
outside 1 to 8, fails the expression, and a failed value fails it too. `0x0006A9E0` reads a value
record of four dwords, kind, value, flag and argument count, then the argument offsets. Kind 1
evaluates the expression at the value offset, kind 2 is the value itself, and kind 3 evaluates
the argument expressions in order, storing each result over its offset, and calls
`0x000225AC(value, arguments, &result)`; when that returns 0 the value fails. A flag of
-1 or less leaves the result, 1 applies logical NOT, and any other flag fails.

## Interpretation

The scripts are a tree of offsets into `ALL.TMB`, with `ALL.TMI` mapping an action number to its
group. The operators work on 32-bit integers and give 1 or 0, and AND and OR evaluate both sides.

## Alternatives

None known.

## How to reproduce

Run `tools/Conqueror.Inspect` against the installation with `--disassemble=ADDR --executable-only` for each address listed, and read the report.
