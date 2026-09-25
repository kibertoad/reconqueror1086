---
id: FND-DRAGON-003
title: After a won dragon run the lair shows the victory text and plays champl30.smk
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00013328..0x000134B8
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

When `0x0001BB5C` returns 1, the lair branch of `0x00013168` shows a `Victory` message box with the
text at `0x00090520`, shows picture `0x156` through `0x00024DB8`, and waits until `0x00024D14`
returns non-zero. When `0x0009ADB8` is not 0 it plays `champl30.smk` (the string at `0x00090588`)
through `0x00030100` with `0x000B084C` set to 1 for the call. Otherwise it loads resource `0x1DA`,
shows picture `0x1BF` and draws text lines from `0x00090598` and `0x000905D8` onwards through
`0x000644D4`.

## Interpretation

Killing the dragon ends the campaign with the champion's investiture, as a movie when movies are
enabled and as a still picture with text otherwise.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00013328` to `0x000134B8`.
