---
id: RULE-ESTATE-004
title: Productivity from staff and buildings
status: sourced
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [SRC-GAMEFAQS-66730, SRC-MANUAL]
conflicting: []
split_with: []
related: [RULE-ESTATE-002, RULE-ESTATE-005, RULE-ESTATE-006]
---

## Summary

The fief's productivity starts at 50% and rises with castle staff, the church and the monastery, each
adding its own bonus. In July it falls when staff are hired without a servant room, when beans make
up less than a quarter of the food tiles, and when there are fewer houses than hundreds of people.
Productivity scales the fief's revenue and population growth.

## When it runs

Whenever revenue or growth is worked out, and at the July check.

## Parameters

None.

## Inputs

The fief's staff, buildings, crops, houses and population.

## Procedure

```text
define productivity(f, july):
    let p = 50
    for each b in fief_improvements(f):
        p = p + improvement_bonus[b]
    if july:
        if fief_has_staff(f) and not fief_has_servant_room(f):
            p = p - 10
        if fief_bean_tiles(f) * 4 < fief_food_tiles(f):
            p = p - 15
        if fief_houses(f) * 100 < fief_population(f):
            p = p - 15
    return p
```

## Outputs

The productivity percentage, at most 100.

## Edge cases

None known.

## What the sources say

SRC-GAMEFAQS-66730 gives each staff member's and building's bonus and the three July penalties, and
reports that building the same set in a different order can give 90% instead of 95%. SRC-MANUAL
(pp. 38 to 42) says that farm choices, a woodward and a prospector affect productivity, and that a
high tax rate may lower it over time.

## Differences between builds

None known.

## Open questions

- How the executable computes productivity; the monthly steps of RULE-ESTATE-002 are expected to hold
  it, and `fiefs[0]` field `+0x14`, which the executable reads capped at 100, may be it.
- Whether the order effect the guide reports is real, and what causes it.
- Whether productivity is limited to 0 to 100.
- Where the executable keeps `fief_improvements`, `improvement_bonus`, `fief_has_staff`,
  `fief_has_servant_room`, `fief_bean_tiles`, `fief_food_tiles` and `fief_houses`.
