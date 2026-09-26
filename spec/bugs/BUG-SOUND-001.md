---
id: BUG-SOUND-001
title: Samples at 11,050 Hz play with the last step their voice used
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: presentation
intent: unclear
player_reliance: unknown
evidence: [FND-SOUND-001, FND-SOUND-003]
conflicting: []
split_with: []
related: [RULE-SOUND-002, FMT-SOUND-001]
---

## Symptom

The five fief management samples tagged 11,050 Hz play at the pitch and speed of whatever sample
their voice played before, or with a step of 0 when the voice was never used.

## Trigger conditions

A fief management screen plays sample 1, 3, 4 or 5 of `fiefmgmt.666`, which the calls at
`0x0001E5F6`, `0x0001E7F1`, `0x00034461` and `0x00034679` do (FND-SOUND-003).

## Mechanism

`play_sample` sets the voice's rate step only for 11,025, 22,050 and 44,100 Hz and leaves the field
alone for any other rate. The five samples say 11,050 Hz, 25 more than the nearest value it knows.

## Frequency

Every play of those samples. After an 11,025 Hz sample on the same voice the result is the intended
speed; after a 22,050 Hz sample it is twice as fast.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

- How the driver treats a step of 0.
- Whether the samples were meant to be 11,025 Hz.
