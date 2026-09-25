---
id: BUG-ESTATE-002
title: Removing a company gives back population that raising it did not take
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

The population can grow by raising and removing companies while it is below 200.

## Trigger conditions

A population below 200 and at least 100 free serfs.

## Mechanism

Raising a company takes 100 from the population only when it is 200 or more, but removing one adds 100 in every case (FND-ESTATE-002).

## Frequency

Each raise and removal while the population is below 200.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

None known.
