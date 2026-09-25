---
id: RULE-ASSAULT-006
title: Actor thinker pass
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-015, FND-ASSAULT-016, FND-ASSAULT-001]
conflicting: []
split_with: []
related: [RULE-ASSAULT-007, RULE-ASSAULT-027]
---

## Summary

Each pass of the assault main loop gives a state decision to about one sixteenth of the placed
combatants, continuing from where the last pass stopped.

## When it runs

Once per pass of the assault main loop, after input and after the effect scheduler
(RULE-ASSAULT-018). The loop does not wait between passes, so the number of passes per second
depends on the machine.

## Parameters

None.

## Inputs

None.

## Procedure

```text
let visits = (count(combatants) - 10) >> 4
for k in 0..visits:
    if thinker_cursor >= count(combatants):
        thinker_cursor = 10
    let c = combatants[thinker_cursor]
    if is_live(c):
        call RULE-ASSAULT-007(c)
    thinker_cursor = thinker_cursor + 1
```

## Outputs

Advances `thinker_cursor` and runs state decisions.

## Edge cases

- A scene with fewer than 16 placed combatants gets no visits from the thinker, and its actors
  decide only when an effect of theirs ends (RULE-ASSAULT-018).
- Because the pass rate depends on the machine, which actor decides first after an order, and
  so which of two actors sent to the same cell arrives first, has no fixed answer.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The index the cursor wraps to is not recorded; the procedure uses 10, the first placed
  combatant.
- Whether the count `visits` has a lower limit of 1 is not recorded.
- Where `combatants` is kept is not recorded.
