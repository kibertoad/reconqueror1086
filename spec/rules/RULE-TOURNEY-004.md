---
id: RULE-TOURNEY-004
title: Tournament melee wager, scene and settlement
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-TOURNEY-001, FND-TOURNEY-003, FND-TOURNEY-005, FND-TOURNEY-006, FND-RNG-002, FND-PERSON-002, SRC-GAMEFAQS-66730, FND-UI-004]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-PERSON-001, RULE-TOURNEY-002, RULE-UI-002]
---

## Summary

The player may fight one melee until the counts are next cleared. The stake grows with the average sword experience of the
player and the opponent, and it is settled in the player's wealth; the winner gains sword
experience and the tallies record the result.

## When it runs

When `tent_action` runs with `tent_mode` 1.

## Parameters

None.

## Inputs

`tent_opponent`, `melees_today`, `place_person`, `player_accepts`.

## Procedure

```text
define tent_melee():
    if melees_today >= 1:
        return
    let half = (attr(0, 15) + attr(tent_opponent, 15)) / 2
    let tier = 4
    if half < 10:
        tier = 0
    else if half < 13:
        tier = 1
    else if half < 16:
        tier = 2
    else if half < 18:
        tier = 3
    let site_digit = place_person % 3
    let wager = (tier + 1) * (random_inclusive(20) + 1)
    let wealth = attr(0, 17)
    if wealth == 0:
        return
    if wager > wealth:
        wager = wealth
    if not player_accepts:
        return
    let melee_wins_before = attr(0, 22)
    # The tier never reaches the name (BUG-TOURNEY-001).
    let scene = sprintf("MELEE%d%d.RES", site_digit, site_digit)
    let side_b = random_inclusive(2) + 8
    let side_a = random_inclusive(2) + 8
    let result = fn_0005877C(scene, side_a, side_b, 0)
    if result == 1:
        tournament_wins = tournament_wins + 1
        fn_0002C218(fn_0002C20C() + wager)
        set_attr(0, 17, attr(0, 17) + wager)
        set_attr(0, 15, attr(0, 15) + 1)
        if tournament_wins == 3:
            set_attr(0, 6, attr(0, 6) + 1)
        let career = melee_wins_before + 1 + attr(0, 20)
        if career > 0 and career % 6 == 0:
            set_attr(0, 0, attr(0, 0) + 1)
            set_attr(0, 1, attr(0, 1) + 1)
        set_attr(0, 22, attr(0, 22) + 1)
        set_attr(tent_opponent, 23, attr(tent_opponent, 23) + 1)
    else:
        fn_0002C218(fn_0002C20C() - wager)
        set_attr(0, 17, attr(0, 17) - wager)
        set_attr(tent_opponent, 15, attr(tent_opponent, 15) + 1)
        set_attr(tent_opponent, 22, attr(tent_opponent, 22) + 1)
        set_attr(0, 23, attr(0, 23) + 1)
    enter_tent()
    melees_today = melees_today + 1
```

## Outputs

With `melees_today` at 1 or more the tent shows a message that everyone is too tired to melee.
With no wealth the opponent refuses. The offer names the opponent and the wager. The game closes
the tent before the fight, and after it shows a win or loss picture for 240 passes of its frame
wait before it reopens the tent.

## Edge cases

`half` is truncated toward zero. The wager draw is made before the wealth check. The two side
draws are made in the order shown, so the first goes to the third argument. The wager is from 1
to 105 before the wealth limit.

## What the sources say

SRC-GAMEFAQS-66730 says the melee is against one of five opponents, with about eight soldiers on
each side, and that wagers usually lie between 20 and 80 shillings. The side draws give 8 to 10.

## Differences between builds

None known.

## Open questions

- Whether anything clears the joust and melee counts each day. The refusal messages speak of today, but the only clears found are the monthly reset, `0x0005C520`, `0x00010FF0` and the village map exit while a tournament is in town (RULE-UI-002).
- What the value `fn_0002C20C` returns and `fn_0002C218` stores
  stands for.
- How `fn_0005877C` uses its arguments; `side_a` and `side_b` are likely the two side sizes.
