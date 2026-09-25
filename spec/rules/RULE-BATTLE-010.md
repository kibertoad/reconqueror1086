---
id: RULE-BATTLE-010
title: Battle clock
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-BATTLE-009, FND-BATTLE-020]
conflicting: []
split_with: []
related: []
---

## Summary

The timer runs at about 250 Hz while the game runs. Each tick adds one to the battle clock, and the
BIOS timer keeps its own rate of about 18.2 Hz through a fractional counter that passes each 65536th
part on to it. The battle reads its clock as four times the tick count, close to milliseconds.

## When it runs

On each tick of `battle_tick`, from when the game registers the callback.

## Parameters

None.

## Inputs

`battle_clock_count`, `bios_chain_accumulator` and `pointer_clock`.

## Procedure

```text
clock battle_tick: 1193182 / 4772 Hz

battle_clock_count = battle_clock_count + 1
bios_chain_accumulator = bios_chain_accumulator + 4771
if bios_chain_accumulator >= 0x10000:
    bios_chain_accumulator = bios_chain_accumulator - 0x10000
    # the chained BIOS timer interrupt raises INT 1Ch, whose handler adds one here
    pointer_clock = pointer_clock + 1

define battle_clock():
    return UINT32(battle_clock_count << 2)
```

## Outputs

Changes `battle_clock_count` and, about once in 13.7 ticks, `pointer_clock`. `battle_clock` returns
`UINT32`.

## Edge cases

The divisor `0x1234DC / 250` is 4772, so a tick is about 4.0 ms. The BIOS step `0x123333 / 250` is
4771 of 65536, which gives about 18.2 counts a second.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- When the game registers the 250 Hz callback, and whether it ever removes it.
- Whether the accumulator subtracts `0x10000` or keeps only its low 16 bits; the two agree.
- The order in which the timer service runs the callback and the chain on one tick.
- Where the game keeps `bios_chain_accumulator`.
