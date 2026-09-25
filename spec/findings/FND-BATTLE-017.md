---
id: FND-BATTLE-017
title: The resolver counts survivors by category and returns 1 for a win
status: recorded
builds: [BLD-GOG-EN]
superseded_by: []
recorded_by: kibertoad
reproduced_by: []
method: static
locations:
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000260D7..0x000264F7
  - build: BLD-GOG-EN
    file: CD:CONQUER.EXE
    address: 0x000932F8..0x000933A7
tool: Conqueror.Inspect disassembler (tools/Conqueror.Inspect)
environment: null
---

## Observation

The caller of `0x00026B88` subtracts 1 from its result: 2 takes the win path, and 1 checks
`0x000A9C70` again to choose between retreat (nonzero) and defeat, and shows the text in a dialog
through `0x00025004`. It then scrolls and redraws the field until `0x00024D14` returns nonzero, and
returns the loop's result minus 1. The texts `You Have WON!`,
`You Have RETREATED!` and `You Have LOST!` sit at `0x000932F8`, `0x0009332C` and `0x00093368`.
`0x000263C4` sets the six counts to 0 and adds 1 for each unit with `+0x20 > 0` to the count of its
lane and category.

## Interpretation

Both lanes empty counts as a win, since the foe is tested first.

## Alternatives

None known.

## How to reproduce

Disassemble `0x000260D7..0x000264F7`, `0x000932F8..0x000933A7`.
