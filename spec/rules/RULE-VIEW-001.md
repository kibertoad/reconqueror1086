---
id: RULE-VIEW-001
title: Heading from one map point to another
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-VIEW-003]
conflicting: []
split_with: []
related: []
---

## Summary

Turns a difference between two map positions into a byte heading, 256 steps to a turn,
with 0 towards decreasing y and headings growing clockwise.

## When it runs

Whenever an actor aims at another actor or a destination (RULE-ASSAULT-010, RULE-ASSAULT-011,
RULE-ASSAULT-015, RULE-ASSAULT-016).

## Parameters

None. The rule defines `heading_to(dx, dy)`, where `dx` and `dy` are the target's position
less the source's, in any consistent unit.

## Inputs

None.

## Procedure

```text
define heading_to(dx, dy):
    # the helper itself takes (a, b) = (-dy, dx)
    let a = -dy
    let b = dx
    if a == 0 and b == 0:
        return 0
    let ma = abs(a)
    let mb = abs(b)
    if b >= 0:
        if a >= 0:
            if ma > mb:
                return mb * 0x20 / ma
            return 0x40 - ma * 0x20 / mb
        if ma >= mb:
            return 0x80 - mb * 0x20 / ma
        return 0x40 + ma * 0x20 / mb
    if a < 0:
        if ma > mb:
            return 0x80 + mb * 0x20 / ma
        return 0xC0 - ma * 0x20 / mb
    if ma < mb:
        return 0xC0 + ma * 0x20 / mb
    return (0x100 - mb * 0x20 / ma) & 0xFF
```

## Outputs

Returns the heading, 0 to 255. Changes no state.

## Edge cases

A zero vector gives 0. Exact diagonals give the multiples of `0x20`. The ties between the two
components go to the branch with `>=` or to the `otherwise` branch as written, so a diagonal
exactly between two octants is not split.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Whether the multiplication by `0x20` can overflow for the largest differences the game passes
  is not recorded; differences of positions within the 128 by 128 map stay far below it.
