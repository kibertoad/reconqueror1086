---
id: RULE-ASSAULT-008
title: Mode transition tables and the test and handler of each mode
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ASSAULT-010, FND-ASSAULT-011, FND-ASSAULT-012, FND-ASSAULT-013, FND-ASSAULT-014]
conflicting: []
split_with: []
related: [RULE-ASSAULT-009, RULE-ASSAULT-010, RULE-ASSAULT-011, RULE-ASSAULT-012, RULE-ASSAULT-013, RULE-ASSAULT-014, RULE-ASSAULT-015, RULE-ASSAULT-016, RULE-ASSAULT-020, RULE-ASSAULT-022, RULE-ASSAULT-027, FMT-ASSAULT-001]
---

## Summary

Each mode from 1 to 17 has one test and one handler for every actor. The actor's kind picks the
pair of modes the actor moves to when the test succeeds or fails. Kind 7 uses kind 2's pairs.

## When it runs

Inside every state decision (RULE-ASSAULT-007).

## Parameters

None. The rule defines `mode_test(c)`, `set_transition(c)` and `run_handler(c)`.

## Inputs

None.

## Procedure

```text
define mode_test(c):
    let m = c.mode
    if m == 1:
        return near_friend(c)
    if m == 2:
        return near_enemy(c)
    if m == 3 or m == 5 or m == 9:
        return seek_friend(c)
    if m == 4 or m == 6 or m == 10:
        return seek_enemy(c)
    if m == 7 or m == 16:
        return friend_in_front(c)
    if m == 8:
        return enemy_in_reach(c)
    if m == 11 or m == 14 or m == 15:
        return fn_0004F89B(c)
    if m == 12:
        return at_destination(c)
    if m == 13:
        return can_rally(c)
    return player_in_sight(c)

define set_transition(c):
    # success and failure modes for modes 1 to 17; 0 marks an entry not recorded
    let success = [4, 11, 7, 8, 7, 8, 1, 11, 9, 5, 0, 0, 2, 11, 11, 17, 16]
    let failure = [3, 1, 2, 1, 5, 6, 5, 6, 6, 2, 0, 0, 10, 13, 13, 17, 17]
    let kind = kind_of(c)
    if kind == 1:
        success = [4, 10, 7, 11, 7, 11, 3, 0, 9, 4, 0, 0, 6, 0, 0, 17, 16]
        failure = [2, 1, 2, 1, 5, 6, 5, 0, 6, 2, 0, 0, 10, 0, 0, 17, 17]
    else if kind == 2 or kind == 7:
        success = [4, 11, 7, 8, 7, 8, 1, 11, 6, 4, 11, 0, 6, 11, 11, 0, 0]
        failure = [6, 1, 1, 2, 5, 6, 6, 6, 1, 3, 13, 0, 10, 13, 13, 0, 0]
    else if kind == 3:
        success = [0, 0, 0, 0, 0, 0, 0, 0, 9, 10, 0, 0, 0, 0, 0, 0, 0]
        failure = [0, 0, 0, 0, 0, 0, 0, 0, 1, 5, 0, 0, 0, 0, 0, 0, 0]
    else if kind == 4:
        success = [4, 11, 7, 8, 7, 8, 1, 11, 6, 4, 11, 0, 6, 11, 11, 0, 0]
        failure = [3, 4, 2, 1, 5, 6, 5, 6, 1, 3, 13, 0, 10, 13, 13, 0, 0]
    else if kind == 5:
        success = [0, 0, 0, 0, 0, 0, 0, 0, 9, 10, 0, 0, 0, 0, 0, 0, 0]
        failure = [0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 0, 0, 0, 0, 0, 0]
    else if kind == 6:
        success = [0, 0, 0, 0, 0, 0, 0, 0, 9, 10, 0, 0, 0, 0, 0, 0, 0]
        failure = [0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0]
    if success[c.mode - 1] != 0:
        c.next_mode = success[c.mode - 1]
    if failure[c.mode - 1] != 0:
        c.fallback_mode = failure[c.mode - 1]

define run_handler(c):
    let m = c.mode
    if m <= 4:
        hold(c)
    else if m == 5 or m == 6 or m == 17:
        wander(c)
    else if m == 7 or m == 8 or m == 16:
        approach(c)
    else if m == 9 or m == 10:
        flee(c)
    else if m == 11:
        strike(c)
    else if m == 12:
        go_to(c)
    else if m == 14:
        hit_look(c)
    else if m == 15:
        dying_look(c)
```

## Outputs

The functions return a test result, set `next_mode` and `fallback_mode`, or run a handler.

## Edge cases

- Mode 9 is entered only from mode 9 in the tables of kinds 0, 1, 2 and 4, and no template
  starts in it, so the shipped actors never use it.
- Friendly Follow alternates between modes 16 and 17 whatever the mode-16 test gives.
- Mode 13's handler does nothing, so an actor in mode 13 waits for the thinker.

## What the sources say

None of the sources describe this.

## Differences between builds

None known.

## Open questions

- The entries marked 0 have not been read: all kinds' mode 12, modes 11 of kinds 0 and 1, modes
  8, 14 and 15 of kind 1, modes 16 and 17 of kinds 2 and 4, and all of kinds 3, 5 and 6 apart
  from modes 9 and 10. The procedure leaves the pair unchanged for them; what the original
  writes there is not known.
- `fn_0004F89B`, the test of modes 11, 14 and 15, is named only by its address. Its result
  decides between mode 11 and mode 13 (RULE-ASSAULT-013).
