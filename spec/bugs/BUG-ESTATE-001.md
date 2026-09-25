---
id: BUG-ESTATE-001
title: Removing a company refunds its price only after the committed companies of its kind are used up
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: rules
intent: unclear
player_reliance: unknown
evidence: [FND-ESTATE-002]
conflicting: []
split_with: []
related: [RULE-ESTATE-001]
---

## Symptom

A company raised and then removed on the War Planning screen does not give its price back when the army already had companies of that kind.

## Trigger conditions

An army with committed companies of a kind; the player raises one of that kind and removes it again before OK.

## Mechanism

The removal refunds only when the army was disbanded on this screen or its committed count of the kind is 0 or less, and lowers that count each time, so the first removals count as removing committed companies (FND-ESTATE-002).

## Frequency

Every such removal until the committed count reaches 0.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

Whether the refund was meant only for companies raised on this screen.
