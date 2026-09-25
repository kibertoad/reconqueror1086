---
id: RULE-DRAGON-002
title: Crown ending
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-DRAGON-004, FND-STRATEGY-001, FND-STRATEGY-003, FND-STRATEGY-019, FND-STRATEGY-021, SRC-GAMEFAQS-66730]
conflicting: []
split_with: []
related: [RULE-STRATEGY-014, RULE-PERSON-001, RULE-RNG-001]
---

## Summary

Taking the king's castle, the cell of person 100, by assault ends the campaign with the crown. The
automatic resolution of a siege never wins it.

## When it runs

At the end of a siege of the cell of the selected record. `person` is `cell_person` of that cell.

## Parameters

`siege_outcome(person, won)`: the person at the besieged cell, and 1 when the siege was won.

## Inputs

None.

## Procedure

```text
define siege_outcome(person, won):
    if won != 1:
        return
    if person == 100:
        # shows the Victory message; plays CROWNL30.SMK when g_0009ADB8 is not 0,
        # otherwise shows a still picture with text
        fn_0001BF54(4)
        map_session_over = 1
        return
    set_attr(0, 24, attr(0, 24) + 1)
```

## Outputs

The crown ending, with its movie or picture. Any other won siege adds 1 to BATTLE_WON.

## Edge cases

A siege of person 100 that the game resolves without the assault always fails.

## What the sources say

SRC-GAMEFAQS-66730 says the crown is won by capturing London, and that the campaign also ends at
age 30 without either victory (RULE-PERSON-006).

## Differences between builds

None known.

## Open questions

- What `fn_0001BF54` does with 4, and what `g_0009ADB8` holds.
- What else the siege routine does after a won siege of another person.
