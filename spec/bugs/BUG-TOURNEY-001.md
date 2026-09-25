---
id: BUG-TOURNEY-001
title: The tournament melee always loads a scene whose two digits repeat the site digit
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: rules
intent: unintended
player_reliance: unknown
evidence: [FND-TOURNEY-006]
conflicting: []
split_with: []
related: [RULE-TOURNEY-004]
---

## Symptom

Every tournament melee is fought in `MELEE00.RES`, `MELEE11.RES` or `MELEE22.RES`. The other twelve tournament scenes never appear, and the fighters' experience does not change the scene.

## Trigger conditions

Any tournament melee.

## Mechanism

The name is joined from `MELEE`, the site digit, the tier digit and `.RES`, but both digits are converted into the same buffer before the join, so both pointers see the site digit, which is converted last (FND-TOURNEY-006).

## Frequency

Every tournament melee.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

None known.
