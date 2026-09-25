---
id: RULE-ESTATE-005
title: Crop and forest revenue
status: sourced
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [SRC-GAMEFAQS-66730, SRC-MANUAL]
conflicting: []
split_with: []
related: [RULE-ESTATE-002, RULE-ESTATE-004]
---

## Summary

Crops are planted in March. Each crop and forest industry costs money and serfs to start and brings a
monthly revenue, and crops a larger one at the July harvest. The guide's revenues are those at 50%
productivity, and the revenue grows with productivity.

## When it runs

Each month, and at the July harvest.

## Parameters

None.

## Inputs

The fief's crops and forest industries, productivity.

## Procedure

```text
define crop_revenue(f, july):
    let total = 0
    for each c in fief_crops(f):
        if july:
            total = total + crop_harvest_revenue[c]
        else:
            total = total + crop_monthly_revenue[c]
    for each i in fief_forest(f):
        total = total + forest_revenue[i]
    return scale_by_productivity(total, productivity(f, july))
```

## Outputs

Revenue added to the fief's wealth.

## Edge cases

None known.

## What the sources say

SRC-GAMEFAQS-66730 gives each crop's and industry's cost, serfs and revenue at 50% productivity.
SRC-MANUAL (pp. 38 to 39) says crops may be planted in March only, that the forest may be the
largest source of revenue, that a woodward reduces poaching and that a prospector may make the mines
more efficient.

## Differences between builds

None known.

## Open questions

- How the executable computes revenue from productivity; the monthly steps of RULE-ESTATE-002 are
  expected to hold it.
- Which of the fief record's lists hold the crops and the forest industries.
- How `scale_by_productivity` grows a value above 50%; the guide gives the 50% values only.
- Where the executable keeps `fief_crops`, `fief_forest`, `crop_monthly_revenue`,
  `crop_harvest_revenue` and `forest_revenue`.
