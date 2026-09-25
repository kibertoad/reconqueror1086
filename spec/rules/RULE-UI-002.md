---
id: RULE-UI-002
title: Village exterior actions
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-UI-005, FND-UI-004, FND-TOURNEY-001, FND-TOURNEY-002, FND-TOURNEY-003, FND-TALK-007, FND-TOURNEY-005, FND-STRATEGY-015, FND-STRATEGY-019, FND-STRATEGY-036]
conflicting: []
split_with: []
related: [RULE-UI-001, RULE-TOURNEY-001, RULE-TOURNEY-002, RULE-TALK-004, SCR-UI-006, SCR-UI-007, SCR-UI-012, SCR-UI-013]
---

## Summary

The six hot spots of the village exterior lead to the tournament, the map, the inn, the forge and
conversations with the lender and the priest. The tournament stays open only until the calendar
field noted on arrival changes, and leaving by the map with a tournament in town ends it.

## When it runs

When the player clicks region `row` of the village exterior.

## Parameters

`row`, the region clicked, 0 to 5.

## Inputs

The person of the place, `place_person`.

## Procedure

```text
define village_action(row):
    # plays the click sound first
    if row == 0:
        if tournament_here == 0:
            # shows the message that there is no tournament here
            return
        if fn_00038678() != tournament_arrival_day:
            # shows the message that the tournament is over
            return
        fn_0005B2C0()
        g_0009AAC4 = -1
        g_0009DED8 = 0
        # switches to screen 8, the tournament grounds
    else if row == 1:
        fn_0005B2C0()
        g_0009AAC4 = -1
        g_0009DED8 = 0
        if tournament_here == 1:
            jousts_today = 0
            melees_today = 0
            tournament_wins = 0
            tournament_here = 0
            opponents_ready = 0
            tournament_place = -1
        g_0009DEF8 = 1
        show SCR-UI-012
    else if row == 2:
        fn_0005B2C0()
        g_0009AAC4 = -1
        g_0009DED8 = 0
        # switches to screen 12, the inn
    else if row == 3:
        show SCR-UI-007
    else if row == 4:
        conversation_partner = persons[place_person].lender_partner
        g_0009DEDC = 1
        show SCR-UI-013
    else if row == 5:
        fn_0005B2C0()
        g_0009DED8 = 0
        conversation_partner = persons[place_person].church_partner
        # starts resource 0x177
        g_0009AAC4 = 0x177
        g_0009DF04 = 1
        show SCR-UI-013
```

## Outputs

The next screen, or a message. Leaving by the map with a tournament in town clears the tournament's
counts and its place.

## Edge cases

A tournament that opened on arrival closes when the calendar field changes while the player is still
in the place; the player then sees that it is over. The counts are cleared only by the map exit and
only when a tournament is in town, so leaving by another route keeps them.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What `fn_0005B2C0`, `g_0009AAC4`, `g_0009DED8`, `g_0009DEF8`, `g_0009DEDC` and `g_0009DF04` do on
  the screens that follow.
- What calendar field `fn_00038678` returns.
