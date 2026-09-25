---
id: FND-STRATEGY-029
title: The player markers take a frame base of eight times the COLOR attribute
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00012A4C..0x00012AE9
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00012F28
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00014230
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000145A4..0x0001464A
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00015EF0
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x00018430
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0006A190
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x0003F0A0
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

`0x00012A4C` loads `icon_men.CSF` (object 2 `+0x4AC`) and `marker.CSF` (`+0x4BC`) through
`0x00018430`, which first probes the file's attributes through `0x0006A190`. At `0x00012A77`..
`0x00012AA5` it stores `0x00015EF0(0, 19) << 3` in `+0x38` of each player record, the `icon_men.CSF`
handle in `+0x110` and 0 in `+0x04`. It stores the same handle in each hostile record's `+0x110`
and 0 in its `+0x38`, and the two handles in the dwords at `0x0009AE80` and `0x0009AE84`.

`0x00014230` stores 0 in attribute 19 of row 0, and the handlers at `0x000145A4`..`0x0001464A` store
0, 3 and 5 for regions 3, 4 and 5 of `CGOPTS.HAT` through `0x00015F0C`.

`0x00012F28` visits the active player records in order and draws frame `+0x38` plus 1 or 6 for a
record whose `+0x04` is 0 or 1, 5 or 0 for the record `0x0009AE6C` names, and 2 or 7 for record 5.
It truncates the position through `0x00063EC0` and passes it to `0x0003F0A0`; for record 5 one
coordinate can come from the selected record. `0x0003F0A0` draws only when
`80 * r + 40 <= x <= 80 * r + 423` and `20 * (c + 1) <= y <= 20 * (c + 1) + 474`, at
`(x - (80 * r + 40) - (20 * (c + 1) + 434) / 2 + 20, y - 20 * (c + 1) - h + 7)` for an image of
height `h`.

## Interpretation

Attribute 19 is COLOR, chosen red, green or blue on the character options screen. Each colour has a
block of eight frames, and the offset shows whether the marker is an army, selected, the player's
own army or the player's figure.

## Alternatives

None known.

## How to reproduce

Disassemble `0x00012A4C..0x00012AE9`, `0x00012F28`, `0x00014230`, `0x000145A4..0x0001464A`, `0x00015EF0`, `0x00018430`, `0x0006A190`, `0x0003F0A0` in `CD:CONQUER.EXE`.
