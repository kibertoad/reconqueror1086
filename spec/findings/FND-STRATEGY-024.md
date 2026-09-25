---
id: FND-STRATEGY-024
title: Joining and leaving an army swap the player record 0x0009AE6C between an army and the avatar
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0001133C..0x0001146B
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00011280..0x0001133B
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x0001133C(i)` stores `i` in the dword at `0x0009AE6C` and 1 in record `i`'s `+0x00`, 0 in `+0x04`
of the record `0x0009AE64` names, `i` in `0x0009AE64`, 1 in record `i`'s `+0x04`, and 0 in record
5's `+0x04` and `+0x00`. `0x000113A4(i)` returns 0 when record 5's `+0x00` is 1. Otherwise it stores
5 in `0x0009AE6C`, 1 in record `i`'s `+0x00` and 0 in its `+0x04`, 0 in `+0x04` of the selected
record, copies record `i`'s `+0x5C`, `+0x60`, `+0x44` and `+0x48` to record 5, stores 0 in
`0x0009AE58`, 5 in `0x0009AE64`, and 0 in record 5's `+0x14`, `+0x18` and `+0x30` and 1 in its
`+0x0C`, `+0x04` and `+0x00`.

`0x00011280(i)` returns 1 when record `i` is active and `0x0002FC70` of its squared distance from the
anchor of the home cell is 150 or more. `0x00011320(i)` returns 1 when `0x0009AE6C` is not 5 and
equals `i`.

## Interpretation

Joining an army hides the player's figure; leaving puts it where the army stands and leaves the army
on the map without its commander. `0x00011280` tells whether an army is away from home and
`0x00011320` whether the player rides with it.

## Alternatives

None known.

## How to reproduce

Disassemble `0x0001133C..0x0001146B`, `0x00011280..0x0001133B` in `CD:CONQUER.EXE`.
