---
id: RULE-ASSAULT-007
title: State decision for one actor
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-010, FND-ASSAULT-015, FND-ASSAULT-026]
conflicting: []
split_with: []
related: [RULE-ASSAULT-008, RULE-ASSAULT-018, RULE-ASSAULT-022, RULE-ASSAULT-027, FMT-ASSAULT-001]
---

## Summary

An actor runs the test of its current mode, takes the transition its kind gives for that mode,
moves to the success or failure mode, and runs that mode's handler. The new mode's test waits
for the next decision.

## When it runs

From the thinker (RULE-ASSAULT-006), and at once whenever one of the actor's effects ends
(RULE-ASSAULT-018).

## Parameters

- `c`: the combatant. The rule also defines `take_transition(c, success)`.

## Inputs

None.

## Procedure

```text
define take_transition(c, success):
    let old = c.mode
    if success:
        c.mode = c.next_mode
    else:
        c.mode = c.fallback_mode
    if c.mode != old:
        cancel_effect(c)

if c.health <= 0:
    remove_dead(c)
    return
let passed = mode_test(c)
set_transition(c)
take_transition(c, passed)
run_handler(c)
```

## Outputs

Changes the actor's mode, and whatever the handler of the new mode changes.

## Edge cases

- A handler that starts no effect (modes 1 to 4 and 13) leaves the actor waiting for the
  thinker's next visit.
- The transition body overwrites `next_mode` and `fallback_mode` before the helper reads them,
  so a mode requested by an order survives only when the helper has already installed it.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The condition under which the state routine removes a combatant whose death effect has
  finished is not recorded; the procedure uses `health <= 0`.
- What counts as a change of state for `take_transition`, and so when the previous effect is
  cancelled, is not recorded; the procedure uses a change of mode.
- How the routine treats the mode value the retainer orders reset to (RULE-ASSAULT-004) is not
  recorded.
