---
id: RULE-TOURNEY-002
title: Tournament tent opponents and actions
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-TOURNEY-002, FND-TOURNEY-003, SRC-GAMEFAQS-66730]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-TOURNEY-003, RULE-TOURNEY-004]
---

## Summary

The first time the player enters the tent at a tournament, the game draws five distinct opponents
from character rows 1 to 14, leaving out row 8, the king. The player picks one from the menu and
then a joust, a melee or leaving.

## When it runs

`enter_tent()` when the tent opens, `choose_tent_opponent` when the player moves through the menu,
and `tent_action()` when the player confirms an action.

## Parameters

`choose_tent_opponent(menu_row)`: the menu row, 0 for the Exit row and 1 to 5 for the opponents.

## Inputs

`tent_mode`, `opponents_ready`.

## Procedure

```text
define pick_tent_opponents():
    for i in 0..5:
        tent_opponents[i] = 0
    for i in 0..5:
        let row = 0
        let taken = true
        while taken:
            row = random_inclusive(14)
            taken = row == 0 or row == 8
            for j in 0..5:
                if tent_opponents[j] == row:
                    taken = true
        tent_opponents[i] = row

define enter_tent():
    if opponents_ready == 0:
        pick_tent_opponents()
        opponents_ready = 1

define choose_tent_opponent(menu_row):
    let slot_of_row = [0, 0, 1, 2, 3, 4]
    tent_opponent = tent_opponents[slot_of_row[menu_row]]

define tent_action():
    if tent_mode == 0:
        tent_joust()
    else if tent_mode == 1:
        tent_melee()
    else if tent_mode == 2:
        fn_0005CA70()
```

## Outputs

`tent_opponents` and `tent_opponent`. The tent shows the opponent's name and, in joust mode, his
JOUST_WON and JOUST_LOST, or in melee mode his MELEE_WON and MELEE_LOST; the Exit row shows its
label and still sets `tent_opponent` to the first slot.

## Edge cases

A drawn row that is 0, 8 or already taken costs another draw. The five are drawn once per
tournament, since only the monthly reset clears `opponents_ready`.

## What the sources say

SRC-GAMEFAQS-66730 says the player chooses among opponents, and that there are five for the
melee. The executable offers the same five for both.

## Differences between builds

None known.

## Open questions

- What `fn_0005CA70` does; it likely leaves the tent.
