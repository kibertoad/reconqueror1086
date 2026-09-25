---
id: RULE-ASSAULT-003
title: Soldiers lost with the retainers who died
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-005, FND-ASSAULT-007]
conflicting: []
split_with: []
related: [RULE-ASSAULT-002]
---

## Summary

When a campaign assault ends, each retainer who did not survive removes one soldier from the
army, taken from the swordsmen's share first, then the halberdiers', then the knights'.

## When it runs

When the assault scene exits and play returns to the campaign.

## Parameters

- `army`: the army that made the assault.

## Inputs

`fn_00029E58`, `fn_00029E80` and `fn_00029D24`.

## Procedure

```text
retainers_left = count_retainers()
let alive = min(retainers_left, retainer_cap)
let lost = retainer_cap - alive
let remaining = 0
for unit_type in 0..3:
    let taken = min(lost, retainer_shares[unit_type])
    lost = lost - taken
    let left = fn_00029E58(army, unit_type) - taken
    fn_00029E80(army, unit_type, left)
    remaining = remaining + left
if remaining == 0:
    fn_00029D24(army)
```

## Outputs

Lowers the army's unit counts and removes the army when all three reach 0.

## Edge cases

When the cap was raised from 0 to 1 (RULE-ASSAULT-001) and that retainer dies, no share holds
the loss and no soldier is removed.

## What the sources say

SRC-GAMEFAQS-66730 reports that an army of one soldier whose companion dies in the assault
disappears. That agrees with the loss of one soldier per retainer lost.

## Differences between builds

None known.

## Open questions

- `fn_00029E58`, `fn_00029E80` and `fn_00029D24` are named only by their addresses; their
  arguments are not recorded.
- Whether an army with other unit types left is also removed when the first three reach 0 is
  not recorded.
- Where `retainer_shares` is kept is not recorded.
