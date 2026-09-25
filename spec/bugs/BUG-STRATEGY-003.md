---
id: BUG-STRATEGY-003
title: A battle against a hostile force counts its result twice
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: rules
intent: unclear
player_reliance: unknown
evidence: [FND-STRATEGY-021, FND-STRATEGY-022]
conflicting: []
split_with: []
related: [RULE-STRATEGY-011]
---

## Symptom

BATTLE_WON or BATTLE_LOST rises by 2 after each battle against a hostile force on the map.

## Trigger conditions

Any battle between a player army and a hostile force.

## Mechanism

The staging routine adds 1 to attribute 24 or 25 by the result, and the encounter routine that calls
it adds 1 to the same attribute again (FND-STRATEGY-021, FND-STRATEGY-022). Brigand fights count
once.

## Frequency

Every hostile encounter.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

None known.
