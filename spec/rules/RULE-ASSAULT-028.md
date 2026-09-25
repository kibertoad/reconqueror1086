---
id: RULE-ASSAULT-028
title: Blood effect of a strike
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-041]
conflicting: []
split_with: []
related: [FMT-ASSAULT-001]
---

## Summary

A player's blow shows the large blood effect when it kills and the small one otherwise. The
effect shows four frames, one per drawn frame, so its duration depends on how fast the machine
draws.

## When it runs

When the player's blow lands (RULE-ASSAULT-005), and then once per pass of the assault main
loop.

## Parameters

- `target`: the combatant struck.

## Inputs

None.

## Procedure

```text
if target.health < 1:
    blood_frame = 43
else:
    blood_frame = 48
blood_step = 0
```

## Outputs

Sets `blood_frame`, the first of four frames of `SKIRMISH.RES/skirmish.csf`, and
`blood_step`. Each pass of the main loop then draws frame `blood_frame + blood_step` and adds 1
to `blood_step`, and after the fourth frame the effect is off.

## Edge cases

The main loop does not wait, so on a fast machine the effect is over in a few milliseconds.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- Where `blood_frame` and `blood_step` are kept is not recorded.
