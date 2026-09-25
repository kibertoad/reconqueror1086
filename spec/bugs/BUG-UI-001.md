---
id: BUG-UI-001
title: The lender of village record 37 takes three of its numbers from record 36
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: presentation
intent: unintended
player_reliance: unknown
evidence: [FND-UI-004, FND-UI-006]
conflicting: []
split_with: []
related: [RULE-UI-001, FMT-UI-003, SCR-UI-006]
---

## Symptom

In the places of persons 21, 28, 81, 87, 90, 141, 146 and 147, the lender's hot spot is not over the lender's house.

## Trigger conditions

Entering the village exterior of a place whose person record selects `VILLAGE.DAT` record 37.

## Mechanism

Row 4 of record 37 separates x, y and width with spaces. The row is read with
`"%d,%d,%d,%d,%d"`, which stops after x, so y, width and height keep the values row 4 of record 36
left: 108, 75 and 69 where the line gives 141, 43 and 52 (FND-UI-006). Row 1 of record 16 has the same
kind of fault, a doubled comma, but record 15's row holds the numbers it meant, so it has no effect.

## Frequency

Every visit to those places.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

Whether the runtime library of the original stops at the space as the standard C library does.
