---
id: BUG-BATTLE-002
title: A unit still turning toward its target turns toward another unit
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: rules
intent: unintended
player_reliance: unknown
evidence: [FND-BATTLE-014]
conflicting: []
split_with: []
related: [RULE-BATTLE-007, RULE-BATTLE-006]
---

## Symptom

A unit that met a foe while walking, but did not yet face it, can turn toward a different unit in the
next update, often the first unit of the player's army.

## Trigger conditions

A unit in state 0 keeps a target from an earlier update and finds no foe at its corners in this one.

## Mechanism

The idle update passes the facing routine the value left in EAX by its corner search, which is the
index of the last unit hit or 0 after a miss, instead of the stored target (FND-BATTLE-014).

## Frequency

Whenever a walking unit meets a foe it does not already face.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

None known.
