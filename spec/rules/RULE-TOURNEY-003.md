---
id: RULE-TOURNEY-003
title: Tournament joust wager and rewards
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-TOURNEY-001, FND-TOURNEY-003, FND-TOURNEY-004, FND-RNG-002, FND-PERSON-002, SRC-GAMEFAQS-66730, FND-TALK-006]
conflicting: []
split_with: []
related: [RULE-RNG-001, RULE-PERSON-001]
---

## Summary

The player may joust three times until the counts are next cleared. The stake is 1 to 30 shillings more than the opponent's
lance experience, and no more than the player has. A joust win counts towards the third win that
gives a point of fame and towards every sixth career win, jousts and melees together, which gives
a point of strength and dexterity.

## When it runs

When `tent_action` runs with `tent_mode` 0.

## Parameters

None.

## Inputs

`tent_opponent`, `jousts_today`, `player_accepts`.

## Procedure

```text
define joust_offer(opponent):
    joust_opponent_lance = attr(opponent, 16)
    let wager = random(30) + joust_opponent_lance + 1
    let wealth = attr(0, 17)
    if wealth == 0:
        return 0
    if wager > wealth:
        wager = wealth
    if not player_accepts:
        return 0
    fn_0003FB28(1)
    joust_player_lance = attr(0, 16)
    fn_0003FCA8(opponent, 120, wager)
    set_attr(opponent, 16, min(joust_opponent_lance, 20))
    if jousts_today == 2:
        fn_000628AC(g_0009A928, 0, 1)
    set_attr(0, 16, min(joust_player_lance, 20))
    return 1

define tent_joust():
    if jousts_today >= 3:
        return
    let wins_before = attr(0, 20)
    if joust_offer(tent_opponent) != 1:
        return
    jousts_today = jousts_today + 1
    if wins_before >= attr(0, 20):
        return
    tournament_wins = tournament_wins + 1
    if tournament_wins == 3:
        set_attr(0, 6, attr(0, 6) + 1)
    let career = wins_before + 1 + attr(0, 22)
    if career > 0 and career % 6 == 0:
        set_attr(0, 0, attr(0, 0) + 1)
        set_attr(0, 1, attr(0, 1) + 1)
```

## Outputs

The joust itself, `fn_0003FCA8`, may change `joust_player_lance` and `joust_opponent_lance`, and
decides whether the player won. With `jousts_today` at 3 the tent shows a message that everyone is
too tired to joust. With no wealth the opponent refuses. The offer names the joust of the day as
first, second or third, the opponent and the wager. Before the joust the game pauses for 60
passes of its frame wait.

## Edge cases

The draw for the wager is made before the wealth check, so a refusal still moves the generator.
The rewards compare JOUST_WON before and after the joust, so they depend on the joust adding to
it. A declined offer does not count towards the three jousts.

## What the sources say

SRC-GAMEFAQS-66730 says there are three jousts a day and that wagers usually lie between 20 and 80
shillings. The executable allows three between clears of the counts, and a joust stake of 1 to 50.

## Differences between builds

None known.

## Open questions

- What `fn_0003FCA8` does with the wager, JOUST_WON and JOUST_LOST, and what its other arguments,
  a value of 99 and the addresses of the two lance values, are for.
- Whether anything clears the joust and melee counts each day. The refusal messages speak of today, but the only clears found are the monthly reset, `0x0005C520` and `0x00010FF0`.
- What `fn_0003FB28`, `fn_000628AC` and `g_0009A928` are.
