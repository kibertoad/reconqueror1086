---
id: BUG-STRATEGY-001
title: The rare pursuit branch of the hostile generator tests a stale property
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: rules
intent: unintended
player_reliance: unknown
evidence: [FND-STRATEGY-004]
conflicting: []
split_with: []
related: [RULE-STRATEGY-003]
---

## Symptom

Whether the timed branch tries player-targeted pursuits depends on the alert byte of whatever
property the generator last worked with, or of the byte before the property table when that was -1.

## Trigger conditions

The timed accumulator reaches 5,000, fewer than five hostile forces are live, and the draw of 0 to
100 is above 96.

## Mechanism

The generator's property local is written only by the reactive finder when it finds a property and by
the pursuit loop, which can store -1. The rare branch reads the alert byte of that local without
setting it first (FND-STRATEGY-004).

## Frequency

About one timed movement in 25.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- What the local holds at the start of a session.
