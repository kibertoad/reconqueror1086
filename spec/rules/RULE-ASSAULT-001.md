---
id: RULE-ASSAULT-001
title: Retainer cap for a campaign castle assault
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-005]
conflicting: []
split_with: []
related: []
---

## Summary

A castle assault in the campaign lets the player bring one retainer for every three soldiers of
each of the first three unit types, at most three per type and at least one in all.

## When it runs

When a campaign castle assault is set up, before the scene is loaded (RULE-ASSAULT-002).

## Parameters

- `army`: the army that makes the assault.

## Inputs

`fn_00029E58`, which gives an army's count of one unit type.

## Procedure

```text
let total = 0
for unit_type in 0..3:
    retainer_shares[unit_type] = min(UINT32(fn_00029E58(army, unit_type)) / 3, 3)
    total = total + retainer_shares[unit_type]
if total == 0:
    total = 1
retainer_cap = total
```

## Outputs

Sets `retainer_cap` and the three `retainer_shares`, which RULE-ASSAULT-003 uses when the
assault ends.

## Edge cases

An army with fewer than three soldiers of every type still gets a cap of 1, while all three
shares stay 0. Nine or more soldiers of a type give that type's full share of 3, so the cap is
at most 9.

## What the sources say

SRC-GAMEFAQS-66730 reports that an army of one soldier brings one companion into an assault.
That agrees with the cap of at least 1.

## Differences between builds

None known.

## Open questions

- `fn_00029E58` is named only by its address: what it reads, and in what order its arguments
  are passed, is not recorded.
- Where `retainer_shares` is kept is not recorded.
- Another caller passes a count divided by 50 to the same setup (FND-ASSAULT-005); which
  assault that is has not been identified.
