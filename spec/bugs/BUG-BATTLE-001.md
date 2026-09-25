---
id: BUG-BATTLE-001
title: Morale adds the remainder by 3 instead of a third
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: rules
intent: unclear
player_reliance: unknown
evidence: [FND-BATTLE-002, FND-BATTLE-012]
conflicting: []
split_with: []
related: [RULE-BATTLE-001, RULE-BATTLE-005]
---

## Symptom

A higher morale does not give a stronger bonus. The automatic battle and each strike on the foe add 0
to 2, and a morale that is a multiple of 3 adds nothing.

## Trigger conditions

Any field battle with a morale value above 2.

## Mechanism

Both places divide the morale by 3 with `idiv` and use EDX, the remainder, where EAX would hold the
third (FND-BATTLE-002, FND-BATTLE-012).

## Frequency

Every automatic battle and every strike on a foe unit.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- Whether the designers meant a third.
