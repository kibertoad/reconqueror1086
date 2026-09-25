---
id: BUG-JOUST-001
title: The dragon lance frame scan reads past its table when the lance is above y 92
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: presentation
intent: unclear
player_reliance: unknown
evidence: [FND-JOUST-011]
conflicting: []
split_with: []
related: [RULE-JOUST-001, RULE-JOUST-003]
---

## Symptom

While the dragon lance is held high, the sprite always shows its last frame, whatever the lance's horizontal position.

## Trigger conditions

The dragon run with the lance above y 92 in movie coordinates.

## Mechanism

The row scan of `lance_frame` (RULE-JOUST-001) has no limit. The dragon thresholds end at 92, so for a smaller y it reads the target tables that follow and stops at a count of 53 or more, and the frame is limited to 24. The practice table ends at 2 and never overruns. The result could have been meant, since frame 24 is the highest lance, which is why the intent is unclear.

## Frequency

Every pass with the lance above y 92.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

None known.
