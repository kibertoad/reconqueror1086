---
id: BUG-TALK-001
title: The conversation variable bound check lets a script reach one variable past the table
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
impact: rules
intent: unintended
player_reliance: unknown
evidence: [FND-TALK-006]
conflicting: []
split_with: []
related: [RULE-TALK-003, FMT-TALK-008]
---

## Symptom

A script that reads or writes variable 190 touches the four bytes after the variable table.

## Trigger conditions

A variable number equal to the count in `ALL.VTB`, 190 in the GOG archive.

## Mechanism

The read and the write reject a number below 0 or greater than the count, so the count itself passes and the copy reaches the element after the last (FND-TALK-006).

## Frequency

Only when a script names that number.

## Player reliance

None known.

## Fixes elsewhere

None known.

## Differences between builds

None known.

## Open questions

Whether any shipped script names variable 190.
