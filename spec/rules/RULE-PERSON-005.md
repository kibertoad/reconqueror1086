---
id: RULE-PERSON-005
title: Courtship and marriage
status: sourced
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [SRC-MANUAL, SRC-GAMEFAQS-66730]
conflicting: []
split_with: []
related: []
---

## Summary

Six ladies sit in the tournament stands. Each judges the knight by his character and standing, may
let him wear her colours in the joust, and warms to him as he wins while wearing them. A lady may
in time offer marriage; once he marries, the others will have nothing to do with him.

## When it runs

When the player talks to a lady in the stands and after a joust.

## Parameters

None known.

## Inputs

The player's attributes, wins and marriage.

## Procedure

```text
# none known: the sources describe the courtship only in words
```

## Outputs

None known.

## Edge cases

None known.

## What the sources say

SRC-MANUAL (pp. 25 to 26) says the stands hold six ladies, that a lady's first attitude depends on
the knight's character and standing, that he should offer to wear her colours, that tournament
success while wearing them wins her favour, that the ladies can give information, prizes and
courtship, that rising fame and power soften even the most aloof, and that marriage ends the
other courtships. SRC-GAMEFAQS-66730 reports that several ladies can be courted at once, and gives
per-lady conditions and rewards.

## Differences between builds

None known.

## Open questions

- How the executable runs courtship; its lady conversations and their actions are the likely
  place, and the TALK area holds them.
- What field 28 (MARRIED) of row 0 holds, and what writes it.
- The conditions and rewards for each lady, which are content.
