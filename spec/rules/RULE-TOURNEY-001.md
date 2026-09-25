---
id: RULE-TOURNEY-001
title: Monthly tournament site
status: supported
builds: [BLD-GOG-EN]
superseded_by: []
evidence: [FND-TOURNEY-001, FND-TOURNEY-002, FND-TOURNEY-003, FND-TOURNEY-004, FND-RNG-002]
conflicting: []
split_with: []
related: [RULE-RNG-001]
---

## Summary

Once a month the game picks a new tournament site, never the one it picked last time, and clears
the joust and melee counts. It keeps the count of wins.

## When it runs

From the routine at `0x0002B1DE`, likely at the turn of each month, followed by the announcement
of the site.

## Parameters

None.

## Inputs

`tournament_previous_site`.

## Procedure

```text
define start_tournament_month():
    let site = random_inclusive(13)
    while site == tournament_previous_site:
        site = random_inclusive(13)
    tournament_previous_site = site
    tournament_site = site
    tournament_place = fn_0004377C(site)
    tournament_unk_08 = 0
    melees_today = 0
    jousts_today = 0
    opponents_ready = 0
    g_0009DED4 = 0
    fn_000628AC(g_0009A928, 0, 0)
```

## Outputs

`tournament_site`, `tournament_place` and the cleared counts. The game then shows a message that a
tournament is held at `tournament_place` this month. At the start of a session the game sets the
three tournament dwords to -1, the three counts and `opponents_ready` to 0 and `g_0009DED4` to 1.
Saving writes the tournament block, `jousts_today`, `melees_today` and `tournament_wins` in that
order, and loading reads them back.

## Edge cases

The first draw is always made, and each repeat is another draw. `tournament_wins` is cleared only
by a new session and the other resets, so a player's third win after that gives fame once and
later wins do not.

## What the sources say

None of the sources describe how the site is chosen.

## Differences between builds

None known.

## Open questions

- Which routine runs at `0x0002B1DE`, and when.
- What `fn_0004377C`, `fn_000628AC`, `g_0009DED4` and `g_0009A928` are.
- What `tournament_unk_08` holds.
- When `0x0005C520` and `0x00010FF0`, which also clear the counts and `tournament_wins`, run.
