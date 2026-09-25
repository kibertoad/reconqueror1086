---
id: FND-TALK-003
title: The prompt variant is drawn after reseeding the generator from the clock
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001A14C..0x0001A1EF
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001A230
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0001A14C(node, id)` calls `0x0006B3B4(0)` and keeps the result, calls `0x0001AB98`, and when the
prompt count (the byte at `+0x08`) is greater than 1 passes the kept value unchanged to
`0x0006B413`, draws once through `0x0006B3F1` and takes the remainder by the count; otherwise the
variant is 0. With more than one variant it formats `"%s%d%c.smk"` from the `CD_PATH` environment
value (or an empty string), `id` and the byte at `0x0009A76C + variant` (the letters `a` to `s`);
with one or none it formats `"%s%d.smk"`. It later calls `0x0001A91C(node, variant)`.

## Interpretation

Every prompt with more than one variant reseeds the game's one generator with the clock value and
draws once, so the variant is chosen by the time, and every draw after it follows the sequence of
that seed. The variant also picks the node's video, named from the node number and a letter.

## Alternatives

That `0x0006B3B4` is `time` is not shown; only its call with 0 was read.

## How to reproduce

Run `tools/Conqueror.Inspect` against the installation with `--disassemble=ADDR --executable-only` for each address listed, and read the report.
