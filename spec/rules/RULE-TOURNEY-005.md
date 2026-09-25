---
id: RULE-TOURNEY-005
title: Practice melee scene
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-TOURNEY-007, FND-RNG-002]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

The practice melee loads one of the three base melee scenes at random.

## When it runs

When the player picks the melee on the practice screen.

## Parameters

None.

## Inputs

None.

## Procedure

```text
define practice_melee():
    let variant = random_inclusive(2)
    let scene = sprintf("MELEE%d.RES", variant)
    let first = random_inclusive(6)
    let extra = random_inclusive(4)
    fn_0005921C(scene, first, first + extra, 0)
```

## Outputs

The scene `MELEE0.RES`, `MELEE1.RES` or `MELEE2.RES` runs.

## Edge cases

None known.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- How `fn_0005921C` uses its second and third arguments.
