---
id: BUG-STRATEGY-002
title: A small pursuit keeps the swordsmen and knights of the slot's previous force
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: rules
intent: unclear
player_reliance: unknown
evidence: [FND-STRATEGY-008]
conflicting: []
split_with: []
related: [RULE-STRATEGY-005]
---

## Symptom

A pursuit of three troops or fewer can carry swordsmen and knights left over from the last force
that used the same hostile record.

## Trigger conditions

A pursuit whose garrison share plus support comes to 3 or less.

## Mechanism

The sizing writes only the halberdiers count, 3, in the small case, and nothing clears the other two
counts when a record is freed or reused (FND-STRATEGY-008).

## Frequency

Every pursuit from a property with a garrison of 2 or less, or with no household.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

None known.
