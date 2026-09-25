---
id: RULE-ESTATE-006
title: Population, food, housing and taxes
status: sourced
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [SRC-GAMEFAQS-66730, SRC-MANUAL]
conflicting: []
split_with: []
related: [RULE-ESTATE-002, RULE-ESTATE-004]
---

## Summary

The population needs one house and one food tile for every 100 people. It grows each month at a rate
that falls in bands as the housing and food capacity runs short, scaled by productivity, and a high
tax rate slows it by about 7%. Villagers pay the tax rate on their wealth on July 1, and the player
pays the overlord 10% of the year's income.

## When it runs

Each month, and on July 1 for taxes.

## Parameters

None.

## Inputs

The fief's population, houses, food tiles, tax rate and productivity.

## Procedure

```text
define population_capacity(f):
    return min(fief_houses(f), fief_food_tiles(f)) * 100

define monthly_growth(f):
    let rate = growth_band_rate(population_capacity(f), fief_population(f))
    rate = scale_by_productivity(rate, productivity(f, false))
    if fief_tax_rate(f) > 10:
        # about 7% slower
        rate = rate * 93 / 100
    return rate
```

## Outputs

The population and the taxes paid and received.

## Edge cases

None known.

## What the sources say

SRC-GAMEFAQS-66730 gives the housing and food needs, growth bands tried against a population of
1,200, and the effect of a high tax. SRC-MANUAL (pp. 41 to 42) says taxes are due July 1, that the
overlord takes 10% of the annual income, that the villagers' default tax rate is 10%, changed in
steps of 5%, and that the rate may affect productivity over time; p. 42 says conquests and a strong
leader draw population.

## Differences between builds

None known.

## Open questions

- How the executable computes growth and taxes; the monthly and July steps of RULE-ESTATE-002 are
  expected to hold them.
- Where the executable keeps `fief_houses`, `fief_food_tiles`, `fief_tax_rate` and the bands of
  `growth_band_rate`, and how `scale_by_productivity` scales the rate.
