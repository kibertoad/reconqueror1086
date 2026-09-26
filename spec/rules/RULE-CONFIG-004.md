---
id: RULE-CONFIG-004
title: Field battle display mode from WAR_MODE
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-CONFIG-004, FND-BATTLE-018]
conflicting: []
split_with: []
related: [RULE-CONFIG-002, RULE-BATTLE-001]
---

## Summary

The interactive field battle is the only part of the game drawn above 640 by 480. `WAR_MODE` 640
keeps 640 by 480; otherwise the battle tries 1024 by 768 (not for 800), then 800 by 600, then goes
back to 640 by 480, and reports the width it got.

## When it runs

Once per interactive field battle, after the backdrop is loaded.

## Parameters

None.

## Inputs

`vesa_1024_available` and `vesa_800_available`, values from outside the game; `fn_0006FF50`.

## Procedure

```text
define select_battle_display() -> INT32:
    let mode = fn_0006FF50()
    if mode == -1:
        let text = ini_value(sprintf("WAR_MODE"))
        if text != 0:
            mode = decimal_value(text)
    if mode == 640:
        return 0
    # the three display buffers are freed
    if mode != 800 and vesa_1024_available == 1:
        # the display is 1024 by 768 with the FONT1024 font, and the clip rectangle is the screen
        return 1024
    if vesa_800_available == 1:
        # the display is 800 by 600
        return 800
    # the display is set to 640 by 480 again
    return 0
```

## Outputs

The width of the mode, or 0 for 640 by 480; `screen_width` and `screen_height` follow the mode.

## Edge cases

- A missing key, or one that is not a number, tries 1024 by 768 first.
- `WAR_MODE=800` never tries 1024 by 768.
- A value of 640 skips freeing the display buffers as well.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What table `fn_0006FF50` searches and under which name; it is written as returning -1 when it
  finds nothing.
