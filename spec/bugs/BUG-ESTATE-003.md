---
id: BUG-ESTATE-003
title: Killing Drogo after refusing to pay leaves the debt
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: rules
intent: unintended
player_reliance: unknown
evidence: [FND-ESTATE-003]
conflicting: []
split_with: []
related: [RULE-ESTATE-003]
---

## Symptom

After the player refuses to pay and kills Drogo, the game says the moneylender will not come again, but Drogo returns the next July for the same debt.

## Trigger conditions

A loan outstanding in July, enough wealth to pay it, and a refusal followed by a won fight.

## Mechanism

Only the branch for a player who cannot pay clears MONEY_BORROWED after a won fight; the refusal branch shows the same message and returns without clearing it (FND-ESTATE-003).

## Frequency

Every July while the debt is left.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

None known.
