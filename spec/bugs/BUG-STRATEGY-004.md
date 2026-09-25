---
id: BUG-STRATEGY-004
title: Hostile forces lost to water or a dropped route are never uncounted
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: rules
intent: unintended
player_reliance: unknown
evidence: [FND-STRATEGY-006, FND-STRATEGY-009, FND-STRATEGY-011]
conflicting: []
split_with: []
related: [RULE-STRATEGY-004, RULE-STRATEGY-006, RULE-STRATEGY-007]
---

## Symptom

The number of hostile forces the map can hold shrinks over a session: once five forces have
vanished on water or a failed route, the generator never starts another.

## Trigger conditions

A direct force meets impassable terrain, or a routed force stops on a zero length, a step of more than
50 units or impassable terrain.

## Mechanism

These paths clear the record's `active` without lowering the live count at `0x0009AE60`; only the
removal routine lowers it (FND-STRATEGY-006, FND-STRATEGY-009, FND-STRATEGY-011). The generator
refuses new forces while the count is 5 or more, and only the reset routine sets it to 0.

## Frequency

Every such stop. How often forces meet water on the shipped routes is not recorded.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- Whether a save and reload, which store the count, ever lower it.
