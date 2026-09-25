---
id: RULE-ASSAULT-011
title: Acquire the nearest visible friend
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-019]
conflicting: []
split_with: []
related: [RULE-ASSAULT-010]
---

## Summary

The same search as RULE-ASSAULT-010 over combatants on the actor's own side, stopping at the
first one closer than `0x200`.

## When it runs

As the test of modes 3, 5 and 9 (RULE-ASSAULT-008).

## Parameters

None. The rule defines `seek_friend(c)`.

## Inputs

None.

## Procedure

```text
define seek_friend(c):
    return seek(c, true, 0x200)
```

## Outputs

As RULE-ASSAULT-010.

## Edge cases

As RULE-ASSAULT-010.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

None known.
