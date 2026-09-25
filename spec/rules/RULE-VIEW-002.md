---
id: RULE-VIEW-002
title: Integer sine, cosine and rotation
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-VIEW-004]
conflicting: []
split_with: []
related: []
---

## Summary

The game's trigonometry: a 64-entry quarter-wave sine table in signed 1.15 fixed point, and a
rotation that rounds each product separately.

## When it runs

Whenever a step, ray or heading is turned (RULE-VIEW-003, RULE-ASSAULT-016, RULE-ASSAULT-017).

## Parameters

None. The rule defines the functions `fixed15(a, b)`, `sin15(heading)`, `cos15(heading)`,
`rotate_x(x, y, heading)` and `rotate_y(x, y, heading)`, and the table `quarter_sine`.

## Inputs

None.

## Procedure

```text
table quarter_sine: INT16[64] = [0, 817, 1633, 2449, 3263, 4074, 4884, 5690, 6493, 7291, 8085, 8875, 9658, 10436, 11207, 11971, 12728, 13477, 14217, 14949, 15671, 16384, 17086, 17778, 18458, 19128, 19785, 20430, 21062, 21681, 22287, 22879, 23457, 24020, 24568, 25101, 25618, 26120, 26605, 27073, 27525, 27960, 28377, 28777, 29158, 29522, 29867, 30194, 30502, 30791, 31061, 31311, 31542, 31754, 31945, 32117, 32269, 32401, 32513, 32604, 32675, 32726, 32757, 32767]

define fixed15(a, b):
    return (a * b + 0x3FFF) >> 15

define sin15(heading):
    let angle = heading & 0xFF
    let quarter = angle >> 6
    if quarter == 0:
        return quarter_sine[angle]
    if quarter == 1:
        return quarter_sine[0x7F - angle]
    if quarter == 2:
        return -quarter_sine[angle - 0x80]
    return -quarter_sine[0xFF - angle]

define cos15(heading):
    return sin15(heading + 0x40)

define rotate_x(x, y, heading):
    return fixed15(sin15(heading), x) + fixed15(cos15(heading), y)

define rotate_y(x, y, heading):
    return -fixed15(cos15(heading), x) + fixed15(sin15(heading), y)
```

## Outputs

Each function returns an integer and changes no state. `rotate_x` and `rotate_y` turn a local
vector `(forward, right)` into map coordinates for a mover facing `heading`: at heading 0,
`(64, 0)` becomes `(0, -64)`.

## Edge cases

The table's last entry is 32767, not 32768, so `sin15` never returns `1.0` exactly. `>> 15` is
an arithmetic shift, so a negative product rounds towards minus infinity after the `0x3FFF` is
added.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The products are written in the default 32-bit width. Whether the original forms any of
  them in 64 bits is not recorded; the values the game passes keep them within 32 bits.
