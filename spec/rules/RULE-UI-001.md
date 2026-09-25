---
id: RULE-UI-001
title: Village exterior hot spots
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-UI-004, FND-UI-006, FND-UI-002, FND-TOURNEY-001, FND-TALK-007, FND-TOURNEY-005, FND-STRATEGY-004, FND-STRATEGY-023, FND-STRATEGY-015, FND-STRATEGY-019, FND-STRATEGY-036, FND-UI-005]
conflicting: []
split_with: []
related: [RULE-UI-002, RULE-TOURNEY-001, FMT-UI-003, SCR-UI-006]
---

## Summary

On entering a place, the village exterior notes whether this month's tournament is held there,
picks the tournament catalog for a tournament away from home and the village catalog otherwise,
and gives its six hot spots the rectangles of the place's record.

## When it runs

When the village exterior (screen 11) is entered.

## Parameters

None.

## Inputs

The person of the place, `place_person`, and the exterior catalogs of FMT-UI-003.

## Procedure

```text
define enter_village():
    if g_0009DF04 != 0:
        g_0009DF04 = 0
        fn_0005B2C0()
        g_0009AAC4 = -1
    if g_0009DED8 == 0:
        # starts resource 0x18D
        g_0009DED8 = 1
        g_0009AAC4 = 0x18D
    g_0009DEF8 = 0
    if tournament_here == 0:
        tournament_arrival_day = fn_00038678()
        if tournament_place == place_person:
            tournament_here = 1
        else:
            tournament_here = 0
    let catalog = "village.dat"
    if tournament_here != 0 and fallback_person != place_person:
        catalog = "tvillage.dat"
    let index = persons[place_person].village_scene
    # an index above 66 shows an error message and the game goes on
    let rows = catalog_rows(catalog, index)
    for i in 0..5:
        if rows[i].enabled != 0:
            set_region(i, rows[i].x, rows[i].y, rows[i].width, rows[i].height)
        else:
            disable_region(i)
    # draws the place's name in the label strip
```

## Outputs

The rectangles and the enabled state of regions 0 to 5 of the exterior; region 6 keeps its
rectangle from `VOPTS.HAT`. `tournament_here`, `tournament_arrival_day`, `g_0009DED8`, `g_0009DEF8` and
`g_0009DF04`.

## Edge cases

The index is the same for both catalogs, so the tournament catalog must hold a record for every
tournament place's index; in the GOG archive the places use indexes 0 to 11 of its 12 records. A row
whose numbers stop early takes the missing fields from the same row of the record read before it
(BUG-UI-001).

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- What calendar field `fn_00038678` returns.
- What `fn_0005B2C0`, `g_0009AAC4`, `g_0009DED8`, `g_0009DEF8` and `g_0009DF04` are.
- What the four digits after `_` in a background name mean, and whether the game checks them.
