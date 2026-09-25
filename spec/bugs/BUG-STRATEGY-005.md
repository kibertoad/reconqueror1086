---
id: BUG-STRATEGY-005
title: The spy report shows the swordsmen count in place of all three troop counts
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: presentation
intent: unintended
player_reliance: unknown
evidence: [FND-STRATEGY-038]
conflicting: []
split_with: []
related: [RULE-STRATEGY-018]
---

## Symptom

Each line of the spy report gives the same number of swordsmen, knights and halberdiers.

## Trigger conditions

Any spy report.

## Mechanism

The report converts the three counts into one buffer before it joins the text, so all three places show the last conversion, the swordsmen count (FND-STRATEGY-038).

## Frequency

Every line of every spy report.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

None known.
