---
id: RULE-ESTATE-003
title: Loans and the July collection
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-ESTATE-003, FND-PERSON-001, FND-STRATEGY-001, FND-STRATEGY-019, FND-STRATEGY-021, FND-STRATEGY-033, FND-STRATEGY-036, FND-TOURNEY-001, FND-TOURNEY-004, SRC-MANUAL, SRC-GAMEFAQS-66730, FND-TALK-006, FND-ASSAULT-048]
conflicting: []
split_with: []
related: [RULE-ESTATE-002, RULE-PERSON-001, RULE-STRATEGY-019]
---

## Summary

A loan is repaid in July with half again as interest. When the fief's wealth covers the amount the
player may pay it; otherwise, or on a refusal, the player fights Drogo in `MONEY.RES`, alone against four hostile
combatants (FND-ASSAULT-048). Losing kills
the player. Winning after the player could not pay clears the debt, but winning after a refusal
leaves it (BUG-ESTATE-003).

## When it runs

`collect_debt` from `monthly_estate_pass` in month 6.

## Parameters

None.

## Inputs

Attribute 26 of row 0, `fiefs`, `player_pays_debt`.

## Procedure

```text
define collect_debt():
    # attribute 26 is MONEY_BORROWED
    let borrowed = attr(0, 26)
    if borrowed <= 0:
        return 0
    let due = borrowed + borrowed / 2
    if due <= fiefs[0].wealth:
        # asks whether the player will pay due
        if player_pays_debt:
            fiefs[0].wealth = fiefs[0].wealth - due
            fn_000628AC(g_0009A928, 6, 0)
            fn_000628AC(g_0009A928, 1, 0)
            set_attr(0, 26, 0)
            return 0
        let screen = fn_00059CDC()
        fn_0003CED8()
        let won = melee_from_map("MONEY.RES")
        fn_0001070C(screen)
        if won == 1:
            # shows that Drogo is dead and the moneylender will not come again
            return 0
    else:
        # shows that Drogo knows the player cannot pay
        let screen = fn_00059CDC()
        fn_0003CED8()
        let won = melee_from_map("MONEY.RES")
        fn_0001070C(screen)
        if won == 1:
            # shows that Drogo is dead and the moneylender will not come again
            fn_000628AC(g_0009A928, 1, 0)
            fn_000628AC(g_0009A928, 6, 0)
            set_attr(0, 26, 0)
            return 0
    # shows that Drogo killed the player
    map_session_over = 1
    fn_000106C0()
    fn_0001BF54(2)
    return 1
```

## Outputs

The fief's wealth and MONEY_BORROWED, two conversation variables, and the end of the session when
the player dies.

## Edge cases

`borrowed / 2` truncates, so an odd loan's interest is rounded down. A debt left by a won fight after
a refusal is collected again the next July, although the message says the moneylender will not come
again.

## What the sources say

SRC-MANUAL (p. 28) says the moneylender lends 20 to 200 shillings at 50 percent interest, due July 1,
and that Drogo is sent after a player who does not pay on time. SRC-GAMEFAQS-66730 gives the same
ceiling and interest. The executable's repayment agrees with the interest.

## Differences between builds

None known.

## Open questions

- Where the loan is taken, and whether the executable enforces the 20 to 200 range the manual gives.
- What conversation variables 1 and 6 of `g_0009A928` hold, and what `fn_000628AC` does with them.
- What `fn_00059CDC`, `fn_0003CED8` and `fn_0001070C` do around the fight, and what `fn_000106C0` and
  `fn_0001BF54` do when the player dies.
